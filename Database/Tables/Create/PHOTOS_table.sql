DROP TABLE IF EXISTS "PHOTOS";

CREATE TABLE "PHOTOS" (
    id SERIAL PRIMARY KEY,
    url VARCHAR(2083) NOT NULL,
    user_id INTEGER NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES "USERSET"(user_id) ON DELETE CASCADE
);