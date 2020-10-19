CREATE TABLE IF NOT EXISTS tasks(
    type smallint NOT NULL,
    guild_id bigint NOT NULL,
    channel_id bigint NOT NULL,
    user_id bigint NOT NULL,
    set_off timestamp without time zone NOT NULL,
    set_at timestamp without time zone NOT NULL,
    contents text NOT NULL
);