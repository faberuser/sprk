-- sprk_json_ints($n) expands a short JSON list of IDs (accounts, slots, Heroes,
-- items). Without a row estimate the planner assumed 1000 rows and hashed the
-- whole other table: friend profiles (`friend::last_stages`) parsed every
-- stored dungeon row of every player for a single account. Ten rows lets
-- `x IN (SELECT sprk_json_ints($n))` use the account and slot indexes.
ALTER FUNCTION sprk_json_ints(text) ROWS 10;
