#!/usr/bin/env bash

set -o errexit
set -o pipefail
set -o nounset
# set -o xtrace

# Set magic variables for current file & dir
__DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
__FILE="${__DIR}/$(basename "${BASH_SOURCE[0]}")"
__BASE="$(basename ${__FILE} .sh)"
__HOSTNAME=`hostname -s`
__NAME=$(basename $0|sed 's/\(\..*\)$//')
__VERSION="0.1"

function version() {
    cat <<EOF >&2
$__NAME - $__VERSION
EOF
}

function usage() {
    cat <<EOUSAGE >&2
[$@]
Usage: $__NAME [OPTIONS] ARGS...
Description on how to use this script...
OPTION      DESCRIPTION
==========  ==================================================================
-v          version
-h          help
-n          image-name
-p          image-prefix
-f          docker-file
-d          docker-file-target
-a          build-args
-u          push-image
-r          release-tag-prefix
EOUSAGE
}

# Log to output
function log() {
    echo "$(date +%Y%m%d.%H%M%S) [$__NAME:$$] $@"
}

#################################################################################
# Main body

# Variables
IMAGE_NAME=""
IMAGE_PREFIX=""
DOCKER_FILE=""
DOCKER_FILE_TARGET=""
BUILD_ARGS=""
PUSH_IMAGE=false
PUSH_ONLY=false
RELEASE_TAG_PREFIX="release"

# Parse options
die() { log "$*" >&2; exit 2; }  # complain to STDERR and exit with error
needs_arg() { if [ -z "$OPTARG" ]; then die "No arg for --$OPT option"; fi; }
arg_required() { die "argument $@ is required, not able to proceed."; }

while getopts "hvuo-:n:p:f:d:a:t:e:" OPT; do
  if [ "$OPT" = "-" ]; then   # long option: reformulate OPT and OPTARG
    OPT="${OPTARG%%=*}"       # extract long option name
    OPTARG="${OPTARG#$OPT}"   # extract long option argument (may be empty)
    OPTARG="${OPTARG#=}"      # if long option argument, remove assigning `=`
  fi
  case $OPT in
     h) usage "Help Requested" ; exit 0;;
     v | version) version; exit 0;;
     n | image-name)          needs_arg; IMAGE_NAME="$OPTARG" ;;
     p | image-prefix)        needs_arg; IMAGE_PREFIX="$OPTARG" ;;
     f | docker-file)         needs_arg; DOCKER_FILE="$OPTARG" ;;
     d | docker-file-target)  needs_arg; DOCKER_FILE_TARGET="$OPTARG" ;;
     a | build-args)          needs_arg; BUILD_ARGS="$OPTARG" ;;
     u | push-image)          PUSH_IMAGE=true ;;
     o | push-only)           PUSH_ONLY=true ;;
     r | release-tag-prefix)  needs_arg; RELEASE_TAG_PREFIX="$OPTARG" ;;
     ??* )          die "Illegal option --$OPT" ;;  # bad long option
     \? )           exit 2 ;;  # bad short option (error reported via getopts)
  esac
done
shift $((OPTIND-1)) # remove parsed options and args from $@ list

# Validate parameters
[ -z "$IMAGE_NAME" ]          && arg_required "-n (--image-name)"
[ -z "$IMAGE_PREFIX" ]        && arg_required "-p (--image-prefix)"
[ -z "$DOCKER_FILE" ]         && arg_required "-f (--docker-file)"
[ -z "$DOCKER_FILE_TARGET" ]  && arg_required "-d (--docker-file-target)"
[ -z "$RELEASE_TAG_PREFIX" ]  && arg_required "-r (--release-tag-prefix)"

# Log paremeters before execution
log "IMAGE_NAME: $IMAGE_NAME"
log "IMAGE_PREFIX: $IMAGE_PREFIX"
log "DOCKER_FILE: $DOCKER_FILE"
log "DOCKER_FILE_TARGET: $DOCKER_FILE_TARGET"
log "BUILD_ARGS: $BUILD_ARGS"
log "RELEASE_TAG_PREFIX: $RELEASE_TAG_PREFIX"
log "PUSH_IMAGE: $PUSH_IMAGE"
log "PUSH_ONLY: $PUSH_ONLY"

