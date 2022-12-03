module default {
    type Guild {
        required property guild_id -> str {
          readonly := true;
          constraint exclusive;
        };
        multi link members -> GuildMember;
    }

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
    }
}
