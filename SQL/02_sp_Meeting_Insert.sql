-- =============================================
-- Stored Procedure: 新增會議
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Meeting_Insert]') AND type in (N'P'))
    DROP PROCEDURE [dbo].[sp_Meeting_Insert]
GO

CREATE PROCEDURE [dbo].[sp_Meeting_Insert]
    @Id UNIQUEIDENTIFIER,
    @Title NVARCHAR(200),
    @MeetingTime DATETIMEOFFSET,
    @Location NVARCHAR(300),
    @Members NVARCHAR(1000),
    @AudioFileId UNIQUEIDENTIFIER = NULL,
    @AudioFileName NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- 新增會議記錄
        INSERT INTO [dbo].[Meetings] (
            [Id],
            [Title],
            [MeetingTime],
            [Location],
            [Members],
            [AudioFileId],
            [AudioFileName],
            [CreatedAt],
            [IsDeleted]
        )
        VALUES (
            @Id,
            @Title,
            @MeetingTime,
            @Location,
            @Members,
            @AudioFileId,
            @AudioFileName,
            SYSDATETIMEOFFSET(),
            0
        );
        
        -- 回傳新增的會議資料
        SELECT 
            [Id],
            [Title],
            [MeetingTime],
            [Location],
            [Members],
            [AudioFileId],
            [AudioFileName],
            [CreatedAt],
            [UpdatedAt],
            [IsDeleted]
        FROM [dbo].[Meetings]
        WHERE [Id] = @Id;
        
        RETURN 0; -- 成功
    END TRY
    BEGIN CATCH
        -- 回傳錯誤訊息
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
        RETURN -1; -- 失敗
    END CATCH
END
GO

PRINT 'sp_Meeting_Insert created successfully.'
GO
