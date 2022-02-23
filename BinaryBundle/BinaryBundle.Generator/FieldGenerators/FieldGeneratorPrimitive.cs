using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BinaryBundle.Generator.FieldGenerators;

internal class FieldGeneratorPrimitive : FieldGenerator {

    // Get our types
    private static Dictionary<string, string> primitiveTypes = new() {
        { "bool", "Bool" },
        { "byte", "Byte" },
        { "sbyte", "SByte"},
        { "char", "Char"},
        { "decimal", "Decimal"},
        { "double", "Double" },
        { "float", "Float" },
        { "int", "Int32" },
        { "uint", "UInt32" },
        { "long", "Int64" },
        { "ulong", "UInt64" },
        { "short", "Int16" },
        { "ushort", "UInt16" },
        { "string", "String" },
    };

    public override bool TryMatch(ITypeSymbol type, string fieldName, FieldContext context, out TypeMethods? result) {
        if (primitiveTypes.TryGetValue(type.ToString(), out string methodName) == false) {
            result = null;
            return false;
        }

        string serialize = $"writer.Write{methodName}(this.{fieldName});";
        string deserialize = $"this.{fieldName} = reader.Read{methodName}();";

        result = new(serialize, deserialize);
        return true;
    }
}
