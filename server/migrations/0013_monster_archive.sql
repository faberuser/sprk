-- Monster Index (Illustrated Book): the archive monsters each account has
-- defeated, sent at login as PlayerArchiveInfos and after each battle as
-- ReservedPlayerArchiveInfos. archive_type is NShared.ArchiveType (the
-- PlayerArchive SubIndex); monster is the CreatureTable index.
CREATE TABLE monster_archive (
    account_id bigint NOT NULL REFERENCES accounts(account_id),
    archive_type smallint NOT NULL,
    monster bigint NOT NULL,
    recorded_time text NOT NULL,
    PRIMARY KEY (account_id, archive_type, monster)
);
