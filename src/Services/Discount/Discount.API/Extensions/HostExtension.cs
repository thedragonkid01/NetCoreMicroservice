using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Discount.API.Extensions
{
    public static class HostExtension
    {
        public static IHost MigrateDatabase<TContext>(this IHost host, int? retry = 0)
        {
            var retryValue = retry.Value;

            using (var scope = host.Services.CreateScope())
            { 
                var service = scope.ServiceProvider;
                var configuration = service.GetRequiredService<IConfiguration>();
                var logger = service.GetRequiredService<ILogger<TContext>>();

                try
                {
                    logger.LogInformation("Database Migration starting...");

                    var connection = new NpgsqlConnection(configuration.GetValue<string>("DatabaseSettings:DiscountDb"));
                    connection.Open();

                    using var command = new NpgsqlCommand
                    {
                        Connection = connection
                    };

                    command.CommandText = "DROP TABLE IF EXISTS Coupon";
                    command.ExecuteNonQuery();

                    command.CommandText = @"CREATE TABLE Coupon (
	                                            Id serial PRIMARY KEY,
	                                            ProductName VARCHAR ( 100 ) UNIQUE NOT NULL,
	                                            Description TEXT NULL,
	                                            Amount int NOT NULL
                                            );";
                    command.ExecuteNonQuery();

                    command.CommandText = @"INSERT INTO Coupon (ProductName, Description, Amount)
                                            VALUES ('IPhone X', 'Phone X Discount',100);";
                    command.ExecuteNonQuery();

                    command.CommandText = @"INSERT INTO Coupon (ProductName, Description, Amount)
                                            VALUES ('Samsung 10', 'amsung 10 Discount',120)";
                    command.ExecuteNonQuery();

                    logger.LogInformation("Database Migration ended...");
                }
                catch (NpgsqlException ex)
                {
                    logger.LogError(ex, "Something went wrong during migration time");

                    if (retryValue < 3)
                    {
                        retryValue++;
                        host.MigrateDatabase<TContext>(retryValue);
                    }
                }
            }

            return host;
        }
    }
}
