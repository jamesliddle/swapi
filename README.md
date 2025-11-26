# SWAPI (GoEngineer Challenge)

A full-stack Star Wars--themed application built with a **.NET 8 REST
API** and an **Angular 20 frontend**.

The project includes **unit tests** and **integration tests** verifying
backend logic and full HTTP request/response flows.

This project demonstrates modern architecture, clean separation of
concerns, and automated test coverage.

------------------------------------------------------------------------

# Quick Start

If you just want to get the app running fast:

# 1. Start backend

```
cd backend/StarshipApi
dotnet run
```

# 2. Start frontend (in a second terminal)
```
cd frontend
npm install
ng serve --open
```

Backend → http://localhost:5000

Frontend → http://localhost:4200

That's it!

------------------------------------------------------------------------

## Project Structure

    swapi/
    ├── backend/                            
    │   ├── StarshipApi/                    # Main .NET 8 Web API project
    │   │   ├── Controllers/
    │   │   ├── Data/
    │   │   ├── Models/
    │   │   ├── Services/
    │   │   └── Program.cs
    │   │
    │   └── StarshipApi.IntegrationTests/   # Integration tests (API-level)
    │
    ├── frontend/                           # Angular 20 frontend application
    │   ├── src/
    │   └── angular.json
    │
    ├── StarshipApi.Tests/                  # Unit test project (root-level)
    │
    └── StarshipApp.sln                     # Visual Studio solution

------------------------------------------------------------------------

## Testing

### Unit Tests:`StarshipApi.Tests/`

Run:

```
dotnet test StarshipApi.Tests
```

------------------------------------------------------------------------

### Integration Tests: `backend/StarshipApi.IntegrationTests/`

Run:

```
dotnet test backend/StarshipApi.IntegrationTests
```

Run all tests:

```
dotnet test
```

------------------------------------------------------------------------

## Running the Application (Detailed)

This guide walks you through installing dependencies, configuring your
environment, and running both the **.NET 8 backend API** and **Angular 20
frontend**.

### 1. Clone the Repository

Open a terminal and run:

```
git clone https://github.com/jamesliddle/swapi.git
cd swapi
```

You should now see the `backend/`, `frontend/`, and test projects.

### 2. Backend Setup (StarshipApi)

The backend is a **.NET 8 Web API** located at `backend/StarshipApi/`

#### 2.1 Install .NET 8

If you haven't installed it yet:

https://dotnet.microsoft.com/en-us/download/dotnet/8.0

Verify installation:

```
dotnet --version
```

Should show `8.x.x`.

#### 2.2 Configure the Database

The backend uses **Entity Framework Core** with **SQL Server** by default.

Your connection string is located in:

```
backend/StarshipApi/appsettings.json
```

Example:

```
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=StarshipsDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

##### Options for database setup:

###### Option A: Use SQL Server (recommended)

If SQL Server or SQL Server Express is installed locally, no changes needed.

###### Option B: Use LocalDB (Windows)

Replace your connection string with:

```
"DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=StarshipsDb;Trusted_Connection=True;"
```

###### Option C: Use SQLite (easy dev)

Modify the connection string:

```
"DefaultConnection": "Data Source=starships.db"
```

And update EF provider in `Program.cs`.

#### 2.3 Apply EF Core Migrations

Navigate to the backend API project:

```
cd backend/StarshipApi
```

Apply migrations:

```
dotnet ef database update
```

If you get a “no EF tools” error, install globally:

`dotnet tool install --global dotnet-ef`

#### 2.4 Run the Backend API

Start the server:

```
dotnet run
```

You should see:

`Now listening on: http://localhost:5000`

The API is now running at:

http://localhost:5000

Test it in your browser:

```
http://localhost:5000/api/starships
```

### 3. Frontend Setup (Angular 20)

The frontend lives in `frontend/`

#### 3.1 Install Node.js and Angular CLI

You need:

* Node.js 18 or newer

* Angular CLI 20.x

Check Node version:

```
node --version
```

If outdated, download latest LTS:

https://nodejs.org/en/download/

Install Angular CLI:

```
npm install -g @angular/cli
```

#### 3.2 Install Frontend Dependencies

Navigate into the `frontend/` folder:

```
cd frontend
npm install
```

This installs Angular, Bootstrap, and all project dependencies.

#### 3.3 Configure the API URL

Open:

```
frontend/src/environments/environment.ts
```

Ensure the backend URL matches your running API:

```
export const environment = {
  apiUrl: 'http://localhost:5000/api'
};
```

#### 3.4 Run the Angular App

Start the dev server:

```
ng serve --open
```

This will:

* Build the Angular application
* Auto-launch your browser

Default frontend URL:

http://localhost:4200

The Angular UI should now fetch data from:

http://localhost:5000/api

### 4. Optional: Run All Tests

To run unit + integration tests:

From the root:

```
dotnet test
```

Individually:

```
dotnet test StarshipApi.Tests
dotnet test backend/StarshipApi.IntegrationTests
```

### You're Ready to Go!

You now have:

* Backend API running at http://localhost:5000
* Angular frontend running at http://localhost:4200
* Database initialized
* Tests runnable from CLI

------------------------------------------------------------------------

## API Endpoints

    GET    /api/starships
    GET    /api/starships/{id}
    POST   /api/starships
    PUT    /api/starships/{id}
    DELETE /api/starships/{id}

------------------------------------------------------------------------

## Features

-   Modern **.NET 8** backend with EF Core
-   **Angular 20** frontend
-   Starship CRUD operations
-   Unit + integration tests
-   Database seeded from SWAPI calls on first run
-   Clean architecture
