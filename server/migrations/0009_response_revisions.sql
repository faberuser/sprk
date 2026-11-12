-- system::revisions: per-account response cache revisions. A change to a
-- row bumps the revision of the account(s) whose cached responses depend on
-- it. ('progression', 0) is the global revision every progression cache also
-- checks; only data read across accounts may bump it.
--
-- Keep the table list in sync with system::revisions::migrate (the SQLite
-- generator) until the port removes it.

CREATE TABLE response_revisions (
    scope text NOT NULL,
    account bigint NOT NULL,
    revision bigint NOT NULL DEFAULT 0,
    PRIMARY KEY (scope, account)
) WITH (fillfactor = 85);
INSERT INTO response_revisions (scope, account) VALUES ('progression', 0);

CREATE FUNCTION sprk_revision_bump(p_scope text, p_account bigint) RETURNS void
LANGUAGE sql AS $$
    INSERT INTO response_revisions (scope, account, revision) VALUES (p_scope, p_account, 1)
    ON CONFLICT (scope, account) DO UPDATE SET revision = response_revisions.revision + 1
$$;

-- Owner is a bigint column: TG_ARGV = (scope, column). An update that moves
-- the row to another owner bumps both.
CREATE FUNCTION sprk_revision_owner() RETURNS trigger LANGUAGE plpgsql AS $$
DECLARE
    scope text := TG_ARGV[0];
    new_owner bigint;
    old_owner bigint;
BEGIN
    IF TG_OP <> 'DELETE' THEN
        new_owner := (to_jsonb(NEW) ->> TG_ARGV[1])::bigint;
        PERFORM sprk_revision_bump(scope, new_owner);
    END IF;
    IF TG_OP <> 'INSERT' THEN
        old_owner := (to_jsonb(OLD) ->> TG_ARGV[1])::bigint;
        IF TG_OP = 'DELETE' OR old_owner IS DISTINCT FROM new_owner THEN
            PERFORM sprk_revision_bump(scope, old_owner);
        END IF;
    END IF;
    RETURN NULL;
END $$;

-- Data read across accounts: TG_ARGV = (scope). Bumps the global revision.
CREATE FUNCTION sprk_revision_global() RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
    PERFORM sprk_revision_bump(TG_ARGV[0], 0);
    RETURN NULL;
END $$;

-- battle_scores: world boss placement ranks every account's scores.
CREATE FUNCTION sprk_revision_world_boss() RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
    IF (TG_OP <> 'DELETE' AND NEW.family = 'world_boss')
       OR (TG_OP <> 'INSERT' AND OLD.family = 'world_boss') THEN
        PERFORM sprk_revision_bump('progression', 0);
    END IF;
    RETURN NULL;
END $$;

-- community_state: conquest history names its player inside the JSON.
CREATE FUNCTION sprk_revision_conquest_history() RETURNS trigger LANGUAGE plpgsql AS $$
DECLARE
    new_match boolean := TG_OP <> 'DELETE' AND NEW.kind = 'guild_conquest_history';
    old_match boolean := TG_OP <> 'INSERT' AND OLD.kind = 'guild_conquest_history';
    new_owner bigint;
    old_owner bigint;
BEGIN
    IF new_match THEN
        new_owner := COALESCE(sprk_json_int(NEW.data, '$.AccountId'), 0);
        PERFORM sprk_revision_bump('progression', new_owner);
    END IF;
    IF old_match THEN
        old_owner := COALESCE(sprk_json_int(OLD.data, '$.AccountId'), 0);
        IF NOT (new_match AND old_owner = new_owner) THEN
            PERFORM sprk_revision_bump('progression', old_owner);
        END IF;
    END IF;
    RETURN NULL;
END $$;

-- Install: one INSERT/DELETE trigger and one UPDATE trigger (only when a
-- watched column changed) per table, scope and owner.
DO $install$
DECLARE
    spec record;
    changed text;
    name text;
    args text;
