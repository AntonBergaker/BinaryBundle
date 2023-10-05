using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BinaryBundle.Generator.FieldGenerators;

internal class FieldGeneratorTypeExtension : FieldGenerator {
    private readonly Dictionary<string, SerializationMethods> methodDictionary;

    public FieldGeneratorTypeExtension(Dictionary<string, SerializationMethods> methodDictionary) {
        this.methodDictionary = methodDictionary;
    }

    public override bool TryMatch(ITypeSymbol type, string fieldName, int depth, bool isAccessor, FieldContext context, out TypeMethods? result) {
        if (methodDictionary.TryGetValue(type.ToString(), out var methods) == false) {
            result = null;
            return false;
        }

        string serialize = $"{methods.SerializationMethodName}(writer, {fieldName});";
        string deserialize = $"{fieldName} = {methods.DeserializationMethodName}(reader);";

        result = new TypeMethods(serialize, deserialize);
        return true;
    }
}