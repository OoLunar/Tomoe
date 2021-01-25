CREATE TABLE IF NOT EXISTS tags(
    title VARCHAR(32) NOT NULL,
    guild_id BIGINT NOT NULL,
    user_id BIGINT NOT NULL,
    id TEXT PRIMARY KEY NOT NULL DEFAULT uuid_in(md5(random()::TEXT || clock_timestamp()::TEXT)::cstring),
    created_at TIMESTAMP WITHOUT TIME ZONE DEFAULT (now())::TIMESTAMP(0) WITHOUT TIME ZONE NOT NULL,
    content VARCHAR(2000) NOT NULL,
    UNIQUE(guild_id, id)
);
