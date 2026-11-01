# Enhanzer – Full Stack Assignment (Angular + ASP.NET Core + SQL Server)

Two pages: **Login** and **Purchase Bill** (protected).

```
enhanzer-assignment/
├── backend/EnhanzerApi/   ASP.NET Core 8 Web API (C#, EF Core, JWT)
├── frontend/              Angular 20 (standalone components, signals, reactive forms)
└── database/EnhanzerDb.sql   SQL Server script
```

## Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Node.js 20.19+ / 22.12+ and npm
- SQL Server (LocalDB, Express, Developer, or Docker)
- VS Code (recommended: C# Dev Kit, Angular Language Service, SQL Server (mssql))

## 1. Database
Choose ONE:
1. **Let the API create it** – nothing to do; tables are created on first start (`EnsureCreated`).
2. **Run the script** – open `database/EnhanzerDb.sql` and execute it.

Set your connection string in `backend/EnhanzerApi/appsettings.json`:

| Setup | Connection string |
|---|---|
| Windows LocalDB (default) | `Server=(localdb)\\MSSQLLocalDB;Database=EnhanzerDb;Trusted_Connection=True;TrustServerCertificate=True` |
| SQL Express | `Server=.\\SQLEXPRESS;Database=EnhanzerDb;Trusted_Connection=True;TrustServerCertificate=True` |
| Docker / SQL login (Mac, Linux, Windows) | `Server=localhost,1433;Database=EnhanzerDb;User Id=sa;Password=Your_password123;TrustServerCertificate=True` |

Docker example:
```
docker run -e ACCEPT_EULA=Y -e MSSQL_SA_PASSWORD=Your_password123 -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
```

## 2. Run the API
```
cd backend/EnhanzerApi
dotnet restore
dotnet run
```
API: http://localhost:5080 — Swagger: http://localhost:5080/swagger

## 3. Run the Angular app (new terminal)
```
cd frontend
npm install
npm start
```
Open http://localhost:4200. `/api/*` calls are proxied to the API by `proxy.conf.json`
(change the port there if you change it in `launchSettings.json`).

## How it works
**Login**: Angular posts `{ email, password }` to `POST /api/auth/login`. The API calls the Enhanzer staging API with
`API_Action: "GetLoginData"`, `Company_Code` and `Username` = email, `Pw` = password. If the response contains
`User_Locations`, the API **upserts them into `Location_Details`**, issues a JWT and returns it. Otherwise it
returns 401 with a message. The session is kept in `sessionStorage`; an `authGuard` protects `/purchase-bill`, an
HTTP interceptor adds the Bearer token and signs out on 401, and the API endpoints use `[Authorize]`.

**Purchase Bill**: Item autocomplete (Mango, Apple, Banana, Orange, Grapes, Kiwi, Strawberry), Batch dropdown filled
from `GET /api/locations` (the `Location_Details` table). Calculations:
- Total Cost = Standard Cost × Qty × (1 − Discount%)  → 100 × 5 − 20% = **400**
- Total Selling = Standard Price × Qty → 150 × 5 = **750**
- Margin = Standard Price − Standard Cost

**Add** puts the row in the table; the Summary updates (Total Items = rows, Total Qty = sum of Qty).
**Save Bill** posts the rows to `POST /api/purchase-bills` (totals are recalculated server-side and stored in
`Purchase_Bills` / `Purchase_Bill_Items`).

## Notes
- Change `Jwt:Key` in `appsettings.json` for any real deployment.
- The external API's response shape is parsed defensively (case-insensitive search for `User_Locations`,
  `Location_Code`, `Location_Name`). If login fails unexpectedly, check the API console log.
- Test credentials from the brief: `info@enhanzer.com` / `Welcome#5`.

## Submission checklist
GitHub repo (commit in small steps) · `database/EnhanzerDb.sql` · this README · 5–10 min screen recording · submission form.
