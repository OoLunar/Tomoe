CREATE OR REPLACE FUNCTION calc_strike_count(guildId BIGINT, victimId BIGINT)
RETURNS INTEGER LANGUAGE plpgsql AS $$
DECLARE
    returnValue INTEGER DEFAULT 1;
BEGIN
    SELECT count(*) INTO returnValue FROM strikes WHERE guild_id=guildId AND victim_id=victimId;
    RETURN returnValue;
END $$;

CREATE TABLE IF NOT EXISTS strikes(
    guild_id BIGINT NOT NULL,
    victim_id BIGINT NOT NULL,
    issuer_id BIGINT NOT NULL,
    reason VARCHAR(2000)[] NOT NULL,
    jumplink VARCHAR(100) NOT NULL,
    victim_messaged BOOLEAN NOT NULL,
    dropped BOOLEAN NOT NULL DEFAULT false,
    created_at timestamp without time zone DEFAULT (now())::timestamp(0) without time zone NOT NULL,
    id SERIAL,
    strike_count int NOT NULL
);