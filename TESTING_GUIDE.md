# 會議管理功能測試指南

## 測試前準備

1. 確認已執行所有 SQL 腳本
2. 確認連接字串已設定
3. 確認應用程式正在運行

## 使用 cURL 測試 API

### 1. 測試新增會議

```bash
curl -X POST http://localhost:5000/api/meetings \
  -H "Content-Type: application/json" \
  -d '{
    "title": "測試會議",
    "meetingTime": "2024-12-25T14:00:00+08:00",
    "location": "測試會議室",
    "members": "測試員A,測試員B"
  }'
```

**預期結果：**
```json
{
  "success": true,
  "message": "會議新增成功",
  "data": {
    "id": "guid-here",
    "title": "測試會議",
    ...
  }
}
```

### 2. 測試查詢會議列表

```bash
curl -X POST http://localhost:5000/api/meetings/query \
  -H "Content-Type: application/json" \
  -d '{
    "pageIndex": 1,
    "pageSize": 10
  }'
```

**預期結果：**
```json
{
  "data": [...],
  "totalCount": 1,
  "pageIndex": 1,
  "pageSize": 10,
  "totalPages": 1
}
```

### 3. 測試修改會議

首先從查詢結果中取得會議 ID，然後：

```bash
curl -X PUT http://localhost:5000/api/meetings/{MEETING_ID} \
  -H "Content-Type: application/json" \
  -d '{
    "title": "測試會議(已修改)",
    "meetingTime": "2024-12-25T15:00:00+08:00",
    "location": "測試會議室B",
    "members": "測試員A,測試員B,測試員C"
  }'
```

### 4. 測試搜尋功能

```bash
curl -X POST http://localhost:5000/api/meetings/query \
  -H "Content-Type: application/json" \
  -d '{
    "pageIndex": 1,
    "pageSize": 10,
    "searchKeyword": "測試"
  }'
```

### 5. 測試刪除會議

```bash
curl -X DELETE http://localhost:5000/api/meetings/{MEETING_ID}
```

**預期結果：**
```json
{
  "success": true,
  "message": "會議刪除成功"
}
```

## 使用 PowerShell 測試

### Windows PowerShell 7+ 版本

```powershell
# 新增會議
$body = @{
    title = "測試會議"
    meetingTime = "2024-12-25T14:00:00+08:00"
    location = "測試會議室"
    members = "測試員A,測試員B"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/meetings" `
  -Method Post `
  -ContentType "application/json" `
  -Body $body

