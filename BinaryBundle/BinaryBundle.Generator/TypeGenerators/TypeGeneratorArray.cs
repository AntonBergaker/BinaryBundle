using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BinaryBundle.Generator.TypeGenerators; 

internal class TypeGeneratorArray : TypeGenerator<TypeGeneratorArray.ArrayTypeData> {
    internal record ArrayTypeData(string FieldName, int Depth, int Rank, string IndexVariable, string InnerTypeName, FieldTypeData UnderlyingType) : FieldTypeData(FieldName);

    private readonly TypeGeneratorCollection _generators;

    public TypeGeneratorArray(TypeGeneratorCollection generators) {
        this._generators = generators;
    }


    public override bool TryGetFieldData(CurrentFieldData fieldData, FieldDataContext context, out ArrayTypeData? result) {
        var arrayType = fieldData.Type as IArrayTypeSymbol;

        if (arrayType == null) {
            result = null;
            return false;
        }

        var depth = fieldData.Depth;
        int rank = arrayType.Rank;

        // Creates "i" for 1d arrays, "i, j" for 2d arrays etc
        string indexVariable = rank == 1 ? "i" + GetDepthNr(depth) :
            string.Join(",", Enumerable.Range(0, rank).Select(x => (char)('i' + x) + GetDepthNr(depth)));

        if (_generators.TryGetFieldData(new(arrayType.ElementType, $"{fieldData.FieldName}[{indexVariable}]", depth + 1, false), context, out var innerTypeData) == false) {
            result = null;
            return false;
        }

        result = new(fieldData.FieldName, depth, rank, indexVariable, arrayType.ElementType.ToDisplayString(), innerTypeData!);
        return true;
    }

    public override SerializationMethods EmitMethods(ArrayTypeData typeData, EmitContext context) {
        var (fieldName, depth, rank, indexVariable, innerTypeName, underlyingTypeData) = typeData;

        var emitMethods = _generators.EmitMethods(underlyingTypeData, context);

        if (rank == 1) {
            return new(
                (codeBuilder) => {
                    codeBuilder.AddLine($"{BinaryBundleGenerator.WriteSizeMethodName}(writer, {fieldName}.Length);");

                    codeBuilder.AddLine($"for (int {indexVariable} = 0; {indexVariable} < {fieldName}.Length; {indexVariable}++) {{");
                    codeBuilder.Indent();

                    emitMethods.WriteSerializeMethod(codeBuilder);

                    codeBuilder.EndBlock();

                },
                (codeBuilder) => {
                    // So our stored field size doesn't have name conflicts, add a code block
                    codeBuilder.AddLine($"{fieldName} = BinaryBundle.BinaryBundleHelpers.CreateArrayIfSizeDiffers({fieldName}, {BinaryBundleGenerator.ReadSizeMethodName}(reader));");

                    codeBuilder.AddLine($"for (int {indexVariable} = 0; {indexVariable} < {fieldName}.Length; {indexVariable}++) {{");
                    codeBuilder.Indent();

                    emitMethods.WriteDeserializeMethod(codeBuilder);

                    codeBuilder.Unindent();
                    codeBuilder.AddLine("}");

                }
            );
        } 
        return new(
            (codeBuilder) => {
                for (int i = 0; i < rank; i++) {
                    codeBuilder.AddLine($"{BinaryBundleGenerator.WriteSizeMethodName}(writer, {fieldName}.GetLength({i}));");
                }

                for (int i = 0; i < rank; i++) {
                    string iteratorVariable = (char)('i' + i) + GetDepthNr(depth);
                    codeBuilder.AddLine(
                        $"for (int {iteratorVariable} = 0; {iteratorVariable} < {fieldName}.GetLength({i}); {iteratorVariable}++) {{");
                    codeBuilder.Indent();
                }

                emitMethods.WriteSerializeMethod(codeBuilder);

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
                    string iteratorVariable = (char)('i' + i) + GetDepthNr(depth); ;
                    codeBuilder.AddLine(
                        $"for (int {iteratorVariable} = 0; {iteratorVariable} < {fieldName}.GetLength({i}); {iteratorVariable}++) {{");
                    codeBuilder.Indent();
                }

                emitMethods.WriteDeserializeMethod(codeBuilder);

                for (int i = 0; i < rank; i++) {
                    codeBuilder.Unindent();
                    codeBuilder.AddLine("}");
                }
            }
        );
    }

    private string GetDepthNr(int depth) {
        return depth == 0 ? "" : depth.ToString();
    }
}