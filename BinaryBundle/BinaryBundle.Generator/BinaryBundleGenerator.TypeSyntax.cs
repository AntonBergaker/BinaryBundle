using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading;

namespace BinaryBundle.Generator;
public partial class BinaryBundleGenerator {

    public const string BundleAttribute = "BinaryBundle.BinaryBundleAttribute";

    private bool TypeSyntaxPredicate(SyntaxNode syntaxNode, CancellationToken token) {
        return (syntaxNode is ClassDeclarationSyntax or StructDeclarationSyntax);
    }

    private SerializableClass? TypeSyntaxTransform(GeneratorSyntaxContext context, CancellationToken token) {
        var typeDeclaration = (TypeDeclarationSyntax)context.Node;

        SemanticModel model = context.SemanticModel;

        var classTypeSymbol = model.GetDeclaredSymbol(typeDeclaration) as ITypeSymbol;
        if (classTypeSymbol == null) {
            return null;
        }

        if (Utils.HasAttribute(classTypeSymbol, BundleAttribute) == false) {
            return null;
        }

        return new SerializableClass(model, classTypeSymbol, typeDeclaration);
    }

    private class SerializableClass {
        public readonly SemanticModel Model;
        public readonly ITypeSymbol Symbol;
        public readonly TypeDeclarationSyntax Declaration;

        public SerializableClass(SemanticModel model, ITypeSymbol symbol, TypeDeclarationSyntax declaration) {
            Model = model;
            Declaration = declaration;
            Symbol = symbol;
        }
    }
}
