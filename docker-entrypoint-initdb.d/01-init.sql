-- ============================================================================
-- OptimusFrame - Docker PostgreSQL Initialization Script
-- ============================================================================
-- This script runs automatically when PostgreSQL container starts for the first time
-- ============================================================================

-- Create database if it doesn't exist
SELECT 'CREATE DATABASE optimusframe_db'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'optimusframe_db')\gexec

-- Connect to the database
\c optimusframe_db;

-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Create Media table
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

-- Create indexes
CREATE INDEX IF NOT EXISTS "IX_Media_UserName" ON "Media" ("UserName");
CREATE INDEX IF NOT EXISTS "IX_Media_Status" ON "Media" ("Status");
CREATE INDEX IF NOT EXISTS "IX_Media_CreatedAt" ON "Media" ("CreatedAt" DESC);
CREATE INDEX IF NOT EXISTS "IX_Media_UserName_Status" ON "Media" ("UserName", "Status");

-- Log successful initialization
DO $$
BEGIN
    RAISE NOTICE 'OptimusFrame database initialized successfully';
END $$;
