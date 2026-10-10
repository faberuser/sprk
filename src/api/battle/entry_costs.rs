//! PvE costs are held logically and deducted only when a victory is committed.
use super::*;

pub(crate) async fn enabled(db: &mut SqliteConnection, a: i64) -> Result<bool> {
    Ok(get(db, a, "entry_policy", 0).await?["RefundPveDefeats"] == true)
}

fn resource(family: &str, kind: i64) -> (&str, i64) {
    if family == "Stamina" && matches!(kind, 3 | 4 | 7) {
        ("Currency", kind)
    } else {
        (family, kind)
    }
}
pub(crate) async fn held(
    db: &mut SqliteConnection,
    a: i64,
    family: &str,
    kind: i64,
) -> Result<i64> {
    let (family, kind) = resource(family, kind);
    Ok(sqlx::query_scalar("SELECT COALESCE(SUM(amount),0) FROM battle_entry_holds WHERE account=? AND family=? AND kind=? AND expires>?")
        .bind(a).bind(family).bind(kind).bind(now()).fetch_one(db).await?)
}
async fn owned_hold(
    db: &mut SqliteConnection,
    a: i64,
    owner: &str,
    family: &str,
    kind: i64,
) -> Result<i64> {
    let (family, kind) = resource(family, kind);
    Ok(sqlx::query_scalar("SELECT COALESCE(SUM(amount),0) FROM battle_entry_holds WHERE account=? AND owner=? AND family=? AND kind=? AND expires>?")
        .bind(a).bind(owner).bind(family).bind(kind).bind(now()).fetch_one(db).await?)
}
fn currency(family: &str, kind: i64) -> Result<&'static str> {
    if family == "ShakmehGauge" {
        return Ok("ShakmehMiddleBossPoint");
    }
    match kind {
        3 => Ok("Gold"),
        4 => Ok("Gem"),
        7 => Ok("Mileage"),
        _ => Err(rule("InvalidCost")),
    }
}
async fn peek(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    family: &str,
    kind: i64,
) -> Result<Value> {
    let (family, kind) = resource(family, kind);
    if family == "Stamina" {
        crate::api::account::stamina::snapshot(db, s, a, kind).await
    } else {
        hero::currency(db, a, currency(family, kind)?, 0).await
    }
}
struct Reservation<'a> {
    owner: &'a str,
    family: &'a str,
    kind: i64,
    total: i64,
    expires: i64,
}

async fn reserve(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    reservation: Reservation<'_>,
) -> Result<Value> {
    let Reservation {
        owner,
        family,
        kind,
        total,
        expires,
    } = reservation;
    let value = peek(db, s, a, family, kind).await?;
    let (family, kind) = resource(family, kind);
    let available = n(&value, "NewValue");
    let others = held(db, a, family, kind).await? - owned_hold(db, a, owner, family, kind).await?;
    if total <= 0 || available - others < total {
        return Err(rule(if family == "Stamina" {
            "NotEnoughDungeonKey"
        } else {
            "InvalidCost"
        }));
    }
    sqlx::query("INSERT INTO battle_entry_holds(account,owner,family,kind,amount,expires) VALUES(?,?,?,?,?,?) ON CONFLICT(account,owner,family,kind) DO UPDATE SET amount=excluded.amount,expires=excluded.expires")
        .bind(a).bind(owner).bind(family).bind(kind).bind(total).bind(expires).execute(db).await?;
    Ok(value)
}
pub(super) fn deferred_receipt() -> Value {
    json!({"VictoryEntryCosts":[],"DeferredEntryCosts":true,"EntryCostReservation":uuid::Uuid::new_v4().to_string()})
}
pub(super) async fn clear_hold(db: &mut SqliteConnection, a: i64, owner: &str) -> Result<()> {
    sqlx::query("DELETE FROM battle_entry_holds WHERE account=? AND owner=?")
        .bind(a)
        .bind(owner)
        .execute(db)
        .await?;
    Ok(())
}
pub(super) async fn prepare(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    entry: &mut Value,
    out: &mut Value,
) -> Result<()> {
    let dungeon = campaign::dungeon(s, r)?;
    // God King charges for opening a dungeon, not for winning a stage.
    // PvP, damage/score competitions, World Boss, Eclipse and Karma keep costs.
    if matches!(
        n(dungeon, "BattleType"),
        4 | 15 | 16 | 19 | 20 | 25 | 26 | 30 | 32 | 35 | 36 | 43 | 44 | 48
    ) || int(r, "WorldBossIndex")? > 0
        || int(r, "EventWorldBossIndex")? > 0
        || !enabled(db, a).await?
    {
        return Ok(());
    }
    merge(entry, deferred_receipt());
    let gate = if n(dungeon, "BattleType") == 47 {
        let raid = s.tables.battle.find(
            "PunishmentRaid",
            &[
                ("RaidIndex", int(r, "RaidIndex")?),
                ("RaidLevel", int(r, "RaidLevel")?),
            ],
        );
        raid.map(|v| ("punishment_open", n(v, "GroupIndex")))
    } else {
        None
    };
    if let Some((state, idx)) = gate {
        let mut gate = get(db, a, state, idx).await?;
        if gate["VictoryRefund"] == true && gate["CostSettled"] != true {
            let kind = n(&gate, "EntryCostType");
            let amount = n(&gate, "EntryCost");
            // Convert the previous physical opening reservation without losing it.
            if gate["DeferredOpening"] != true && gate["EntryPaid"] == true {
                out["StaminaResult"] =
                    crate::api::account::stamina::refund(db, s, a, kind, amount).await?;
            }
            let owner = gate["EntryCostReservation"]
                .as_str()
                .map(str::to_owned)
                .unwrap_or_else(|| uuid::Uuid::new_v4().to_string());
            reserve(
                db,
                s,
                a,
                Reservation {
                    owner: &owner,
                    family: "Stamina",
                    kind,
                    total: amount,
                    expires: now() + settings(s, "BattleExpirySeconds", 14400),
                },
            )
            .await?;
            gate["DeferredOpening"] = json!(true);
            gate["EntryPaid"] = json!(false);
            gate["EntryCostReservation"] = json!(owner);
            note(entry, "Stamina", kind, amount);
            entry["VictoryEntryCosts"]
                .as_array_mut()
                .unwrap()
                .last_mut()
                .unwrap()["ReservationOwner"] = json!(owner);
            entry["EntryCostGate"] = json!({"State":state,"Index":idx});
            put(db, a, state, idx, &gate).await?;
        }
    }
    Ok(())
}

