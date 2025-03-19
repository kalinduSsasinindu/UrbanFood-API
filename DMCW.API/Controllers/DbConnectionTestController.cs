using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Text;

namespace DMCW.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DbConnectionTestController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public DbConnectionTestController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private async Task<string> TestConnectivity(string host)
        {
            try
            {
                using (var ping = new Ping())
                {
                    var reply = await ping.SendPingAsync(host, 3000);
                    return $"Ping to {host}: {reply.Status}, Time: {reply.RoundtripTime}ms";
                }
            }
            catch (Exception ex)
            {
                return $"Ping to {host} failed: {ex.Message}";
            }
        }

        [HttpGet("mongodb")]
        public async Task<IActionResult> TestMongoDbConnection()
        {
            try
            {
                // Get connection details from configuration
                var connectionString = _configuration.GetConnectionString("MongoDB");
                var databaseName = _configuration.GetConnectionString("DatabaseName") ?? "dmcw-dev";

                if (string.IsNullOrEmpty(connectionString))
                {
                    return BadRequest(new { success = false, message = "MongoDB connection string is not configured" });
                }

                // Test network connectivity first
                var mongoHost = "dmcw-dev.72zay.mongodb.net";
                var connectivityResult = await TestConnectivity(mongoHost);

                // Configure MongoDB client settings
                var settings = MongoClientSettings.FromConnectionString(connectionString);
                settings.ServerApi = new ServerApi(ServerApiVersion.V1);
                settings.ConnectTimeout = TimeSpan.FromSeconds(10);
                settings.ServerSelectionTimeout = TimeSpan.FromSeconds(10);
                settings.DirectConnection = false;
                settings.SslSettings = new SslSettings
                {
                    EnabledSslProtocols = SslProtocols.Tls12
                };

                // Attempt to connect using the configured settings
                var client = new MongoClient(settings);

                // Try listing databases first (lighter operation)
                var databaseList = await client.ListDatabaseNamesAsync();
                var dbNames = await databaseList.ToListAsync();

                // If that succeeds, try to access the specific database
                var database = client.GetDatabase(databaseName);
                var pingCommand = new MongoDB.Bson.BsonDocument("ping", 1);
                await database.RunCommandAsync<dynamic>(pingCommand);

                return Ok(new
                {
                    success = true,
                    message = "Successfully connected to MongoDB Atlas",
                    database = databaseName,
                    availableDatabases = dbNames,
                    diagnostics = new
                    {
                        connectivityTest = connectivityResult,
                        server = mongoHost,
                        sslProtocol = "TLS 1.2",
                        connectionTimeout = $"{settings.ConnectTimeout.TotalSeconds} seconds",
                        serverSelectionTimeout = $"{settings.ServerSelectionTimeout.TotalSeconds} seconds"
                    }
                });
            }
            catch (TimeoutException tex)
            {
                var mongoHost = "dmcw-dev.72zay.mongodb.net";
                var connectivityResult = await TestConnectivity(mongoHost);

                return StatusCode(500, new
                {
                    success = false,
                    message = "Connection timeout",
                    error = tex.Message,
                    diagnostics = new
                    {
                        connectivityTest = connectivityResult,
                        possibleIssues = new[]
                        {
                            "IP not whitelisted in MongoDB Atlas",
                            "Firewall blocking connection",
                            "VPN or proxy interference",
                            "DNS resolution issues"
                        }
                    },
                    recommendations = new[]
                    {
                        "Add your IP to MongoDB Atlas Network Access",
                        "Check your firewall settings for port 27017",
                        "Try connecting from a different network",
                        "Verify the connection string is correct"
                    }
                });
            }
            catch (MongoAuthenticationException authEx)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Authentication failed",
                    error = authEx.Message,
                    recommendations = new[]
                    {
                        "Verify username and password in connection string",
                        "Check if the user has appropriate permissions",
                        "Ensure the user is associated with the correct database"
                    }
                });
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException?.InnerException?.Message ??
                                 ex.InnerException?.Message ??
                                 ex.Message;

                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to connect to MongoDB",
                    error = errorMessage,
                    details = "Check your network settings and MongoDB Atlas configuration",
                    recommendations = new[]
                    {
                        "Verify your MongoDB Atlas cluster is running",
                        "Check if your IP is whitelisted in MongoDB Atlas",
                        "Ensure your network allows outbound connections to port 27017",
                        "Verify SSL/TLS settings in your network"
                    }
                });
            }
        }
    }
}