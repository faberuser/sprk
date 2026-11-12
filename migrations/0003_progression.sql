-- progression::{schema, play_record, mission_history} and extensions::schema.

CREATE TABLE mission_category_seen (
    account_id bigint NOT NULL,
    main_category bigint NOT NULL,
    sub_category bigint NOT NULL,
    PRIMARY KEY (account_id, main_category, sub_category)
);

CREATE TABLE progression_claims (
    account_id bigint NOT NULL,
    family text NOT NULL,
    idx bigint NOT NULL,
    step bigint NOT NULL DEFAULT 1,
    period text NOT NULL DEFAULT 'all',
    claimed_time text NOT NULL DEFAULT sprk_now(),
    PRIMARY KEY (account_id, family, idx, step, period)
);

-- Daily counters written by the triggers in 0008; very high churn.
CREATE TABLE progression_metrics (
    account_id bigint NOT NULL,
    day text NOT NULL,
    kind text NOT NULL,
    arg bigint NOT NULL DEFAULT 0,
    arg2 bigint NOT NULL DEFAULT 0,
    value bigint NOT NULL DEFAULT 0,
    PRIMARY KEY (account_id, day, kind, arg, arg2)
) WITH (fillfactor = 85);

CREATE TABLE progression_login (
    account_id bigint PRIMARY KEY,
    days bigint NOT NULL DEFAULT 0,
    last_day text NOT NULL DEFAULT ''
);

CREATE TABLE attendance_calendar_state (
    account_id bigint NOT NULL,
    idx bigint NOT NULL,
    claims bigint NOT NULL DEFAULT 0,
    last_day text NOT NULL DEFAULT '',
    completed_time text,
    PRIMARY KEY (account_id, idx)
);

CREATE TABLE progression_entitlements (
    account_id bigint NOT NULL,
    product_index bigint NOT NULL,
    created_time text NOT NULL DEFAULT sprk_now(),
    PRIMARY KEY (account_id, product_index)
);

CREATE TABLE progression_world_events (
    account_id bigint NOT NULL,
    chapter bigint NOT NULL,
    dungeon bigint NOT NULL,
    event bigint NOT NULL,
    data text NOT NULL,
    PRIMARY KEY (account_id, chapter, dungeon, event)
);

CREATE TABLE progression_main_quest (
    account_id bigint PRIMARY KEY,
    step bigint NOT NULL DEFAULT 0,
    progress bigint NOT NULL DEFAULT 0
);

CREATE TABLE play_record_values (
    account_id bigint NOT NULL,
    kind text NOT NULL,
    idx bigint NOT NULL DEFAULT 0,
    value bigint NOT NULL DEFAULT 0,
    PRIMARY KEY (account_id, kind, idx)
) WITH (fillfactor = 85);

CREATE TABLE arena_history_archive (
    account bigint NOT NULL,
    kind bigint NOT NULL,
    day text NOT NULL,
    matches bigint NOT NULL,
    wins bigint NOT NULL,
    best_streak bigint NOT NULL,
    best_tier bigint NOT NULL,
    PRIMARY KEY (account, kind, day)
);

-- extensions::schema
CREATE TABLE equipment_pending (
    account_id bigint NOT NULL,
    slot_index bigint NOT NULL REFERENCES equip_items(slot_index) ON DELETE CASCADE,
    kind text NOT NULL,
    data text NOT NULL,
    PRIMARY KEY (account_id, slot_index, kind)
);

CREATE TABLE extension_state (
    account_id bigint NOT NULL,
    kind text NOT NULL,
    idx bigint NOT NULL,
    data text NOT NULL,
    PRIMARY KEY (account_id, kind, idx)
) WITH (fillfactor = 85);
