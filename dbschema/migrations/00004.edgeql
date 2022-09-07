CREATE MIGRATION m144cbtmnesw2cc2hbm6p5nqucwmkmxso4quvazakw6kmu35zcjzga
    ONTO m1fumn6cgnmdmeqkaya566qh5u7uivdtd6aaq7frzd3cv7tage5dpq
{
  CREATE TYPE default::PollOption {
      CREATE REQUIRED PROPERTY creator_id -> std::str {
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
      CREATE REQUIRED PROPERTY channel_id -> std::str {
          SET readonly := true;
      };
      CREATE REQUIRED PROPERTY creator_id -> std::str {
          SET readonly := true;
      };
      CREATE REQUIRED PROPERTY expires_at -> std::datetime;
      CREATE PROPERTY guild_id -> std::str {
          SET readonly := true;
      };
      CREATE REQUIRED PROPERTY message_id -> std::str {
          SET readonly := true;
      };
      CREATE REQUIRED PROPERTY question -> std::str {
          SET readonly := true;
      };
  };
  CREATE TYPE default::PollVote {
      CREATE REQUIRED LINK poll -> default::Poll {
          SET readonly := true;
      };
      CREATE LINK option -> default::PollOption;
      CREATE REQUIRED PROPERTY voter_id -> std::str {
          SET readonly := true;
      };
  };
  ALTER TYPE default::Poll {
      CREATE REQUIRED LINK votes -> default::PollVote;
  };
};
