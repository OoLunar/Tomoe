CREATE TABLE IF NOT EXISTS strikes(
    guild_id BIGINT NOT NULL,
    victim_id BIGINT NOT NULL,
    issuer_id BIGINT NOT NULL,
    reason VARCHAR(2000)[] NOT NULL,
    jumplink VARCHAR(100) NOT NULL,
    victim_messaged BOOLEAN NOT NULL,
    dropped BOOLEAN NOT NULL DEFAULT FALSE,
    created_at timestamp without time zone DEFAULT (now())::timestamp(0) without time zone NOT NULL,
    id SERIAL
);