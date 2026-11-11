-- Gameplay triggers ported from SQLite: progression metrics (progression::
-- schema), play records (progression::play_record), mail arrival
-- notifications (database.rs) and arena run upkeep (progression::
-- mission_history). Effects match the SQLite triggers; several SQLite
-- triggers on the same table are merged into one function.

CREATE FUNCTION sprk_metric_add(p_account bigint, p_day text, p_kind text, p_arg bigint, p_arg2 bigint, p_value bigint)
RETURNS void LANGUAGE sql AS $$
    INSERT INTO progression_metrics (account_id, day, kind, arg, arg2, value)
    VALUES (p_account, p_day, p_kind, p_arg, p_arg2, p_value)
    ON CONFLICT (account_id, day, kind, arg, arg2)
    DO UPDATE SET value = progression_metrics.value + EXCLUDED.value
$$;

CREATE FUNCTION sprk_play_record_add(p_account bigint, p_kind text, p_idx bigint, p_value bigint)
RETURNS void LANGUAGE sql AS $$
    INSERT INTO play_record_values (account_id, kind, idx, value)
    VALUES (p_account, p_kind, p_idx, p_value)
    ON CONFLICT (account_id, kind, idx)
    DO UPDATE SET value = play_record_values.value + EXCLUDED.value
$$;

CREATE FUNCTION sprk_play_record_max(p_account bigint, p_kind text, p_idx bigint, p_value bigint)
RETURNS void LANGUAGE sql AS $$
    INSERT INTO play_record_values (account_id, kind, idx, value)
    VALUES (p_account, p_kind, p_idx, p_value)
    ON CONFLICT (account_id, kind, idx)
    DO UPDATE SET value = GREATEST(play_record_values.value, EXCLUDED.value)
$$;

-- user_info: progression_{gain_gold,spend_gold,spend_gem,spend_stamina,stamina_uses}
CREATE FUNCTION sprk_user_info_metrics() RETURNS trigger LANGUAGE plpgsql AS $$
DECLARE day text := sprk_utc_day();
BEGIN
    IF NEW.gold > OLD.gold THEN
        PERFORM sprk_metric_add(NEW.account_id, day, 'GetGold', 0, 0, NEW.gold - OLD.gold);
    ELSIF NEW.gold < OLD.gold THEN
        PERFORM sprk_metric_add(NEW.account_id, day, 'ConsumeGold', 0, 0, OLD.gold - NEW.gold);
    END IF;
    IF NEW.gem + NEW.pay_gem < OLD.gem + OLD.pay_gem THEN
        PERFORM sprk_metric_add(NEW.account_id, day, 'ConsumeGem', 0, 0, OLD.gem + OLD.pay_gem - NEW.gem - NEW.pay_gem);
    END IF;
    IF NEW.stamina < OLD.stamina THEN
        PERFORM sprk_metric_add(NEW.account_id, day, 'ConsumeStamina', 0, 0, OLD.stamina - NEW.stamina);
        PERFORM sprk_metric_add(NEW.account_id, day, 'StaminaUses', 0, 0, 1);
    END IF;
    RETURN NULL;
END $$;
CREATE TRIGGER user_info_metrics AFTER UPDATE ON user_info FOR EACH ROW
    WHEN (NEW.gold <> OLD.gold OR NEW.gem + NEW.pay_gem < OLD.gem + OLD.pay_gem OR NEW.stamina < OLD.stamina)
    EXECUTE FUNCTION sprk_user_info_metrics();

-- campaign_progress: progression_clear_{insert,update}
CREATE FUNCTION sprk_campaign_metrics() RETURNS trigger LANGUAGE plpgsql AS $$
DECLARE cleared bigint := NEW.clear_count - CASE WHEN TG_OP = 'UPDATE' THEN OLD.clear_count ELSE 0 END;
BEGIN
    IF cleared > 0 THEN
        PERFORM sprk_metric_add(NEW.account_id, sprk_utc_day(), 'ClearDungeon', NEW.chapter_id, NEW.dungeon_id, cleared);
    END IF;
    RETURN NULL;
END $$;
CREATE TRIGGER campaign_progress_metrics AFTER INSERT OR UPDATE OF clear_count ON campaign_progress
    FOR EACH ROW EXECUTE FUNCTION sprk_campaign_metrics();

