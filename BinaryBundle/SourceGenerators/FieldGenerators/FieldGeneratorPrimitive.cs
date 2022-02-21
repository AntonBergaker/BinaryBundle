using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerators.FieldGenerators;

internal class FieldGeneratorPrimitive : FieldGenerator {

    // Get our types
    private static Dictionary<string, string> primitiveTypes = new() {
        { "int", "Int32" },
        { "short", "Int16" },
        { "byte", "Byte" },
        { "bool", "Bool" },
        { "string", "String" },
        { "long", "Int64" },
        { "float", "Float" },
        { "double", "Double" },
    };

    public override bool TryMatch(FieldDeclarationSyntax field, FieldContext context, out TypeMethods? result) {
        if (primitiveTypes.TryGetValue(GetFieldType(field), out string methodName) == false) {
            result = null;
            return false;
        }

        var fieldName = GetFieldName(field);

        string serialize = $"writer.Write{methodName}(this.{fieldName});";
        string deserialize = $"this.{fieldName} = reader.Read{methodName}();";

        result = new(serialize, deserialize);
        return true;
    }
}
