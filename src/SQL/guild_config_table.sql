CREATE TABLE IF NOT EXISTS guild_config(
    guild_id text,
    anti_invite boolean DEFAULT FALSE,
    anti_everyone boolean DEFAULT FALSE,
    anti_duplicate boolean DEFAULT FALSE,
    allowed_invites text[],
    max_lines int DEFAULT 0,
    max_mentions int DEFAULT 0,
    auto_dehoist boolean DEFAULT false,
    auto_raidmode boolean DEFAULT true,
    timezone text,
    no_meme_role text,
    mute_role text,
    ignored_channels text[],
    administraitive_roles text[]
);