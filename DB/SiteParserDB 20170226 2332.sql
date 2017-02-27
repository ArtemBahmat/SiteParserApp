
USE master
GO

IF DB_NAME() <> N'master' SET NOEXEC ON


PRINT (N'Создать базу данных [SiteParserDB]')
GO
CREATE DATABASE SiteParserDB
ON PRIMARY(
    NAME = N'SiteParserDB',
    FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL11.STACIONAR\MSSQL\DATA\SiteParserDB.mdf',
    SIZE = 4160KB,
    MAXSIZE = UNLIMITED,
    FILEGROWTH = 1024KB
)
LOG ON(
    NAME = N'SiteParserDB_log',
    FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL11.STACIONAR\MSSQL\DATA\SiteParserDB_log.ldf',
    SIZE = 1040KB,
    MAXSIZE = UNLIMITED,
    FILEGROWTH = 10%
)
GO


PRINT (N'Изменить базу данных')
GO
ALTER DATABASE SiteParserDB
  SET
    ANSI_NULL_DEFAULT OFF,
    ANSI_NULLS OFF,
    ANSI_PADDING OFF,
    ANSI_WARNINGS OFF,
    ARITHABORT OFF,
    AUTO_CLOSE OFF,
    AUTO_CREATE_STATISTICS ON,
    AUTO_SHRINK OFF,
    AUTO_UPDATE_STATISTICS ON,
    AUTO_UPDATE_STATISTICS_ASYNC OFF,
    COMPATIBILITY_LEVEL = 110,
    CONCAT_NULL_YIELDS_NULL OFF,
    CURSOR_CLOSE_ON_COMMIT OFF,
    CURSOR_DEFAULT GLOBAL,
    DATE_CORRELATION_OPTIMIZATION OFF,
    DB_CHAINING OFF,
    HONOR_BROKER_PRIORITY OFF,
    MULTI_USER,
    NESTED_TRIGGERS = ON,
    NUMERIC_ROUNDABORT OFF,
    PAGE_VERIFY CHECKSUM,
    PARAMETERIZATION SIMPLE,
    QUOTED_IDENTIFIER OFF,
    READ_COMMITTED_SNAPSHOT ON,
    RECOVERY FULL,
    RECURSIVE_TRIGGERS OFF,
    TRANSFORM_NOISE_WORDS = OFF,
    TRUSTWORTHY OFF
    WITH ROLLBACK IMMEDIATE
GO

ALTER DATABASE SiteParserDB
  SET ENABLE_BROKER
GO

ALTER DATABASE SiteParserDB
  SET ALLOW_SNAPSHOT_ISOLATION OFF
GO

ALTER DATABASE SiteParserDB
  SET FILESTREAM (NON_TRANSACTED_ACCESS = OFF)
GO

USE SiteParserDB
GO

IF DB_NAME() <> N'SiteParserDB' SET NOEXEC ON
GO


PRINT (N'Создать таблицу [dbo].[Sites]')
GO
CREATE TABLE dbo.Sites (
  Id int IDENTITY,
  Name nvarchar(max) NULL,
  Threads int NOT NULL,
  MaxNestingLevel int NOT NULL,
  ToParseExternalLinks bit NOT NULL,
  CONSTRAINT [PK_dbo.Sites] PRIMARY KEY CLUSTERED (Id)
)
ON [PRIMARY]
TEXTIMAGE_ON [PRIMARY]
GO


PRINT (N'Создать таблицу [dbo].[Urls]')
GO
CREATE TABLE dbo.Urls (
  Id int IDENTITY,
  SiteId int NOT NULL,
  Name nvarchar(450) NOT NULL,
  ParentName nvarchar(max) NULL,
  NestingLevel int NOT NULL,
  HtmlSize int NOT NULL,
  IsExternal bit NOT NULL,
  ResponseTime float NOT NULL,
  DateTimeOfLastScan datetime NULL,
  CONSTRAINT [PK_dbo.Urls] PRIMARY KEY CLUSTERED (Id)
)
ON [PRIMARY]
TEXTIMAGE_ON [PRIMARY]
GO


PRINT (N'Создать индекс [Name] для объекта типа таблица [dbo].[Urls]')
GO
CREATE INDEX Name
  ON dbo.Urls (Name)
  ON [PRIMARY]
GO


