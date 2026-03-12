-- ============================================================================
-- OptimusFrame Database Setup Script
-- ============================================================================
-- Description: Complete database setup for OptimusFrame Core API
-- Version: 1.0
-- Date: 2026-03-12
-- ============================================================================

-- ============================================================================
-- 1. DATABASE CREATION
-- ============================================================================
-- Note: Run this section separately if database doesn't exist
-- Uncomment the lines below if running manually

-- DROP DATABASE IF EXISTS optimusframe_db;
-- CREATE DATABASE optimusframe_db;
-- \c optimusframe_db;

-- ============================================================================
-- 2. ENABLE EXTENSIONS
-- ============================================================================
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ============================================================================
-- 3. CREATE TABLES
-- ============================================================================

-- Media table - stores video upload and processing information
CREATE TABLE IF NOT EXISTS "Media" (
    "MediaId" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "UserName" VARCHAR(255) NOT NULL,
    "FileName" VARCHAR(500) NOT NULL,
    "Base64" TEXT NOT NULL,
    "UrlBucket" VARCHAR(1000) NOT NULL,
    "Status" INTEGER NOT NULL DEFAULT 0,
    "OutputUri" VARCHAR(1000) NULL,
    "ErrorMessage" TEXT NULL,
    "CreatedAt" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    "CompletedAt" TIMESTAMP WITHOUT TIME ZONE NULL
);

-- ============================================================================
-- 4. CREATE INDEXES
-- ============================================================================

-- Index for user video lookups (frequently queried)
CREATE INDEX IF NOT EXISTS "IX_Media_UserName"
ON "Media" ("UserName");

-- Index for status filtering
CREATE INDEX IF NOT EXISTS "IX_Media_Status"
ON "Media" ("Status");

-- Index for date range queries
CREATE INDEX IF NOT EXISTS "IX_Media_CreatedAt"
ON "Media" ("CreatedAt" DESC);

-- Composite index for user + status queries
CREATE INDEX IF NOT EXISTS "IX_Media_UserName_Status"
ON "Media" ("UserName", "Status");

-- ============================================================================
-- 5. COMMENTS (Documentation)
-- ============================================================================

COMMENT ON TABLE "Media" IS 'Stores video upload metadata and processing status';
COMMENT ON COLUMN "Media"."MediaId" IS 'Unique identifier for the media record';
COMMENT ON COLUMN "Media"."UserName" IS 'Username or email of the user who uploaded the video';
COMMENT ON COLUMN "Media"."FileName" IS 'Original filename of the uploaded video';
COMMENT ON COLUMN "Media"."Base64" IS 'Base64 encoded video data (deprecated - use S3 instead)';
COMMENT ON COLUMN "Media"."UrlBucket" IS 'S3 bucket URL where the original video is stored';
COMMENT ON COLUMN "Media"."Status" IS 'Processing status: 0=Process, 1=Uploaded, 2=Error, 3=Completed, 4=Failed';
COMMENT ON COLUMN "Media"."OutputUri" IS 'S3 URL of the processed output (ZIP file with frames)';
COMMENT ON COLUMN "Media"."ErrorMessage" IS 'Error details if processing failed';
COMMENT ON COLUMN "Media"."CreatedAt" IS 'Timestamp when the video was uploaded';
COMMENT ON COLUMN "Media"."CompletedAt" IS 'Timestamp when processing completed or failed';

-- ============================================================================
-- 6. SEED DATA (Development/Testing)
-- ============================================================================
-- Uncomment to insert sample data for development

/*
INSERT INTO "Media" (
    "MediaId",
    "UserName",
    "FileName",
    "Base64",
    "UrlBucket",
    "Status",
    "OutputUri",
    "ErrorMessage",
    "CreatedAt",
    "CompletedAt"
) VALUES
    -- Completed video
    (
        uuid_generate_v4(),
        'test@example.com',
        'sample-video-1.mp4',
        'base64encodeddata',
        's3://optimus-bucket/videos/sample-video-1.mp4',
        3, -- Completed
        's3://optimus-bucket/outputs/sample-video-1.zip',
        NULL,
        NOW() - INTERVAL '2 days',
        NOW() - INTERVAL '2 days' + INTERVAL '5 minutes'
    ),
    -- Processing video
    (
        uuid_generate_v4(),
        'test@example.com',
        'sample-video-2.mp4',
        'base64encodeddata',
        's3://optimus-bucket/videos/sample-video-2.mp4',
        0, -- Processing
        NULL,
        NULL,
        NOW() - INTERVAL '10 minutes',
        NULL
    ),
    -- Failed video
    (
        uuid_generate_v4(),
        'user@example.com',
        'corrupted-video.mp4',
        'base64encodeddata',
        's3://optimus-bucket/videos/corrupted-video.mp4',
        4, -- Failed
        NULL,
        'Video file is corrupted or in unsupported format',
        NOW() - INTERVAL '1 day',
        NOW() - INTERVAL '1 day' + INTERVAL '2 minutes'
    ),
    -- Recently uploaded
    (
        uuid_generate_v4(),
        'user@example.com',
        'new-upload.mp4',
        'base64encodeddata',
        's3://optimus-bucket/videos/new-upload.mp4',
        1, -- Uploaded
        NULL,
        NULL,
        NOW() - INTERVAL '5 minutes',
        NULL
    );
*/

-- ============================================================================
-- 7. VERIFICATION QUERIES
-- ============================================================================
-- Run these to verify the setup

-- Check if tables exist
SELECT
    table_name,
    table_type
FROM
    information_schema.tables
WHERE
    table_schema = 'public'
    AND table_name = 'Media';

-- Check indexes
SELECT
    indexname,
    indexdef
FROM
    pg_indexes
WHERE
    tablename = 'Media';

-- Check record count (should be 0 without seed data, 4 with seed data)
SELECT COUNT(*) as total_records FROM "Media";

-- ============================================================================
-- 8. CLEANUP (Development Only)
-- ============================================================================
-- Uncomment to reset database during development

/*
DROP TABLE IF EXISTS "Media" CASCADE;
*/

-- ============================================================================
-- END OF SCRIPT
-- ============================================================================

-- For migration-based setup, use Entity Framework instead:
-- dotnet ef database update --project src/OptimusFrame.Core.Infrastructure --startup-project src/OptimusFrame.Core.API
