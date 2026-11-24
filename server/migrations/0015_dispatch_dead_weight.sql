-- Finished dispatch jobs kept their settlement reply (`CollectionResponse`,
-- up to 2.9 MB, 11 of the 12 MB of dispatch rows in production) although
-- nothing reads it back: collecting a finished job again fails with
-- AlreadyFinishDispatchBattle. Every dispatch list (polled every 10 s while a
-- job runs), campaign entry and dispatch start parsed it for all of the
-- player's jobs. Settlement no longer stores it; drop it from stored rows, with
-- the worker snapshot that rows finished by older builds may still hold inline.
UPDATE battle_state
SET data = (data::jsonb - 'CollectionResponse' - 'SimulationSnapshot')::text
WHERE kind = 'dispatch'
  AND (data::jsonb ? 'CollectionResponse'
       OR (data::jsonb ? 'SimulationSnapshot'
           AND data::jsonb->>'State' IN ('Complete', 'Cancel')));
