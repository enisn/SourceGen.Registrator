# SourceGen.Registrator
 An advanced dependency service registering logic with Source Generators. Tens of times faster than reflection registrations.

 ## Features
 - Source Generators. Easy to debug. No runtime reflection errors.
 - **AOT** compatible. Everything is written in your project and compiled with your assembly according to your target architecture.
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

## Work in progress
This is a simple PoC of source generator service registration. Project will be completed soon...
