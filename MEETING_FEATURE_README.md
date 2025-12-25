# 會議管理功能說明

## 功能概述

此功能提供完整的會議管理系統，包含：
- 會議資料的新增、修改、刪除
- 會議列表查詢 (支援分頁和搜尋)
- 音訊檔案上傳與關聯
- 基於 MSSQL 資料庫的儲存

## 部署步驟

### 1. 資料庫設定

#### 1.1 執行 SQL 腳本

按照順序執行 `SQL/` 資料夾中的腳本：

```sql
-- 1. 建立資料表
SQL/01_CreateTable.sql

-- 2. 建立新增會議的 Stored Procedure
SQL/02_sp_Meeting_Insert.sql

-- 3. 建立修改會議的 Stored Procedure
SQL/03_sp_Meeting_Update.sql

-- 4. 建立刪除會議的 Stored Procedure
SQL/04_sp_Meeting_Delete.sql

-- 5. 建立查詢會議的 Stored Procedure
SQL/05_sp_Meeting_Query.sql
```

詳細說明請參考 `SQL/README.md`

#### 1.2 設定連接字串

在 `appsettings.json` 或 `appsettings.Development.json` 中設定資料庫連接字串：

**使用 SQL Server 驗證：**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=YOUR_DATABASE;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  }
}
```

**使用 Windows 驗證：**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=YOUR_DATABASE;Integrated Security=True;TrustServerCertificate=True;"
  }
}
```

**連接字串參數說明：**
- `YOUR_SERVER`: SQL Server 伺服器位址 (例：`localhost` 或 `192.168.1.100`)
- `YOUR_DATABASE`: 資料庫名稱 (例：`MeetingDB`)
- `YOUR_USER`: 資料庫使用者名稱 (SQL Server 驗證時)
- `YOUR_PASSWORD`: 資料庫密碼 (SQL Server 驗證時)

### 2. 啟動應用程式

```bash
dotnet run
```

或使用開發模式：

```bash
dotnet watch run
```

### 3. 存取會議管理頁面

啟動後，瀏覽器開啟：
- **會議管理頁面**: `http://localhost:5000/app/meetings.html`
- **首頁 (音訊轉文字)**: `http://localhost:5000/` 或 `http://localhost:5000/app/index.html`

> 注意：實際端口號請根據應用程式啟動時的輸出確認

## API 端點說明

### 查詢會議列表
```
POST /api/meetings/query
Content-Type: application/json

{
  "pageIndex": 1,
  "pageSize": 10,
  "searchKeyword": "專案",  // 可選，搜尋標題、地點、成員
  "startDate": "2024-01-01T00:00:00Z",  // 可選
  "endDate": "2024-12-31T23:59:59Z"     // 可選
}
```

**回應範例：**
```json
{
  "data": [
    {
      "id": "guid",
      "title": "專案啟動會議",
      "meetingTime": "2024-01-15T14:00:00+08:00",
      "location": "會議室A",
      "members": "張三,李四,王五",
      "audioFileId": "audio-guid",
      "audioFileName": "meeting_audio.mp3",
      "createdAt": "2024-01-10T10:00:00+08:00",
      "updatedAt": null,
      "isDeleted": false
    }
  ],
  "totalCount": 100,
  "pageIndex": 1,
  "pageSize": 10,
  "totalPages": 10
}
```

### 根據ID取得會議
```
GET /api/meetings/{id}
```

### 新增會議
```
POST /api/meetings
Content-Type: application/json

{
  "title": "專案啟動會議",
  "meetingTime": "2024-01-15T14:00:00+08:00",
  "location": "會議室A",
  "members": "張三,李四,王五",
  "audioFileId": "audio-guid",  // 可選
  "audioFileName": "audio.mp3"   // 可選
}
```

### 修改會議
```
PUT /api/meetings/{id}
Content-Type: application/json

{
  "title": "專案啟動會議(修改)",
  "meetingTime": "2024-01-15T15:00:00+08:00",
  "location": "會議室B",
  "members": "張三,李四,王五,趙六"
}
```

### 刪除會議
```
DELETE /api/meetings/{id}
```

> 注意：刪除為軟刪除，資料不會真正從資料庫移除

## 前端功能說明

### 會議列表頁面 (`/app/meetings.html`)

**主要功能：**

1. **會議列表顯示**
   - 顯示所有會議記錄
   - 支援分頁瀏覽
   - 顯示會議標題、時間、地點、成員、音訊檔案

2. **搜尋功能**
   - 可搜尋會議標題、地點或成員
   - 即時搜尋結果更新

3. **新增會議**
   - 點擊「新增會議」按鈕開啟表單
   - 填寫必填欄位：標題、時間、地點
   - 選填欄位：成員、音訊檔案
   - 支援音訊檔案上傳

4. **編輯會議**
   - 點擊列表項目的「編輯」按鈕
   - 修改會議資訊
   - 可更換或新增音訊檔案

5. **刪除會議**
   - 點擊列表項目的「刪除」按鈕
   - 確認後刪除會議

6. **音訊檔案整合**
   - 上傳音訊檔案自動關聯到會議
   - 音訊檔案會自動加入轉文字處理佇列
   - 可在音訊轉文字頁面查詢處理狀態

