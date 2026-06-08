using Gateway.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddGraphQL().AddTypes();

builder.Services.AddHttpClient<HttpIotClient>();


var app = builder.Build();

app.MapGraphQL();

app.RunWithGraphQLCommands(args);
