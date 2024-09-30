
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace BinaryBundle.Generator.TypeGenerators;

class TypeGeneratorNullable : TypeGenerator<TypeGeneratorNullable.NullableTypeData> {
    internal record NullableTypeData(string FieldName, FieldTypeData InnerField) : FieldTypeData(FieldName);

    private readonly TypeGeneratorCollection _generators;

    public TypeGeneratorNullable(TypeGeneratorCollection generators) {
        this._generators = generators;
    }

    public override SerializationMethods EmitMethods(NullableTypeData typeData, CurrentEmitData emitData, EmitContext context) {
        var innerEmitMethods = _generators.EmitMethods(typeData.InnerField, new(emitData.Depth, false), context);
        var tempName = GetTempVariable(emitData.Depth);
        
        var construct = (CodeBuilder code) => {
            code.StartBlock($"if (reader.ReadBool())");
            innerEmitMethods.WriteConstructMethod(code);
            code.EndBlock();
            code.StartBlock("else");
            code.AddLine($"{typeData.FieldName} = null;");
            code.EndBlock();
        };
        var serialize = (CodeBuilder code) => {
            code.StartBlock($"if ({typeData.FieldName} != null)");
            code.AddLine("writer.WriteBool(true);");
            innerEmitMethods.WriteSerializeMethod(code);
            code.EndBlock();
            code.StartBlock("else");
            code.AddLine("writer.WriteBool(false);");
            code.EndBlock();
        };
        var deserialize = (CodeBuilder code) => {
            code.StartBlock($"if (reader.ReadBool())");
            code.StartBlock($"if ({typeData.FieldName} == null)");
            innerEmitMethods.WriteConstructMethod(code);
            code.EndBlock();
            code.StartBlock("else");
            innerEmitMethods.WriteDeserializeMethod(code);
            code.EndBlock(); ;
            code.EndBlock();
            code.StartBlock("else");
            code.AddLine($"{typeData.FieldName} = null;");
            code.EndBlock();
        };

        return new(deserialize, serialize, deserialize);
    }

    public override bool TryGetFieldData(CurrentFieldData currentField, FieldDataContext context, out NullableTypeData? result) {
        if (currentField.Type is not INamedTypeSymbol typeSymbol) {
            result = null;
            return false;
        }
        if (typeSymbol.NullableAnnotation != NullableAnnotation.Annotated) {
            result = null;
            return false;
        }

        if (_generators.TryGetFieldData(
            currentField with { Type = typeSymbol.WithNullableAnnotation(NullableAnnotation.NotAnnotated) },
            context, out var innerResult) == false) {
            result = null;
            return false;
        }

        result = new NullableTypeData(currentField.FieldName, innerResult!);
        return true;
    }
}
