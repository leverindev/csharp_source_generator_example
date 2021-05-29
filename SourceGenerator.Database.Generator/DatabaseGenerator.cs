using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceGeneratorExample.DatabaseAttributes;

namespace SourceGenerator.Database.Generator
{
    [Generator]
    public class DatabaseGenerator : ISourceGenerator
    {
        private static readonly Type EntityAttributeType = typeof(EntityAttribute);
        private static readonly Type KeyAttributeType = typeof(KeyAttribute);

        private const string SourceTemplate = @"
using System;
using System.Collections.Generic;
using System.Linq;

namespace SourceGeneratorExample.Database
{
    public partial class DbContext
    {
        private readonly List<###CLASS###> _###class###s = new List<###CLASS###>();

        public ###CLASS### Get###Class###(###key_type### ###key###)
        {
            return _###class###s.FirstOrDefault(x => x.###KEY### == ###key###);
        }

        public IEnumerable<###CLASS###> GetAll###Class###s()
        {
            return _###class###s;
        }

        public void Add(###CLASS### entity)
        {
            _###class###s.Add(entity);
        }

        public void Remove(###CLASS### entity)
        {
            _###class###s.Remove(entity);
        }
    }
}
";

        public void Initialize(GeneratorInitializationContext context)
        {
            //if (!Debugger.IsAttached)
            //{
            //    Debugger.Launch();
            //}
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var compilation = context.Compilation;

            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                var semanticModel = compilation.GetSemanticModel(syntaxTree);

                var allClasses = syntaxTree
                    .GetRoot().DescendantNodesAndSelf()
                    .Where(x => x.IsKind(SyntaxKind.ClassDeclaration))
                    .OfType<ClassDeclarationSyntax>();

                foreach (var classDeclarationSyntax in allClasses)
                {
                    if (!classDeclarationSyntax.DescendantNodes().OfType<AttributeSyntax>().Any())
                    {
                        continue;
                    }

                    if (!HasAttribute(classDeclarationSyntax, semanticModel, EntityAttributeType))
                    {
                        continue;
                    }

                    var classSymbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax);
                    var keyProperty = classSymbol.GetMembers().OfType<IPropertySymbol>().First(p => HasAttribute(p, KeyAttributeType));

                    context.AddSource($"DbContext.{classSymbol.Name}.Generated.cs", BuildSource(classSymbol, keyProperty));
                }
            }
        }

        private string BuildSource(INamedTypeSymbol classSymbol, IPropertySymbol propertySymbol)
        {
            return SourceTemplate
                .Replace("###CLASS###", classSymbol.ToString())
                .Replace("###Class###", classSymbol.Name)
                .Replace("###class###", classSymbol.Name.ToLower())
                .Replace("###KEY###", propertySymbol.Name)
                .Replace("###key###", propertySymbol.Name.ToLower())
                .Replace("###key_type###", propertySymbol.Type.Name);
        }

        private bool HasAttribute(SyntaxNode syntaxNode, SemanticModel semanticModel, Type attributeType)
        {
            var nodes = syntaxNode
                .DescendantNodes()
                .OfType<AttributeSyntax>()
                .FirstOrDefault(a => a
                    .DescendantTokens()
                    .Any(dt =>
                        dt.IsKind(SyntaxKind.IdentifierToken) &&
                        semanticModel.GetTypeInfo(dt.Parent).Type.Name == attributeType.Name))
                ?.DescendantTokens()
                ?.Where(dt => dt.IsKind(SyntaxKind.IdentifierToken))
                ?.ToList();

            return nodes is { Count: > 0 };
        }

        private bool HasAttribute(IPropertySymbol propertySymbol, Type attributeType)
        {
            return propertySymbol
                .GetAttributes()
                .Any(a => a?.AttributeClass?.Name == attributeType.Name);
        }
    }
}
