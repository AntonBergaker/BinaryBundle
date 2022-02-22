using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BinaryBundle.Generator.FieldGenerators;

internal class FieldGeneratorEnum : FieldGenerator {
    public override bool TryMatch(FieldDeclarationSyntax field, FieldContext context, out TypeMethods? result) {
        var type = context.Model.GetTypeInfo(field.Declaration.Type).Type;

        if (type == null || type.TypeKind != TypeKind.Enum) {
            result = null;
            return false;
        }

        string fieldName = GetFieldName(field);

        string serialize = $"writer.WriteEnum(this.{fieldName});";
        string deserialize = $"this.{fieldName} = reader.ReadEnum<{type}>();";

        result = new TypeMethods(serialize, deserialize);
        return true;
    }
}
