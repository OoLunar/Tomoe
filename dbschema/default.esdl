module default {
    type GuildMember {
        required link guild -> Guild {
            readonly := true;
        };

        required property user_id -> str {
            readonly := true;
        };

        required property joined_at -> datetime {
            readonly := true;
        };

        property disabled -> bool;
        property roles -> array<str>;
    }

    type GuildPrefix {
        required property prefix -> str {
            readonly := true;
        };

        required property creator_id -> str {
            readonly := true;
        };

        required property created_at -> datetime {
            readonly := true;
        };
    }

    type Guild {
        required property guild_id -> str {
          readonly := true;
          constraint exclusive;
        };

        property disabled -> bool;
        property keep_roles -> bool;
        property try_dm_reminders -> bool;
        multi link prefixes -> GuildPrefix;
        multi link members -> GuildMember;
        index on (.guild_id);
    }
};