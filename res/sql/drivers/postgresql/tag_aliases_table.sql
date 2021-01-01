CREATE TABLE IF NOT EXISTS tag_aliases(
    title varchar(32) NOT NULL,
    guild_id bigint NOT NULL,
    user_id bigint NOT NULL,
    id text NOT NULL,
    created_at timestamp without time zone DEFAULT (now())::timestamp(0) without time zone NOT NULL,
    UNIQUE(title, guild_id),
    CONSTRAINT tags_aliases_lock FOREIGN KEY(id) REFERENCES tags(id) ON DELETE CASCADE
);