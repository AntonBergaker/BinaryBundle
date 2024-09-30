using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace BinaryBundle.Generator.TypeGenerators;

internal class TypeGeneratorEnum : TypeGenerator<TypeGeneratorEnum.EnumTypeData> {
    internal record EnumTypeData(string FieldName, string MethodName, string EnumName, string UnderlyingType) : FieldTypeData(FieldName);

    private readonly TypeGeneratorCollection _generators;

    public TypeGeneratorEnum(TypeGeneratorCollection generators) {
        this._generators = generators;
    }


    public override bool TryGetFieldData(CurrentFieldData currentField, FieldDataContext context, out EnumTypeData? result) {
        var type = currentField.Type;
        if (type.TypeKind != TypeKind.Enum) {
            result = null;
            return false;
        }

        var namedType = type as INamedTypeSymbol;
        if (namedType == null || namedType.EnumUnderlyingType == null) {
            result = null;
            return false;
        }

        if (currentField.IsReadOnly) {
            result = null;
            return false;
        }

        _generators.TryGetFieldData(new(namedType.EnumUnderlyingType, "not used, so please don't emit me", 0, false), context, out var generatorResult) ;
        var primitiveResult = generatorResult as TypeGeneratorPrimitive.PrimitiveTypeData;
        // Should never happen, unless C# introduces a new primitive type
        if (primitiveResult == null) {
            result = null;
            return false;
        }
        

        result = new(currentField.FieldName, primitiveResult.MethodName, type.ToDisplayString(), namedType.EnumUnderlyingType.ToDisplayString());
        return true;
    }

    public override SerializationMethods EmitMethods(EnumTypeData typeData, CurrentEmitData emitData, EmitContext context) {

        string serialize = $"writer.Write{typeData.MethodName}(({typeData.UnderlyingType}){typeData.FieldName});";
        string deserialize = $"{typeData.FieldName} = ({typeData.EnumName})reader.Read{typeData.MethodName}();";

        return new(deserialize, serialize, deserialize);

    }
}
