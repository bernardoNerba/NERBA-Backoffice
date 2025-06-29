# NERBA - Backoffice

## About NERBA

**NERBA – Business Association of the District of Bragança** is a private, non-profit organization recognized as being of public utility. Operating across the district, it focuses on promoting **entrepreneurship**, organizing **events**, and delivering **training and qualification programs**.

## About the Backoffice System

This project was born out of the need to **automate repetitive tasks**, **organize operations**, and **securely manage critical data**. The Backoffice system is designed to be a daily-use tool supporting NERBA’s internal management processes, particularly in the **training and qualification** sector.

In line with its commitment to innovation, NERBA has made this software **open and accessible**, encouraging **collaboration** and enabling others to **adapt and reuse** it for their own organizational needs.

## Getting Started

This project uses .NET Aspire as a Orchestrator, so in order to run it you will need in your machine the following tools:

- [.NET SDK 9.0](https://dotnet.microsoft.com/pt-br/download)
- .NET Aspire Workload

```bash
dotnet workload install aspire
```

- [Docker](https://www.docker.com/products/docker-desktop/) - for containerized volumes and services like PostgreSQL, pgAdmin and Redis.
- [Node.js](https://nodejs.org/) - required to build and run Angular frontend
- [Angular v19 CLI (Optional)](https://angular.dev/tools/cli)

```bash
npm install -g @angular/cli
```

### Running the Project

1. Clone the repo

```bash
git clone https://github.com/bernardoNerba/NERBA-Backoffice.git
cd NERBA-Backoffice
```

2. Create `appsettings.Development.json` on the root of `NERBABO.ApiService`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Admin": {
    "username": "admin",
    "email": "admin@example.com",
    "password": "AsecurePassword123!"
  },
  "JWT": {
    "Key": "j+X!w*Ky4n:HRxJSfc#Zr2KknShQLZ2A@YPd+=w)NJGDbBirk?4ZN8",
    "ExpiresInDays": 15,
    "Issuer": "http://localhost:8080",
    "ClientUrl": "http://localhost:4200"
  }
}
```

4. If this is the first time you are running a asp.net core web api project you may need to run:
   
```bash
dotnet dev-certs https --trust
```

5. Install npm dependencies
   
```bash
cd NERBABO.Frontend
npm install
```

6. Create environment files
Create: `src/environments/environment.development.ts`

- `environment.development.ts` example:
``` ts
export const environment = {
  production: false,
  appUrl: 'http://localhost:8080',
  userKey: 'NerbaBackofficeUser',
  roles: ['Admin', 'User', 'CQ', 'FM'],
};
```

7. Start the Application Use the .NET Aspire orchestrator to run all services:

```bash
dotnet run --project NERBABO.Backend/NERBABO.AppHost
```

This will:

- Spin up the ASP.NET Core Web API
- Launch the Angular frontend
- Start PostgreSQL, pgAdmin, and Redis containers

8. Access the Applications Once running, you can access:

- **Frontend(Angular)**: [http://localhost:4200](http://localhost:4200)
- **Backend (Web API)**: [http://localhost:8080](http://localhost:8080)
- **Postgres** with **PgAdmin**
- **Redis** with **RedisInsight**

### Working on Angular Separately

```bash
ng serve #starts the application
ng g c features/<feature_name>/<feature_component> #generates component
ng g s core/services/<feature_name> #generates service bootstrap file
```

### Apply EF Core migrations

```bash
dotnet ef migrations add <MigrationName>
```

## Contributions

We welcome and appreciate contributions of all kinds — whether it's developing _new features, writing integration tests, improving documentation, finding security vulnerabilities or anything else that helps the project grow_.

As a token of appreciation, the **NERBA Association offers a formal letter of participation to contributors**. This letter outlines the specific tasks or areas you contributed to and can serve as evidence of your skills and involvement in open-source projects — useful for resumes, job applications, or professional portfolios.

If you're interested in contributing, please feel free to open a pull request or reach out for guidance on where help is needed.

## ToDo's

- [ ] Add new Integration tests project
- [ ] Document, with comments, critical methods and classes
- [ ] Write the frontend code for Teachers, Students, Modules, Companies, Courses features
- [ ] Frontend data filtering for user-friendly and efficient interaction

## Code of Conduct

This project follows the Contributor Covenant v2.1

## Licencing

Apache License 2.0
