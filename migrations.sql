CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    migration_id character varying(150) NOT NULL,
    product_version character varying(32) NOT NULL,
    CONSTRAINT pk___ef_migrations_history PRIMARY KEY (migration_id)
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20250620182407_InitialCreate') THEN
    CREATE TABLE videos (
        id uuid NOT NULL,
        user_id uuid NOT NULL,
        title character varying(200) NOT NULL,
        description character varying(1000) NOT NULL,
        text_prompt character varying(2000) NOT NULL,
        input_type character varying(50) NOT NULL,
        status character varying(50) NOT NULL,
        resolution character varying(50) NOT NULL,
        duration_seconds integer NOT NULL,
        image_url character varying(500),
        video_url character varying(500),
        thumbnail_url character varying(500),
        freepik_task_id character varying(100),
        freepik_image_task_id character varying(100),
        completed_at timestamp with time zone,
        error_message character varying(2000),
        file_size_bytes bigint NOT NULL DEFAULT 0,
        metadata jsonb,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        created_by character varying(100),
        updated_by character varying(100),
        CONSTRAINT pk_videos PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20250620182407_InitialCreate') THEN
    CREATE INDEX ix_videos_created_at ON videos (created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20250620182407_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_videos_freepik_image_task_id ON videos (freepik_image_task_id) WHERE freepik_image_task_id IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20250620182407_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_videos_freepik_task_id ON videos (freepik_task_id) WHERE freepik_task_id IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20250620182407_InitialCreate') THEN
    CREATE INDEX ix_videos_status ON videos (status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20250620182407_InitialCreate') THEN
    CREATE INDEX ix_videos_user_id ON videos (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20250620182407_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20250620182407_InitialCreate', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20250624095927_AddVideoAspectRatio') THEN
    ALTER TABLE videos ADD aspect_ratio integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20250624095927_AddVideoAspectRatio') THEN
    ALTER TABLE videos ADD style text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20250624095927_AddVideoAspectRatio') THEN
    INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20250624095927_AddVideoAspectRatio', '8.0.11');
    END IF;
END $EF$;
COMMIT;

