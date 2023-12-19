namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddWebServices(this IServiceCollection services, IWebHostEnvironment env)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CheckConsumptionCommand).Assembly));
        services.AddSingleton<IConsumptionAggregateFactory, ConsumptionAggregateFactory>();
        if (env.IsDevelopment())
        {
            services.AddSingleton<IHttpHandler, HttpHandlerMock>();
        }
        else
        {
            services.AddSingleton<IHttpHandler, HttpHandler>();
        }
        services.AddSingleton<HttpClient>();
        services.AddSingleton<IHomeApplianceAggregateFactory, HomeApplianceAggregateFactory>();
        services.AddSingleton<PowerPriceAggregateFactory>();
        services.AddSingleton<IPowerPriceService, PowerPriceService>();
        services.AddHostedService<ConsumptionWorker>();
        services.AddHostedService<PowerPricesWorker>();

        var config = GetConfiguration();
        services.Configure<HttpSettings>(config.GetSection("HttpSettings"));
        services.Configure<ConsumptionSettings>(config.GetSection("ConsumptionSettings"));
        services.Configure<PowerPriceSettings>(config.GetSection("PowerPriceSettings"));
        services.Configure<HomeApplianceSettings>(config.GetSection("HomeApplianceSettings"));

        return services;
    }

    private static IConfiguration GetConfiguration()
    {
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "Config"));
        configurationBuilder.AddJsonFile("appsettings.json");
        configurationBuilder.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true);
        var configuration = configurationBuilder.Build();
        return configuration;
    }
}