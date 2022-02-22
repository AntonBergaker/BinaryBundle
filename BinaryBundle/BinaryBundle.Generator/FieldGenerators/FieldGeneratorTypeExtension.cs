using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BinaryBundle.Generator.FieldGenerators;

internal class FieldGeneratorTypeExtension : FieldGenerator {
    private Dictionary<string, (string? serializeMethod, string? deserializeMethod)> methodDictionary;

    public FieldGeneratorTypeExtension(Dictionary<string, (string? serializeMethod, string? deserializeMethod)> methodDictionary) {
        this.methodDictionary = methodDictionary;
    }

    public override bool TryMatch(FieldDeclarationSyntax field, FieldContext context, out TypeMethods? result) {
        var type = context.Model.GetTypeInfo(field.Declaration.Type).Type;

        if (type == null) {
            result = null;
            return false;
        }

        if (methodDictionary.TryGetValue(type.ToString(), out var methods) == false) {
            result = null;
            return false;
        }

        var fieldName = GetFieldName(field);

        string serialize = $"{methods.serializeMethod}(writer, this.{fieldName});";
        string deserialize = $"this.{fieldName} = {methods.deserializeMethod}(reader);";

        result = new TypeMethods(serialize, deserialize);
        return true;
    }
}