# Get latest tag
LATEST_TAG=$(git describe --match "$RELEASE_TAG_PREFIX-*" --abbrev=0 --tags $(git rev-list --tags --max-count=1) 2>/dev/null || echo "" )
GIT_HEAD_HASH=$(git rev-parse HEAD)
GIT_LATEST_TAG_HASH=$(git log $LATEST_TAG -1 --pretty=%H)
[ ! -z $LATEST_TAG ] && [ $GIT_HEAD_HASH == $GIT_LATEST_TAG_HASH ] && APP_VERSION=$LATEST_TAG || APP_VERSION="non-$RELEASE_TAG_PREFIX"

log "Latset tag:$LATEST_TAG Latest tag hash:$GIT_LATEST_TAG_HASH Head hash:$GIT_HEAD_HASH App Version: $APP_VERSION"

GIT_COMMIT=$(git log -1 --format=%h)
BUILD_TIME=$(date +"%Y-%m-%d_%T")

log "LATEST_TAG: $LATEST_TAG GIT_COMMIT: $GIT_COMMIT BUILD_TIME: $BUILD_TIME"

function main (){

  # Build docker image
  if [ $PUSH_ONLY == false ]; then
    sudo docker build -f "$DOCKER_FILE" -t $DOCKER_FILE_TARGET-local-build \
      --build-arg APP_GIT_COMMIT=$GIT_COMMIT \
      --build-arg APP_VERSION=$APP_VERSION \
      --build-arg APP_BUILD_TIME=$BUILD_TIME \
      $BUILD_ARGS \
      --target $DOCKER_FILE_TARGET ../
  else
    log "PUSH_ONLY is $PUSH_ONLY, skip build image"
  fi

  # Build local docker images
  tag_image $GIT_COMMIT
  
  # Get git branch
  GIT_RAW_BRANCH=$(git rev-parse --abbrev-ref HEAD)
  # Remove / for valid image tag name
  GIT_BRANCH=${GIT_RAW_BRANCH//\//\-}
  log "Branch:$GIT_BRANCH"
  
  # Crete branch image
  tag_image $GIT_BRANCH

  # Tag latest if current branch is master
  if [ $GIT_BRANCH == "master" ]; then
    log "Starting tag latest"

    tag_image "latest"
  fi

  # Tag version
  if [ ! -z $LATEST_TAG ]; then

    if [ $GIT_HEAD_HASH == $GIT_LATEST_TAG_HASH ]; then
      log "Starting tag label.."

      TAG=${LATEST_TAG%-*}

      log "Starting tag git tag:$TAG"
      tag_image $TAG

      if [ $GIT_BRANCH == "master" ] && [ $TAG == $RELEASE_TAG_PREFIX ]; then

        REVISION=${LATEST_TAG##*-}
        MINOR=${REVISION%.*}
        MAJOR=${MINOR%.*}

        log "It's master and git tag prefix is $RELEASE_TAG_PREFIX, starting tag revisions: Revision:$REVISION Minor:$MINOR Major:$MAJOR"

        tag_image $REVISION
        tag_image $MINOR
        tag_image $MAJOR
      fi

    else
      log "Latest tag is not head, skipped tagging."
    fi
  else
    log "There's no tag (e.g. $RELEASE_TAG_PREFIX-*) found, skip tag image."
  fi
}

function tag_image() {
  
  if [ $PUSH_ONLY == false ]; then
    log "In tag_image $IMAGE_PREFIX/$IMAGE_NAME:$1"
    sudo docker tag $DOCKER_FILE_TARGET-local-build $IMAGE_PREFIX/$IMAGE_NAME:$1
    log "Images have been tagged."
  else
    log "PUSH_ONLY is $PUSH_ONLY, skip tag image"
  fi

  if [ $PUSH_IMAGE == true ]; then
    log "Starting to push images to repository... "
    sudo docker push $IMAGE_PREFIX/$IMAGE_NAME:$1
    log "Images are pushed to repository."
  fi
}

# Execute main function
main