-- equip_items: play_record_equip_{count,star_new,star}, progression_equip_gain
CREATE FUNCTION sprk_equip_records() RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
    IF TG_OP = 'INSERT' THEN
        PERFORM sprk_play_record_add(NEW.account_id, 'equip_count', NEW.item_index, 1);
        PERFORM sprk_play_record_max(NEW.account_id, 'equip_star', NEW.item_index, NEW.star);
        PERFORM sprk_metric_add(NEW.account_id, sprk_utc_day(), 'AddEquip', NEW.item_index, 0, 1);
    ELSIF NEW.star > OLD.star THEN
        PERFORM sprk_play_record_max(NEW.account_id, 'equip_star', NEW.item_index, NEW.star);
    END IF;
    RETURN NULL;
END $$;
CREATE TRIGGER equip_items_records AFTER INSERT OR UPDATE OF star ON equip_items
    FOR EACH ROW EXECUTE FUNCTION sprk_equip_records();

-- extension_state: play_record_soul_{grade,reinforce}[_new]
CREATE FUNCTION sprk_soul_records() RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
    PERFORM sprk_play_record_max(NEW.account_id, 'soul_grade', 0, COALESCE(sprk_json_int(NEW.data -> 'Grade'), 0));
    PERFORM sprk_play_record_max(NEW.account_id, 'soul_reinforce', 0, COALESCE(sprk_json_int(NEW.data -> 'ReinforceLevel'), 0));
    RETURN NULL;
END $$;
CREATE TRIGGER extension_state_soul_records AFTER INSERT OR UPDATE ON extension_state
    FOR EACH ROW WHEN (NEW.kind = 'soul') EXECUTE FUNCTION sprk_soul_records();

-- arena_scores: play_record_arena_{score,streak,wins,losses}
CREATE FUNCTION sprk_arena_score_records() RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
    IF NEW.wins > OLD.wins THEN
        PERFORM sprk_play_record_max(NEW.account, 'arena_score', 0, NEW.score);
        PERFORM sprk_play_record_max(NEW.account, 'arena_streak', 0, COALESCE(sprk_json_int(NEW.data -> 'SuccessiveWin'), 0));
        PERFORM sprk_play_record_add(NEW.account, 'arena_wins', 0, NEW.wins - OLD.wins);
    END IF;
    IF NEW.losses > OLD.losses THEN
        PERFORM sprk_play_record_add(NEW.account, 'arena_losses', 0, NEW.losses - OLD.losses);
    END IF;
    RETURN NULL;
END $$;
CREATE TRIGGER arena_scores_records AFTER UPDATE ON arena_scores FOR EACH ROW
    WHEN (NEW.kind = 0 AND (NEW.wins > OLD.wins OR NEW.losses > OLD.losses))
    EXECUTE FUNCTION sprk_arena_score_records();

-- battle_scores: progress_world_boss_complete_{insert,update}. Uses the
-- score's own day key, not today.
CREATE FUNCTION sprk_world_boss_metrics() RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
    IF NEW.score > (CASE WHEN TG_OP = 'UPDATE' THEN OLD.score ELSE 0 END) THEN
        PERFORM sprk_metric_add(NEW.account, NEW.day, 'CompleteWorldBoss', NEW.boss, 0, 1);
    END IF;
    RETURN NULL;
END $$;
CREATE TRIGGER battle_scores_world_boss AFTER INSERT OR UPDATE ON battle_scores
    FOR EACH ROW WHEN (NEW.family = 'world_boss') EXECUTE FUNCTION sprk_world_boss_metrics();

-- friend_points: progression_friend_send
CREATE FUNCTION sprk_friend_send_metrics() RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
    PERFORM sprk_metric_add(NEW.account_id, sprk_utc_day(), 'SendFriendshipPoint', 0, 0, 1);
    RETURN NULL;
END $$;
CREATE TRIGGER friend_points_send AFTER UPDATE ON friend_points FOR EACH ROW
    WHEN (NEW.last_send_time IS NOT NULL AND NEW.last_send_time IS DISTINCT FROM OLD.last_send_time)
    EXECUTE FUNCTION sprk_friend_send_metrics();

