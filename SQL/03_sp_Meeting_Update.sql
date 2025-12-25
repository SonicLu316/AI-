-- =============================================
-- Stored Procedure: 修改會議
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Meeting_Update]') AND type in (N'P'))
    DROP PROCEDURE [dbo].[sp_Meeting_Update]
GO

CREATE PROCEDURE [dbo].[sp_Meeting_Update]
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
        -- 檢查會議是否存在且未被刪除
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Meetings] WHERE [Id] = @Id AND [IsDeleted] = 0)
        BEGIN
            RAISERROR('會議不存在或已被刪除', 16, 1);
            RETURN -1;
        END
        
        -- 更新會議記錄
        UPDATE [dbo].[Meetings]
        SET 
            [Title] = @Title,
            [MeetingTime] = @MeetingTime,
            [Location] = @Location,
            [Members] = @Members,
            [AudioFileId] = @AudioFileId,
            [AudioFileName] = @AudioFileName,
            [UpdatedAt] = SYSDATETIMEOFFSET()
        WHERE [Id] = @Id AND [IsDeleted] = 0;
        
        -- 回傳更新後的會議資料
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

PRINT 'sp_Meeting_Update created successfully.'
GO
