CREATE TABLE IF NOT EXISTS mod_log(
    guild_id bigint NOT NULL,
    victim_id bigint NOT NULL,
    issuer_id bigint NOT NULL,
    action smallint NOT NULL,
    reason varchar(2000),
    created_at timestamp without time zone DEFAULT (now())::timestamp(0) without time zone NOT NULL,
    id SERIAL
)