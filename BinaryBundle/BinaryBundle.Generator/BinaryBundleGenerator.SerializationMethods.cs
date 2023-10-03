using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace BinaryBundle.Generator;

partial class BinaryBundleGenerator {
	public const string TypeExtensionSerializationName = "BinaryBundle.BundleSerializeTypeExtensionAttribute";
	public const string TypeExtensionDeserializationName = "BinaryBundle.BundleDeserializeTypeExtensionAttribute";

	private bool SerializationMethodsPredicate(SyntaxNode syntaxNode, CancellationToken token) {
		return (syntaxNode is MethodDeclarationSyntax);
	}

	private SerializationMethod? SerializationMethodsTransform(GeneratorSyntaxContext context, CancellationToken token) {
		SemanticModel model = context.SemanticModel;
		var method = (MethodDeclarationSyntax)context.Node;
		var methodTypeSymbol = model.GetDeclaredSymbol(method) as IMethodSymbol;
		if (methodTypeSymbol == null) {
			return null;
		}

		bool isSerializationTypeExtension =
			Utils.HasAttribute(methodTypeSymbol, TypeExtensionSerializationName);
		bool isDeserializeTypeExtension =
			Utils.HasAttribute(methodTypeSymbol, TypeExtensionDeserializationName);

		if (isSerializationTypeExtension == false &&
			isDeserializeTypeExtension == false) {
			return null;
		}

		string typeName;
		SerializationMethodType type;

		if (isSerializationTypeExtension) {
			typeName = methodTypeSymbol.Parameters[1].Type.ToString();
			type = SerializationMethodType.Serialization;

        } else {
			typeName = methodTypeSymbol.ReturnType.ToString();
            type = SerializationMethodType.Deserialization;
        }

		string methodName = $"{methodTypeSymbol.ContainingSymbol}.{methodTypeSymbol.Name}";

		return new SerializationMethod(type, typeName, methodName);
    }

    enum SerializationMethodType {
        Serialization,
        Deserialization,
    }

    private class SerializationMethod {
        public readonly SerializationMethodType Type;
        public readonly string TypeName;
        public readonly string MethodName;
        public SerializationMethod(SerializationMethodType type, string typeName, string methodName) {
            Type = type;
            TypeName = typeName;
            MethodName = methodName;
        }
    }
}