PRINT (N'Создать внешний ключ для объекта типа таблица [dbo].[Urls]')
GO
ALTER TABLE dbo.Urls
  ADD FOREIGN KEY (SiteId) REFERENCES dbo.Sites (Id)
GO

SET QUOTED_IDENTIFIER, ANSI_NULLS ON
GO


GO
PRINT (N'Создать представление [dbo].[TotalNumberPagesPerHost]')
GO
CREATE VIEW dbo.TotalNumberPagesPerHost 
AS SELECT s.Name AS SiteName, Count(u.id) AS PagesCount 
  FROM Sites s
  JOIN Urls u 
  ON s.Id = u.SiteId
  GROUP BY s.Name
GO


GO
PRINT (N'Создать представление [dbo].[Top10SlowestPagesPerHost]')
GO
CREATE VIEW dbo.Top10SlowestPagesPerHost 
AS SELECT nq.Name, nq.ResponseTime
    FROM (
        SELECT *, Rank() 
          over (Partition BY u.SiteId
                ORDER BY u.ResponseTime DESC) AS Rank
        FROM Urls u
        ) nq WHERE Rank <= 10
GO


GO
PRINT (N'Создать представление [dbo].[Top10PagesWithMoreExternalLinks]')
GO
CREATE VIEW dbo.Top10PagesWithMoreExternalLinks 
AS SELECT TOP(10) u.ParentName AS ParentPage, Count(u.Name) AS CountExternals
FROM Urls u
WHERE u.IsExternal = 1
GROUP BY u.ParentName
ORDER BY CountExternals DESC
GO


GO
PRINT (N'Создать представление [dbo].[Top10FastestPagesPerHost]')
GO
CREATE VIEW dbo.Top10FastestPagesPerHost 
AS SELECT nq.Name, nq.ResponseTime
    FROM (
        SELECT *, Rank() 
          over (Partition BY u.SiteId
                ORDER BY u.ResponseTime ASC) AS Rank
        FROM Urls u
        ) nq WHERE Rank <= 10
GO


GO
PRINT (N'Создать представление [dbo].[AvgPageResponseTimePerHost]')
GO
CREATE VIEW dbo.AvgPageResponseTimePerHost 
AS SELECT s.Name AS SiteName, AVG (u.ResponseTime) AS AvgResponseTime
  FROM Sites s
  JOIN Urls u 
  ON s.Id = u.SiteId
  GROUP BY s.Name
GO


PRINT (N'Создать таблицу [dbo].[Resources]')
GO
CREATE TABLE dbo.Resources (
  Id int IDENTITY,
  ParentSiteId int NOT NULL,
  Name nvarchar(450) NOT NULL,
  Type int NOT NULL,
  ParentName nvarchar(max) NULL,
  CONSTRAINT [PK_dbo.Resources] PRIMARY KEY CLUSTERED (Id)
)
ON [PRIMARY]
TEXTIMAGE_ON [PRIMARY]
GO


PRINT (N'Создать индекс [Name] для объекта типа таблица [dbo].[Resources]')
GO
CREATE INDEX Name
  ON dbo.Resources (Name)
  ON [PRIMARY]
GO


PRINT (N'Создать таблицу [dbo].[ImportUrls]')
GO
CREATE TABLE dbo.ImportUrls (
  Name nvarchar(450) NOT NULL,
  CONSTRAINT [PK_dbo.ImportUrls] PRIMARY KEY CLUSTERED (Name)
)
ON [PRIMARY]
GO


PRINT (N'Создать таблицу [dbo].[__MigrationHistory]')
GO
CREATE TABLE dbo.__MigrationHistory (
  MigrationId nvarchar(150) NOT NULL,
  ContextKey nvarchar(300) NOT NULL,
  Model varbinary(max) NOT NULL,
  ProductVersion nvarchar(32) NOT NULL,
  CONSTRAINT [PK_dbo.__MigrationHistory] PRIMARY KEY CLUSTERED (MigrationId, ContextKey)
)
ON [PRIMARY]
TEXTIMAGE_ON [PRIMARY]
GO


GO
PRINT (N'Создать процедуру [dbo].[TruncateImportUrls]')
GO
CREATE PROCEDURE dbo.TruncateImportUrls
AS
DECLARE @SQL VARCHAR(2000)
SET @SQL='TRUNCATE TABLE ImportUrls'
EXEC (@SQL)
GO
SET NOEXEC OFF
GO