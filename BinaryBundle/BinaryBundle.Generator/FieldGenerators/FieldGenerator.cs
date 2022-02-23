using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace BinaryBundle.Generator.FieldGenerators;


abstract class FieldGenerator {
    public abstract bool TryMatch(ITypeSymbol type, string fieldName, FieldContext context, out TypeMethods? result);
    
    protected string GetFieldName(FieldDeclarationSyntax field) {
        return field.Declaration.Variables.First().Identifier.Text;
    }

    protected string GetFieldType(FieldDeclarationSyntax field) {
        return field.Declaration.Type.ToString();
    }
}

class FieldContext {
    public readonly SemanticModel Model;
    public readonly HashSet<string> ExtraSerializableClasses;

    public FieldContext(SemanticModel model, HashSet<string> extraSerializableClasses) {
        Model = model;
        ExtraSerializableClasses = extraSerializableClasses;
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