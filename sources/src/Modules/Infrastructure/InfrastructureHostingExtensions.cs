using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class InfrastructureHostingExtensions
    {
        public static IServiceCollection AddUserPostgreStorage(
                this IServiceCollection serviceCollection,
                IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            serviceCollection.AddDbContext<InvoiceDbContext>(builder =>
            {
                builder.UseNpgsql(connectionString);
            });

            return serviceCollection;
        }
    }

}