pub(super) fn note(entry: &mut Value, family: &str, kind: i64, amount: i64) {
    if amount > 0 {
        if let Some(costs) = entry["VictoryEntryCosts"].as_array_mut() {
            costs.push(json!({"Family":family,"Kind":kind,"Amount":amount}));
        }
    }
}

pub(super) async fn charge(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    entry: &mut Value,
    kind: i64,
    amount: i64,
) -> Result<Value> {
    if entry["DeferredEntryCosts"] == true && amount > 0 {
        let owner = entry["EntryCostReservation"]
            .as_str()
            .ok_or_else(|| rule("InvalidCost"))?
            .to_owned();
        let total = owned_hold(db, a, &owner, "Stamina", kind).await? + amount;
        let result = reserve(
            db,
            s,
            a,
            Reservation {
                owner: &owner,
                family: "Stamina",
                kind,
                total,
                expires: now() + settings(s, "BattleExpirySeconds", 14400),
            },
        )
        .await?;
        note(entry, "Stamina", kind, amount);
        Ok(result)
    } else if entry["VictoryEntryCosts"].is_array() {
        let result = dungeons::charge_reserved(db, s, a, kind, amount).await?;
        note(entry, "Stamina", kind, amount);
        Ok(result)
    } else {
        dungeons::charge(db, s, a, kind, amount).await
    }
}

pub(super) async fn charge_currency(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    entry: &mut Value,
    family: &str,
    kind: i64,
    amount: i64,
) -> Result<Value> {
    if amount == 0 {
        return Ok(Value::Null);
    }
    let result = if entry["DeferredEntryCosts"] == true {
        let owner = entry["EntryCostReservation"]
            .as_str()
            .ok_or_else(|| rule("InvalidCost"))?
            .to_owned();
        let total = owned_hold(db, a, &owner, family, kind).await? + amount;
        reserve(
            db,
            s,
            a,
            Reservation {
                owner: &owner,
                family,
                kind,
                total,
                expires: now() + settings(s, "BattleExpirySeconds", 14400),
            },
        )
        .await?
    } else {
        hero::currency(db, a, currency(family, kind)?, -amount).await?
    };
    note(entry, family, kind, amount);
    Ok(result)
}

