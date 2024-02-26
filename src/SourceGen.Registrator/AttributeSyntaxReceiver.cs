using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceGen.Registrator.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SourceGen.Registrator;
public class AttributeSyntaxReceiver<TAttribute> : ISyntaxReceiver
       where TAttribute : Attribute
{
    public IList<ClassDeclarationSyntax> Classes { get; } = new List<ClassDeclarationSyntax>();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax &&
            classDeclarationSyntax.AttributeLists.Count > 0 &&
            classDeclarationSyntax.AttributeLists
                .Any(al => al.Attributes
                    .Any(a => a.Name.ToString().EnsureEndsWith("Attribute").Equals(typeof(TAttribute).Name))))
        {
            Classes.Add(classDeclarationSyntax);
        }
    }
}

public class AttributesSyntaxReceiver : ISyntaxReceiver
{
    private readonly Type[] attributeTypes;
    private readonly string[] attributeNames;

    public List<AttributeData> Classes { get; } = new();

    public AttributesSyntaxReceiver(params Type[] attributeTypes)
    {
        this.attributeTypes = attributeTypes;
        this.attributeNames = attributeTypes.Select(x => x.Name).ToArray();
    }

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax &&
            classDeclarationSyntax.AttributeLists.Count > 0)
        {
            foreach (var attributeName in attributeNames)
            {
                if (classDeclarationSyntax.AttributeLists
                    .Any(x => x.Attributes
                        .Any(a => a.Name.ToString().EnsureEndsWith("Attribute").Equals(attributeName)
                    )))
                {
                    Classes.Add(new(attributeName, classDeclarationSyntax));
                }
            }
        }
    }

    public struct AttributeData
    {
        public string Name { get; }
        public ClassDeclarationSyntax ClassDeclaration { get; }
        public AttributeData(string name, ClassDeclarationSyntax classDeclaration)
        {
            Name = name;
            ClassDeclaration = classDeclaration;
        }
    }
}
