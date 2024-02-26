using Microsoft.Extensions.DependencyInjection;
using SourceGen.Registrator;

namespace MyServiceConsoleApp;

public static class Program
{
    private static void Main(string[] args)
    {
        IServiceCollection services = new ServiceCollection();

        services.RegisterMyServiceConsoleAppServices();

        var serviceProvider = services.BuildServiceProvider();

        var helloService = serviceProvider.GetRequiredService<Others.IHelloService>();

        helloService.SayHello();
    }
}