var builder = DistributedApplication.CreateBuilder(args);

// postgres + pg admin + volume
var db = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin();

// redis + insight + volume
var redis = builder.AddRedis("redis")
    .WithDataVolume()
    .WithRedisInsight();

//  asp.net rest api
var apiService = builder.AddProject<Projects.NERBABO_ApiService>("api")
    .WaitFor(db)
    .WaitFor(redis)
    .WithReference(db)
    .WithReference(redis);

// angular proj
builder.AddNpmApp("angular", "../../NERBABO.Frontend")
    .WithReference(apiService)
    .WaitFor(apiService)
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();
