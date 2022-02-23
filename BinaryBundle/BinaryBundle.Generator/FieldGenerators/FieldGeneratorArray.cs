using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BinaryBundle.Generator.FieldGenerators; 

internal class FieldGeneratorArray : FieldGenerator {
    private readonly List<FieldGenerator> generators;

    public FieldGeneratorArray(List<FieldGenerator> generators) {
        this.generators = generators;
    }

    public override bool TryMatch(ITypeSymbol type, string fieldName, FieldContext context, out TypeMethods? result) {
        var arrayType = type as IArrayTypeSymbol;

        if (arrayType == null) {
            result = null;
            return false;
        }

        TypeMethods? innerTypeMethods = null;
        foreach (FieldGenerator generator in generators) {
            if (generator.TryMatch(arrayType.ElementType, $"{fieldName}[i]", context, out innerTypeMethods)) {
                break;
            }
        }

        if (innerTypeMethods == null) {
            result = null;
            return false;
        }

        result = new(
            (codeBuilder) => {
                codeBuilder.AddLine($"writer.WriteInt16((short){fieldName}.Length);");
                codeBuilder.AddLine($"for (int i = 0;i < {fieldName}.Length; i++) {{");
                codeBuilder.Indent();
                innerTypeMethods.WriteSerializeMethod(codeBuilder);
                codeBuilder.Unindent();
                codeBuilder.AddLine("}");
            },
            (codeBuilder) => {
                // So our stored field size doesn't have name conflicts, add a code block
                codeBuilder.AddLine("{");
                codeBuilder.Indent();
                codeBuilder.AddLine("short arrayLength = reader.ReadInt16();");
                // If the array doesn't exist or if its size is incorrect, create it
                codeBuilder.AddLine($"if ({fieldName} == null || {fieldName}.Length != arrayLength) {{");
                codeBuilder.Indent();
                codeBuilder.AddLine($"{fieldName} = new {arrayType.ElementType}[arrayLength];");
                codeBuilder.Unindent();
                codeBuilder.AddLine("}");

                codeBuilder.AddLine($"for (int i = 0;i < arrayLength; i++) {{");
                codeBuilder.Indent();
                innerTypeMethods.WriteDeserializeMethod(codeBuilder);
                codeBuilder.Unindent();
                codeBuilder.AddLine("}");

                codeBuilder.Unindent();
                codeBuilder.AddLine("}");
            }
        );

        return true;
    }
}