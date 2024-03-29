using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceGen.Registrator.Extensions;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SourceGen.Registrator;

[Generator]
public class RegistratorGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new AttributesSyntaxReceiver(
            typeof(RegisterAsSingletonAttribute),
            typeof(RegisterAsScopedAttribute),
            typeof(RegisterAsTransientAttribute)));
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not AttributesSyntaxReceiver receiver)
        {
            return;
        }

        //if (!Debugger.IsAttached)
        //{
        //    Debugger.Launch();
        //}

        var @namespace = context.Compilation.AssemblyName ?? "SourceGen.Registrator";
        var safeAssemblyName = @namespace.Replace(".", string.Empty);
        var builder = new StringBuilder();
        builder.AppendLine("using System;");
        builder.AppendLine("using SourceGen.Registrator;");
        builder.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        builder.AppendLine();
        builder.AppendLine($"namespace {@namespace};");
        builder.AppendLine($"public static class {safeAssemblyName}Registrator");
        builder.AppendLine("{");
        builder.AppendLine($"    public static IServiceCollection Register{safeAssemblyName}Services(this IServiceCollection services)");
        builder.AppendLine("    {");
        foreach (var item in receiver.Classes)
        {
            var classDeclaration = item.ClassDeclaration;

            var attribute = classDeclaration.AttributeLists
                .SelectMany(x => x.Attributes)
                .FirstOrDefault(x => x.Name.ToString().EnsureEndsWith("Attribute").Equals(item.Name));

            var serviceTypeList = attribute.ArgumentList?.Arguments
                .Select(x => x.Expression)
                .OfType<TypeOfExpressionSyntax>()
                .Select(x => x.Type)
                .Select(x => GetFullyQualifiedTypeName(context, x))
                .ToList();

            serviceTypeList ??= new();

            if (serviceTypeList.Count == 0)
            {
                serviceTypeList.Add(GetFullyQualifiedTypeName(context, classDeclaration));
            }

            var className = GetFullyQualifiedTypeName(context, classDeclaration);
            var lifetime = GetLifetimeByAttributeName(item.Name);

            foreach (var serviceName in serviceTypeList)
            {
                if (serviceName == className)
                {
                    builder.AppendLine($"        services.AddSingleton<{serviceName}>();");
                }
                else
                {
                    builder.AppendLine($"        services.Add{lifetime}<{serviceName}, {className}>();");
                }
            }
        }
        builder.AppendLine("        return services;");
        builder.AppendLine("    }");
        builder.AppendLine("}");

        context.AddSource($"{safeAssemblyName}Registrator.g.cs", builder.ToString());
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

    private static string GetLifetimeByAttributeName(string attributeName)
    {
        return attributeName switch
        {
            "RegisterAsSingletonAttribute" => "Singleton",
            "RegisterAsScopedAttribute" => "Scoped",
            "RegisterAsTransientAttribute" => "Transient",
            _ => throw new NotSupportedException()
        };
    }
}
