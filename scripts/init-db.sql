-- BeanShare PostgreSQL Database Initialization
-- This script runs when the PostgreSQL container starts for the first time

-- Ensure the database exists
SELECT 'CREATE DATABASE beanshare_dev'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'beanshare_dev');

-- Create extensions that might be useful
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Log initialization
DO $$
BEGIN
    RAISE NOTICE 'BeanShare PostgreSQL database initialized successfully';
END $$;