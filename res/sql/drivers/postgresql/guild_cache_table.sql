CREATE TABLE IF NOT EXISTS guild_cache(
    guild_id bigint NOT NULL,
    user_id bigint NOT NULL,
    role_ids bigint[],
    is_muted boolean NOT NULL DEFAULT FALSE,
    is_antimemed boolean NOT NULL DEFAULT FALSE,
    is_voice_banned boolean NOT NULL DEFAULT FALSE
);
