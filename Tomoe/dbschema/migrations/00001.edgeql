CREATE MIGRATION m1um7apkdnjdphitook3377jlcj22m6btydl42jvsk62pikxq4dv2a
    ONTO initial
{
  CREATE FUTURE nonrecursive_access_policies;
  CREATE TYPE default::Guild {
      CREATE REQUIRED PROPERTY guild_id -> std::int64 {
          SET readonly := true;
          CREATE CONSTRAINT std::exclusive;
      };
  };
  CREATE TYPE default::GuildMember {
      CREATE REQUIRED LINK guild -> default::Guild {
          SET readonly := true;
      };
      CREATE REQUIRED PROPERTY joined_at -> std::datetime {
          SET readonly := true;
      };
      CREATE REQUIRED PROPERTY user_id -> std::int64 {
          SET readonly := true;
      };
  };
  ALTER TYPE default::Guild {
      CREATE MULTI LINK members -> default::GuildMember;
  };
};
