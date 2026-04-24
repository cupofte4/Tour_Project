#!/usr/bin/env bash
set -euo pipefail

DOCKERHUB_USERNAME="${1:-${DOCKERHUB_USERNAME:-}}"
IMAGE_TAG="${2:-${IMAGE_TAG:-latest}}"
PLATFORM="${PLATFORM:-linux/amd64}"

if [[ -z "${DOCKERHUB_USERNAME}" ]]; then
  echo "Usage: ./deploy-backend.sh <dockerhub-username> [tag]"
  echo "Or set DOCKERHUB_USERNAME in the environment."
  exit 1
fi

IMAGE_NAME="${DOCKERHUB_USERNAME}/tour-backend:${IMAGE_TAG}"

echo "Building and pushing ${IMAGE_NAME} for platform ${PLATFORM}..."
docker buildx build \
  --platform "${PLATFORM}" \
  -f backend/Dockerfile \
  -t "${IMAGE_NAME}" \
  --push .

echo "Done."
echo "EC2 update commands:"
echo "  sudo docker compose -f docker-compose.yml pull backend"
echo "  sudo docker compose -f docker-compose.yml up -d backend"
