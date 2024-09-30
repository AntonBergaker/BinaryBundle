using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace BinaryBundle.Generator.TypeGenerators;

internal class TypeGeneratorDictionary : TypeGenerator<TypeGeneratorDictionary.DictionaryTypeData> {
    internal record DictionaryTypeData(string FieldName, LimitData? LimitData, 
        string KeyType, FieldTypeData KeyTypeData, 
        string ValueType, FieldTypeData ValueTypeData) : FieldTypeData(FieldName);

    private readonly TypeGeneratorCollection _generators;

    public TypeGeneratorDictionary(TypeGeneratorCollection generators) {
        this._generators = generators;
    }

    public override bool TryGetFieldData(CurrentFieldData currentField, FieldDataContext context, out DictionaryTypeData? result) {
        if (currentField.Type is not INamedTypeSymbol namedType) {
            result = null;
            return false;
        }

        if (namedType.OriginalDefinition.ToString() != "System.Collections.Generic.Dictionary<TKey, TValue>") {
            result = null;
            return false;
        }

        var depth = currentField.Depth;

        string keyTempVariable = "key" + GetTempVariable(depth);
        ITypeSymbol innerKeyType = namedType.TypeArguments[0];
        if (_generators.TryGetFieldData(new(innerKeyType, keyTempVariable, depth + 1, false), context, out var keyFieldData) == false) {
            result = null;
            return false;
        }


        string valueTempVariable = "value" + GetTempVariable(depth);
        ITypeSymbol innerValueType = namedType.TypeArguments[1];
        if (_generators.TryGetFieldData(new(innerValueType, valueTempVariable, depth + 1, false), context, out var valueFieldData) == false) {
            result = null;
            return false;
        }

        result = new(currentField.FieldName, currentField.Limit, innerKeyType.ToDisplayString(), keyFieldData!, innerValueType.ToDisplayString(), valueFieldData!);
        return true;
    }

    public override SerializationMethods EmitMethods(DictionaryTypeData typeData, CurrentEmitData emitData, EmitContext context) {
        var (fieldName, limitData, keyType, keyFieldData, valueType, valueFieldData) = typeData;
        var depth = emitData.Depth;
        var canHaveNeighbors = emitData.CanHaveNeighbors;

        string indexVariable = depth == 0 ? "i" : "i" + depth;
        string iteratorVariable = depth == 0 ? "pair" : "pair" + depth;
        string sizeVariable = depth == 0 ? "size" : "size" + depth;

        var keyEmitData = _generators.EmitMethods(keyFieldData, new(depth + 1, false), context);
        var valueEmitData = _generators.EmitMethods(valueFieldData, new(depth + 1, false), context);

        var serialize = (CodeBuilder code) => {
            if (canHaveNeighbors) {
                code.StartBlock();
            }
            var isClamped = limitData != null && limitData?.LimitBehavior == BundleLimitBehavior.Clamp;
            code.AddLine($"int {sizeVariable} = {fieldName}.Count;");
            EmitWriteCollectionSizeWithLimits(code, sizeVariable, limitData);

            if (isClamped) {
                code.AddLine($"int {indexVariable} = 0;");
            }

            code.StartBlock($"foreach (var {iteratorVariable} in {fieldName})");
            // Exit when clamped
            if (isClamped) {
                code.StartBlock($"if (++{indexVariable} > {sizeVariable})");
                code.AddLine("break;");
                code.EndBlock();
            }

            code.AddLine($"{keyType} {keyFieldData.FieldName} = {iteratorVariable}.Key;");
            keyEmitData.WriteSerializeMethod(code);

            code.AddLine($"{valueType} {valueFieldData.FieldName} = {iteratorVariable}.Value;");
            valueEmitData.WriteSerializeMethod(code);

            code.EndBlock();
            if (canHaveNeighbors) {
                code.EndBlock();
            }
        };
        var deserialize = (CodeBuilder code) => {
            EmitReadCollectionSizeWithLimits(code, sizeVariable, limitData);

            code.StartBlock($"for (int {indexVariable} = 0; {indexVariable} < {sizeVariable}; {indexVariable}++)");

            code.AddLine($"{keyType} {keyFieldData.FieldName};");
            keyEmitData.WriteConstructMethod(code);

            code.AddLine($"{valueType} {valueFieldData.FieldName};");
            valueEmitData.WriteConstructMethod(code);

            code.AddLine($"{fieldName}.Add({keyFieldData.FieldName}, {valueFieldData.FieldName});");
            code.EndBlock();
        };

        return new(
            (CodeBuilder code) => {
                code.AddLine($"{fieldName} = new Dictionary<{keyType}, {valueType}>()");
                deserialize(code);
            }
            , serialize, 
            (CodeBuilder code) => {
                code.AddLine($"{fieldName}.Clear();");
                deserialize(code);
            });
    }
}