BEGIN
    FOR spec IN
        SELECT * FROM (VALUES
            -- scope, table, function, function args, watched columns (NULL = all)
            ('progression', 'accounts', 'sprk_revision_owner', 'account_id', ARRAY['account_id', 'login_id', 'created_at']),
            ('progression', 'heroes', 'sprk_revision_owner', 'account_id', NULL),
            ('progression', 'hero_details', 'sprk_revision_owner', 'account_id', NULL),
            ('progression', 'equip_items', 'sprk_revision_owner', 'account_id', NULL),
            ('progression', 'extension_state', 'sprk_revision_owner', 'account_id', NULL),
            ('progression', 'campaign_progress', 'sprk_revision_owner', 'account_id', NULL),
            ('progression', 'progression_metrics', 'sprk_revision_owner', 'account_id', NULL),
            ('progression', 'progression_claims', 'sprk_revision_owner', 'account_id', NULL),
            ('progression', 'progression_login', 'sprk_revision_owner', 'account_id', NULL),
            ('progression', 'user_info', 'sprk_revision_owner', 'account_id', ARRAY['account_id', 'team_level']),
            ('progression', 'costumes', 'sprk_revision_owner', 'account_id', NULL),
            ('progression', 'guild_members', 'sprk_revision_owner', 'account_id', NULL),
            ('progression', 'tutorial_progress', 'sprk_revision_owner', 'account_id', NULL),
            ('progression', 'items', 'sprk_revision_owner', 'account_id', ARRAY['account_id', 'item_index', 'count']),
            ('progression', 'progression_entitlements', 'sprk_revision_owner', 'account_id', NULL),
            ('progression', 'progression_main_quest', 'sprk_revision_owner', 'account_id', NULL),
            ('progression', 'progression_world_events', 'sprk_revision_owner', 'account_id', NULL),
            ('progression', 'attendance_calendar_state', 'sprk_revision_owner', 'account_id', NULL),
            ('progression', 'play_record_values', 'sprk_revision_owner', 'account_id', NULL),
            -- Queue heartbeats (arena_runs.last_poll) change no response.
            ('progression', 'arena_runs', 'sprk_revision_owner', 'account', ARRAY['*', '-last_poll']),
            ('progression', 'battle_state', 'sprk_revision_owner', 'account', NULL),
            ('progression', 'guild_battle_scores', 'sprk_revision_owner', 'account', NULL),
            ('progression', 'community_claims', 'sprk_revision_owner', 'account', NULL),
            ('progression', 'guild_arena_records', 'sprk_revision_owner', 'account', NULL),
            ('progression', 'guild_arena_decks', 'sprk_revision_owner', 'account', NULL),
            -- Accepted friends are counted from either side of the row.
            ('progression', 'friends', 'sprk_revision_owner', 'account_id', NULL),
            ('progression', 'friends', 'sprk_revision_owner', 'friend_account_id', NULL),
            ('progression', 'community_state', 'sprk_revision_conquest_history', NULL, NULL),
            ('progression', 'battle_scores', 'sprk_revision_owner', 'account', NULL),
            ('progression', 'battle_scores', 'sprk_revision_world_boss', NULL, NULL),
            -- Linked status joins by login_id; links change rarely.
            ('progression', 'credentials', 'sprk_revision_global', NULL, NULL),
            ('equipment', 'heroes', 'sprk_revision_owner', 'account_id', ARRAY['account_id', 'hero_index', 'level',
                'equip_item_slot_index_1', 'equip_item_slot_index_2', 'equip_item_slot_index_3', 'equip_item_slot_index_4',
                'equip_item_slot_index_5', 'equip_item_slot_index_6', 'equip_item_slot_index_7', 'equip_item_slot_index_8',
                'equip_item_slot_index_9', 'equip_item_slot_index_10']),
            ('equipment', 'equip_items', 'sprk_revision_owner', 'account_id', ARRAY['account_id', 'slot_index', 'item_index', 'level']),
            ('equipment', 'user_info', 'sprk_revision_owner', 'account_id', ARRAY['account_id', 'team_level']),
            ('equipment', 'extension_state', 'sprk_revision_owner', 'account_id', NULL)
        ) AS s(scope, tbl, fn, owner, cols)
    LOOP
        IF spec.cols IS NULL THEN
            changed := 'OLD.* IS DISTINCT FROM NEW.*';
        ELSIF spec.cols[1] = '*' THEN
            changed := format('to_jsonb(OLD) - %L IS DISTINCT FROM to_jsonb(NEW) - %L',
                              ltrim(spec.cols[2], '-'), ltrim(spec.cols[2], '-'));
        ELSE
            SELECT format('(%s) IS DISTINCT FROM (%s)',
                          string_agg(format('OLD.%I', c), ', '), string_agg(format('NEW.%I', c), ', '))
            INTO changed FROM unnest(spec.cols) AS c;
        END IF;
        -- e.g. rev_progression_friends_friend_account_id_u (names max 63 bytes).
        name := format('rev_%s_%s_%s', spec.scope, spec.tbl,
                       COALESCE(spec.owner, replace(spec.fn, 'sprk_revision_', '')));
        args := concat_ws(', ', quote_literal(spec.scope), quote_literal(spec.owner));
        IF octet_length(name) > 61 THEN
            RAISE EXCEPTION 'revision trigger name too long: %', name;
        END IF;
        EXECUTE format('CREATE TRIGGER %I AFTER INSERT OR DELETE ON %I FOR EACH ROW EXECUTE FUNCTION %I(%s)',
            name || '_w', spec.tbl, spec.fn, args);
        EXECUTE format('CREATE TRIGGER %I AFTER UPDATE ON %I FOR EACH ROW WHEN (%s) EXECUTE FUNCTION %I(%s)',
            name || '_u', spec.tbl, changed, spec.fn, args);
    END LOOP;
END
$install$;
