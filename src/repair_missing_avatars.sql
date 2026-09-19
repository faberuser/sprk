-- Zero hides the portrait in the client. Preserve any chosen avatar.
UPDATE user_info
SET avatar_hero_index = (
    SELECT hero_index * 10 + star FROM heroes
    WHERE heroes.account_id = user_info.account_id
    ORDER BY hero_id LIMIT 1
)
WHERE avatar_hero_index = 0
  AND EXISTS (SELECT 1 FROM heroes WHERE heroes.account_id = user_info.account_id);

-- Repair only missing historical portraits, not valid snapshots.
UPDATE chat_messages
SET content = json_set(content, '$.SenderAvatarIndex', (
    SELECT avatar_hero_index FROM user_info
    WHERE user_info.account_id = chat_messages.sender_id
))
WHERE json_valid(content)
  AND COALESCE(CAST(json_extract(content, '$.SenderAvatarIndex') AS INTEGER), 0) = 0
  AND EXISTS (SELECT 1 FROM user_info
      WHERE user_info.account_id = chat_messages.sender_id AND avatar_hero_index > 0);
