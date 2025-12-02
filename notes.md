# Grapher notes

## `dotnet` CLI

### Initialize MVC project
```bash
dotnet new mvc -n Grapher -o src/Grapher
dotnet new gitignore
```

### Install packages
```bash
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 9.0.0
dotnet add package Microsoft.EntityFrameworkCore.Design --version 9.0.0
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 9.0.0
```

### Entity Framework CLI tooling
`dotnet tool install --global dotnet-ef`

### Run
`dotnet run --project src/Grapher`


## `docker`

### Start the database
Creates the environment as described. Like a Makefile.
`docker compose up -d`

### Secrets
Have a `.env` file that is inside the `.gitignore` and stores the private information. `docker-compose.yml `loads from it automatically.

### Misc
To view logs: `docker compose logs -f`


## Project settings

### Database connection
```json
{
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=grapherdb;Username=admin;Password=password"
  }
}
```

## C#


## Migrations (wtf are they)
*Migrations* track changes of the *C#* models and apply SQL commands to the database to bring it to the same state as the C# model.

### Create a migration
`dotnet ef migrations add <name_of_the_migration>`

### Apply to database
`dotnet ef database update`

## C#

