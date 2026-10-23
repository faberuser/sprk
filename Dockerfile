FROM rust:1.93-bookworm AS build
WORKDIR /build
COPY Cargo.toml Cargo.lock ./
COPY src ./src
RUN cargo build --locked --release

FROM debian:bookworm-slim AS runtime
RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates libssl3 curl openssl wine64 xvfb xauth fonts-liberation \
    && rm -rf /var/lib/apt/lists/* \
    && groupadd --gid 10001 sprk \
    && useradd --uid 10001 --gid sprk --no-create-home sprk \
    && mkdir /data \
    && chown sprk:sprk /data
COPY --from=build /build/target/release/sprk-server /usr/local/bin/sprk-server
COPY tables /app/tables
COPY config/client-update-public.pem /app/client-update-public.pem
COPY --chown=sprk:sprk build-input/worker /opt/sprk-worker
COPY --chmod=755 scripts/docker/docker-entrypoint.sh /usr/local/bin/sprk-entrypoint
COPY --chmod=755 scripts/docker/docker-battle-worker.sh /usr/local/bin/sprk-battle-worker
COPY --chmod=755 scripts/docker/docker-healthcheck.sh /usr/local/bin/sprk-healthcheck
ARG CLIENT_UPDATE_TAG=""
ARG CLIENT_UPDATE_REPO=""
ENV SPRK_UPDATES_GITHUB_TAG=${CLIENT_UPDATE_TAG} SPRK_UPDATES_GITHUB_REPO=${CLIENT_UPDATE_REPO}
ENV SPRK_UPDATES_PUBLIC_KEY_FILE=/app/client-update-public.pem
ENV SPRK_SERVICE_MODE=game GAME_TABLES_PATH=/app/tables PORT=8080
WORKDIR /data
USER sprk
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=5s --start-period=3m --retries=3 \
    CMD /usr/local/bin/sprk-healthcheck
ENTRYPOINT ["sprk-entrypoint"]

# Optional integration target: the real native combat tests run with the same
# runtime, signed client and Wine wrapper as production, without a live DB.
FROM build AS test-build
COPY scripts/DllPatcher/Resources/LegacyData/RestoredCrafting.json scripts/DllPatcher/Resources/LegacyData/RestoredCrafting.json
RUN cargo test --locked --no-run \
    && find target/debug/deps -maxdepth 1 -type f -executable -name 'sprk_server-*' \
       -exec cp {} /sprk-native-tests \;

FROM runtime AS native-tests
USER root
COPY --from=test-build /sprk-native-tests /usr/local/bin/sprk-native-tests
RUN mkdir -p /app/target/conquest-lab /app/target/party-followup \
    && chown -R sprk:sprk /app/target
USER sprk
WORKDIR /app
ENV CONQUEST_NATIVE_EXECUTABLE=/usr/local/bin/sprk-battle-worker
ENV SPRK_TABLE_FIXTURE=/app/tables
HEALTHCHECK NONE
ENTRYPOINT ["sprk-native-tests"]

FROM runtime AS production