# 查詢會議列表
$queryBody = @{
    pageIndex = 1
    pageSize = 10
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/meetings/query" `
  -Method Post `
  -ContentType "application/json" `
  -Body $queryBody
```

## 前端測試清單

### 測試場景 1: 新增會議流程

- [ ] 開啟會議管理頁面
- [ ] 點擊「新增會議」按鈕
- [ ] 填寫所有必填欄位
- [ ] 不選擇音訊檔案直接儲存
- [ ] 確認會議成功新增到列表

### 測試場景 2: 新增包含音訊的會議

- [ ] 點擊「新增會議」
- [ ] 填寫必填欄位
- [ ] 選擇音訊檔案
- [ ] 等待上傳完成
- [ ] 確認會議新增成功且包含音訊檔案名稱

### 測試場景 3: 編輯會議

- [ ] 在列表中找到一個會議
- [ ] 點擊「編輯」按鈕
- [ ] 修改標題
- [ ] 儲存
- [ ] 確認列表中的標題已更新

### 測試場景 4: 搜尋功能

- [ ] 在搜尋框輸入關鍵字
- [ ] 點擊搜尋或按 Enter
- [ ] 確認只顯示符合條件的會議
- [ ] 清空搜尋框並重新搜尋
- [ ] 確認顯示所有會議

### 測試場景 5: 分頁功能

- [ ] 確保資料庫中有超過 10 筆會議
- [ ] 確認第一頁顯示 10 筆
- [ ] 點擊「下一頁」
- [ ] 確認顯示第二頁的資料
- [ ] 點擊「上一頁」
- [ ] 確認回到第一頁

### 測試場景 6: 刪除會議

- [ ] 點擊某個會議的「刪除」按鈕
- [ ] 確認彈出確認對話框
- [ ] 點擊「確定」
- [ ] 確認會議從列表中消失

### 測試場景 7: 表單驗證

- [ ] 點擊「新增會議」
- [ ] 不填寫任何欄位直接點擊「儲存」
- [ ] 確認顯示「請輸入會議標題」錯誤訊息
- [ ] 只填寫標題，不填地點
- [ ] 確認顯示「請輸入會議地點」錯誤訊息

## 資料庫驗證

### 檢查資料表

```sql
-- 查看所有會議
SELECT * FROM Meetings WHERE IsDeleted = 0;

-- 查看包含音訊的會議
SELECT * FROM Meetings WHERE AudioFileId IS NOT NULL AND IsDeleted = 0;

-- 查看已刪除的會議
SELECT * FROM Meetings WHERE IsDeleted = 1;

-- 統計會議數量
SELECT COUNT(*) AS TotalMeetings FROM Meetings WHERE IsDeleted = 0;
```

### 測試 Stored Procedures

```sql
-- 測試新增
DECLARE @NewId UNIQUEIDENTIFIER = NEWID();
EXEC sp_Meeting_Insert 
    @Id = @NewId,
    @Title = N'測試會議',
    @MeetingTime = '2024-12-25 14:00:00 +08:00',
    @Location = N'測試會議室',
    @Members = N'測試員A,測試員B';

-- 測試查詢
EXEC sp_Meeting_Query @PageIndex = 1, @PageSize = 10;

-- 測試修改
EXEC sp_Meeting_Update 
    @Id = @NewId,
    @Title = N'測試會議(修改)',
    @MeetingTime = '2024-12-25 15:00:00 +08:00',
    @Location = N'測試會議室B',
    @Members = N'測試員A,測試員B,測試員C';

-- 測試刪除
EXEC sp_Meeting_Delete @Id = @NewId;

-- 驗證刪除 (應該看到 IsDeleted = 1)
SELECT * FROM Meetings WHERE Id = @NewId;
```

## 常見測試問題

### 問題：API 回傳 500 錯誤

**檢查項目：**
1. 檢查應用程式 Console 輸出的詳細錯誤訊息
2. 確認資料庫連接字串正確
3. 確認所有 Stored Procedures 已建立
4. 檢查資料表是否存在

### 問題：前端頁面無法載入

**檢查項目：**
1. 確認應用程式正在運行
2. 檢查瀏覽器 Console (F12) 的錯誤訊息
3. 確認 CDN 資源可以正常載入
4. 檢查網路連接

### 問題：音訊上傳失敗

**檢查項目：**
1. 確認檔案大小未超過 200MB
2. 檢查 `App_Data/uploads` 資料夾權限
3. 檢查磁碟空間
4. 查看應用程式 Console 的錯誤訊息

## 效能測試

### 大量資料測試

建立測試資料：

```sql
-- 插入 1000 筆測試會議
DECLARE @i INT = 1;
WHILE @i <= 1000
BEGIN
    DECLARE @Id UNIQUEIDENTIFIER = NEWID();
    EXEC sp_Meeting_Insert 
        @Id = @Id,
        @Title = CONCAT(N'測試會議', @i),
        @MeetingTime = DATEADD(day, @i, '2024-01-01'),
        @Location = N'會議室',
        @Members = N'成員A,成員B';
    SET @i = @i + 1;
END
```

測試查詢效能：

```sql
-- 測試分頁查詢
SET STATISTICS TIME ON;
EXEC sp_Meeting_Query @PageIndex = 1, @PageSize = 10;
EXEC sp_Meeting_Query @PageIndex = 50, @PageSize = 10;
EXEC sp_Meeting_Query @PageIndex = 100, @PageSize = 10;
SET STATISTICS TIME OFF;

-- 測試搜尋效能
SET STATISTICS TIME ON;
EXEC sp_Meeting_Query @PageIndex = 1, @PageSize = 10, @SearchKeyword = N'測試';
SET STATISTICS TIME OFF;
```

## 測試報告範本

完成測試後，可使用以下範本記錄結果：

```
測試日期：____________________
測試人員：____________________

API 測試結果：
□ 新增會議 - 通過/失敗
□ 查詢會議列表 - 通過/失敗
□ 修改會議 - 通過/失敗
□ 刪除會議 - 通過/失敗
□ 搜尋功能 - 通過/失敗

前端測試結果：
□ 頁面載入 - 通過/失敗
□ 新增功能 - 通過/失敗
□ 編輯功能 - 通過/失敗
□ 刪除功能 - 通過/失敗
□ 搜尋功能 - 通過/失敗
□ 分頁功能 - 通過/失敗
□ 音訊上傳 - 通過/失敗

發現的問題：
1. ____________________
2. ____________________
3. ____________________

建議改進：
1. ____________________
2. ____________________
3. ____________________
```
