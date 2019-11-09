# worker-coravel

dotnet add package Coravel --version 3.1.0

dotnet add package Microsoft.EntityFrameworkCore --version 3.0.0
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 3.0.0

dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 3.0.0
dotnet add package Microsoft.EntityFrameworkCore.Design --version 3.0.0

dotnet add package Pomelo.EntityFrameworkCore.MySql --version 3.0.0-rc1.final
dotnet add package Pomelo.EntityFrameworkCore.MySql.Design --version 1.1.2

dotnet ef database drop -c SQLServerContext
dotnet ef migrations add Initial -c SQLServerContext -o Migrations/SQLServerMigrations
dotnet ef database update -c SQLServerContext

dotnet ef migrations add NovaTabela -c SQLServerContext -o Migrations/SQLServerMigrations
dotnet ef database update -c SQLServerContext