## 使用範例

### 範例 1: 新增包含音訊的會議

1. 開啟會議管理頁面
2. 點擊「新增會議」
3. 填寫資訊：
   - 標題：「2024 Q1 業績檢討會議」
   - 時間：選擇會議日期和時間
   - 地點：「總部大樓 3F 會議室」
   - 成員：「經理A,專員B,專員C」
   - 音訊檔案：選擇會議錄音檔案
4. 點擊「儲存」
5. 系統會自動上傳音訊並將其加入轉文字處理佇列

### 範例 2: 搜尋特定會議

1. 在搜尋框輸入關鍵字，例如「業績」
2. 點擊「搜尋」或按 Enter
3. 列表會顯示所有包含「業績」的會議

### 範例 3: 修改會議資訊

1. 找到要修改的會議
2. 點擊「編輯」按鈕
3. 修改需要更新的欄位
4. 點擊「儲存」

## 資料庫結構

### Meetings 資料表

| 欄位 | 類型 | 說明 | 必填 |
|------|------|------|------|
| Id | UNIQUEIDENTIFIER | 主鍵，會議唯一識別碼 | 是 |
| Title | NVARCHAR(200) | 會議標題 | 是 |
| MeetingTime | DATETIMEOFFSET | 會議時間 | 是 |
| Location | NVARCHAR(300) | 會議地點 | 是 |
| Members | NVARCHAR(1000) | 會議成員 (逗號分隔) | 否 |
| AudioFileId | UNIQUEIDENTIFIER | 關聯的音訊檔案ID | 否 |
| AudioFileName | NVARCHAR(500) | 音訊檔案名稱 | 否 |
| CreatedAt | DATETIMEOFFSET | 建立時間 | 是 |
| UpdatedAt | DATETIMEOFFSET | 更新時間 | 否 |
| IsDeleted | BIT | 軟刪除標記 | 是 |

**索引：**
- `IX_Meetings_MeetingTime` - 提升按時間查詢的效能
- `IX_Meetings_IsDeleted` - 提升過濾已刪除記錄的效能
- `IX_Meetings_CreatedAt` - 提升按建立時間排序的效能

## 故障排除

### 問題 1: 無法連接到資料庫

**症狀：** 應用程式啟動時或使用會議功能時出現資料庫連接錯誤

**解決方法：**
1. 檢查 `appsettings.json` 中的連接字串是否正確
2. 確認 SQL Server 服務是否正在執行
3. 確認網路連接和防火牆設定
4. 測試資料庫連接：
   ```bash
   sqlcmd -S YOUR_SERVER -d YOUR_DATABASE -U YOUR_USER -P YOUR_PASSWORD
   ```

### 問題 2: Stored Procedure 不存在

**症狀：** API 呼叫失敗，錯誤訊息提示找不到 stored procedure

**解決方法：**
1. 確認已執行 `SQL/` 資料夾中的所有 SQL 腳本
2. 在 SQL Server Management Studio 中檢查 stored procedures 是否存在
3. 重新執行相關的 SQL 腳本

### 問題 3: 會議列表頁面空白

**症狀：** 瀏覽器開啟會議管理頁面時顯示空白

**解決方法：**
1. 開啟瀏覽器開發者工具 (F12) 檢查 Console 錯誤
2. 確認 React 和 Bootstrap CDN 資源是否載入成功
3. 檢查網路連接
4. 嘗試清除瀏覽器快取

### 問題 4: 音訊上傳失敗

**症狀：** 新增或編輯會議時上傳音訊失敗

**解決方法：**
1. 檢查檔案大小是否超過限制 (預設 ~200MB)
2. 確認 `App_Data/uploads` 資料夾是否存在且有寫入權限
3. 檢查磁碟空間是否充足
4. 確認檔案格式是否支援

## 技術規格

- **後端框架**: ASP.NET Core 10.0
- **資料庫**: Microsoft SQL Server
- **ORM**: ADO.NET (Microsoft.Data.SqlClient)
- **前端框架**: React 18
- **UI 框架**: Bootstrap 5.3
- **圖示**: Bootstrap Icons

## 安全性考量

1. **SQL 注入防護**: 使用參數化查詢和 Stored Procedures
2. **軟刪除**: 刪除操作不會真正移除資料，可追蹤記錄
3. **輸入驗證**: API 端點包含基本的輸入驗證
4. **檔案上傳限制**: 設定檔案大小限制防止 DoS 攻擊

## 後續擴展建議

1. **身份驗證與授權**
   - 添加使用者登入功能
   - 實作基於角色的存取控制

2. **會議提醒**
   - 郵件或推播通知
   - 會議前提醒功能

3. **會議記錄整合**
   - 自動關聯轉文字結果
   - 會議摘要自動生成

4. **進階搜尋**
   - 依日期範圍搜尋
   - 依成員篩選
   - 依地點分類

5. **統計報表**
   - 會議統計圖表
   - 參與度分析
   - 音訊使用情況

## 授權與支援

如有問題或建議，請聯繫系統管理員或開發團隊。
