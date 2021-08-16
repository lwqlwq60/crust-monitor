#!/usr/bin/env bash

set -o errexit
set -o pipefail
set -o nounset
# set -o xtrace

PUSH_IMAGE=false

[[ $# > 0 ]] && [ $1 == "-y" ] && PUSH_IMAGE=true

echo "PUSH_IMAGE:$PUSH_IMAGE"

# Build images

bash ./common/build-docker-image.sh -n "scheduler" \
                                    -p "registry.garrettmotion.io" \
                                    -f "./Dockerfile-scheduler" \
                                    -d "runtime-scheduler"

# Push images
# Execute after all build success.
# Since we need to publish multiple tags, the main logic of build-docker image need to be re executed.

if [ $PUSH_IMAGE == true ]; then

bash ./common/build-docker-image.sh   -n "scheduler" \
                                      -p "registry.garrettmotion.io" \
                                      -f "./Dockerfile-scheduler" \
                                      -d "runtime-scheduler"\
                                      -u \
                                      -o

fi
