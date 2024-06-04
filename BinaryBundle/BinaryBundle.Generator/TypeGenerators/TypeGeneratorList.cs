using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace BinaryBundle.Generator.TypeGenerators; 

internal class TypeGeneratorList : TypeGenerator<TypeGeneratorList.ListTypeData> {
    internal record ListTypeData(string FieldName, int Depth, string InnerType, FieldTypeData UnderlyingType) : FieldTypeData(FieldName);

    private readonly TypeGeneratorCollection _generators;

    public TypeGeneratorList(TypeGeneratorCollection generators) {
        this._generators = generators;
    }

    public override bool TryGetFieldData(CurrentFieldData currentField, FieldDataContext context, out ListTypeData? result) {
        if (currentField.Type is not INamedTypeSymbol namedType) {
            result = null;
            return false;
        }

        if (namedType.OriginalDefinition.ToString() != "System.Collections.Generic.List<T>") {
            result = null;
            return false;
        }

        ITypeSymbol innerType = namedType.TypeArguments[0];
        var depth = currentField.Depth;
        string tempVariable = GetTempVariable(depth);

        if (_generators.TryGetFieldData(new(innerType, tempVariable, depth + 1, false), context, out var innerTypeData) == false) {
            result = null;
            return false;
        }

        result = new(currentField.FieldName, depth, innerType.ToDisplayString(), innerTypeData!);
        return true;
    }

    public override SerializationMethods EmitMethods(ListTypeData typeData, EmitContext context) {
        var (fieldName, depth, innerType, innerTypeData) = typeData;
        var innerTypeMethods = _generators.EmitMethods(innerTypeData, context);
        string tempVariable = GetTempVariable(depth);
        string indexVariable = depth == 0 ? "i" : "i" + depth;
        string sizeVariable = depth == 0 ? "size" : "size" + depth;

        return new((code) => {
            code.AddLine($"{BinaryBundleGenerator.WriteSizeMethodName}(writer, {fieldName}.Count);");
            code.StartBlock($"for (int {indexVariable} = 0; {indexVariable} < {fieldName}.Count; {indexVariable}++)");
            code.AddLine($"{innerType} {tempVariable} = {fieldName}[{indexVariable}];");
            innerTypeMethods.WriteSerializeMethod(code);
            code.EndBlock();
        }, (code) => {
            code.StartBlock();

            code.AddLine($"int {sizeVariable} = {BinaryBundleGenerator.ReadSizeMethodName}(reader);");
            code.AddLine($"{fieldName} = BinaryBundle.BinaryBundleHelpers.ClearListAndPrepareCapacity({fieldName}, {sizeVariable});");
            code.StartBlock($"for (int {indexVariable} = 0; {indexVariable} < {sizeVariable}; {indexVariable}++)");
            code.AddLine($"{innerType} {tempVariable} = default;");
            innerTypeMethods.WriteDeserializeMethod(code);
            code.AddLine($"{fieldName}.Add({tempVariable});");

            code.EndBlock();

            code.EndBlock();
        });
    }
}