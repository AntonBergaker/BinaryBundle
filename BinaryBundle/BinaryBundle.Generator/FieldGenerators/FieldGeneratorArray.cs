using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BinaryBundle.Generator.FieldGenerators; 

internal class FieldGeneratorArray : FieldGenerator {
    private readonly List<FieldGenerator> generators;

    public FieldGeneratorArray(List<FieldGenerator> generators) {
        this.generators = generators;
    }

    public override bool TryMatch(ITypeSymbol type, string fieldName, int depth, FieldContext context, out TypeMethods? result) {
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


        TypeMethods? innerTypeMethods = null;
        foreach (FieldGenerator generator in generators) {
            if (generator.TryMatch(arrayType.ElementType, $"{fieldName}[{indexVariable}]", depth + 1, context, out innerTypeMethods)) {
                break;
            }
        }

        if (innerTypeMethods == null) {
            result = null;
            return false;
        }

        if (rank == 1) {
            result = new(
                (codeBuilder) => {
                    codeBuilder.AddLine($"writer.WriteInt16((short){fieldName}.Length);");

                    string indexVariable = "i" + GetDepthNr();
                    codeBuilder.AddLine($"for (int {indexVariable} = 0; {indexVariable} < {fieldName}.Length; {indexVariable}++) {{");
                    codeBuilder.Indent();

                    innerTypeMethods.WriteSerializeMethod(codeBuilder);

                    codeBuilder.Unindent();
                    codeBuilder.AddLine("}");
                    
                },
                (codeBuilder) => {
                    // So our stored field size doesn't have name conflicts, add a code block
                    codeBuilder.AddLine($"{fieldName} = BinaryBundle.BinaryBundleHelpers.CreateArrayIfSizeDiffers({fieldName}, reader.ReadInt16());");

                    string indexVariable = "i" + GetDepthNr();
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
                        codeBuilder.AddLine($"writer.WriteInt16((short){fieldName}.GetLength({i}));");
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
                        codeBuilder.AddLine("reader.ReadInt16()" + (i + 1 == rank ? "" : ","));
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