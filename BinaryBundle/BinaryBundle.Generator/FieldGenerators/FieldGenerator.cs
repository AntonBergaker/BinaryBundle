using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
namespace BinaryBundle.Generator.FieldGenerators;


abstract class FieldGenerator {
    public abstract bool TryMatch(ITypeSymbol type, string fieldName, int depth, bool isAccessor, FieldContext context, out TypeMethods? result);

    protected static string GetTempVariable(int depth) {
        return depth == 0 ? "temp" : "temp" + depth;
    }
}

class FieldContext {
    public readonly SemanticModel Model;
    public readonly HashSet<string> ExtraSerializableClasses;
    public readonly string InterfaceName;
    public readonly string WriterName;
    public readonly string ReaderName;

    public FieldContext(SemanticModel model, HashSet<string> extraSerializableClasses, string interfaceName, string writerName, string readerName) {
        Model = model;
        ExtraSerializableClasses = extraSerializableClasses;
        InterfaceName = interfaceName;
        WriterName = writerName;
        ReaderName = readerName;
    }
}

class TypeMethods {
    public TypeMethods(string serialize, string deserialize) {
        serializeMethod = (code) => code.AddLine(serialize);
        deserializeMethod = (code) => code.AddLine(deserialize);
    }

    public TypeMethods(Action<CodeBuilder> serializeMethod, Action<CodeBuilder> deserializeMethod) {
        this.serializeMethod = serializeMethod;
        this.deserializeMethod = deserializeMethod;
    }

    private readonly Action<CodeBuilder> serializeMethod;
    private readonly Action<CodeBuilder> deserializeMethod;

    public void WriteSerializeMethod(CodeBuilder codeBuilder) => serializeMethod(codeBuilder);

    public void WriteDeserializeMethod(CodeBuilder codeBuilder) => deserializeMethod(codeBuilder);
}