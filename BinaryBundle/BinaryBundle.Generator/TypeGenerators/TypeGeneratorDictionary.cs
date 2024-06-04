using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace BinaryBundle.Generator.TypeGenerators;

internal class TypeGeneratorDictionary : TypeGenerator<TypeGeneratorDictionary.DictionaryTypeData> {
    internal record DictionaryTypeData(string FieldName, int Depth, 
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

        result = new(currentField.FieldName, depth, innerKeyType.ToDisplayString(), keyFieldData!, innerValueType.ToDisplayString(), valueFieldData!);
        return true;
    }

    public override SerializationMethods EmitMethods(DictionaryTypeData typeData, EmitContext context) {
        var (fieldName, depth, keyType, keyFieldData, valueType, valueFieldData) = typeData;

        string indexVariable = depth == 0 ? "i" : "i" + depth;
        string iteratorVariable = depth == 0 ? "pair" : "pair" + depth;
        string sizeVariable = depth == 0 ? "size" : "size" + depth;

        var keyEmitData = _generators.EmitMethods(keyFieldData, context);
        var valueEmitData = _generators.EmitMethods(valueFieldData, context);

        return new((code) => {
            code.AddLine($"{BinaryBundleGenerator.WriteSizeMethodName}(writer, {fieldName}.Count);");
            code.StartBlock($"foreach (var {iteratorVariable} in {fieldName})");

            code.AddLine($"{keyType} {keyFieldData.FieldName} = {iteratorVariable}.Key;");
            keyEmitData.WriteSerializeMethod(code);

            code.AddLine($"{valueType} {valueFieldData.FieldName} = {iteratorVariable}.Value;");
            valueEmitData.WriteSerializeMethod(code);

            code.EndBlock();
        }, (code) => {
            code.StartBlock();

            code.AddLine($"int {sizeVariable} = {BinaryBundleGenerator.ReadSizeMethodName}(reader);");
            code.AddLine($"{fieldName}.Clear();");
            code.StartBlock($"for (int {indexVariable} = 0; {indexVariable} < {sizeVariable}; {indexVariable}++)");

            code.AddLine($"{keyType} {keyFieldData.FieldName} = default;");
            keyEmitData.WriteDeserializeMethod(code);

            code.AddLine($"{valueType} {valueFieldData.FieldName} = default;");
            valueEmitData.WriteDeserializeMethod(code);

            code.AddLine($"{fieldName}.Add({keyFieldData.FieldName}, {valueFieldData.FieldName});");
            code.EndBlock();

            code.EndBlock();
        });
    }
}