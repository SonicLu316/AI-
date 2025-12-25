-- =============================================
-- Stored Procedure: 刪除會議 (軟刪除)
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Meeting_Delete]') AND type in (N'P'))
    DROP PROCEDURE [dbo].[sp_Meeting_Delete]
GO

CREATE PROCEDURE [dbo].[sp_Meeting_Delete]
    @Id UNIQUEIDENTIFIER
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
        
        -- 軟刪除會議記錄
        UPDATE [dbo].[Meetings]
        SET 
            [IsDeleted] = 1,
            [UpdatedAt] = SYSDATETIMEOFFSET()
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

PRINT 'sp_Meeting_Delete created successfully.'
GO
