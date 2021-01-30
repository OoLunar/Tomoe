PRAGMA case_sensitive_like = true;
CREATE TABLE IF NOT EXISTS assignments(
	assignment_type TINYINT NOT NULL,
	guild_id BIGINT NOT NULL,
	channel_id BIGINT NOT NULL,
	message_id BIGINT NOT NULL,
	user_id BIGINT NOT NULL,
	set_off DATETIME NOT NULL,
	set_at DATETIME NOT NULL,
	content TEXT NOT NULL,
	ID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL
);
REINDEX assignments;
