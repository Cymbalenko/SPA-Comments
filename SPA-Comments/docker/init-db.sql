USE master;
GO
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'CommentsDb')
BEGIN
    CREATE DATABASE [CommentsDb];
END
GO
