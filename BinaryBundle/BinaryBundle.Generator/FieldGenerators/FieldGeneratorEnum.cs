using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BinaryBundle.Generator.FieldGenerators;

internal class FieldGeneratorEnum : FieldGenerator {
    public override bool TryMatch(ITypeSymbol type, string fieldName, int depth, FieldContext context, out TypeMethods? result) {
        if (type.TypeKind != TypeKind.Enum) {
            result = null;
            return false;
        }

        string serialize = $"writer.WriteEnum({fieldName});";
        string deserialize = $"{fieldName} = reader.ReadEnum<{type}>();";

        result = new TypeMethods(serialize, deserialize);
        return true;
    }
}
