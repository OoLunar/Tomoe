CREATE OR REPLACE FUNCTION get_tag_value(guildId BIGINT, tagTitle VARCHAR(32))
RETURNS VARCHAR(2000) LANGUAGE plpgsql AS $$
DECLARE
    returnValue VARCHAR(2000) := NULL;
BEGIN
    SELECT content INTO returnValue FROM tags WHERE guild_id=guildId AND title=tagTitle;
    IF NOT FOUND THEN
        SELECT content INTO returnValue FROM tags WHERE id=(SELECT id FROM tag_aliases WHERE guild_id=guildId AND title=tagTitle);
    END IF;
    RETURN returnValue;
END $$;

CREATE OR REPLACE FUNCTION get_tag_author(guildId BIGINT, tagTitle VARCHAR(32))
RETURNS BIGINT LANGUAGE plpgsql AS $$
DECLARE
    returnValue BIGINT;
BEGIN
    SELECT user_id INTO returnValue FROM tags WHERE guild_id=guildId AND title=tagTitle;
    IF NOT FOUND THEN
        SELECT user_id INTO returnValue FROM tag_aliases WHERE guild_id=guildId AND title=tagTitle;
    END IF;
    RETURN returnValue;
END $$;

CREATE OR REPLACE FUNCTION tag_exists(guildId BIGINT, tagTitle VARCHAR(32))
RETURNS BOOLEAN LANGUAGE plpgsql AS $$
DECLARE
    returnValue BOOLEAN;
BEGIN
    SELECT EXISTS(SELECT 1 FROM tags WHERE guild_id=guildId AND title=tagTitle) INTO returnValue;
    IF(returnValue = 'f' OR NOT FOUND) THEN
        SELECT EXISTS(SELECT 1 FROM tag_aliases WHERE guild_id=guildId AND title=tagTitle) INTO returnValue;
    END IF;
    RETURN returnValue;
END $$;

CREATE OR REPLACE FUNCTION tag_claim(guildId BIGINT, userId BIGINT, tagTitle VARCHAR(32))
RETURNS void LANGUAGE plpgsql AS $$
BEGIN
    IF(exists(SELECT 1 FROM tags WHERE guild_id=guildId AND title=tagTitle)) THEN
        UPDATE tags SET user_id=userId WHERE guild_id=guildId AND title=tagTitle;
    ELSE 
        IF(exists(SELECT 1 FROM tag_aliases WHERE guild_id=guildId AND title=tagTitle)) THEN
            UPDATE tag_aliases SET user_id=userId WHERE guild_id=guildId AND title=tagTitle;
        END IF;
    END IF;
END $$;