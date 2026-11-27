-- account::profile_image: one optional custom profile picture per player.
-- The client uploads a small square PNG; the leader hero (avatar) is unchanged.
CREATE TABLE profile_images (
    account_id bigint PRIMARY KEY REFERENCES accounts(account_id) ON DELETE CASCADE,
    image bytea NOT NULL,
    updated_at text NOT NULL DEFAULT sprk_now()
);
