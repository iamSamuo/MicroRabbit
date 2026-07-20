using MediatR;
using MicroRabbit.Infra.IoC;
using MicroRabbit.Transfer.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace MicroRabbit.Transfer.Api;
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {

        services.AddDbContext<TransferDbContext>(options =>
        {
            options.UseSqlServer(Configuration.GetConnectionString("TransferDbConnection"));
        });

        services.AddControllers();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Banking Microservice",
                Version = "v1"
            });
        });

        services.AddMediatR(typeof(Startup));

        RegisterServices(services);
    }

    private void RegisterServices(IServiceCollection services)
    {
        DependencyContainer.RegisterServices(services);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();

        }
        else
        {
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Banking Microserveice V1"));

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });      
    }
}