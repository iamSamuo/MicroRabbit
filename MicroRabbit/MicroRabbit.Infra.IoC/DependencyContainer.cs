using MicroRabbit.Banking.Application.Interfaces;
using MicroRabbit.Banking.Application.Services;
using MicroRabbit.Banking.Data.Context;
using MicroRabbit.Banking.Data.Repository;
using MicroRabbit.Banking.Domain.Interfaces;
using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Infra.Bus;
using Microsoft.Extensions.DependencyInjection;

namespace MicroRabbit.Infra.IoC
{
    public class DependencyContainer
    {
        public static void RegisterServices(IServiceCollection services)
        {
            // Here you would register your services with the dependency injection container.
            // For example, if you're using Microsoft.Extensions.DependencyInjection:
            // var services = new ServiceCollection();
            // services.AddTransient<IMyService, MyService>();
            // ... and so on for other services and repositories.

            //Domain Bus
            services.AddTransient<IEventBus, RabbitMQBus>();

            // application services
            services.AddTransient<IAccountService, AccountService>();

            // data
            services.AddTransient<IAccountRepository, AccountRepository>();
            services.AddTransient<BankingDbContext>();

        }
    }
}
