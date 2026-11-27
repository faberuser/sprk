#!/usr/bin/env bash
# Build once, then run local game and launcher update services together (Linux/macOS).
exec bash "$(dirname -- "${BASH_SOURCE[0]}")/server/scripts/local/start_local_server.sh" "$@"