pub(super) async fn mark_gate(
    db: &mut SqliteConnection,
    a: i64,
    gate: &mut Value,
    kind: i64,
    amount: i64,
) -> Result<()> {
    if amount > 0 && enabled(db, a).await? {
        gate["VictoryRefund"] = json!(true);
        gate["EntryPaid"] = json!(false);
        gate["DeferredOpening"] = json!(true);
        let owner = uuid::Uuid::new_v4().to_string();
        sqlx::query("INSERT INTO battle_entry_holds(account,owner,family,kind,amount,expires) VALUES(?,?,'Stamina',?,?,?)")
            .bind(a).bind(&owner).bind(kind).bind(amount).bind((now()/86400+1)*86400).execute(&mut *db).await?;
        gate["EntryCostReservation"] = json!(owner);
        gate["EntryCostType"] = json!(kind);
        gate["EntryCost"] = json!(amount);
        gate["CostSettled"] = json!(false);
    }
    Ok(())
}

pub(super) async fn opening_charge(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    kind: i64,
    amount: i64,
) -> Result<Value> {
    if enabled(db, a).await? {
        let result = peek(db, s, a, "Stamina", kind).await?;
        if n(&result, "NewValue") - held(db, a, "Stamina", kind).await? < amount {
            return Err(rule("NotEnoughStamina"));
        }
        Ok(result)
    } else {
        dungeons::charge(db, s, a, kind, amount).await
    }
}

fn stamina_result(out: &mut Value, result: Value) {
    if out["StaminaResult"].is_null() || out["StaminaResult"]["Type"] == result["Type"] {
        let add = n(&out["StaminaResult"], "AddValue") + n(&result, "AddValue");
        out["StaminaResult"] = result;
        out["StaminaResult"]["AddValue"] = json!(add);
    } else {
        out["StaminaResult2"] = result;
    }
}

pub(crate) async fn settle(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    entry: &mut Value,
    won: bool,
    out: &mut Value,
) -> Result<()> {
    let Some(costs) = entry["VictoryEntryCosts"].as_array().cloned() else {
        return Ok(());
    };
    if entry["EntryCostsSettled"] == true {
        return Ok(());
    }
    let deferred = entry["DeferredEntryCosts"] == true;
    if deferred {
        let mut owners = BTreeSet::new();
        if let Some(owner) = entry["EntryCostReservation"].as_str() {
            owners.insert(owner.to_owned());
        }
        for cost in &costs {
            if let Some(owner) = cost["ReservationOwner"].as_str() {
                owners.insert(owner.to_owned());
            }
        }
        for owner in owners {
            clear_hold(db, a, &owner).await?;
        }
    }
    let mut exp_cost = 0;
    for cost in costs {
        let kind = n(&cost, "Kind");
        let amount = n(&cost, "Amount");
        // Old saved God King receipts must not refund their opening ticket.
        if cost["Family"] == "Stamina" && kind == 21 {
            continue;
        }
        if deferred {
            let family = cost["Family"].as_str().unwrap_or("Stamina");
            let result = if won {
                if family == "Stamina" && !matches!(kind, 3 | 4 | 7) {
                    dungeons::charge_reserved(db, s, a, kind, amount).await?
                } else {
                    hero::currency(db, a, currency(family, kind)?, -amount).await?
                }
            } else {
                peek(db, s, a, family, kind).await?
            };
            if family == "Stamina" && !matches!(kind, 3 | 4 | 7) {
                stamina_result(out, result);
            } else {
                let mut values = out["CurrencyResults2"]
                    .as_array()
                    .cloned()
                    .unwrap_or_default();
                values.push(result.clone());
                out["CurrencyResults2"] = json!(values);
                let mut values = out["ReservedCurrencyResults"]
                    .as_array()
                    .cloned()
                    .unwrap_or_default();
                values.push(result);
                out["ReservedCurrencyResults"] = json!(values);
            }
        }
        if won {
            if cost["Family"] == "Stamina" && kind == 1 {
                exp_cost += amount;
            }
        } else if !deferred && cost["Family"] == "Stamina" && !matches!(kind, 3 | 4 | 7) {
            let result = crate::api::account::stamina::refund(db, s, a, kind, amount).await?;
            stamina_result(out, result);
        } else if !deferred {
            let currency = if cost["Family"] == "ShakmehGauge" {
                "ShakmehMiddleBossPoint"
            } else {
                match kind {
                    3 => "Gold",
                    4 => "Gem",
                    7 => "Mileage",
                    _ => return Err(rule("InvalidCost")),
                }
            };
            let result = hero::currency(db, a, currency, amount).await?;
            let mut results = out["CurrencyResults2"]
                .as_array()
                .cloned()
                .unwrap_or_default();
            results.push(result.clone());
            out["CurrencyResults2"] = json!(results);
            let mut reserved = out["ReservedCurrencyResults"]
                .as_array()
                .cloned()
                .unwrap_or_default();
            reserved.push(result);
            out["ReservedCurrencyResults"] = json!(reserved);
        }
    }
    if won && entry["ShakmehFinal"] == true {
        put(db, a, "shakmeh_passive", 0, &json!([])).await?;
        let snapshot = hero::currency(db, a, "ShakmehMiddleBossPoint", 0).await?;
        for key in [
            "CurrencyResults",
            "CurrencyResults2",
            "ReservedCurrencyResults",
        ] {
            if let Some(values) = out[key].as_array_mut() {
                for value in values {
                    if value["CurrencyType"] == "ShakmehMiddleBossPoint" {
                        value["NewValue"] = snapshot["NewValue"].clone();
                    }
                }
            }
        }
    }
    if exp_cost > 0 {
        out["ExpResult2"] = dungeons::stamina_exp(db, s, a, exp_cost).await?;
    }
    if let Some(state) = entry["EntryCostGate"]["State"].as_str() {
        if state != "godking" {
            let idx = n(&entry["EntryCostGate"], "Index");
            let mut gate = get(db, a, state, idx).await?;
            gate["EntryPaid"] = json!(won);
            gate["CostSettled"] = json!(won);
            put(db, a, state, idx, &gate).await?;
        }
    }
    entry["EntryCostsSettled"] = json!(true);
    Ok(())
}

