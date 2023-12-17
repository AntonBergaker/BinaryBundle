using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BinaryBundle.Generator.FieldGenerators; 

internal class FieldGeneratorArray : FieldGenerator {
    private readonly FieldGeneratorCollection generators;

    public FieldGeneratorArray(FieldGeneratorCollection generators) {
        this.generators = generators;
    }

    public override bool TryMatch(ITypeSymbol type, string fieldName, int depth, bool isAccessor, FieldContext context, out TypeMethods? result) {
        var arrayType = type as IArrayTypeSymbol;

        if (arrayType == null) {
            result = null;
            return false;
        }

        int rank = arrayType.Rank;

        string GetDepthNr() {
            return depth == 0 ? "" : depth.ToString();
        }

        // Creates "i" for 1d arrays, "i, j" for 2d arrays etc
        string indexVariable = rank == 1 ? "i"+GetDepthNr() :
            string.Join(",", Enumerable.Range(0, rank).Select(x => (char)('i'+x)+GetDepthNr()) );

        if (generators.TryMatch(arrayType.ElementType, $"{fieldName}[{indexVariable}]", depth + 1, false, context, out var innerTypeMethods) == false) {
            result = null;
            return false;
        }
        _ = innerTypeMethods ?? throw new Exception("Stop shouting at me, I can't fix it.");

        if (rank == 1) {
            result = new(
                (codeBuilder) => {
                    codeBuilder.AddLine($"{BinaryBundleGenerator.WriteSizeMethodName}(writer, {fieldName}.Length);");

                    codeBuilder.AddLine($"for (int {indexVariable} = 0; {indexVariable} < {fieldName}.Length; {indexVariable}++) {{");
                    codeBuilder.Indent();

                    innerTypeMethods.WriteSerializeMethod(codeBuilder);

                    codeBuilder.EndBlock();
                    
                },
                (codeBuilder) => {
                    // So our stored field size doesn't have name conflicts, add a code block
                    codeBuilder.AddLine($"{fieldName} = BinaryBundle.BinaryBundleHelpers.CreateArrayIfSizeDiffers({fieldName}, {BinaryBundleGenerator.ReadSizeMethodName}(reader));");

                    codeBuilder.AddLine($"for (int {indexVariable} = 0; {indexVariable} < {fieldName}.Length; {indexVariable}++) {{");
                    codeBuilder.Indent();
                    
                    innerTypeMethods.WriteDeserializeMethod(codeBuilder);

                    codeBuilder.Unindent();
                    codeBuilder.AddLine("}");
                    
                }
            );
        }
        else {
            result = new(
                (codeBuilder) => {
                    for (int i = 0; i < rank; i++) {
                        codeBuilder.AddLine($"{BinaryBundleGenerator.WriteSizeMethodName}(writer, {fieldName}.GetLength({i}));");
                    }

                    for (int i = 0; i < rank; i++) {
                        string iteratorVariable = (char)('i' + i) + GetDepthNr();
                        codeBuilder.AddLine(
                            $"for (int {iteratorVariable} = 0; {iteratorVariable} < {fieldName}.GetLength({i}); {iteratorVariable}++) {{");
                        codeBuilder.Indent();
                    }

                    innerTypeMethods.WriteSerializeMethod(codeBuilder);

                    for (int i = 0; i < rank; i++) {
                        codeBuilder.Unindent();
                        codeBuilder.AddLine("}");
                    }
                },
                (codeBuilder) => {
                    // So our stored field size doesn't have name conflicts, add a code block
                    codeBuilder.AddLine(
                        $"{fieldName} = BinaryBundle.BinaryBundleHelpers.CreateArrayIfSizeDiffers({fieldName},");
                    codeBuilder.Indent();
                    for (int i = 0; i < rank; i++) {
                        codeBuilder.AddLine($"{BinaryBundleGenerator.ReadSizeMethodName}(reader) " + (i + 1 == rank ? "" : ","));
                    }

                    codeBuilder.Unindent();
                    codeBuilder.AddLine(");");
                    
                    for (int i = 0; i < rank; i++) {
                        string iteratorVariable = (char)('i' + i) + GetDepthNr(); ;
                        codeBuilder.AddLine(
                            $"for (int {iteratorVariable} = 0; {iteratorVariable} < {fieldName}.GetLength({i}); {iteratorVariable}++) {{");
                        codeBuilder.Indent();
                    }

                    innerTypeMethods.WriteDeserializeMethod(codeBuilder);

                    for (int i = 0; i < rank; i++) {
                        codeBuilder.Unindent();
                        codeBuilder.AddLine("}");
                    }
                }
            );
        }

        return true;
    }
}