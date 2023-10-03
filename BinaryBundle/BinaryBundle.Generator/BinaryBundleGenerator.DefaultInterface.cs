using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

namespace BinaryBundle.Generator;
public partial class BinaryBundleGenerator {

    public const string DefaultInterfaceAttributeName = "BinaryBundle.BundleDefaultInterfaceAttribute";
    public const string DefaultInterfaceBaseName = "BinaryBundle.IBundleSerializableBase<TWriter, TReader>";

    private bool DefaultInterfacePredicate(SyntaxNode syntaxNode, CancellationToken token) {
        return (syntaxNode is InterfaceDeclarationSyntax);
    }

    private DefaultInterfaceDeclaration? DefaultInterfaceTransform(GeneratorSyntaxContext context, CancellationToken token) {
        SemanticModel model = context.SemanticModel;

        var classTypeSymbol = model.GetDeclaredSymbol(context.Node) as INamedTypeSymbol;
        if (classTypeSymbol == null) {
            return null;
        }

        AttributeData? attributeInterface = null;
        var attributes = classTypeSymbol.GetAttributes();
        foreach (var attribute in attributes) {
            if (attribute.AttributeClass?.ToString() == DefaultInterfaceAttributeName) {
                attributeInterface = attribute;
                break;
            }
        }

        if (attributeInterface == null) {
            return null;
        }

        if (attributeInterface.ConstructorArguments.Length > 0) {
            var argument = attributeInterface.ConstructorArguments[0];
            var typeSymbol = argument.Value as INamedTypeSymbol;
            if (typeSymbol != null) {
                classTypeSymbol = typeSymbol;
            }
        }

        foreach (INamedTypeSymbol implementedInterface in classTypeSymbol.Interfaces) {
            if (implementedInterface.OriginalDefinition.ToString() != DefaultInterfaceBaseName) {
                continue;
            }

            var interfaceName = classTypeSymbol.ToString();
            var types = implementedInterface.TypeArguments;
            var writerName = types[0].ToString();
            var readerName = types[1].ToString();

            return new DefaultInterfaceDeclaration(interfaceName, writerName, readerName);
        }

        return null;
    }

    private DefaultInterfaceDeclaration DefaultInterfaceCollect(ImmutableArray<DefaultInterfaceDeclaration> interfaces, CancellationToken _) {
        var first = interfaces.FirstOrDefault();
        if (first != null) {
            return first;
        }
        return new DefaultInterfaceDeclaration("BinaryBundle.IBundleSerializable", "BinaryBundle.BufferWriter", "BinaryBundle.BufferReader");
    }

    private class DefaultInterfaceDeclaration {
        public readonly string Name;
        public readonly string WriterName;
        public readonly string ReaderName;

        public DefaultInterfaceDeclaration(string name, string writerName, string readerName) {
            Name = name;
            WriterName = writerName;
            ReaderName = readerName;
        }
    }
}