-- friends: progression_friend_request
CREATE FUNCTION sprk_friend_request_metrics() RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
    PERFORM sprk_metric_add(NEW.account_id, sprk_utc_day(), 'FriendRequest', 0, 0, 1);
    RETURN NULL;
END $$;
CREATE TRIGGER friends_request AFTER INSERT ON friends FOR EACH ROW
    WHEN (NEW.status = 'pending') EXECUTE FUNCTION sprk_friend_request_metrics();

-- hero_friendly_info: progression_inn_{greet,conversation,gift}. Each changed
-- action counts once, as three separate SQLite triggers did.
CREATE FUNCTION sprk_inn_metrics() RETURNS trigger LANGUAGE plpgsql AS $$
DECLARE actions bigint := 0;
BEGIN
    IF NEW.last_greeting_time IS NOT NULL AND NEW.last_greeting_time IS DISTINCT FROM OLD.last_greeting_time THEN actions := actions + 1; END IF;
    IF NEW.last_conversation_time IS NOT NULL AND NEW.last_conversation_time IS DISTINCT FROM OLD.last_conversation_time THEN actions := actions + 1; END IF;
    IF NEW.last_gift_time IS NOT NULL AND NEW.last_gift_time IS DISTINCT FROM OLD.last_gift_time THEN actions := actions + 1; END IF;
    IF actions > 0 THEN
        PERFORM sprk_metric_add(NEW.account_id, sprk_utc_day(), 'DoHeroFriendly', NEW.hero_index, 0, actions);
    END IF;
    RETURN NULL;
END $$;
CREATE TRIGGER hero_friendly_info_metrics AFTER UPDATE ON hero_friendly_info
    FOR EACH ROW EXECUTE FUNCTION sprk_inn_metrics();

-- hero_friendly_state: progression_hero_friendly
CREATE FUNCTION sprk_hero_friendly_metrics() RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
    PERFORM sprk_metric_add(NEW.account_id, sprk_utc_day(), 'DoHeroFriendly', NEW.hero_index, 0, 1);
    RETURN NULL;
END $$;
CREATE TRIGGER hero_friendly_state_metrics AFTER UPDATE ON hero_friendly_state FOR EACH ROW
    WHEN (NEW.friendly_point > OLD.friendly_point) EXECUTE FUNCTION sprk_hero_friendly_metrics();

-- hero_inn_roulette_spins: progression_roulette_{first,more}
CREATE FUNCTION sprk_roulette_metrics() RETURNS trigger LANGUAGE plpgsql AS $$
DECLARE spins bigint := NEW.spin_count - CASE WHEN TG_OP = 'UPDATE' THEN OLD.spin_count ELSE 0 END;
BEGIN
    IF spins > 0 THEN
        PERFORM sprk_metric_add(NEW.account_id, sprk_utc_day(), 'RotateRulletDaily', 0, 0, spins);
    END IF;
    RETURN NULL;
END $$;
CREATE TRIGGER hero_inn_roulette_metrics AFTER INSERT OR UPDATE ON hero_inn_roulette_spins
    FOR EACH ROW EXECUTE FUNCTION sprk_roulette_metrics();

-- items: progression_item_{new,gain}
CREATE FUNCTION sprk_item_metrics() RETURNS trigger LANGUAGE plpgsql AS $$
DECLARE gained bigint := NEW.count - CASE WHEN TG_OP = 'UPDATE' THEN OLD.count ELSE 0 END;
BEGIN
    IF gained > 0 THEN
        PERFORM sprk_metric_add(NEW.account_id, sprk_utc_day(), 'AddItem', NEW.item_index, 0, gained);
    END IF;
    RETURN NULL;
END $$;
CREATE TRIGGER items_metrics AFTER INSERT OR UPDATE ON items
    FOR EACH ROW EXECUTE FUNCTION sprk_item_metrics();

-- mails: mail_arrival_notification, progression_mail_received. The mail
-- notifier polls mail_notifications for committed rows.
CREATE FUNCTION sprk_mail_arrival() RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
    INSERT INTO mail_notifications (mail_id, account_id, is_global)
    VALUES (NEW.mail_id, NEW.account_id, CASE WHEN NEW.global_id IS NOT NULL THEN 1 ELSE 0 END);
    RETURN NULL;
