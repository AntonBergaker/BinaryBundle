using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BinaryBundle.Generator;

internal class Utils {

    public static bool TypeImplements(ITypeSymbol type, string typeName) {
        return (type.Name == typeName ||
                (type.AllInterfaces.Any(x => x.Name == typeName)));
    }

    public static bool TypeImplements(TypeInfo typeInfo, string typeName) {
        if (typeInfo.Type == null) {
            return false;
        }
        return TypeImplements(typeInfo.Type, typeName);
    }

    public static bool HasAttribute(ISymbol? type, string fullAttributeName) {
        return type?.GetAttributes().Any(x => x.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == fullAttributeName) ?? false;
    }

    public static bool TypeOrInheritanceHasAttribute(ITypeSymbol type, string fullAttributeName) {
        var baseType = type;
        while (baseType != null) {
            if (HasAttribute(baseType, fullAttributeName)) { 
                return true; 
            }
            baseType = baseType.BaseType;
        }
        return false;
    }

    public static bool IsTypeSerializable(ITypeSymbol symbol, string interfaceName) {
        return TypeImplements(symbol, interfaceName) || TypeOrInheritanceHasAttribute(symbol, BinaryBundleGenerator.BundleAttributeNameWithGlobal);
    }
}
