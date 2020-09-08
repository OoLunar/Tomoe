CREATE TABLE IF NOT EXISTS guild_cache(
    user_id text NOT NULL,
    role_ids text[],
    strikes smallint NOT NULl DEFAULT 0,
    no_memed boolean NOT NULL DEFAULT FALSE,
    muted boolean NOT NULL DEFAULT FALSE,
    banned boolean NOT NULL DEFAULT FALSE
);