END $$;
CREATE TRIGGER mail_arrival_notification AFTER INSERT ON mails
    FOR EACH ROW EXECUTE FUNCTION sprk_mail_arrival();

CREATE FUNCTION sprk_mail_received_metrics() RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
    PERFORM sprk_metric_add(NEW.account_id, sprk_utc_day(), 'ReceiveMail', 0, 0, 1);
    RETURN NULL;
END $$;
CREATE TRIGGER mails_received_metrics AFTER UPDATE ON mails FOR EACH ROW
    WHEN (NEW.is_received = 1 AND OLD.is_received = 0) EXECUTE FUNCTION sprk_mail_received_metrics();

-- shop_purchase_ledger: progression_shop_{new,more}
CREATE FUNCTION sprk_shop_purchase_metrics() RETURNS trigger LANGUAGE plpgsql AS $$
DECLARE bought bigint := NEW.purchased - CASE WHEN TG_OP = 'UPDATE' THEN OLD.purchased ELSE 0 END;
BEGIN
    IF bought > 0 THEN
        PERFORM sprk_metric_add(NEW.account_id, sprk_utc_day(), 'BuyShopItem', NEW.shop_index, NEW.item_index, bought);
    END IF;
    RETURN NULL;
END $$;
CREATE TRIGGER shop_purchase_metrics AFTER INSERT OR UPDATE ON shop_purchase_ledger
    FOR EACH ROW EXECUTE FUNCTION sprk_shop_purchase_metrics();

-- shop_stock: progression_shop_restock
CREATE FUNCTION sprk_shop_restock_metrics() RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
    PERFORM sprk_metric_add(NEW.account_id, sprk_utc_day(), 'ResetShop', NEW.shop_index, 0, 1);
    RETURN NULL;
END $$;
CREATE TRIGGER shop_stock_restock AFTER UPDATE ON shop_stock FOR EACH ROW
    WHEN (NEW.restock_count > OLD.restock_count) EXECUTE FUNCTION sprk_shop_restock_metrics();

