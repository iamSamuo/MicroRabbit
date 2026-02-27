using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Infra.Bus;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

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


        }
    }
}
