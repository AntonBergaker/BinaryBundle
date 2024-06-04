using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace BinaryBundle.Generator.TypeGenerators;

internal class TypeGeneratorPrimitive : TypeGenerator<TypeGeneratorPrimitive.PrimitiveTypeData> {
    internal record PrimitiveTypeData(string MethodName, string FieldName) : FieldTypeData(FieldName);

    // Get our types
    private static Dictionary<string, string> _primitiveTypes = new() {
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
        { "string?", "String" },
    };

    public override SerializationMethods EmitMethods(PrimitiveTypeData typeData, EmitContext context) {

        string serialize = $"writer.Write{typeData.MethodName}({typeData.FieldName});";
        string deserialize = $"{typeData.FieldName} = reader.Read{typeData.MethodName}();";

        return new(serialize, deserialize);
    }

    public override bool TryGetFieldData(CurrentFieldData currentField, FieldDataContext context, out PrimitiveTypeData? result) {
        if (_primitiveTypes.TryGetValue(currentField.Type.ToDisplayString(), out string methodName) == false) {
            result = null;
            return false;
        }

        if (currentField.IsReadOnly) {
            result = null;
            return false;
        }

        result = new PrimitiveTypeData(methodName, currentField.FieldName);
        return true;
    }

}
