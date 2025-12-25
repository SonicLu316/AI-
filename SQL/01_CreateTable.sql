-- =============================================
-- 會議管理資料表建立腳本
-- =============================================

-- 建立會議資料表
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Meetings]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Meetings] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [Title] NVARCHAR(200) NOT NULL,
        [MeetingTime] DATETIMEOFFSET NOT NULL,
        [Location] NVARCHAR(300) NOT NULL,
        [Members] NVARCHAR(1000) NOT NULL,
        [AudioFileId] UNIQUEIDENTIFIER NULL,
        [AudioFileName] NVARCHAR(500) NULL,
        [CreatedAt] DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
        [UpdatedAt] DATETIMEOFFSET NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0
    );

    -- 建立索引以提升查詢效能
    CREATE INDEX IX_Meetings_MeetingTime ON [dbo].[Meetings]([MeetingTime]);
    CREATE INDEX IX_Meetings_IsDeleted ON [dbo].[Meetings]([IsDeleted]);
    CREATE INDEX IX_Meetings_CreatedAt ON [dbo].[Meetings]([CreatedAt]);
END
GO

PRINT 'Meetings table created successfully.'
GO
