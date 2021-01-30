PRAGMA case_sensitive_like = true;
CREATE TABLE IF NOT EXISTS guild_config(
	guild_id BIGINT NOT NULL,
	anti_invite BOOLEAN DEFAULT FALSE,
	anti_everyone BOOLEAN DEFAULT FALSE,
	antiraid_setoff TINYINT DEFAULT 5,
	antiraid BOOLEAN DEFAULT false,
	anti_duplicate BOOLEAN DEFAULT FALSE,
	allowed_invites TEXT,
	max_lines TINYINT DEFAULT -1,
	max_mentions TINYINT DEFAULT -1,
	auto_dehoist BOOLEAN DEFAULT false,
	auto_raidmode BOOLEAN DEFAULT true,
	mute_role BIGINT,
	antimeme_role BIGINT,
	novc_role BIGINT,
	ignored_channels TEXT,
	admin_roles TEXT
);
REINDEX guild_config;
