# Grapher notes

## `dotnet` CLI
*Note:* should probably add `--version 9.0.0` when adding packages

### Initialize MVC project
```bash
dotnet new mvc -n Grapher -o src/Grapher
dotnet new gitignore
```

### Install base packages
```bash
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 9.0.0
dotnet add package Microsoft.EntityFrameworkCore.Design --version 9.0.0
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 9.0.0
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 9.0.0
```

### Code generator
#### Installation
```bash
dotnet add package Microsoft.VisualStudio.Web.CodeGeneration.Design --version 9.0.0
dotnet tool install -g dotnet-aspnet-codegenerator
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 9.0.0
dotnet add package Microsoft.AspNetCore.Identity.UI --version 9.0.0
```
#### Usage
*Note:* `aspnet-codegenerator` requires SDK 8.0.0 but `DOTNET_ROLL_FORWARD` tells it to go up to the nearest available major version (which is 9.0.0 for us).
##### Classic
```bash
DOTNET_ROLL_FORWARD=Major                                  \
                    dotnet aspnet-codegenerator controller \
                    -name <controller_name>                \
                    -m <model_name>                        \
                    -dc ApplicationDbContext               \
                    --relativeFolderPath Controllers       \
                    --useDefaultLayout                     \
                    --referenceScriptLibraries
```
##### Identity
```bash
# Must be run from src/Grapher (because of the relative path)
DOTNET_ROLL_FORWARD=Major                                                    \
                    dotnet aspnet-codegenerator identity                     \
                    -dc ApplicationDbContext                                 \
                    --files "Account.Register;Account.Login;Account.Logout;" \
                    --layout "/Views/Shared/_Layout.cshtml"
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
"ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=grapherdb;Username=admin;Password=password"
}
```

## Migrations (wtf are they)
*Migrations* track changes of the *C#* models and apply SQL commands to the database to bring it to the same state as the C# model.

### Create a migration
`dotnet ef migrations add <name_of_the_migration>`

### Apply to database
`dotnet ef database update`

## C#

### Attributes
```cs
using System.ComponentModel.DataAnnotations; // Required, StringLength etc
using System.ComponentModel.DataAnnotations.Schema; // Key, ForeignKey, Timestamp etc
```
- `[Key]` -> `PRIMARY KEY`
- `[Required]` -> `NOT NULL`
- `[ForeignKey("MyId)]` -> corresponds to the ID stored in `MyId`

### Dummy email sender
Microsoft really likes sending confirmation emails so we need a dummy email sender class to not have to set up a real email service. We pass it with `builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, EmailSender>();`

