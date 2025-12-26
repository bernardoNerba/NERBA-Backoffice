<p align="center">
  <img src="NERBABO.Frontend/public/images/logo_g.png" alt="NERBA Logo" width="200"/>
</p>
<h1 align="center">NERBA - Backoffice</h1>

## About NERBA

**NERBA – Associação Empresarial do Distrito de Bragança** is a private, non-profit organization recognized as being of public utility. Operating across the district, it focuses on promoting **entrepreneurship**, organizing **events**, and delivering **training and qualification programs**.

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

2. Configure Environment Variables

Copy the `.env.example` file to `.env` and update the values with your specific configuration:

```bash
cp .env.example .env
```

**Important:** Open the `.env` file and change the following crucial values:

- `POSTGRES_PASSWORD` - Strong password for PostgreSQL database
- `REDIS_PASSWORD` - Strong password for Redis cache
- `JWT_KEY` - Secure key for JWT token signing (minimum 32 characters)
- `JWT_ISSUER` - Your server IP address (e.g., `http://192.168.1.100:5001`)
- `CLIENT_URL` - Your server IP for frontend (e.g., `http://192.168.1.100:4200`)
- `CLIENT_URL_HTTPS` - HTTPS version if using SSL
- `CLIENT_URL_PROXY` - Your server IP for nginx proxy (e.g., `http://192.168.1.100`)
- `CLIENT_URL_PROXY_HTTPS` - HTTPS version for nginx proxy
- `ADMIN_USERNAME` - Admin user username
- `ADMIN_EMAIL` - Admin user email
- `ADMIN_PASSWORD` - Strong password for admin user
- `PGADMIN_PASSWORD` - Password for PgAdmin interface

**Note:** For development, you can use the default values, but for production deployments, you **must** change all passwords and keys to secure values.

3. Trust the ASP.NET Core HTTPS certificate (first time only)

If this is the first time you are running an ASP.NET Core web API project, you may need to run:

```bash
dotnet dev-certs https --trust
```

4. Install npm dependencies

```bash
cd NERBABO.Frontend
npm install
```

5. Build, run migrations and Start the Application

Use the .NET Aspire orchestrator to run all services:

```bash
cd ..
dotnet build --project NERBABO.Backend/NERBABO.AppHost
dotnet ef migrations add "InitialMigration" -o Data/Migrations --project NERBABO.Backend/NERBABO.ApiService
dotnet run --project NERBABO.Backend/NERBABO.AppHost
```

This will:

- Spin up the ASP.NET Core Web API
- Launch the Angular frontend
- Start PostgreSQL, pgAdmin, and Redis containers
- Open Swagger UI

6. Access the Applications

Once running, you can access:

- **Frontend (Angular)**: [http://localhost:4200](http://localhost:4200)
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

This project supports two production deployment options:

1. **Direct Docker Deployment** - Run containers directly with exposed ports (Quick Start)
2. **Nginx Reverse Proxy** - Use an external nginx server for standard web ports (Recommended for internet-facing deployments)

For detailed production setup instructions, see the [Production Setup Guide](PROD.md)

## Contributions

We welcome and appreciate contributions of all kinds — whether it's developing _new features, writing integration tests, improving documentation, finding security vulnerabilities or anything else that helps the project grow_.

As a token of appreciation, the **NERBA Association offers a formal letter of participation to contributors**. This letter outlines the specific tasks or areas you contributed to and can serve as evidence of your skills and involvement in open-source projects — useful for resumes, job applications, or professional portfolios.

If you're interested in contributing, please feel free to open a pull request or reach out for guidance on where help is needed.

## ToDo's

- [ ] Add new Integration tests project to the Backend;
- [ ] Document, with comments, critical methods, classes and interfaces;
- [ ] Optimize Services Queries;
- [ ] Refactor Authorization and Authentication logic to apply additional security;
- [ ] Add volumes presistente storage and automated backup logic, for production;
- [ ] User Instructions Manual.

## Code of Conduct

This project follows the Contributor Covenant v2.1

## Licencing

Apache License 2.0
