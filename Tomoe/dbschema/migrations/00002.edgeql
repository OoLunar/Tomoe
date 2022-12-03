CREATE MIGRATION m1iufxr4w6xcossmymzwlf54uq3cryf5xaoavmpbl5mlx572eyzsfq
    ONTO m1um7apkdnjdphitook3377jlcj22m6btydl42jvsk62pikxq4dv2a
{
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
};
