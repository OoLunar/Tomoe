CREATE MIGRATION m1ivvtkqm4orh43t2j6ce5jpdb5h2j2suijstcwnlxpjfuvppkbmnq
    ONTO initial
{
  CREATE TYPE default::GuildMember {
      CREATE PROPERTY roles -> array<std::int64>;
      CREATE PROPERTY disabled -> std::bool;
      CREATE REQUIRED PROPERTY joined_at -> std::datetime {
          SET readonly := true;
      };
      CREATE REQUIRED PROPERTY user_id -> std::int64 {
          SET readonly := true;
      };
  };
  CREATE TYPE default::GuildPrefix {
      CREATE REQUIRED PROPERTY created_at -> std::datetime {
          SET readonly := true;
      };
      CREATE REQUIRED PROPERTY creator_id -> std::int64 {
          SET readonly := true;
      };
      CREATE REQUIRED PROPERTY prefix -> std::str {
          SET readonly := true;
      };
  };
  CREATE TYPE default::Guild {
      CREATE REQUIRED PROPERTY guild_id -> std::int64 {
          SET readonly := true;
          CREATE CONSTRAINT std::exclusive;
      };
      CREATE INDEX ON (.guild_id);
      CREATE MULTI LINK members -> default::GuildMember;
      CREATE MULTI LINK prefixes -> default::GuildPrefix;
      CREATE PROPERTY disabled -> std::bool;
      CREATE PROPERTY keep_roles -> std::bool;
      CREATE PROPERTY try_dm_reminders -> std::bool;
  };
  ALTER TYPE default::GuildMember {
      CREATE REQUIRED LINK guild -> default::Guild {
          SET readonly := true;
      };
  };
};
