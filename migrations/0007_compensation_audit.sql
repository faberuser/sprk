-- Audit records written by the out-of-tree compensation tool on production
-- (not created by the server). Kept so the data migration preserves them.

CREATE TABLE sprk_compensation_campaigns (
    campaign text PRIMARY KEY,
    applied_at text NOT NULL,
    recipient_count bigint NOT NULL,
    plan_sha256 text NOT NULL,
    backup_path text NOT NULL,
    report_json text NOT NULL
);

CREATE TABLE sprk_compensation_grants (
    campaign text NOT NULL REFERENCES sprk_compensation_campaigns(campaign),
    account_id bigint NOT NULL,
    mail_id bigint NOT NULL,
    attachments text NOT NULL,
    PRIMARY KEY (campaign, account_id)
);
