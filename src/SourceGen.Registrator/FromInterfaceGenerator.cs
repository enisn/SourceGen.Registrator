using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceGen.Registrator.Extensions;

namespace SourceGen.Registrator;

[Generator]
public class FromInterfaceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new InterfaceSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not InterfaceSyntaxReceiver receiver)
        {
            return;
        }

        var @namespace = context.Compilation.AssemblyName ?? "SourceGen.Registrator";
        var safeAssemblyName = @namespace.Replace(".", string.Empty);
        var builder = new StringBuilder();
        builder.AppendLine("using System;");
        builder.AppendLine("using SourceGen.Registrator;");
        builder.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        builder.AppendLine();
        builder.AppendLine($"namespace {@namespace};");
        builder.AppendLine($"public static class {safeAssemblyName}InterfaceRegistrator");
        builder.AppendLine("{");
        builder.AppendLine($"    public static IServiceCollection Register{safeAssemblyName}ServicesFromInterface(this IServiceCollection services)");
        builder.AppendLine("    {");
        foreach (var item in receiver.LifeTimeRegisterContextPairs)
        {
            if (item.ServiceTypeSyntax == null)
            {
                builder.AppendLine($"        services.Add{item.Lifetime}<{GetFullyQualifiedTypeName(context, item.ImplementationSyntax)}>();");
                continue;
            }

            builder.AppendLine($"        services.Add{item.Lifetime}<{GetFullyQualifiedTypeName(context, item.ServiceTypeSyntax) },{GetFullyQualifiedTypeName(context, item.ImplementationSyntax)}>();");
        }
        builder.AppendLine("        return services;");
        builder.AppendLine("    }");
        builder.AppendLine("}");
        context.AddSource($"{safeAssemblyName}InterfaceRegistrator.g.cs", builder.ToString());
    }

    private static string GetFullyQualifiedTypeName(GeneratorExecutionContext context, TypeSyntax typeSyntax)
    {
        var semanticModel = context.Compilation.GetSemanticModel(typeSyntax.SyntaxTree);
        var typeSymbol = semanticModel.GetSymbolInfo(typeSyntax).Symbol as INamedTypeSymbol;
        var ns = typeSymbol.ContainingNamespace;

        if (ns.IsGlobalNamespace)
        {
            return typeSymbol.Name;
        }

        return $"{ns}.{typeSymbol.Name}";
    }

    private static string GetFullyQualifiedTypeName(GeneratorExecutionContext context, ClassDeclarationSyntax classDeclarationSyntax)
    {
        var semanticModel = context.Compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);
        var typeSymbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax);
        var ns = typeSymbol.ContainingNamespace;

        if (ns.IsGlobalNamespace)
        {
            return typeSymbol.Name;
        }

        return $"{ns}.{typeSymbol.Name}";
    }
}

internal class InterfaceSyntaxReceiver : ISyntaxReceiver
{
    public List<RegisterContext> LifeTimeRegisterContextPairs { get; } = new();
    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        // Find the classes that implements itself or implementation inherited the interface ISingletonDependency, ITransientDependency, IScopedDependency
        if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax)
        {
            var interfaceName = classDeclarationSyntax.BaseList?.Types
                .Select(x => x.Type)
                .OfType<IdentifierNameSyntax>()
                .FirstOrDefault(x => x.Identifier.Text == "ISingletonDependency" || x.Identifier.Text == "ITransientDependency" || x.Identifier.Text == "IScopedDependency")?.Identifier.Text;

            if (interfaceName is not null)
            {
                var lifeTime = interfaceName switch
                {
                    "ISingletonDependency" => "Singleton",
                    "ITransientDependency" => "Transient",
                    "IScopedDependency" => "Scoped",
                    _ => "Transient"
                };

                var implementationTypeDeclarationSyntax = classDeclarationSyntax;

                var servicteTypes = classDeclarationSyntax.BaseList?.Types
                    .Select(x => x.Type)
                    .OfType<IdentifierNameSyntax>()
                    .Where(x => x.Identifier.Text != interfaceName)
                    .ToArray();

                // Register self always
                LifeTimeRegisterContextPairs.Add(new RegisterContext(lifeTime, null, implementationTypeDeclarationSyntax));

                foreach (var serviceType in servicteTypes)
                {
                    var serviceTypeName = serviceType.Identifier.Text;
                    if (serviceTypeName == "ISingletonDependency" || serviceTypeName == "ITransientDependency" || serviceTypeName == "IScopedDependency")
                    {
                        continue;
                    }

                    LifeTimeRegisterContextPairs.Add(new RegisterContext(lifeTime, serviceType, implementationTypeDeclarationSyntax));
                }
            }
        }
    }
}

public class RegisterContext
{
    public RegisterContext(string lifetime, TypeSyntax serviceTypeSyntax, ClassDeclarationSyntax implementationSyntax)
    {
        Lifetime = lifetime;
        ServiceTypeSyntax = serviceTypeSyntax;
        ImplementationSyntax = implementationSyntax;
    }

    public string Lifetime { get; set; } = "Transient";

    public TypeSyntax? ServiceTypeSyntax { get; set; }

    public ClassDeclarationSyntax ImplementationSyntax { get; set; }
}