-- arena_runs: arena_retention_keys_*, arena_mission_summary_*. SQLite used
-- AFTER triggers that updated the row again; a BEFORE trigger sets the same
-- values on the row being written.
CREATE FUNCTION sprk_arena_runs_derive() RETURNS trigger LANGUAGE plpgsql AS $$
DECLARE duel bigint := sprk_json_int(NEW.data -> 'DuelType');
BEGIN
    NEW.season := sprk_json_int(NEW.data -> 'Season');
    NEW.nonduel := CASE WHEN duel IS NOT DISTINCT FROM 1 THEN 0 ELSE 1 END;
    NEW.squad := CASE WHEN NEW.nonduel = 1
        AND COALESCE(sprk_json_array_length(NEW.data -> 'Heroes') BETWEEN 1 AND 4, false) THEN 1 ELSE 0 END;
    IF NEW.status = 'complete' THEN
        NEW.mission_summary := jsonb_build_object(
            'DuelType', COALESCE(NULLIF(NEW.data -> 'DuelType', 'null'::jsonb), '0'::jsonb),
            'CompletedAt', NEW.data -> 'CompletedAt',
            'End', jsonb_build_object(
                'MatchResult', NEW.data #> '{End,MatchResult}',
                'BattleInfo', jsonb_build_object('SuccessiveWin', NEW.data #> '{End,BattleInfo,SuccessiveWin}'),
                'BattleBanPickInfo', jsonb_build_object('SuccessiveWin', NEW.data #> '{End,BattleBanPickInfo,SuccessiveWin}')));
    END IF;
    RETURN NEW;
END $$;
CREATE TRIGGER arena_runs_derive BEFORE INSERT OR UPDATE OF data, status ON arena_runs
    FOR EACH ROW EXECUTE FUNCTION sprk_arena_runs_derive();

-- arena_retention_{insert,finish}: keep each account's 20 most recent
-- finished runs per kind plus the latest completed run per season, squad
-- run per season and defense/duel run; fold expired completed matches into
-- arena_history_archive, then delete them. squad/nonduel are 0/1 and
-- compared with <> 0 to keep SQLite's truthiness, NULLs included.
CREATE FUNCTION sprk_arena_retention(p_account bigint) RETURNS void LANGUAGE plpgsql AS $$
DECLARE expired bigint[];
BEGIN
    SELECT array_agg(id) INTO expired FROM (
        SELECT id, status, squad, nonduel,
            row_number() OVER (PARTITION BY kind ORDER BY id DESC) AS recent,
            row_number() OVER (PARTITION BY kind, season, status = 'complete' ORDER BY id DESC) AS season_last,
            row_number() OVER (PARTITION BY kind, season, (status = 'complete' AND squad <> 0) ORDER BY id DESC) AS squad_last,
            row_number() OVER (PARTITION BY kind, nonduel ORDER BY id DESC) AS defense_last
        FROM arena_runs
        WHERE account = p_account AND status NOT IN ('waiting', 'battle')
    ) ranked
    WHERE recent > 20
      AND NOT (status = 'complete' AND season_last = 1)
      AND NOT (status = 'complete' AND squad <> 0 AND squad_last = 1)
      AND NOT (nonduel <> 0 AND defense_last = 1);
    IF expired IS NULL THEN RETURN; END IF;

    INSERT INTO arena_history_archive (account, kind, day, matches, wins, best_streak, best_tier)
    SELECT account, kind,
        to_char(to_timestamp(GREATEST(COALESCE(sprk_json_int(mission_summary -> 'CompletedAt'), 0), started)) AT TIME ZONE 'UTC', 'YYYY-MM-DD') AS day,
        count(*),
        count(*) FILTER (WHERE COALESCE(sprk_json_int(mission_summary #> '{End,MatchResult,Win}'), 0) = 1),
        max(COALESCE(sprk_json_int(CASE kind WHEN 1 THEN mission_summary #> '{End,BattleBanPickInfo,SuccessiveWin}'
                                             ELSE mission_summary #> '{End,BattleInfo,SuccessiveWin}' END), 0)),
        max(COALESCE(sprk_json_int(mission_summary #> '{End,MatchResult,NewTierIndex}'), 0))
    FROM arena_runs
    WHERE id = ANY (expired) AND status = 'complete'
      AND COALESCE(sprk_json_int(mission_summary -> 'DuelType'), 0) <> 1
      AND jsonb_typeof(mission_summary #> '{End,MatchResult}') = 'object'
      AND COALESCE(sprk_json_int(mission_summary #> '{End,MatchResult,Canceled}'), 0) <> 1
    GROUP BY account, kind, 3
    ON CONFLICT (account, kind, day) DO UPDATE SET
        matches = arena_history_archive.matches + EXCLUDED.matches,
        wins = arena_history_archive.wins + EXCLUDED.wins,
        best_streak = GREATEST(arena_history_archive.best_streak, EXCLUDED.best_streak),
        best_tier = GREATEST(arena_history_archive.best_tier, EXCLUDED.best_tier);

    DELETE FROM arena_runs WHERE id = ANY (expired);
END $$;

CREATE FUNCTION sprk_arena_runs_retention() RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
    PERFORM sprk_arena_retention(NEW.account);
    RETURN NULL;
END $$;
CREATE TRIGGER arena_retention_insert AFTER INSERT ON arena_runs FOR EACH ROW
    WHEN (NEW.status NOT IN ('waiting', 'battle')) EXECUTE FUNCTION sprk_arena_runs_retention();
CREATE TRIGGER arena_retention_finish AFTER UPDATE OF status ON arena_runs FOR EACH ROW
    WHEN (OLD.status IN ('waiting', 'battle') AND NEW.status NOT IN ('waiting', 'battle'))
    EXECUTE FUNCTION sprk_arena_runs_retention();

-- arena_snapshots_cleanup
CREATE FUNCTION sprk_arena_snapshots_cleanup() RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
    DELETE FROM arena_snapshots WHERE run = OLD.id;
    RETURN NULL;
END $$;
CREATE TRIGGER arena_snapshots_cleanup AFTER DELETE ON arena_runs
    FOR EACH ROW EXECUTE FUNCTION sprk_arena_snapshots_cleanup();
