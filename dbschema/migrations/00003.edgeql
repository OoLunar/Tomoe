CREATE MIGRATION m1fumn6cgnmdmeqkaya566qh5u7uivdtd6aaq7frzd3cv7tage5dpq
    ONTO m1vzwhgvtzyfsgrlrahcm3byl37jwo5kizou3dl65j53xh63keaokq
{
  ALTER TYPE default::GuildMember {
      ALTER PROPERTY roles {
          SET TYPE array<std::str> USING (<array<std::str>>.roles);
      };
  };
  ALTER TYPE default::Guild {
      ALTER PROPERTY guild_id {
          SET TYPE std::str USING (<std::str>.guild_id);
      };
  };
  ALTER TYPE default::GuildMember {
      ALTER PROPERTY user_id {
          SET TYPE std::str USING (<std::str>.user_id);
      };
  };
  ALTER TYPE default::GuildPrefix {
      ALTER PROPERTY creator_id {
          SET TYPE std::str USING (<std::str>.creator_id);
      };
  };
};
