-- system::revisions / progression::cache: which parts of an account's
-- progression changed. Every progression revision bump also records its part
-- (the table, or `table:kind` for a table with a `kind` column) with the
-- revision it produced, so the progression cache reloads and re-evaluates
-- only the parts changed since the revision it holds. A progression metric
-- also names its day (`progression_metrics:kind@day`), so only that day's rows
-- are reloaded. Other scopes record nothing. The trigger installation in 0009 is unchanged; only the trigger
-- functions are replaced, so an image built before this migration keeps
-- working against it.

CREATE TABLE response_revision_parts (
    scope text NOT NULL,
    account bigint NOT NULL,
    part text NOT NULL,
    revision bigint NOT NULL,
    PRIMARY KEY (scope, account, part)
) WITH (fillfactor = 85);

CREATE FUNCTION sprk_revision_bump_part(p_scope text, p_account bigint, p_part text) RETURNS void
LANGUAGE plpgsql AS $$
DECLARE
    bumped bigint;
    added boolean;
    kind_part text;
    collapsed bigint;
BEGIN
    INSERT INTO response_revisions (scope, account, revision) VALUES (p_scope, p_account, 1)
    ON CONFLICT (scope, account) DO UPDATE SET revision = response_revisions.revision + 1
    RETURNING revision INTO bumped;
    IF p_scope = 'progression' THEN
        INSERT INTO response_revision_parts (scope, account, part, revision)
        VALUES (p_scope, p_account, p_part, bumped)
        ON CONFLICT (scope, account, part) DO UPDATE SET revision = excluded.revision
        RETURNING xmax = 0 INTO added;
        -- Bound the per-day metric parts: the first change of a kind on a new
        -- day folds that kind's other days into its whole-kind part (which
        -- reloads every day of the kind) with their latest revision, so no
        -- change since any revision is lost.
        IF added AND left(p_part, 20) = 'progression_metrics:' AND strpos(p_part, '@') > 0 THEN
            kind_part := left(p_part, length(p_part) - strpos(reverse(p_part), '@'));
            WITH folded AS (
                DELETE FROM response_revision_parts
                WHERE scope = p_scope AND account = p_account AND part <> p_part
                  AND left(part, length(kind_part) + 1) = kind_part || '@'
                RETURNING revision
            ) SELECT max(revision) INTO collapsed FROM folded;
            IF collapsed IS NOT NULL THEN
                INSERT INTO response_revision_parts (scope, account, part, revision)
                VALUES (p_scope, p_account, kind_part, collapsed)
                ON CONFLICT (scope, account, part)
                DO UPDATE SET revision = GREATEST(response_revision_parts.revision, excluded.revision);
            END IF;
        END IF;
    END IF;
END $$;

-- As in 0009, plus the part. A row whose kind (or metric day) changes for the
-- same owner records both parts.
CREATE OR REPLACE FUNCTION sprk_revision_owner() RETURNS trigger LANGUAGE plpgsql AS $$
DECLARE
    scope text := TG_ARGV[0];
    new_row jsonb;
    old_row jsonb;
    new_owner bigint;
    old_owner bigint;
    new_part text;
    old_part text;
BEGIN
    IF TG_OP <> 'DELETE' THEN
        new_row := to_jsonb(NEW);
        new_owner := (new_row ->> TG_ARGV[1])::bigint;
        new_part := TG_TABLE_NAME || COALESCE(':' || (new_row ->> 'kind'), '')
            || CASE WHEN TG_TABLE_NAME = 'progression_metrics' THEN '@' || (new_row ->> 'day') ELSE '' END;
        PERFORM sprk_revision_bump_part(scope, new_owner, new_part);
    END IF;
    IF TG_OP <> 'INSERT' THEN
        old_row := to_jsonb(OLD);
        old_owner := (old_row ->> TG_ARGV[1])::bigint;
        old_part := TG_TABLE_NAME || COALESCE(':' || (old_row ->> 'kind'), '')
            || CASE WHEN TG_TABLE_NAME = 'progression_metrics' THEN '@' || (old_row ->> 'day') ELSE '' END;
        IF TG_OP = 'DELETE' OR old_owner IS DISTINCT FROM new_owner
           OR old_part IS DISTINCT FROM new_part THEN
            PERFORM sprk_revision_bump_part(scope, old_owner, old_part);
        END IF;
    END IF;
    RETURN NULL;
END $$;

CREATE OR REPLACE FUNCTION sprk_revision_global() RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
    PERFORM sprk_revision_bump_part(TG_ARGV[0], 0, TG_TABLE_NAME);
    RETURN NULL;
END $$;

CREATE OR REPLACE FUNCTION sprk_revision_world_boss() RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
    IF (TG_OP <> 'DELETE' AND NEW.family = 'world_boss')
       OR (TG_OP <> 'INSERT' AND OLD.family = 'world_boss') THEN
        PERFORM sprk_revision_bump_part('progression', 0, 'battle_scores');
    END IF;
    RETURN NULL;
END $$;

CREATE OR REPLACE FUNCTION sprk_revision_conquest_history() RETURNS trigger LANGUAGE plpgsql AS $$
DECLARE
    new_match boolean := TG_OP <> 'DELETE' AND NEW.kind = 'guild_conquest_history';
    old_match boolean := TG_OP <> 'INSERT' AND OLD.kind = 'guild_conquest_history';
    new_owner bigint;
    old_owner bigint;
BEGIN
    IF new_match THEN
        new_owner := COALESCE(sprk_json_int(NEW.data, '$.AccountId'), 0);
        PERFORM sprk_revision_bump_part('progression', new_owner, 'community_state:guild_conquest_history');
    END IF;
    IF old_match THEN
        old_owner := COALESCE(sprk_json_int(OLD.data, '$.AccountId'), 0);
        IF NOT (new_match AND old_owner = new_owner) THEN
            PERFORM sprk_revision_bump_part('progression', old_owner, 'community_state:guild_conquest_history');
        END IF;
    END IF;
    RETURN NULL;
END $$;

-- progression::state reloads only the metric kinds that changed.
CREATE INDEX progression_metrics_kind ON progression_metrics (account_id, kind);
