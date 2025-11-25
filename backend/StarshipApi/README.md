# Starship API (ASP.NET Core 8 + SQL Server)

A minimal backend API built with **.NET 8**, **ASP.NET Core**, **Entity Framework Core**, and **SQL Server**.  
The API:

- Fetches Star Wars starships from the SWAPI API
- Seeds them into a local SQL Server database
- Exposes full CRUD REST endpoints
- Is ready to be consumed by an Angular frontend

---

## Features

### ? Tech Requirements
- **.NET 8**
- **ASP.NET Core Web API**
- **Entity Framework Core**
- **SQL Server**
- **Automatic DB Migrations on startup**
- **Live seeding from SWAPI**
- **CRUD endpoints for Starships**
- **Swagger UI enabled in Development**

### ? Starship Functionality
- Fetch and seed starships from SWAPI (tries:
  1. `https://swapi.dev/api/starships/`
  2. `https://swapi.info/starships` as fallback)
- Store results in SQL Server
- Return starships through REST endpoints
- Support sorting, filtering, and paging
- Support:
  - `GET`
  - `POST`
  - `PUT`
  - `DELETE`

---

## Requirements

### Software Installed
- **.NET 8 SDK**
- **SQL Server / LocalDB**
- **PowerShell (for commands)**

### .NET SDK Version
A `global.json` file is included:

```json
{
  "sdk": {
    "version": "8.0.100"
  }
}
