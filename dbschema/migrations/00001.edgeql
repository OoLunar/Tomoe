CREATE MIGRATION m1wydw4qokd52hgci2bvlv2e5u5tdp4xanfxfhh37bgfbxgvlhc2aa
    ONTO initial
{
  CREATE TYPE default::Audit {
      CREATE REQUIRED PROPERTY affected_users -> array<std::int64> {
          SET readonly := true;
      };
      CREATE REQUIRED PROPERTY command_name -> std::str {
          SET readonly := true;
      };
      CREATE REQUIRED PROPERTY created_at -> std::datetime {
          SET default := (std::datetime_current());
          SET readonly := true;
      };
      CREATE PROPERTY duration_length -> range<std::datetime> {
          SET readonly := true;
      };
      CREATE PROPERTY notes -> array<std::str> {
          SET readonly := true;
      };
      CREATE REQUIRED PROPERTY reason -> std::str {
          SET readonly := true;
      };
      CREATE REQUIRED PROPERTY successful -> std::bool {
          SET readonly := true;
      };
  };
  CREATE TYPE default::GuildMember {
      CREATE PROPERTY disabled -> std::bool;
      CREATE REQUIRED PROPERTY joined_at -> std::datetime {
          SET readonly := true;
      };
      CREATE PROPERTY roles -> array<std::str>;
      CREATE REQUIRED PROPERTY user_id -> std::int64 {
          SET readonly := true;
      };
  };
  ALTER TYPE default::Audit {
      CREATE REQUIRED LINK authorizer -> default::GuildMember {
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
  ALTER TYPE default::Audit {
      CREATE REQUIRED LINK guild -> default::Guild {
          SET readonly := true;
      };
  };
  ALTER TYPE default::GuildMember {
      CREATE REQUIRED LINK guild -> default::Guild {
          SET readonly := true;
      };
  };
  CREATE TYPE default::Reminder {
      CREATE LINK guild -> default::Guild {
          SET readonly := true;
      };
      CREATE PROPERTY content -> std::str;
      CREATE REQUIRED PROPERTY created_at -> std::datetime {
          SET default := (std::datetime_current());
          SET readonly := true;
      };
      CREATE REQUIRED PROPERTY expires_at -> std::datetime;
      CREATE REQUIRED PROPERTY message_link -> std::str {
          SET readonly := true;
      };
      CREATE REQUIRED PROPERTY owner_id -> std::int64 {
          SET readonly := true;
      };
  };
  CREATE TYPE default::PollOption {
      CREATE REQUIRED PROPERTY creator_id -> std::int64 {
          SET readonly := true;
      };
      CREATE REQUIRED PROPERTY option -> std::str {
          SET readonly := true;
      };
  };
  CREATE TYPE default::Poll {
      CREATE REQUIRED LINK options -> default::PollOption {
          SET readonly := true;
      };
      CREATE REQUIRED PROPERTY channel_id -> std::int64 {
          SET readonly := true;
      };
      CREATE REQUIRED PROPERTY creator_id -> std::int64 {
          SET readonly := true;
      };
      CREATE REQUIRED PROPERTY expires_at -> std::datetime;
      CREATE PROPERTY guild_id -> std::int64 {
          SET readonly := true;
      };
      CREATE PROPERTY message_id -> std::int64;
      CREATE REQUIRED PROPERTY question -> std::str {
          SET readonly := true;
      };
  };
  CREATE TYPE default::PollVote {
      CREATE REQUIRED LINK poll -> default::Poll {
          SET readonly := true;
      };
      CREATE LINK option -> default::PollOption;
      CREATE REQUIRED PROPERTY voter_id -> std::int64 {
          SET readonly := true;
      };
  };
  ALTER TYPE default::Poll {
      CREATE REQUIRED LINK votes -> default::PollVote;
  };
};
