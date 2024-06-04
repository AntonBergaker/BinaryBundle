using Microsoft.CodeAnalysis;

namespace BinaryBundle.Generator.TypeGenerators;

internal class TypeGeneratorTypeExtension : TypeGenerator<TypeGeneratorTypeExtension.TypeDataTypeExtension> {
    internal record TypeDataTypeExtension(string FieldName, TypeExtensionMethods Methods) : FieldTypeData(FieldName);

    public override SerializationMethods EmitMethods(TypeDataTypeExtension typeData, CurrentEmitData emitData, EmitContext context) {
        var (fieldName, methods) = typeData;
        string serialize = $"{methods.SerializationMethodName}(writer, {fieldName});";
        string deserialize = $"{fieldName} = {methods.DeserializationMethodName}(reader);";

        return new SerializationMethods(serialize, deserialize);
    }

    public override bool TryGetFieldData(CurrentFieldData currentField, FieldDataContext context, out TypeDataTypeExtension? result) {
        if (context.TypeExtensionMethods.TryGetValue(currentField.Type.ToString(), out var methods) == false) {
            result = null;
            return false;
        }

        result = new(currentField.FieldName, methods);
        return true;
    }

}