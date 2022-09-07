CREATE MIGRATION m1vzwhgvtzyfsgrlrahcm3byl37jwo5kizou3dl65j53xh63keaokq
    ONTO m1ivvtkqm4orh43t2j6ce5jpdb5h2j2suijstcwnlxpjfuvppkbmnq
{
  ALTER TYPE default::GuildMember {
      ALTER PROPERTY roles {
          SET TYPE array<std::bigint>;
      };
  };
  ALTER TYPE default::Guild {
      ALTER PROPERTY guild_id {
          SET TYPE std::bigint;
      };
  };
  ALTER TYPE default::GuildMember {
      ALTER PROPERTY user_id {
          SET TYPE std::bigint;
      };
  };
  ALTER TYPE default::GuildPrefix {
      ALTER PROPERTY creator_id {
          SET TYPE std::bigint;
      };
  };
};
