using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace BinaryBundle.Generator.FieldGenerators;


abstract class FieldGenerator {
    public abstract bool TryMatch(FieldDeclarationSyntax field, FieldContext context, out TypeMethods? result);
    
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
        SerializeMethod = serialize;
        DeserializeMethod = deserialize;
    }

    public string DeserializeMethod { get; }

    public string SerializeMethod { get; }
}