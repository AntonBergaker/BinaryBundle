using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace BinaryBundle.Generator.TypeGenerators; 

internal class TypeGeneratorList : TypeGenerator<TypeGeneratorList.ListTypeData> {
    internal record ListTypeData(string FieldName, string InnerType, LimitData? LimitData, bool IsReadOnly, FieldTypeData UnderlyingType) : FieldTypeData(FieldName);

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

        result = new(currentField.FieldName, innerType.ToDisplayString(), currentField.Limit, currentField.IsReadOnly, innerTypeData!);
        return true;
    }

    public override SerializationMethods EmitMethods(ListTypeData typeData, CurrentEmitData emitData, EmitContext context) {
        var (fieldName, innerType, limitData, isReadOnly, innerTypeData) = typeData;
        var depth = emitData.Depth;
        var hasNeighbors = emitData.CanHaveNeighbors;
        var innerTypeMethods = _generators.EmitMethods(innerTypeData, new(depth + 1, false), context);
        string tempVariable = GetTempVariable(depth);
        string indexVariable = depth == 0 ? "i" : "i" + depth;
        string sizeVariable = GetSizeVariable(depth);

        return new((code) => {
            if (hasNeighbors) {
                code.StartBlock();
            }
            code.AddLine($"int {sizeVariable} = {fieldName}.Count;");
            EmitWriteCollectionSizeWithLimits(code, sizeVariable, limitData);

            code.StartBlock($"for (int {indexVariable} = 0; {indexVariable} < {sizeVariable}; {indexVariable}++)");
            code.AddLine($"{innerType} {tempVariable} = {fieldName}[{indexVariable}];");
            innerTypeMethods.WriteSerializeMethod(code);
            code.EndBlock();
            if (hasNeighbors) {
                code.EndBlock();
            }
        }, (code) => {
            if (hasNeighbors) {
                code.StartBlock();
            }

            EmitReadCollectionSizeWithLimits(code, sizeVariable, limitData);

            if (isReadOnly) {
                code.AddLine($"BinaryBundle.BinaryBundleHelpers.ClearListAndPrepareCapacity({fieldName}, {sizeVariable});");
            } else {
                code.AddLine($"{fieldName} = BinaryBundle.BinaryBundleHelpers.ClearListAndPrepareCapacity({fieldName}, {sizeVariable});");
            }
            code.StartBlock($"for (int {indexVariable} = 0; {indexVariable} < {sizeVariable}; {indexVariable}++)");
            code.AddLine($"{innerType} {tempVariable} = default;");
            innerTypeMethods.WriteDeserializeMethod(code);
            code.AddLine($"{fieldName}.Add({tempVariable});");

            code.EndBlock();

            if (hasNeighbors) {
                code.EndBlock();
            }
        });
    }
}