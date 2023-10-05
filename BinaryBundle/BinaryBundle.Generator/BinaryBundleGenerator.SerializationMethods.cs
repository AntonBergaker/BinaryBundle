using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections.Immutable;
using System.Linq;

namespace BinaryBundle.Generator;

partial class BinaryBundleGenerator {
	public const string TypeExtensionSerializationName = "BinaryBundle.BundleSerializeTypeExtensionAttribute";
	public const string TypeExtensionDeserializationName = "BinaryBundle.BundleDeserializeTypeExtensionAttribute";

	private static bool SerializationMethodsPredicate(SyntaxNode syntaxNode, CancellationToken token) {
		return (syntaxNode is MethodDeclarationSyntax);
	}

	private static SerializationMethod? SerializationMethodsTransform(GeneratorAttributeSyntaxContext context, CancellationToken token, SerializationMethodType type) {
		var methodTypeSymbol = context.TargetSymbol as IMethodSymbol;
		if (methodTypeSymbol == null) {
			return null;
		}

		string typeName;

		if (type == SerializationMethodType.Serialization) {
			typeName = methodTypeSymbol.Parameters[1].Type.ToString();
			type = SerializationMethodType.Serialization;

        } else {
			typeName = methodTypeSymbol.ReturnType.ToString();
            type = SerializationMethodType.Deserialization;
        }

		string methodName = $"{methodTypeSymbol.ContainingSymbol}.{methodTypeSymbol.Name}";

		return new SerializationMethod(typeName, methodName);
    }

    private static Dictionary<string, SerializationMethods> SerializationMethodsCombine(
        (ImmutableArray<SerializationMethod> serializations, ImmutableArray<SerializationMethod> deserializations) methods,
        CancellationToken token) {
        var serializations = methods.serializations.ToDictionary(x => x.TypeName);

        var result = new Dictionary<string, SerializationMethods>();
        foreach (var deserializationMethod in methods.deserializations) {
            if (serializations.TryGetValue(deserializationMethod.TypeName, out var serializationMethod) == false) {
                continue;
            }
            result.Add(serializationMethod.TypeName, new(serializationMethod.MethodName, deserializationMethod.MethodName));
        }

        return result;
    }

    enum SerializationMethodType {
        Serialization,
        Deserialization,
    }

    private class SerializationMethod : IEquatable<SerializationMethod?> {
        public readonly string TypeName;
        public readonly string MethodName;
        public SerializationMethod(string typeName, string methodName) {
            TypeName = typeName;
            MethodName = methodName;
        }

        public bool Equals(SerializationMethod? other) {
            return other is not null &&
                   TypeName == other.TypeName &&
                   MethodName == other.MethodName;
        }
    }

}
