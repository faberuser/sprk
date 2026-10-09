FROM rust:1.93-bookworm AS build
WORKDIR /build
COPY Cargo.toml Cargo.lock ./
COPY src ./src
RUN cargo build --locked --release

FROM debian:bookworm-slim
RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates libssl3 curl \
    && rm -rf /var/lib/apt/lists/* \
    && groupadd --gid 10001 sprk \
    && useradd --uid 10001 --gid sprk --no-create-home sprk \
    && mkdir /data \
    && chown sprk:sprk /data
COPY --from=build /build/target/release/sprk-server /usr/local/bin/sprk-server
COPY tables /app/tables
ENV SPRK_SERVICE_MODE=game GAME_TABLES_PATH=/app/tables PORT=8080
WORKDIR /data
USER sprk
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 \
    CMD curl --fail --silent "http://127.0.0.1:${PORT}/health" || exit 1
ENTRYPOINT ["sprk-server"]
