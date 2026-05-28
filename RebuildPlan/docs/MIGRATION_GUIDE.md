# Rebuild Execution Guide

## 1) Create clean solution
```bash
dotnet new sln -n MayaAstroRebuild
```

## 2) Backend
```bash
cd src
dotnet new webapi -n backend
```

Install packages:
- Microsoft.EntityFrameworkCore.SqlServer
- Microsoft.EntityFrameworkCore.Tools
- BCrypt.Net-Next

## 3) Frontend
```bash
npm create vite@latest frontend -- --template react
cd frontend
npm i react-router-dom axios
```

## 4) Apply DB scripts
Run:
- `database/001_schema.sql`
- `database/002_seed.sql`

## 5) Build modules only requested
- Blog CRUD
- Quote CRUD
- YouTube CRUD
- SEO settings CRUD

## 6) Decommission old modules
Do not port legacy features outside requested scope.
