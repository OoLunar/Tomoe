CREATE MIGRATION m1j6mzkzebhuon3olx7tckv7rocao527tiz4jhniats4nfbu6rcjgq
    ONTO m1q42ol75xwg5ik3yy3axxy6q73ekzbdy7dhtdkkzqjxgb6h2e3tza
{
  CREATE TYPE default::Reminder {
      CREATE LINK guild -> default::Guild {
          SET readonly := true;
      };
      CREATE REQUIRED LINK owner -> default::GuildMember {
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
  };
};
