CREATE TABLE IF NOT EXISTS tags(
    title varchar(32) NOT NULL,
    guild_id bigint NOT NULL,
    user_id bigint NOT NULL,
    id text PRIMARY KEY NOT NULL DEFAULT uuid_in(md5(random()::text || clock_timestamp()::text)::cstring),
    created_at timestamp without time zone DEFAULT (now())::timestamp(0) without time zone NOT NULL,
    content varchar(2000) NOT NULL,
    UNIQUE(guild_id, id)
);