using Consul;
using IApplicationLifetime = Microsoft.Extensions.Hosting.IApplicationLifetime;

namespace ServiceDiscovery.Config
{
    public static class Serviceregistration
    {
       public static IServiceCollection AddConsuConfig(this IServiceCollection services)
        {
            string ConsulAddress = "http://localhost:8500/";

            services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulComfig =>
            {
                consulComfig.Address = new Uri(ConsulAddress);
            }));
            return services;
        }
    
    public static IApplicationBuilder UseConsul(this IApplicationBuilder app)
    {
        var consulClient = app.ApplicationServices.GetRequiredService<IConsulClient>();
        var logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger("AppExtentions");
        var lifetime = app.ApplicationServices.GetRequiredService<IApplicationLifetime>();

        var registration = new AgentServiceRegistration()
        {
            ID = "ServiceRegistrationID13",
            Name = "Service2",
            Address="localhost",
            Port=5175

        };

        logger.LogInformation("Registrating with Consul");
        consulClient.Agent.ServiceDeregister(registration.ID).ConfigureAwait(true);
        consulClient.Agent.ServiceRegister(registration).ConfigureAwait(true);

        lifetime.ApplicationStopping.Register(async () =>
        {
            logger.LogInformation("Unregistering");
            consulClient.Agent.ServiceDeregister(registration.ID).Wait();
        });


        return app;
    }
}
}
