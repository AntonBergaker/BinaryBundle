using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace BinaryBundle.Generator.TypeGenerators;

internal class TypeGeneratorSerializable : TypeGenerator<TypeGeneratorSerializable.SerializableTypeData> {
    internal record SerializableTypeData(string FieldName, string TypeName, bool ShouldBeReassigned) : FieldTypeData(FieldName);


    public override bool TryGetFieldData(CurrentFieldData currentField, FieldDataContext context, out SerializableTypeData? result) {
        if (currentField.Type.TypeKind is not (TypeKind.Class or TypeKind.Struct or TypeKind.Interface)) {
            result = null;
            return false;
        }

        string interfaceName = context.InterfaceName;

        if (Utils.IsTypeSerializable(currentField.Type, interfaceName) == false) {
            result = null;
            return false;
        }

        result = new(currentField.FieldName, currentField.Type.ToDisplayString(), currentField.IsAccessor && currentField.Type.IsValueType);
        return true;
    }

    public override SerializationMethods EmitMethods(SerializableTypeData typeData, CurrentEmitData emitData, EmitContext context) {
        var (fieldName, typeName, shouldBeReassigned) = typeData;

        var serialize = (CodeBuilder code) => code.AddLine($"{fieldName}.Serialize(writer);");
        var deserialize = (CodeBuilder code) => code.AddLine($"{fieldName}.Deserialize(reader);");
        
        var tempVariable = GetTempVariable(emitData.Depth);

        var constructorType = BundleConstructorType.NoConstructor;
        if (context.BundledTypes.TryGetValue(typeName, out var foundTypeData)) {
            constructorType = foundTypeData.ConstructorType;
        }

        var construct = (CodeBuilder code) => {
            if (emitData.CanHaveNeighbors) {
                code.StartBlock();
            }

            if (constructorType == BundleConstructorType.NoConstructor) {
                code.AddLine($"{typeName} {tempVariable} = default;");
                code.AddLine($"{tempVariable}.Deserialize(reader);");
                code.AddLine($"{fieldName} = {tempVariable};");
            } else if (constructorType == BundleConstructorType.EmptyConstructor) {
                code.AddLine($"{typeName} {tempVariable} = new();");
                code.AddLine($"{tempVariable}.Deserialize(reader);");
                code.AddLine($"{fieldName} = {tempVariable};");
            } else if (constructorType == BundleConstructorType.FieldConstructor) {
                code.AddLine($"{fieldName} = {typeName}.ConstructFromBuffer(reader);");
            }


            if (emitData.CanHaveNeighbors) {
                code.EndBlock();
            }
        };

        if (shouldBeReassigned) {
            deserialize = (code) => {
                if (emitData.CanHaveNeighbors) {
                    code.StartBlock();
                }
                code.AddLine($"{typeName} {tempVariable} = {fieldName};");
                code.AddLine($"{tempVariable}.Deserialize(reader);");
                code.AddLine($"{fieldName} = {tempVariable};");
                if (emitData.CanHaveNeighbors) {
                    code.EndBlock();
                }
            };
        }

        return new SerializationMethods(construct, serialize, deserialize);
    }
}