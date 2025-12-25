# SQL 資料庫設定指南

## 概述
此資料夾包含會議管理功能所需的 MSSQL 資料庫腳本。

## 執行順序

請按照以下順序執行 SQL 腳本：

1. **01_CreateTable.sql** - 建立會議資料表和索引
2. **02_sp_Meeting_Insert.sql** - 建立新增會議的 Stored Procedure
3. **03_sp_Meeting_Update.sql** - 建立修改會議的 Stored Procedure
4. **04_sp_Meeting_Delete.sql** - 建立刪除會議的 Stored Procedure (軟刪除)
5. **05_sp_Meeting_Query.sql** - 建立查詢會議列表的 Stored Procedure

## 連接字串設定

在 `appsettings.json` 或 `appsettings.Development.json` 中添加連接字串：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=YOUR_DATABASE;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  }
}
```

或使用 Windows 驗證：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=YOUR_DATABASE;Integrated Security=True;TrustServerCertificate=True;"
  }
}
```

## 資料表結構

### Meetings 資料表

| 欄位名稱 | 資料型態 | 說明 |
|---------|---------|------|
| Id | UNIQUEIDENTIFIER | 主鍵，會議唯一識別碼 |
| Title | NVARCHAR(200) | 會議標題 |
| MeetingTime | DATETIMEOFFSET | 會議時間 |
| Location | NVARCHAR(300) | 會議地點 |
| Members | NVARCHAR(1000) | 會議成員 (以逗號分隔) |
| AudioFileId | UNIQUEIDENTIFIER | 音訊檔案ID (可為空) |
| AudioFileName | NVARCHAR(500) | 音訊檔案名稱 (可為空) |
| CreatedAt | DATETIMEOFFSET | 建立時間 |
| UpdatedAt | DATETIMEOFFSET | 更新時間 (可為空) |
| IsDeleted | BIT | 是否已刪除 (軟刪除標記) |

## Stored Procedures 說明

### sp_Meeting_Insert
**用途：** 新增會議記錄

**參數：**
- @Id - 會議ID (UNIQUEIDENTIFIER)
- @Title - 會議標題
- @MeetingTime - 會議時間
- @Location - 會議地點
- @Members - 會議成員
- @AudioFileId - 音訊檔案ID (可選)
- @AudioFileName - 音訊檔案名稱 (可選)

**回傳：** 新增的會議記錄

### sp_Meeting_Update
**用途：** 修改會議記錄

**參數：** 與 sp_Meeting_Insert 相同

**回傳：** 更新後的會議記錄

### sp_Meeting_Delete
**用途：** 刪除會議記錄 (軟刪除)

**參數：**
- @Id - 會議ID

**回傳：** 執行結果 (0=成功, -1=失敗)

### sp_Meeting_Query
**用途：** 查詢會議列表 (支援分頁和搜尋)

**參數：**
- @PageIndex - 頁碼 (預設: 1)
- @PageSize - 每頁筆數 (預設: 10)
- @SearchKeyword - 搜尋關鍵字 (可選，搜尋標題、地點、成員)
- @StartDate - 開始日期 (可選)
- @EndDate - 結束日期 (可選)

**回傳：** 
- 第一個結果集：會議列表資料
- 第二個結果集：總筆數

## 使用範例

### 新增會議
```sql
DECLARE @NewId UNIQUEIDENTIFIER = NEWID();
EXEC sp_Meeting_Insert 
    @Id = @NewId,
    @Title = N'專案啟動會議',
    @MeetingTime = '2024-01-15 14:00:00 +08:00',
    @Location = N'會議室A',
    @Members = N'張三,李四,王五',
    @AudioFileId = NULL,
    @AudioFileName = NULL;
```

### 修改會議
```sql
EXEC sp_Meeting_Update 
    @Id = 'YOUR-MEETING-ID',
    @Title = N'專案啟動會議(修改)',
    @MeetingTime = '2024-01-15 15:00:00 +08:00',
    @Location = N'會議室B',
    @Members = N'張三,李四,王五,趙六';
```

### 刪除會議
```sql
EXEC sp_Meeting_Delete @Id = 'YOUR-MEETING-ID';
```

### 查詢會議列表
```sql
-- 查詢第1頁，每頁10筆
EXEC sp_Meeting_Query @PageIndex = 1, @PageSize = 10;

-- 搜尋包含關鍵字的會議
EXEC sp_Meeting_Query 
    @PageIndex = 1, 
    @PageSize = 10, 
    @SearchKeyword = N'專案';

-- 查詢特定時間範圍的會議
EXEC sp_Meeting_Query 
    @PageIndex = 1, 
    @PageSize = 10, 
    @StartDate = '2024-01-01 00:00:00 +08:00',
    @EndDate = '2024-12-31 23:59:59 +08:00';
```

## 注意事項

1. 所有的 Stored Procedures 都包含錯誤處理機制
2. 刪除操作為軟刪除，資料不會真正從資料庫移除
3. 查詢操作會自動排除已刪除的記錄
4. 建議在正式環境執行前先在測試環境驗證
