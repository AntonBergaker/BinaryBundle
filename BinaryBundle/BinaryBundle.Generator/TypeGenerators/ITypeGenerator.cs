using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.CodeAnalysis;
namespace BinaryBundle.Generator.TypeGenerators;


interface ITypeGenerator {
    bool TryGetFieldData(CurrentFieldData fieldData, FieldDataContext context, out FieldTypeData? result);

    SerializationMethods EmitMethods(FieldTypeData typeData, CurrentEmitData emitData, EmitContext context);

    Type Handles { get; }
}

abstract class TypeGenerator<T> : ITypeGenerator where T: FieldTypeData {

    public Type Handles => typeof(T);

    protected static string GetTempVariable(int depth) {
        return depth == 0 ? "temp" : "temp" + depth;
    }
    protected static string GetSizeVariable(int depth) {
        return depth == 0 ? "size" : "size" + depth;
    }

    protected static void EmitWriteCollectionSizeWithLimits(CodeBuilder codeBuilder, string sizeVariable, LimitData? limit) {
        if (limit != null) {
            var limitNum = limit.Value.Limit;

            codeBuilder.StartBlock($"if ({sizeVariable} > {limitNum})");
            if (limit.Value.LimitBehavior == BundleLimitBehavior.ThrowException) {
                codeBuilder.AddLine($"throw new {BinaryBundleGenerator.LimitExceptionName}({sizeVariable}, {limitNum});");
            } else {
                codeBuilder.AddLine($"{sizeVariable} = {limitNum};");
            }
            codeBuilder.EndBlock();
        }

        bool fitsInByte = limit != null && limit?.Limit <= 255;
        if (fitsInByte) {
            codeBuilder.AddLine($"writer.WriteByte((byte){sizeVariable});");
        } else {
            codeBuilder.AddLine($"{BinaryBundleGenerator.WriteSizeMethodName}(writer, {sizeVariable});");
        }
    }

    protected static void EmitReadCollectionSizeWithLimits(CodeBuilder codeBuilder, string sizeVariable, LimitData? limit) {
        bool fitsInByte = limit != null && limit?.Limit <= 255;
        if (fitsInByte) {
            codeBuilder.AddLine($"int {sizeVariable} = reader.ReadByte();");
        } else {
            codeBuilder.AddLine($"int {sizeVariable} = {BinaryBundleGenerator.ReadSizeMethodName}(reader);");
        }
        if (limit != null) {
            var limitNum = limit.Value.Limit;
            codeBuilder.StartBlock($"if ({sizeVariable} > {limitNum})");
            codeBuilder.AddLine($"throw new {BinaryBundleGenerator.LimitExceptionName}({sizeVariable}, {limitNum});");
            codeBuilder.EndBlock();
        }
    }

    bool ITypeGenerator.TryGetFieldData(CurrentFieldData fieldData, FieldDataContext context, out FieldTypeData? result) {
        var success = TryGetFieldData(fieldData, context, out var typedResult);
        result = typedResult;
        return success;
    }
    public abstract bool TryGetFieldData(CurrentFieldData fieldData, FieldDataContext context, out T? result);

    SerializationMethods ITypeGenerator.EmitMethods(FieldTypeData typeData, CurrentEmitData emitData, EmitContext context) {
        return EmitMethods((T)typeData, emitData, context);
    }

    public abstract SerializationMethods EmitMethods(T typeData, CurrentEmitData emitData, EmitContext context);
}

record struct CurrentFieldData(ITypeSymbol Type, string FieldName, int Depth, bool IsAccessor, LimitData? Limit = null, bool IsReadOnly = false);

public enum BundleLimitBehavior {
    ThrowException,
    Clamp
}

public record struct LimitData(int Limit, BundleLimitBehavior LimitBehavior);

record FieldDataContext(string InterfaceName, Dictionary<string, TypeExtensionMethods> TypeExtensionMethods);

public record struct CurrentEmitData(int Depth, bool CanHaveNeighbors);
record EmitContext(string InterfaceName, string WriterName, string ReaderName, Dictionary<string, BundledType> BundledTypes);

class SerializationMethods {
    public SerializationMethods(string construct, string serialize, string deserialize) {
        _constructMethod = (code) => code.AddLine(construct);
        _serializeMethod = (code) => code.AddLine(serialize);
        _deserializeMethod = (code) => code.AddLine(deserialize);
    }

    public SerializationMethods(Action<CodeBuilder> constructMethod, Action<CodeBuilder> serializeMethod, Action<CodeBuilder> deserializeMethod) {
        _constructMethod = constructMethod;
        _serializeMethod = serializeMethod;
        _deserializeMethod = deserializeMethod;
    }

    private readonly Action<CodeBuilder> _constructMethod;
    private readonly Action<CodeBuilder> _serializeMethod;
    private readonly Action<CodeBuilder> _deserializeMethod;

    public void WriteConstructMethod(CodeBuilder codeBuilder) => _constructMethod(codeBuilder);

    public void WriteSerializeMethod(CodeBuilder codeBuilder) => _serializeMethod(codeBuilder);

    public void WriteDeserializeMethod(CodeBuilder codeBuilder) => _deserializeMethod(codeBuilder);
}