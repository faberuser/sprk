FROM python:3.12-slim-bookworm AS illustration-model
COPY server/scripts/docker/prepare_illustration_runtime.py /prepare.py
RUN python /prepare.py --server /models

FROM rust:1.99-bookworm AS chef
RUN cargo install cargo-chef --version 0.1.78 --locked
WORKDIR /build

FROM chef AS planner
COPY Cargo.toml Cargo.lock ./
COPY server/Cargo.toml ./server/Cargo.toml
COPY server/src ./server/src
RUN cargo chef prepare --recipe-path recipe.json

# Source edits leave the dependency recipe unchanged, preserving these layers.
FROM chef AS test-deps
COPY --from=planner /build/recipe.json recipe.json
RUN cargo chef cook --locked --tests --recipe-path recipe.json

FROM chef AS build
COPY --from=planner /build/recipe.json recipe.json
RUN cargo chef cook --locked --release --recipe-path recipe.json
COPY Cargo.toml Cargo.lock ./
COPY server/Cargo.toml ./server/Cargo.toml
COPY server/src ./server/src
# Embedded at compile time by sqlx::migrate!().
COPY server/migrations ./server/migrations
RUN cargo build --locked --release

FROM scratch AS server-binary
COPY --from=build /build/target/release/sprk-server /sprk-server

FROM debian:bookworm-slim AS runtime-base
RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates libssl3 curl openssl wine64 xvfb xauth fonts-liberation \
    && rm -rf /var/lib/apt/lists/* \
    && groupadd --gid 10001 sprk \
    && useradd --uid 10001 --gid sprk --no-create-home sprk \
    && mkdir /data \
    && chown sprk:sprk /data
COPY tables /app/tables
COPY config/client-update-public.pem /app/client-update-public.pem
COPY --from=illustration-model /models /app/illustration-models
COPY --chown=sprk:sprk build-input/worker /opt/sprk-worker
COPY --chmod=755 server/scripts/docker/docker-entrypoint.sh /usr/local/bin/sprk-entrypoint
COPY --chmod=755 server/scripts/docker/docker-battle-worker.sh /usr/local/bin/sprk-battle-worker
COPY --chmod=755 server/scripts/docker/docker-healthcheck.sh /usr/local/bin/sprk-healthcheck
ARG CLIENT_UPDATE_TAG=""
ARG CLIENT_UPDATE_REPO=""
ENV SPRK_UPDATES_GITHUB_TAG=${CLIENT_UPDATE_TAG} SPRK_UPDATES_GITHUB_REPO=${CLIENT_UPDATE_REPO}
ENV SPRK_UPDATES_PUBLIC_KEY_FILE=/app/client-update-public.pem
ENV SPRK_ILLUSTRATION_MODELS_DIR=/app/illustration-models
ENV SPRK_SERVICE_MODE=game GAME_TABLES_PATH=/app/tables PORT=8080
WORKDIR /data
USER sprk
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=5s --start-period=3m --retries=3 \
    CMD /usr/local/bin/sprk-healthcheck
ENTRYPOINT ["sprk-entrypoint"]

FROM runtime-base AS runtime
COPY --from=build /build/target/release/sprk-server /usr/local/bin/sprk-server

# Deployment consumes the binary artifact compiled in parallel with the client.
FROM runtime-base AS precompiled-production
COPY --chmod=755 build-input/server/sprk-server /usr/local/bin/sprk-server

FROM runtime AS production
