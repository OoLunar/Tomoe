CREATE TABLE IF NOT EXISTS guild_config(
    guild_id bigint,
    anti_invite boolean DEFAULT FALSE,
    anti_everyone boolean DEFAULT FALSE,
    antiraid_setoff smallint DEFAULT 5,
    antiraid boolean DEFAULT false,
    anti_duplicate boolean DEFAULT FALSE,
    allowed_invites text[],
    max_lines smallint DEFAULT 0,
    max_mentions smallint DEFAULT 0,
    auto_dehoist boolean DEFAULT false,
    auto_raidmode boolean DEFAULT true,
    no_meme_role hstore,
    mute_role hstore,
    ignored_channels bigint[],
    administraitive_roles bigint[]
);