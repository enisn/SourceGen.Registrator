# SourceGen.Registrator
 An advanced dependency service registering logic with Source Generators. Tens of times faster than reflection registrations.

 ## Features
 - **Source Generators**.
     -  _Easy to debug. No runtime reflection errors._
 - **AOT** compatible:
     - _Everything is written in your project and compiled with your assembly according to your target architecture._
 - ...

## Usage

1. Mark services with attributes to notify source generator.
```cs
[RegisterAsTransient]
class MyService
{
}
```

2. Use generated extension method. Method name will be `Register{YourAssemblyName}Services()`

```
services.RegisterMyProjectServices();
```

### Flexible Customization
Registrator provides flexible registration options unlike alternatives, you can use multiple service types or lifetime by combining attributes:

```csharp
[RegisterAsTransient(typeof(IServiceA), typeof(IServiceB))]
[RegisterAsScoped(typeof(IServiceC))]
class MyServiceA : IServiceA, IServiceB, IServiceC
{
}
```

## Work in progress
This is a simple PoC of source generator service registration. Project will be completed soon...
