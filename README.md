# AI 錄音文字轉換系統

<div align="center">

![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-10.0-purple?logo=.net)
![React](https://img.shields.io/badge/React-18-blue?logo=react)
![SQL Server](https://img.shields.io/badge/SQL%20Server-2019+-red?logo=microsoftsqlserver)
![License](https://img.shields.io/badge/License-MIT-green)

**一個功能完整的音訊轉文字系統，整合會議管理與 AI 語音辨識技術**

[功能特色](#功能特色) •
[快速開始](#快速開始) •
[API 文檔](#api-文檔) •
[架構說明](#系統架構) •
[開發指南](#開發指南)

</div>

---

## 📋 目錄

- [專案概述](#專案概述)
- [功能特色](#功能特色)
- [系統架構](#系統架構)
- [技術棧](#技術棧)
- [目錄結構](#目錄結構)
- [環境需求](#環境需求)
- [快速開始](#快速開始)
- [設定說明](#設定說明)
- [API 文檔](#api-文檔)
- [前端介面](#前端介面)
- [資料庫架構](#資料庫架構)
- [使用範例](#使用範例)
- [故障排除](#故障排除)
- [開發指南](#開發指南)
- [效能優化](#效能優化)
- [安全性](#安全性)
- [授權](#授權)

---

## 📖 專案概述

**AI 錄音文字轉換系統**是一個完整的企業級解決方案，整合了音訊轉文字、會議管理與智能摘要功能。本系統使用 **Buzz CLI** 作為語音辨識引擎，支援多種 Whisper 模型，並提供友善的 Web 介面進行操作。

### 核心功能

🎙️ **音訊轉文字**
- 支援多種音訊/影片格式
- 使用 Whisper AI 模型進行高精度語音辨識
- 支援多語言轉換與翻譯
- 自動生成字幕檔案（SRT/VTT）
- 智能文字摘要

📅 **會議管理**
- 完整的 CRUD 操作
- 分頁與搜尋功能
- 音訊檔案自動關聯
- 軟刪除機制保護資料

⚡ **背景處理**
- 非同步任務佇列
- 多 Profile 支援（轉錄/翻譯）
- 即時狀態追蹤
- 錯誤處理與重試機制

---

## ✨ 功能特色

### 🔥 核心功能

| 功能 | 說明 |
|------|------|
| 🎤 **音訊上傳** | 支援拖放上傳，最大 200MB，自動驗證格式 |
| 🤖 **AI 轉換** | 基於 Whisper 模型，支援繁體中文、英文等多語言 |
| 📝 **字幕生成** | 自動生成 SRT/VTT 格式字幕，包含時間軸 |
| 💡 **智能摘要** | AI 自動提取重點，生成會議摘要 |
| 📊 **即時追蹤** | WebSocket 風格輪詢，即時查看轉換狀態 |
| 📁 **多格式輸出** | TXT、SRT、VTT 多種格式自由選擇 |
| 🗂️ **會議管理** | 完整的會議記錄系統，關聯音訊檔案 |
| 🔍 **全文搜尋** | 快速搜尋會議標題、地點、成員 |
| 📱 **響應式設計** | 完美支援桌面、平板、手機裝置 |
| 🔒 **資料安全** | 軟刪除機制，參數化查詢防止 SQL 注入 |

### 🎯 進階特色

- **多 Profile 支援**：同時執行轉錄與翻譯任務
- **背景處理**：使用 `BackgroundService` 實現非同步處理
- **狀態持久化**：記憶體存儲配合檔案系統，確保資料不遺失
- **錯誤恢復**：自動重試機制，詳細錯誤日誌
- **性能優化**：資料庫索引優化，分頁查詢降低負載

---

## 🏗️ 系統架構

### 整體架構圖

```
┌─────────────────────────────────────────────────────────────┐
│                        使用者介面層                           │
│  ┌──────────────────┐         ┌──────────────────────┐      │
│  │   index.html     │         │   meetings.html      │      │
│  │  (音訊轉文字)     │         │    (會議管理)        │      │
│  │   React + CDN    │         │   React + CDN        │      │
│  └──────────────────┘         └──────────────────────┘      │
└─────────────────────────────────────────────────────────────┘
                             │
                        HTTPS/REST API
                             │
┌─────────────────────────────────────────────────────────────┐
│                      ASP.NET Core 應用層                      │
│  ┌──────────────────────────────────────────────────────┐   │
│  │                    Controllers                        │   │
│  │  • TranscriptionsController (音訊轉文字 API)          │   │
│  │  • MeetingsController (會議管理 API)                  │   │
│  └──────────────────────────────────────────────────────┘   │
│                             │                                │
│  ┌──────────────────────────────────────────────────────┐   │
│  │                     Services                          │   │
│  │  • TranscriptionQueue (任務佇列)                      │   │
│  │  • TranscriptionStore (狀態儲存)                      │   │
│  │  • TranscriptionWorker (背景處理)                     │   │
│  │  • MeetingService (資料庫服務)                        │   │
│  │  • TextSummarizer (文字摘要)                          │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                             │
          ┌──────────────────┴──────────────────┐
          │                                     │
┌─────────▼─────────┐               ┌──────────▼─────────┐
│   Buzz CLI 引擎   │               │   SQL Server       │
│  (Whisper AI)     │               │   (會議資料庫)     │
│  • 語音辨識       │               │  • Meetings 資料表 │
│  • 字幕生成       │               │  • Stored Procs    │
│  • 多語言支援     │               │  • 索引優化        │
└───────────────────┘               └────────────────────┘
          │
┌─────────▼─────────┐
│   檔案系統        │
│  • App_Data/      │
│    - uploads/     │
│    - processing/  │
│    - output/      │
└───────────────────┘
```

### 資料流程

#### 音訊轉文字流程

```
1. 使用者上傳音訊 → 2. 儲存至 uploads/ → 3. 加入任務佇列
   ↓
4. Worker 取出任務 → 5. 複製至 processing/ → 6. 呼叫 Buzz CLI
   ↓
7. Buzz 執行轉換 → 8. 生成 TXT/SRT/VTT → 9. 移至 output/
   ↓
10. 生成摘要 → 11. 更新狀態為完成 → 12. 使用者下載結果
```

#### 會議管理流程

```
1. 使用者建立會議 → 2. (可選)上傳音訊 → 3. 儲存至資料庫
   ↓
4. 音訊自動加入轉換佇列 → 5. 關聯 AudioFileId → 6. 追蹤轉換狀態
   ↓
7. 查詢/編輯/刪除會議 → 8. 呼叫 Stored Procedures → 9. 即時更新介面
```

---

## 🛠️ 技術棧

### 後端技術

| 技術 | 版本 | 用途 |
|------|------|------|
| **ASP.NET Core** | 10.0 | Web API 框架 |
| **C#** | 12.0 | 主要開發語言 |
| **Microsoft.Data.SqlClient** | 6.1.3 | SQL Server 資料庫連接 |
| **Buzz CLI** | Latest | Whisper AI 語音辨識引擎 |

### 前端技術

| 技術 | 版本 | 用途 |
|------|------|------|
| **React** | 18 | UI 框架 (CDN) |
| **Bootstrap** | 5.3 | CSS 框架 |
| **Bootstrap Icons** | 1.11 | 圖示庫 |
| **Babel Standalone** | Latest | JSX 轉譯 (開發用) |

### 資料庫技術

| 技術 | 說明 |
|------|------|
| **SQL Server** | 關聯式資料庫 |
| **Stored Procedures** | 業務邏輯封裝 |
| **索引優化** | 查詢效能提升 |
| **軟刪除** | 資料保護機制 |

### 開發工具

- **Visual Studio 2022** / **VS Code**
- **SQL Server Management Studio (SSMS)**
- **.NET SDK 10.0**
- **Git**

---

## 📁 目錄結構

```
AI-/
│
├── Controllers/                    # API 控制器層
│   ├── HomeController.cs          # 首頁控制器
│   ├── MeetingsController.cs      # 會議管理 API
│   └── TranscriptionsController.cs # 音訊轉文字 API
│
├── Models/                         # 資料模型層
│   ├── BuzzOptions.cs             # Buzz 設定模型
│   ├── ErrorViewModel.cs          # 錯誤視圖模型
│   ├── Meeting.cs                 # 會議資料模型
│   ├── TranscriptionDownloadRequest.cs
│   ├── TranscriptionJob.cs        # 轉換任務模型
│   └── TranscriptionQueryRequest.cs
│
├── Services/                       # 業務邏輯層
│   ├── MeetingService.cs          # 會議資料庫服務
│   ├── TextSummarizer.cs          # 文字摘要服務
│   ├── TranscriptionQueue.cs      # 任務佇列服務
│   ├── TranscriptionStore.cs      # 狀態儲存服務
│   └── TranscriptionWorker.cs     # 背景工作服務
│
├── Views/                          # MVC 視圖 (錯誤頁面)
│   ├── Home/
│   └── Shared/
│
├── wwwroot/                        # 靜態資源
│   ├── app/
│   │   ├── index.html             # 音訊轉文字 SPA
│   │   └── meetings.html          # 會議管理 SPA
│   ├── css/
│   ├── js/
│   ├── lib/                       # 第三方函式庫
│   └── favicon.ico
│
├── SQL/                            # 資料庫腳本
│   ├── README.md                  # SQL 設定指南
│   ├── 01_CreateTable.sql         # 建立資料表
│   ├── 02_sp_Meeting_Insert.sql   # 新增 SP
│   ├── 03_sp_Meeting_Update.sql   # 修改 SP
│   ├── 04_sp_Meeting_Delete.sql   # 刪除 SP
│   └── 05_sp_Meeting_Query.sql    # 查詢 SP
│
├── Properties/
│   └── launchSettings.json        # 啟動設定
│
├── App_Data/                       # 資料目錄 (執行時建立)
│   ├── uploads/                   # 上傳檔案
│   ├── processing/                # 處理中檔案
│   └── output/                    # 輸出結果
│
├── Program.cs                      # 應用程式入口
├── appsettings.json                # 應用程式設定
├── appsettings.Development.json    # 開發環境設定
├── AI錄音文字轉換.csproj            # 專案檔
├── AI錄音文字轉換.slnx              # 方案檔
│
├── README.md                       # 主要文檔 (本檔案)
├── IMPLEMENTATION_SUMMARY.md       # 實作摘要
├── MEETING_FEATURE_README.md       # 會議功能文檔
└── TESTING_GUIDE.md                # 測試指南
```

---

## 💻 環境需求

### 必要環境

| 項目 | 版本需求 | 說明 |
|------|---------|------|
| **.NET SDK** | 10.0+ | 執行 ASP.NET Core 應用程式 |
| **SQL Server** | 2019+ | 資料庫伺服器 (支援 Express 版) |
| **Buzz CLI** | Latest | Whisper AI 語音辨識引擎 |
| **作業系統** | Windows 10+, Linux, macOS | 跨平台支援 |

### 可選環境

- **Visual Studio 2022** - 推薦的 IDE
- **VS Code** - 輕量級編輯器
- **SQL Server Management Studio** - 資料庫管理工具
- **Postman** - API 測試工具

### 安裝 Buzz CLI

Buzz 是基於 OpenAI Whisper 的語音辨識工具，支援多種平台。

**Windows (使用 Chocolatey):**
```bash
choco install buzz
```

**macOS (使用 Homebrew):**
```bash
brew install buzz
```

**Linux / 從源碼安裝:**
```bash
pip install buzz-captions
```

**驗證安裝:**
```bash
buzz --version
```

詳細安裝說明請參考：[Buzz 官方 GitHub 儲存庫](https://github.com/chidiwilliams/buzz)

---

## 🚀 快速開始

### 1. 克隆專案

```bash
git clone https://github.com/SonicLu316/AI-.git
cd AI-
```

### 2. 設定資料庫

#### 2.1 建立資料庫

在 SQL Server 中建立新資料庫：

```sql
CREATE DATABASE MeetingDB;
GO
```

#### 2.2 執行 SQL 腳本

按照順序執行 `SQL/` 資料夾中的腳本：

```bash
# 使用 sqlcmd 執行
sqlcmd -S localhost -d MeetingDB -i SQL/01_CreateTable.sql
sqlcmd -S localhost -d MeetingDB -i SQL/02_sp_Meeting_Insert.sql
sqlcmd -S localhost -d MeetingDB -i SQL/03_sp_Meeting_Update.sql
sqlcmd -S localhost -d MeetingDB -i SQL/04_sp_Meeting_Delete.sql
sqlcmd -S localhost -d MeetingDB -i SQL/05_sp_Meeting_Query.sql
```

或使用 SSMS 手動執行每個腳本。

詳細說明請參考 [`SQL/README.md`](SQL/README.md)

### 3. 設定連接字串

編輯 `appsettings.json` 或 `appsettings.Development.json`：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MeetingDB;User Id=sa;Password=YourPassword123;TrustServerCertificate=True;"
  },
  "Buzz": {
    "UploadPath": "App_Data/uploads",
    "BuzzProcessingPath": "App_Data/processing",
    "BuzzOutputPath": "App_Data/output",
    "BuzzExecutablePath": "buzz",
    "Profiles": [
      {
        "Name": "Transcribe",
        "Task": "transcribe",
        "ModelType": "whisper",
        "ModelSize": "large",
        "Language": "zh",
        "OutputSrt": true
      }
    ]
  }
}
```

### 4. 還原 NuGet 套件

```bash
dotnet restore
```

### 5. 建置專案

```bash
dotnet build
```

### 6. 執行應用程式

```bash
dotnet run
```

或使用監看模式（開發時推薦）：

```bash
dotnet watch run
```

### 7. 存取應用程式

開啟瀏覽器訪問：

- **首頁 (音訊轉文字)**: http://localhost:5000/
- **會議管理**: http://localhost:5000/app/meetings.html
- **API Swagger** (如已啟用): http://localhost:5000/swagger

> 📝 **注意**: 實際端口號請根據應用程式啟動時的控制台輸出確認

---

## ⚙️ 設定說明

### appsettings.json 詳解

#### ConnectionStrings

資料庫連接字串配置：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=<伺服器>;Database=<資料庫>;User Id=<帳號>;Password=<密碼>;TrustServerCertificate=True;"
  }
}
```

**Windows 驗證範例:**
```json
"DefaultConnection": "Server=localhost;Database=MeetingDB;Integrated Security=True;TrustServerCertificate=True;"
```

**SQL Server 驗證範例:**
```json
"DefaultConnection": "Server=192.168.1.100;Database=MeetingDB;User Id=sa;Password=MyPassword123;TrustServerCertificate=True;"
```

#### Buzz 設定

Buzz CLI 語音辨識引擎配置：

```json
{
  "Buzz": {
    "UploadPath": "App_Data/uploads",          // 上傳檔案存放路徑
    "BuzzProcessingPath": "App_Data/processing", // 處理中檔案路徑
    "BuzzOutputPath": "App_Data/output",        // 輸出結果路徑
    "BuzzExecutablePath": "buzz",               // Buzz CLI 執行檔路徑
    "Profiles": [                               // 轉換設定檔
      {
        "Name": "Transcribe",                   // Profile 名稱
        "Task": "transcribe",                   // 任務類型: transcribe/translate
        "ModelType": "whisper",                 // 模型類型
        "ModelSize": "large",                   // 模型大小: tiny/base/small/medium/large
        "Language": "zh",                       // 語言代碼 (zh=中文)
        "OutputSrt": true,                      // 輸出 SRT 字幕
        "OutputTxt": true,                      // 輸出純文字
        "OutputVtt": false                      // 輸出 VTT 字幕
      }
    ]
  }
}
```

**模型大小說明:**

| 模型 | 參數量 | 記憶體 | 速度 | 準確度 |
|------|--------|--------|------|--------|
| tiny | 39M | ~1GB | 最快 | 一般 |
| base | 74M | ~1GB | 快 | 良好 |
| small | 244M | ~2GB | 中等 | 良好 |
| medium | 769M | ~5GB | 慢 | 很好 |
| large | 1550M | ~10GB | 最慢 | 最佳 |

**推薦配置:**
- 開發/測試: `small` 或 `medium`
- 生產環境: `large` (如硬體允許)
- 資源受限: `base` 或 `tiny`

#### Logging 設定

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

---

## 📡 API 文檔

### 音訊轉文字 API

Base URL: `/api/transcriptions`

#### 1. 上傳音訊檔案

**端點:** `POST /api/transcriptions/audioAdd`

**說明:** 上傳音訊或影片檔案，系統會自動加入轉換佇列

**請求:**
- Method: `POST`
- Content-Type: `multipart/form-data`
- Body:
  - `file` (file): 音訊或影片檔案 (最大 200MB)

**回應範例:**
```json
{
  "jobId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "fileName": "meeting_audio.mp3",
  "status": "Pending"
}
```

**cURL 範例:**
```bash
curl -X POST http://localhost:5000/api/transcriptions/audioAdd \
  -F "file=@/path/to/audio.mp3"
```

#### 2. 查詢轉換狀態

**端點:** `POST /api/transcriptions/transcriptionQry`

**說明:** 查詢指定任務的處理狀態

**請求:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**回應範例:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "originalFileName": "meeting_audio.mp3",
  "status": "Completed",
  "errorMessage": null,
  "outputFiles": {
    "Transcribe.txt": "/path/to/output.txt",
    "Transcribe.srt": "/path/to/output.srt"
  },
  "summaryPath": "/path/to/summary.txt",
  "createdAt": "2024-01-15T10:00:00+08:00",
  "completedAt": "2024-01-15T10:05:00+08:00"
}
```

**狀態代碼:**
- `Pending` (0): 等待處理
- `Processing` (1): 處理中
- `Completed` (2): 已完成
- `Failed` (3): 失敗

#### 3. 下載轉換結果

**端點:** `POST /api/transcriptions/transcriptionFileQry`

**說明:** 下載指定的輸出檔案或摘要

**請求:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "key": "Transcribe.txt",  // 選填，outputFiles 的 key
  "summary": false          // true=下載摘要, false=下載輸出檔案
}
```

**回應:** 檔案串流 (application/octet-stream)

**cURL 範例:**
```bash
# 下載轉換結果
curl -X POST http://localhost:5000/api/transcriptions/transcriptionFileQry \
  -H "Content-Type: application/json" \
  -d '{"id":"3fa85f64-5717-4562-b3fc-2c963f66afa6","key":"Transcribe.txt"}' \
  -o output.txt

# 下載摘要
curl -X POST http://localhost:5000/api/transcriptions/transcriptionFileQry \
  -H "Content-Type: application/json" \
  -d '{"id":"3fa85f64-5717-4562-b3fc-2c963f66afa6","summary":true}' \
  -o summary.txt
```

---

### 會議管理 API

Base URL: `/api/meetings`

#### 1. 查詢會議列表

**端點:** `POST /api/meetings/query`

**說明:** 分頁查詢會議列表，支援搜尋與日期篩選

**請求:**
```json
{
  "pageIndex": 1,
  "pageSize": 10,
  "searchKeyword": "專案",       // 選填，搜尋標題/地點/成員
  "startDate": "2024-01-01T00:00:00Z",  // 選填
  "endDate": "2024-12-31T23:59:59Z"     // 選填
}
```

**回應範例:**
```json
{
  "data": [
    {
      "id": "guid-1",
      "title": "專案啟動會議",
      "meetingTime": "2024-01-15T14:00:00+08:00",
      "location": "會議室A",
      "members": "張三,李四,王五",
      "audioFileId": "audio-guid-1",
      "audioFileName": "meeting_2024_01_15.mp3",
      "createdAt": "2024-01-10T10:00:00+08:00",
      "updatedAt": null,
      "isDeleted": false
    }
  ],
  "totalCount": 50,
  "pageIndex": 1,
  "pageSize": 10,
  "totalPages": 5
}
```

#### 2. 取得單一會議

**端點:** `GET /api/meetings/{id}`

**說明:** 根據 ID 取得會議詳細資料

**回應:** 單一 Meeting 物件

#### 3. 新增會議

**端點:** `POST /api/meetings`

**說明:** 建立新會議記錄

**請求:**
```json
{
  "title": "2024 Q1 業績檢討會議",
  "meetingTime": "2024-01-20T15:00:00+08:00",
  "location": "總部 3F 大會議室",
  "members": "總經理,業務主管,財務主管",
  "audioFileId": "audio-guid",  // 選填
  "audioFileName": "q1_review.mp3"  // 選填
}
```

**回應:**
```json
{
  "success": true,
  "message": "會議新增成功",
  "data": { /* Meeting 物件 */ }
}
```

#### 4. 修改會議

**端點:** `PUT /api/meetings/{id}`

**說明:** 更新現有會議資料

**請求:** 同新增會議，但 ID 從 URL 取得

**回應:**
```json
{
  "success": true,
  "message": "會議修改成功",
  "data": { /* 更新後的 Meeting 物件 */ }
}
```

#### 5. 刪除會議

**端點:** `DELETE /api/meetings/{id}`

**說明:** 軟刪除會議 (設定 IsDeleted=true)

**回應:**
```json
{
  "success": true,
  "message": "會議刪除成功"
}
```

> ⚠️ **注意**: 刪除為軟刪除，資料不會真正從資料庫移除，可通過資料庫手動復原

---

## 🖥️ 前端介面

### 音訊轉文字頁面 (`index.html`)

**功能特色:**
- ✅ 拖放上傳音訊檔案
- ✅ 即時顯示轉換狀態
- ✅ 自動輪詢狀態更新
- ✅ 多檔案下載 (TXT/SRT/VTT)
- ✅ 摘要檔案下載
- ✅ 操作日誌顯示

**使用流程:**
1. 選擇或拖放音訊檔案
2. 點擊「上傳並開始排程」
3. 系統顯示 JobId 並開始輪詢狀態
4. 狀態變更為「Completed」後可下載結果
5. 可選擇下載轉換結果或摘要檔案

---

### 會議管理頁面 (`meetings.html`)

**功能特色:**
- ✅ 會議列表分頁瀏覽
- ✅ 全文搜尋 (標題/地點/成員)
- ✅ 新增/編輯/刪除會議
- ✅ Modal 表單互動
- ✅ 音訊檔案上傳與關聯
- ✅ 響應式設計

**使用流程:**

**新增會議:**
1. 點擊「新增會議」按鈕
2. 填寫表單 (標題、時間、地點為必填)
3. 可選上傳音訊檔案 (會自動加入轉換佇列)
4. 點擊「儲存」

**搜尋會議:**
1. 在搜尋框輸入關鍵字
2. 按 Enter 或點擊「搜尋」按鈕
3. 列表即時更新

**編輯會議:**
1. 點擊會議項目的「編輯」按鈕
2. 修改資料
3. 點擊「儲存」

**刪除會議:**
1. 點擊會議項目的「刪除」按鈕
2. 確認刪除
3. 記錄標記為已刪除

---

## 🗄️ 資料庫架構

### ERD (實體關聯圖)

```
┌─────────────────────────────────────────┐
│              Meetings                    │
├─────────────────────────────────────────┤
│ PK  Id (UNIQUEIDENTIFIER)               │
│     Title (NVARCHAR(200)) NOT NULL      │
│     MeetingTime (DATETIMEOFFSET) NOT NULL│
│     Location (NVARCHAR(300)) NOT NULL   │
│     Members (NVARCHAR(1000))            │
│ FK  AudioFileId (UNIQUEIDENTIFIER)      │◄─┐ (關聯關係)
│     AudioFileName (NVARCHAR(500))       │  │
│     CreatedAt (DATETIMEOFFSET) NOT NULL │  │
│     UpdatedAt (DATETIMEOFFSET)          │  │
│     IsDeleted (BIT) NOT NULL            │  │
└─────────────────────────────────────────┘  │
                                             │
                                             │
                                             │
┌─────────────────────────────────────────┐  │
│        TranscriptionJob (記憶體)         │  │
├─────────────────────────────────────────┤  │
│ PK  Id (Guid)                           │◄─┘
│     OriginalFileName (string)           │
│     StoredFilePath (string)             │
│     Status (enum)                       │
│     OutputFiles (Dictionary)            │
│     SummaryPath (string)                │
│     CreatedAt (DateTimeOffset)          │
│     CompletedAt (DateTimeOffset?)       │
└─────────────────────────────────────────┘
```

### Meetings 資料表

**欄位說明:**

| 欄位名稱 | 資料型態 | 限制 | 說明 |
|---------|---------|------|------|
| Id | UNIQUEIDENTIFIER | PK, NOT NULL | 會議唯一識別碼 |
| Title | NVARCHAR(200) | NOT NULL | 會議標題 |
| MeetingTime | DATETIMEOFFSET | NOT NULL | 會議時間 (含時區) |
| Location | NVARCHAR(300) | NOT NULL | 會議地點 |
| Members | NVARCHAR(1000) | NULL | 會議成員 (逗號分隔) |
| AudioFileId | UNIQUEIDENTIFIER | NULL | 關聯的音訊檔案 ID |
| AudioFileName | NVARCHAR(500) | NULL | 音訊檔案名稱 |
| CreatedAt | DATETIMEOFFSET | NOT NULL, DEFAULT | 建立時間 |
| UpdatedAt | DATETIMEOFFSET | NULL | 最後更新時間 |
| IsDeleted | BIT | NOT NULL, DEFAULT(0) | 軟刪除標記 |

**索引:**

| 索引名稱 | 類型 | 欄位 | 說明 |
|---------|------|------|------|
| PK_Meetings | CLUSTERED | Id | 主鍵索引 |
| IX_Meetings_MeetingTime | NONCLUSTERED | MeetingTime | 提升時間查詢效能 |
| IX_Meetings_IsDeleted | NONCLUSTERED | IsDeleted | 提升過濾已刪除記錄效能 |
| IX_Meetings_CreatedAt | NONCLUSTERED | CreatedAt | 提升排序效能 |

### Stored Procedures

| SP 名稱 | 功能 | 說明 |
|---------|------|------|
| sp_Meeting_Insert | 新增會議 | 插入新記錄並返回完整資料 |
| sp_Meeting_Update | 修改會議 | 更新記錄並自動設定 UpdatedAt |
| sp_Meeting_Delete | 刪除會議 | 軟刪除，設定 IsDeleted=1 |
| sp_Meeting_Query | 查詢會議 | 支援分頁、搜尋、日期篩選 |

詳細 SQL 腳本請參考 [`SQL/README.md`](SQL/README.md)

---

## 📚 使用範例

### 範例 1: 完整工作流程 - 從上傳到下載

**場景:** 上傳會議錄音，取得轉換結果與摘要

```bash
# 1. 上傳音訊檔案
curl -X POST http://localhost:5000/api/transcriptions/audioAdd \
  -F "file=@meeting_2024_01_15.mp3"

# 回應:
# {
#   "jobId": "abc-123-def",
#   "fileName": "meeting_2024_01_15.mp3",
#   "status": "Pending"
# }

# 2. 查詢處理狀態 (輪詢)
curl -X POST http://localhost:5000/api/transcriptions/transcriptionQry \
  -H "Content-Type: application/json" \
  -d '{"id":"abc-123-def"}'

# 3. 狀態變更為 Completed 後下載結果
curl -X POST http://localhost:5000/api/transcriptions/transcriptionFileQry \
  -H "Content-Type: application/json" \
  -d '{"id":"abc-123-def","key":"Transcribe.txt"}' \
  -o transcript.txt

# 4. 下載摘要
curl -X POST http://localhost:5000/api/transcriptions/transcriptionFileQry \
  -H "Content-Type: application/json" \
  -d '{"id":"abc-123-def","summary":true}' \
  -o summary.txt
```

### 範例 2: 建立帶音訊的會議記錄

**場景:** 先上傳音訊，再建立會議並關聯音訊

```bash
# 1. 上傳音訊
RESPONSE=$(curl -X POST http://localhost:5000/api/transcriptions/audioAdd \
  -F "file=@q1_meeting.mp3")
AUDIO_ID=$(echo $RESPONSE | jq -r '.jobId')
AUDIO_NAME=$(echo $RESPONSE | jq -r '.fileName')

# 2. 建立會議並關聯音訊
curl -X POST http://localhost:5000/api/meetings \
  -H "Content-Type: application/json" \
  -d '{
    "title": "2024 Q1 業績檢討會議",
    "meetingTime": "2024-01-20T15:00:00+08:00",
    "location": "總部 3F 大會議室",
    "members": "總經理,業務主管,財務主管",
    "audioFileId": "'$AUDIO_ID'",
    "audioFileName": "'$AUDIO_NAME'"
  }'
```

### 範例 3: 批次查詢會議

**場景:** 搜尋 2024 年 1 月的所有專案會議

```bash
curl -X POST http://localhost:5000/api/meetings/query \
  -H "Content-Type: application/json" \
  -d '{
    "pageIndex": 1,
    "pageSize": 20,
    "searchKeyword": "專案",
    "startDate": "2024-01-01T00:00:00Z",
    "endDate": "2024-01-31T23:59:59Z"
  }'
```

### 範例 4: PowerShell 腳本自動化

```powershell
# upload_and_track.ps1 - 自動上傳並追蹤轉換進度

$audioFile = "C:\Meetings\recording.mp3"
$baseUrl = "http://localhost:5000"

# 上傳檔案
$uploadResponse = Invoke-WebRequest -Uri "$baseUrl/api/transcriptions/audioAdd" `
    -Method Post `
    -Form @{ file = Get-Item $audioFile }
$jobData = $uploadResponse.Content | ConvertFrom-Json
$jobId = $jobData.jobId

Write-Host "Job ID: $jobId" -ForegroundColor Green

# 輪詢狀態直到完成
do {
    Start-Sleep -Seconds 5
    $statusResponse = Invoke-RestMethod -Uri "$baseUrl/api/transcriptions/transcriptionQry" `
        -Method Post `
        -ContentType "application/json" `
        -Body (@{ id = $jobId } | ConvertTo-Json)
    
    Write-Host "Status: $($statusResponse.Status)" -ForegroundColor Yellow
} while ($statusResponse.Status -ne "Completed" -and $statusResponse.Status -ne "Failed")

if ($statusResponse.Status -eq "Completed") {
    Write-Host "轉換完成！可用檔案:" -ForegroundColor Green
    $statusResponse.outputFiles.PSObject.Properties | ForEach-Object {
        Write-Host "  - $($_.Name)"
    }
}
```

---

## 🔧 故障排除

### 問題 1: 無法連接到資料庫

**症狀:**
```
SqlException: Cannot open database "MeetingDB" requested by the login.
```

**解決方法:**
1. 確認 SQL Server 服務正在執行
   ```bash
   # Windows
   sc query MSSQLSERVER
   
   # 啟動服務
   net start MSSQLSERVER
   ```

2. 檢查連接字串設定
   - 伺服器名稱是否正確
   - 資料庫名稱是否存在
   - 帳號密碼是否正確
   - 防火牆是否允許連接

3. 測試連接
   ```bash
   sqlcmd -S localhost -U sa -P YourPassword -Q "SELECT @@VERSION"
   ```

### 問題 2: Buzz CLI 找不到

**症狀:**
```
InvalidOperationException: Failed to start buzz process
```

**解決方法:**
1. 確認 Buzz 已安裝
   ```bash
   buzz --version
   ```

2. 如果使用自訂路徑，更新 `appsettings.json`:
   ```json
   "BuzzExecutablePath": "C:\\Program Files\\Buzz\\buzz.exe"
   ```

3. 確認 PATH 環境變數包含 Buzz 路徑

### 問題 3: 檔案上傳失敗 (413 Payload Too Large)

**症狀:**
```
HTTP 413: Request Entity Too Large
```

**解決方法:**
在 `Program.cs` 或 `web.config` 增加檔案大小限制：

```csharp
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 500_000_000; // 500MB
});
```

### 問題 4: 前端頁面空白

**症狀:** 瀏覽器開啟頁面顯示空白或載入錯誤

**解決方法:**
1. 開啟瀏覽器開發者工具 (F12) 檢查 Console
2. 確認 CDN 資源是否載入成功：
   - React
   - React-DOM
   - Babel Standalone
   - Bootstrap
3. 檢查網路連接
4. 如果 CDN 被封鎖，考慮下載到本地：
   ```bash
   npm install react react-dom bootstrap
   # 修改 HTML 引用路徑
   ```

### 問題 5: 轉換任務卡在 Processing 狀態

**症狀:** 任務長時間停留在 Processing 狀態

**解決方法:**
1. 檢查 Buzz CLI 是否正確執行
   ```bash
   # 手動測試 Buzz
   buzz add --task transcribe --model-type whisper --model-size small test.mp3
   ```

2. 檢查應用程式日誌：
   ```bash
   # 查看 Console 輸出或日誌檔案
   dotnet run --verbosity detailed
   ```

3. 確認處理資料夾權限
   ```bash
   # Linux/macOS
   chmod -R 755 App_Data/
   
   # Windows: 右鍵 → 屬性 → 安全性 → 編輯權限
   ```

4. 檢查磁碟空間
   ```bash
   df -h  # Linux/macOS
   wmic logicaldisk get size,freespace,caption  # Windows
   ```

### 問題 6: CORS 錯誤

**症狀:**
```
Access to fetch at 'http://localhost:5000/api/...' has been blocked by CORS policy
```

**解決方法:**
在 `Program.cs` 添加 CORS 設定：

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ...

app.UseCors();
```

---

## 👨‍💻 開發指南

### 本地開發環境設定

1. **安裝必要工具**
   ```bash
   # .NET SDK
   winget install Microsoft.DotNet.SDK.10
   
   # SQL Server Express
   winget install Microsoft.SQLServer.2022.Express
   
   # Visual Studio Code
   winget install Microsoft.VisualStudioCode
   ```

2. **設定開發資料庫**
   ```sql
   -- 使用本地 SQL Server
   CREATE DATABASE MeetingDB_Dev;
   ```

3. **使用開發設定檔**
   ```bash
   # 設定環境變數
   set ASPNETCORE_ENVIRONMENT=Development  # Windows
   export ASPNETCORE_ENVIRONMENT=Development  # Linux/macOS
   ```

### 程式碼風格

本專案遵循以下規範：
- **C# 編碼規範**: [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- **命名規範**:
  - PascalCase: 類別、方法、屬性
  - camelCase: 區域變數、參數
  - _camelCase: private 欄位
- **註解**: 使用 XML 文件註解 (`///`) 描述 public API

### 新增功能指南

#### 新增 API 端點

1. 在 `Models/` 建立資料模型
2. 在 `Services/` 實作業務邏輯
3. 在 `Controllers/` 建立控制器
4. 更新 `Program.cs` 註冊服務

範例:
```csharp
// Models/Product.cs
public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

// Services/IProductService.cs
public interface IProductService
{
    Task<Product> GetByIdAsync(Guid id);
}

// Services/ProductService.cs
public class ProductService : IProductService
{
    public async Task<Product> GetByIdAsync(Guid id)
    {
        // 實作邏輯
    }
}

// Controllers/ProductsController.cs
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;
    
    public ProductsController(IProductService service)
    {
        _service = service;
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var product = await _service.GetByIdAsync(id);
        return Ok(product);
    }
}

// Program.cs
builder.Services.AddScoped<IProductService, ProductService>();
```

### 測試指南

#### 單元測試範例

```csharp
using Xunit;

public class TextSummarizerTests
{
    [Fact]
    public async Task SummarizeAsync_EmptyContent_ReturnsEmpty()
    {
        // Arrange
        var summarizer = new DefaultTextSummarizer();
        
        // Act
        var result = await summarizer.SummarizeAsync("", CancellationToken.None);
        
        // Assert
        Assert.Empty(result);
    }
}
```

詳細測試指南請參考 [`TESTING_GUIDE.md`](TESTING_GUIDE.md)

---

## ⚡ 效能優化

### 資料庫優化

1. **使用索引**: 已在 `MeetingTime`, `IsDeleted`, `CreatedAt` 建立索引
2. **分頁查詢**: 避免一次載入大量資料
3. **Stored Procedures**: 減少網路往返，提升執行計畫快取

### 應用程式優化

1. **非同步處理**: 使用 `BackgroundService` 處理耗時任務
2. **檔案串流**: 下載大檔案使用 `PhysicalFile` 串流傳輸
3. **記憶體快取**: TranscriptionStore 使用 `ConcurrentDictionary` 快取狀態

### 前端優化

1. **CDN 快取**: React/Bootstrap 使用 CDN 加速載入
2. **延遲載入**: 使用分頁避免一次渲染大量資料
3. **防抖動**: 搜尋功能可添加 debounce 減少請求頻率

---

## 🔒 安全性

### 實施的安全措施

1. **SQL 注入防護**
   - 使用參數化查詢
   - 使用 Stored Procedures

2. **檔案上傳安全**
   - 檔案大小限制 (200MB)
   - 檔案類型驗證
   - 檔案存放於應用程式控制的目錄

3. **資料保護**
   - 軟刪除機制
   - 敏感資料不記錄日誌

4. **輸入驗證**
   - API 端點參數驗證
   - 前端表單驗證

### 建議的額外措施

⚠️ **生產環境部署前請實施:**

1. **身份驗證與授權**
   ```csharp
   builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
       .AddJwtBearer(options => { /* ... */ });
   ```

2. **HTTPS 強制**
   ```csharp
   app.UseHttpsRedirection();
   app.UseHsts();
   ```

3. **速率限制**
   ```csharp
   builder.Services.AddRateLimiter(/* ... */);
   ```

4. **安全標頭**
   ```csharp
   app.Use(async (context, next) =>
   {
       context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
       context.Response.Headers.Add("X-Frame-Options", "DENY");
       await next();
   });
   ```

---

## 🤝 貢獻指南

歡迎貢獻! 請遵循以下步驟：

1. Fork 本專案
2. 建立功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交變更 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 開啟 Pull Request

### 提交訊息規範

- `feat:` 新功能
- `fix:` 錯誤修復
- `docs:` 文檔更新
- `style:` 程式碼格式
- `refactor:` 重構
- `test:` 測試
- `chore:` 建置/工具

---

## 📝 授權

本專案採用 MIT 授權條款 - 詳見 [LICENSE](LICENSE) 檔案

---

## 📞 聯絡資訊

- **專案維護者**: SonicLu316
- **GitHub**: [https://github.com/SonicLu316/AI-](https://github.com/SonicLu316/AI-)
- **問題回報**: [GitHub Issues](https://github.com/SonicLu316/AI-/issues)

---

## 🙏 致謝

- [OpenAI Whisper](https://github.com/openai/whisper) - 強大的語音辨識模型
- [Buzz](https://github.com/chidiwilliams/buzz) - Whisper 的 CLI 包裝工具
- [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/) - 高效能的 Web 框架
- [React](https://reactjs.org/) - 現代化的前端框架
- [Bootstrap](https://getbootstrap.com/) - 優雅的 UI 框架

---

## 📚 相關文檔

- [會議功能詳細說明](MEETING_FEATURE_README.md)
- [SQL 資料庫設定指南](SQL/README.md)
- [測試指南](TESTING_GUIDE.md)
- [實作摘要](IMPLEMENTATION_SUMMARY.md)

---

<div align="center">

**⭐ 如果這個專案對您有幫助，請給個星星支持！ ⭐**

Made with ❤️ by SonicLu316

</div>