pub(super) async fn release_gate(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    state: &str,
    idx: i64,
    gate: &mut Value,
) -> Result<()> {
    if state != "godking" && gate["DeferredOpening"] == true && gate["CostSettled"] != true {
        if let Some(owner) = gate["EntryCostReservation"].as_str() {
            clear_hold(db, a, owner).await?;
        }
    } else if state != "godking"
        && gate["VictoryRefund"] == true
        && gate["EntryPaid"] == true
        && gate["CostSettled"] != true
    {
        crate::api::account::stamina::refund(
            db,
            s,
            a,
            n(gate, "EntryCostType"),
            n(gate, "EntryCost"),
        )
        .await?;
        gate["EntryPaid"] = json!(false);
        put(db, a, state, idx, gate).await?;
    }
    Ok(())
}

// Adopt an open dungeon saved by the temporary refund policy. Pay an unpaid
// opening once, preserving its open state and all clear records.
pub(super) async fn restore_godking_gate(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    idx: i64,
) -> Result<()> {
    let mut gate = get(db, a, "godking", idx).await?;
    if gate["VictoryRefund"] != true {
        return Ok(());
    }
    if n(&gate, "IsOpen") == 1
        && gate["EntryPaid"] != true
        && gate["CostSettled"] != true
    {
        dungeons::charge(db, s, a, 21, 1).await?;
    }
    if let Some(fields) = gate.as_object_mut() {
        for key in [
            "VictoryRefund",
            "EntryPaid",
            "CostSettled",
            "EntryCostType",
            "EntryCost",
        ] {
            fields.remove(key);
        }
    }
    put(db, a, "godking", idx, &gate).await?;
    Ok(())
}

pub(super) async fn abandon_on_login(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
) -> Result<()> {
    if let Some(row) =
        sqlx::query("SELECT entry,started FROM battle_runs WHERE account=? AND completed=0")
            .bind(a)
            .fetch_optional(&mut *db)
            .await?
    {
        let mut entry: Value = read_json(row.get::<&str, _>("entry"))?;
        let expired =
            now() - row.get::<i64, _>("started") > settings(s, "BattleExpirySeconds", 14400);
        if (entry["DeferredEntryCosts"] == true || super::karma::is_entry(s, &entry))
            && (expired || (entry["ServiceOwned"] != true && entry["ServiceRequired"] != true))
        {
            settle(db, s, a, &mut entry, false, &mut json!({})).await?;
            entry["AbandonedOnLogin"] = json!(true);
            sqlx::query("UPDATE battle_runs SET completed=1,entry=? WHERE account=?")
                .bind(entry.to_string())
                .bind(a)
                .execute(&mut *db)
                .await?;
        }
    }
    let active_service = sqlx::query_scalar::<_, String>(
        "SELECT entry FROM battle_runs WHERE account=? AND completed=0",
    )
    .bind(a)
    .fetch_optional(&mut *db)
    .await?
    .map(|raw| read_json::<Value>(&raw))
    .transpose()?
    .is_some_and(|entry| entry["ServiceOwned"] == true || entry["ServiceRequired"] == true);
    if !active_service {
        for gate in list(db, a, "punishment_open").await? {
            if gate["DeferredOpening"] == true && gate["CostSettled"] != true {
                if let Some(owner) = gate["EntryCostReservation"].as_str() {
                    clear_hold(db, a, owner).await?;
                }
            }
        }
    }
    Ok(())
}
