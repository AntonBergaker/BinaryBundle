using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.CodeAnalysis;
namespace BinaryBundle.Generator.TypeGenerators;


interface ITypeGenerator {
    bool TryGetFieldData(CurrentFieldData fieldData, FieldDataContext context, out FieldTypeData? result);

    SerializationMethods EmitMethods(FieldTypeData typeData, EmitContext context);

    Type Handles { get; }
}

abstract class TypeGenerator<T> : ITypeGenerator where T: FieldTypeData {

    public Type Handles => typeof(T);

    protected static string GetTempVariable(int depth) {
        return depth == 0 ? "temp" : "temp" + depth;
    }

    bool ITypeGenerator.TryGetFieldData(CurrentFieldData fieldData, FieldDataContext context, out FieldTypeData? result) {
        var success = TryGetFieldData(fieldData, context, out var typedResult);
        result = typedResult;
        return success;
    }
    public abstract bool TryGetFieldData(CurrentFieldData fieldData, FieldDataContext context, out T? result);

    SerializationMethods ITypeGenerator.EmitMethods(FieldTypeData typeData, EmitContext context) {
        return EmitMethods((T)typeData, context);
    }

    public abstract SerializationMethods EmitMethods(T typeData, EmitContext context);
}

record struct CurrentFieldData(ITypeSymbol Type, string FieldName, int Depth, bool IsAccessor, int Limit = int.MaxValue, bool IsReadOnly = false);
record FieldDataContext(string InterfaceName, Dictionary<string, TypeExtensionMethods> TypeExtensionMethods);

record EmitContext(string InterfaceName, string WriterName, string ReaderName);

class SerializationMethods {
    public SerializationMethods(string serialize, string deserialize) {
        serializeMethod = (code) => code.AddLine(serialize);
        deserializeMethod = (code) => code.AddLine(deserialize);
    }

    public SerializationMethods(Action<CodeBuilder> serializeMethod, Action<CodeBuilder> deserializeMethod) {
        this.serializeMethod = serializeMethod;
        this.deserializeMethod = deserializeMethod;
    }

    private readonly Action<CodeBuilder> serializeMethod;
    private readonly Action<CodeBuilder> deserializeMethod;

    public void WriteSerializeMethod(CodeBuilder codeBuilder) => serializeMethod(codeBuilder);

    public void WriteDeserializeMethod(CodeBuilder codeBuilder) => deserializeMethod(codeBuilder);
}