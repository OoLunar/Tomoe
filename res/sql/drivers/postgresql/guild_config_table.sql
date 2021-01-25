CREATE TABLE IF NOT EXISTS guild_config(
    guild_id BIGINT,
    anti_invite BOOLEAN DEFAULT FALSE,
    anti_everyone BOOLEAN DEFAULT FALSE,
    antiraid_setoff SMALLINT DEFAULT 5,
    antiraid BOOLEAN DEFAULT false,
    anti_duplicate BOOLEAN DEFAULT FALSE,
    allowed_invites text[],
    max_lines SMALLINT DEFAULT -1,
    max_mentions SMALLINT DEFAULT -1,
    auto_dehoist BOOLEAN DEFAULT false,
    auto_raidmode BOOLEAN DEFAULT true,
    mute_role BIGINT DEFAULT NULL,
    antimeme_role BIGINT DEFAULT NULL,
	voice_ban_role BIGINT DEFAULT NULL,
    ignored_channels BIGINT[],
    administraitive_roles BIGINT[]
);
