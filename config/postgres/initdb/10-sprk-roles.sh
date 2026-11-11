#!/bin/sh
# Runs once, when the postgres volume is initialized (docker-entrypoint-initdb.d).
# Creates the sprk database with two roles:
#   sprk_owner  owns the schema and runs migrations (DATABASE_MIGRATION_URL)
#   sprk_app    the game's runtime role, data access only (DATABASE_URL)
# Changing the passwords later is done with ALTER ROLE, not by editing this file.
set -eu
: "${SPRK_DB_OWNER_PASSWORD:?set SPRK_DB_OWNER_PASSWORD}"
: "${SPRK_DB_APP_PASSWORD:?set SPRK_DB_APP_PASSWORD}"

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname postgres \
    -v owner_password="$SPRK_DB_OWNER_PASSWORD" -v app_password="$SPRK_DB_APP_PASSWORD" <<'SQL'
CREATE ROLE sprk_owner LOGIN PASSWORD :'owner_password';
CREATE ROLE sprk_app LOGIN PASSWORD :'app_password';
CREATE DATABASE sprk OWNER sprk_owner;
REVOKE ALL ON DATABASE sprk FROM PUBLIC;
GRANT CONNECT, TEMPORARY ON DATABASE sprk TO sprk_owner, sprk_app;

-- A stuck request must not hold locks indefinitely. Times are UTC like
-- SQLite's datetime('now').
ALTER ROLE sprk_app IN DATABASE sprk SET timezone = 'UTC';
ALTER ROLE sprk_app IN DATABASE sprk SET statement_timeout = '30s';
ALTER ROLE sprk_app IN DATABASE sprk SET lock_timeout = '10s';
ALTER ROLE sprk_app IN DATABASE sprk SET idle_in_transaction_session_timeout = '60s';
ALTER ROLE sprk_owner IN DATABASE sprk SET timezone = 'UTC';

\connect sprk
ALTER SCHEMA public OWNER TO sprk_owner;
REVOKE CREATE ON SCHEMA public FROM PUBLIC;
GRANT USAGE ON SCHEMA public TO sprk_app;
-- Tables and identity sequences created by migrations become usable by the app.
ALTER DEFAULT PRIVILEGES FOR ROLE sprk_owner IN SCHEMA public
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO sprk_app;
ALTER DEFAULT PRIVILEGES FOR ROLE sprk_owner IN SCHEMA public
    GRANT USAGE, SELECT ON SEQUENCES TO sprk_app;

-- Query statistics for diagnostics (needs shared_preload_libraries).
CREATE EXTENSION IF NOT EXISTS pg_stat_statements;
GRANT pg_read_all_stats TO sprk_owner;
SQL
