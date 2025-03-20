using DMCW.API.Extensions;
using FluentValidation.AspNetCore;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);


// Configure services
builder.Services.ConfigureCustomServices(builder.Configuration);
builder.Services.ConfigureSwagger();
builder.Services.ConfigureAuthentication(builder.Configuration);
builder.Services.AddControllers()
    .AddFluentValidation();

// Configure Application Insights
builder.Services.Configure<ApplicationInsightsServiceOptions>(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
});


// Add logging configuration
builder.Logging.AddApplicationInsights();

var app = builder.Build();
app.UseStaticFiles(); // Serve static files from wwwroot




// Configure middleware
app.ConfigureMiddlewares(app.Environment);

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
   // endpoints.MapHub<MainHub>("/commhub");
});

app.Run();
