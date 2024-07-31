using SourceGen.Registrator;

namespace MyServiceConsoleApp.Others;

public interface IModuleA
{

}

public class ModuleA : IModuleA, ISingletonDependency
{

}

public class ExtendedModuleA : ModuleA
{

}

public interface IHelloService
{
    void SayHello();
}

[RegisterAsSingleton(typeof(IHelloService))]
public class ConsoleHelloService : IHelloService
{
    public void SayHello()
    {
        Console.WriteLine("Hello, World!");
    }
}

public interface IServiceA
{
}

[RegisterAsSingleton(typeof(IServiceA))]
public class ServiceAImp : IServiceA
{

}

[RegisterAsSingleton]
public class ServiceASingleImp : IServiceA
{

}

public interface IServiceB
{
}

[RegisterAsTransient(typeof(ServiveBImp), typeof(IServiceB), typeof(IServiceA))]
[RegisterAsSingleton(typeof(IServiceB), typeof(IServiceA))]
public class ServiveBImp : IServiceB, IServiceA
{

}