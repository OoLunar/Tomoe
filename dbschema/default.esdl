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

    type PollOption {
        required property option -> str {
            readonly := true;
        };

        required property creator_id -> str {
            readonly := true;
        };
    }

    type PollVote {
        required link poll -> Poll {
            readonly := true;
        };

        required property voter_id -> str {
            readonly := true;
        };

        link option -> PollOption;
    }

    type Poll {
        required property creator_id -> str {
            readonly := true;
        };

        property guild_id -> str {
            readonly := true;
        };

        required property channel_id -> str {
            readonly := true;
        };

        property message_id -> str;

        required property question -> str {
            readonly := true;
        };

        required property expires_at -> datetime;

        required link options -> PollOption {
            readonly := true;
        };

        required link votes -> PollVote;
    }

    type Audit {
        required property command_name -> str {
            readonly := true;
        };

        required property created_at -> datetime {
            readonly := true;
            default := (datetime_current());
        };

        required link guild -> Guild {
            readonly := true;
        };

        required link authorizer -> GuildMember {
            readonly := true;
        };

        required property affected_users -> array<str> {
            readonly := true;
        };

        required property reason -> str {
            readonly := true;
        };

        required property successful -> bool {
            readonly := true;
        };

        property notes -> array<str> {
            readonly := true;
        };

        property duration_length -> range<datetime> {
            readonly := true;
        };
    }

    type Reminder {
        required property owner_id -> str {
            readonly := true;
        };

        required property message_link -> str {
            readonly := true;
        };

        required property created_at -> datetime {
            readonly := true;
            default := datetime_current();
        };

        required property expires_at -> datetime;
        property content -> str;

        link guild -> Guild {
            readonly := true;
        };
    }
};