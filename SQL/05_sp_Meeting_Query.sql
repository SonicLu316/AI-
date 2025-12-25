-- =============================================
-- Stored Procedure: 查詢會議列表
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Meeting_Query]') AND type in (N'P'))
    DROP PROCEDURE [dbo].[sp_Meeting_Query]
GO

CREATE PROCEDURE [dbo].[sp_Meeting_Query]
    @PageIndex INT = 1,
    @PageSize INT = 10,
    @SearchKeyword NVARCHAR(200) = NULL,
    @StartDate DATETIMEOFFSET = NULL,
    @EndDate DATETIMEOFFSET = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- 計算偏移量
        DECLARE @Offset INT = (@PageIndex - 1) * @PageSize;
        
        -- 查詢會議列表 (帶分頁和搜尋條件)
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
        WHERE [IsDeleted] = 0
            AND (@SearchKeyword IS NULL OR 
                 [Title] LIKE '%' + @SearchKeyword + '%' OR 
                 [Location] LIKE '%' + @SearchKeyword + '%' OR 
                 [Members] LIKE '%' + @SearchKeyword + '%')
            AND (@StartDate IS NULL OR [MeetingTime] >= @StartDate)
            AND (@EndDate IS NULL OR [MeetingTime] <= @EndDate)
        ORDER BY [MeetingTime] DESC, [CreatedAt] DESC
        OFFSET @Offset ROWS
        FETCH NEXT @PageSize ROWS ONLY;
        
        -- 回傳總筆數
        SELECT COUNT(*) AS TotalCount
        FROM [dbo].[Meetings]
        WHERE [IsDeleted] = 0
            AND (@SearchKeyword IS NULL OR 
                 [Title] LIKE '%' + @SearchKeyword + '%' OR 
                 [Location] LIKE '%' + @SearchKeyword + '%' OR 
                 [Members] LIKE '%' + @SearchKeyword + '%')
            AND (@StartDate IS NULL OR [MeetingTime] >= @StartDate)
            AND (@EndDate IS NULL OR [MeetingTime] <= @EndDate);
        
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

PRINT 'sp_Meeting_Query created successfully.'
GO
