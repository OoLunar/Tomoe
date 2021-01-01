CREATE TABLE IF NOT EXISTS guild_cache(
    guild_id bigint NOT NULL,
    user_id bigint NOT NULL,
    role_ids bigint[],
    muted boolean NOT NULL DEFAULT FALSE,
    no_memed boolean NOT NULL DEFAULT FALSE,
    no_voicechat boolean NOT NULL DEFAULT FALSE
);