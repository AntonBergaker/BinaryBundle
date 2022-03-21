using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BinaryBundle.Generator.FieldGenerators;

internal class FieldGeneratorSerializable : FieldGenerator {
    public override bool TryMatch(ITypeSymbol type, string fieldName, int depth, bool isAccessor, FieldContext context, out TypeMethods? result) {
        if (type is not { TypeKind: TypeKind.Class or TypeKind.Struct or TypeKind.Interface }) {
            result = null;
            return false;
        }

        string typeName = context.InterfaceName;

        if (Utils.TypeImplements(type, typeName) == false && 
            context.ExtraSerializableClasses.Contains(type.ToString()) == false) {
            result = null;
            return false;
        }

        var serialize = (CodeBuilder code) => code.AddLine($"{fieldName}.Serialize(writer);");
        var deserialize = (CodeBuilder code) => code.AddLine($"{fieldName}.Deserialize(reader);");

        if (isAccessor && type.IsValueType) {
            string tempVariable = GetTempVariable(depth);
            deserialize = (code) => {
                code.StartBlock();
                code.AddLine($"{type} {tempVariable} = default;");
                code.AddLine($"{tempVariable}.Deserialize(reader);");
                code.AddLine($"{fieldName} = {tempVariable};");
                code.EndBlock();
            };
        }

        result = new TypeMethods(serialize, deserialize);
        return true;
    }
}