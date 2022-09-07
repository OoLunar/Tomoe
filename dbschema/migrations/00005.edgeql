CREATE MIGRATION m1q42ol75xwg5ik3yy3axxy6q73ekzbdy7dhtdkkzqjxgb6h2e3tza
    ONTO m144cbtmnesw2cc2hbm6p5nqucwmkmxso4quvazakw6kmu35zcjzga
{
  CREATE TYPE default::Audit {
      CREATE REQUIRED LINK authorizer -> default::GuildMember {
          SET readonly := true;
      };
      CREATE REQUIRED LINK guild -> default::Guild {
          SET readonly := true;
      };
      CREATE REQUIRED PROPERTY affected_users -> array<std::str> {
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
  ALTER TYPE default::Poll {
      ALTER PROPERTY message_id {
          RESET readonly;
          RESET OPTIONALITY;
      };
  };
};
