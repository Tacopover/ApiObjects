using Microsoft.Extensions.DependencyInjection;


namespace CollabAPIMEP
{
    public static class ServiceConfigurator
    {

        public static ServiceProvider ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();

            // Register the APS service
            serviceCollection.AddSingleton<APS>(provider => new APS("680LcYsfUK8oOJv9pr0Y0KIJNKgzowhF1AieLR0F8KVr0reC", "SGgDhxDIIAOTAdgkVV6lm09B4G4Nw0KAwJAc4lBiRIQjETH7McHy8BYORmrzipNE", "http://www.google.com"));

            // Register other services as needed

            return serviceCollection.BuildServiceProvider();
        }
    }
}