# NERBA - Backoffice

## About NERBA

**NERBA – Business Association of the District of Bragança** is a private, non-profit organization recognized as being of public utility. Operating across the district, it focuses on promoting **entrepreneurship**, organizing **events**, and delivering **training and qualification programs**.

## About the Backoffice System

This project was born out of the need to **automate repetitive tasks**, **organize operations**, and **securely manage critical data**. The Backoffice system is designed to be a daily-use tool supporting NERBA’s internal management processes, particularly in the **training and qualification** sector.

In line with its commitment to innovation, NERBA has made this software **open and accessible**, encouraging **collaboration** and enabling others to **adapt and reuse** it for their own organizational needs.

## Getting Started

This project uses .NET Aspire as an orchestrator, so in order to run it you will need in your machine the following tools:

- [.NET SDK 9.0](https://dotnet.microsoft.com/pt-br/download)
- .NET Aspire Workload
- [Entity Framework Core tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)

```bash
dotnet workload install aspire
dotnet tool install --global dotnet-ef
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

2. Navigate to the Backend API Service

```bash
cd NERBABO.Backend
```

3. If this is the first time you are running a asp.net core web api project you may need to run:

```bash
dotnet dev-certs https --trust
```

4. Install npm dependencies

```bash
cd ../NERBABO.Frontend
npm install
```

5. Build, run migrations and Start the Application, use the .NET Aspire orchestrator to run all services:

```bash
dotnet build --project NERBABO.Backend/NERBABO.AppHost
dotnet ef migrations add "InitialMigration" -o Data/Migrations --project NERBABO.Backend/NERBABO.ApiService
dotnet run --project NERBABO.Backend/NERBABO.AppHost
```

This will:

- Spin up the ASP.NET Core Web API
- Launch the Angular frontend
- Start PostgreSQL, pgAdmin, and Redis containers
- Open Swagger UI

6. Access the Applications, once running, you can access:

- **Frontend(Angular)**: [http://localhost:4200](http://localhost:4200)
- **Backend (Web API)**: [http://localhost:8080](http://localhost:8080)
- **Swagger (Web API Documentation)**: [http://localhost:8080/swagger](http://localhost:8080/swagger/index.html)
- **Postgres** with **PgAdmin**
- **Redis** with **RedisInsight**

## Working with Swagger

Swagger allows us to build a documentation page that can also be used to test endpoints.

1. Swagger runs on /swagger url.
2. If the ApiService runs succesfully it will automatically open swagger for you.
3. Authenticate through the login endpoint, body example:

```json
{
  "usernameOrEmail": "admin",
  "password": "AsecurePassword123!"
}
```

3. This should return a json object like this:

```json
{
  "firstName": "Admin",
  "lastName": "Admin",
  "jwt": "the issued jwt token"
}
```

4. Copy the jwt token from the response
5. Go to the top of the page and click on the "Authorize" button.
6. You will be prompted to the jwt, use the Bearer format. "Bearer <token_goes_here>".

## Working on Angular Separately

```bash
ng serve #starts the application
ng g c features/<feature_name>/<feature_component> #generates component
ng g s core/services/<feature_name> #generates service bootstrap file
```

## Working on a Linux machine

When running the project on a Linux distribution, you may encounter a warning similar to the following:

```bash
warn: Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServer[8]
      The ASP.NET Core developer certificate is not trusted. For guidance on trusting the ASP.NET Core developer certificate, see https://aka.ms/aspnet/https-trust-dev-cert
```

This is a common issue when running HTTPS services on Linux. To resolve it, you can:

- Follow the instructions at the provided link to trust the developer certificate and try to find a solution;
- Develop using HTTP mode.
- Run the application in production mode.

Choose the approach that best suits your development or deployment needs.

## Run Production

## Contributions

We welcome and appreciate contributions of all kinds — whether it's developing _new features, writing integration tests, improving documentation, finding security vulnerabilities or anything else that helps the project grow_.

As a token of appreciation, the **NERBA Association offers a formal letter of participation to contributors**. This letter outlines the specific tasks or areas you contributed to and can serve as evidence of your skills and involvement in open-source projects — useful for resumes, job applications, or professional portfolios.

If you're interested in contributing, please feel free to open a pull request or reach out for guidance on where help is needed.

## ToDo's

- [ ] Add new Integration tests project
- [ ] Document, with comments, critical methods and classes
- [ ] Write the frontend code for Teachers, Students features
- [ ] Frontend data filtering for user-friendly and efficient interaction
- [ ] Write docker prod environment workflow documentation

## Code of Conduct

This project follows the Contributor Covenant v2.1

## Licencing

Apache License 2.0
