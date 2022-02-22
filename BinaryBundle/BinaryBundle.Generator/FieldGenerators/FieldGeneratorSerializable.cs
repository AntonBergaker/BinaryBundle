using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BinaryBundle.Generator.FieldGenerators;

internal class FieldGeneratorSerializable : FieldGenerator {
    public override bool TryMatch(FieldDeclarationSyntax field, FieldContext context, out TypeMethods? result) {
        var type = context.Model.GetTypeInfo(field.Declaration.Type).Type;

        if (type is not { TypeKind: TypeKind.Class or TypeKind.Struct or TypeKind.Interface }) {
            result = null;
            return false;
        }

        string typeName = BinaryBundleGenerator.InterfaceName;

        if (Utils.TypeImplements(type, typeName) == false && 
            context.ExtraSerializableClasses.Contains(type.ToString()) == false) {
            result = null;
            return false;
        }

        var fieldName = GetFieldName(field);

        string serialize = $"this.{fieldName}.Serialize(writer);";
        string deserialize = $"this.{fieldName}.Deserialize(reader);";

        result = new TypeMethods(serialize, deserialize);
        return true;
    }
}