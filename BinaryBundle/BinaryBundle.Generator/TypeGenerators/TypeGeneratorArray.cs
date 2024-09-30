using System.Linq;
using Microsoft.CodeAnalysis;

namespace BinaryBundle.Generator.TypeGenerators; 

internal class TypeGeneratorArray : TypeGenerator<TypeGeneratorArray.ArrayTypeData> {
    internal record ArrayTypeData(string FieldName, int Rank, LimitData? LimitData,string InnerTypeName, FieldTypeData UnderlyingType) : FieldTypeData(FieldName);

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

        result = new(fieldData.FieldName, rank, fieldData.Limit, arrayType.ElementType.ToDisplayString(), innerTypeData!);
        return true;
    }

    public override SerializationMethods EmitMethods(ArrayTypeData typeData, CurrentEmitData emitData, EmitContext context) {
        var (fieldName, rank, limit, innerTypeName, underlyingTypeData) = typeData;
        var depth = emitData.Depth;

        var emitMethods = _generators.EmitMethods(underlyingTypeData, new(depth+1, false), context);

        if (rank == 1) {
            var indexVariable = "i" + GetDepthNr(depth);
            var serialize = (CodeBuilder codeBuilder) => {
                if (emitData.CanHaveNeighbors) {
                    codeBuilder.StartBlock();
                }
                var sizeVariable = GetSizeVariable(depth);
                codeBuilder.AddLine($"int {sizeVariable} = {fieldName}.Length;");
                EmitWriteCollectionSizeWithLimits(codeBuilder, $"{sizeVariable}", limit);

                codeBuilder.StartBlock($"for (int {indexVariable} = 0; {indexVariable} < {sizeVariable}; {indexVariable}++)");

                emitMethods.WriteSerializeMethod(codeBuilder);

                codeBuilder.EndBlock();
                if (emitData.CanHaveNeighbors) {
                    codeBuilder.EndBlock();
                }
            };
            var deserialize = (CodeBuilder codeBuilder, bool isConstruct) => {
                // So our stored field size doesn't have name conflicts, add a code block
                if (emitData.CanHaveNeighbors) {
                    codeBuilder.StartBlock();
                }
                var sizeVariable = GetSizeVariable(depth);
                EmitReadCollectionSizeWithLimits(codeBuilder, $"{sizeVariable}", limit);
                if (isConstruct) {
                    codeBuilder.AddLine($"{fieldName} = new {innerTypeName}[{sizeVariable}];");
                } else {
                    codeBuilder.AddLine($"{fieldName} = BinaryBundle.BinaryBundleHelpers.CreateArrayIfSizeDiffers({fieldName}, {sizeVariable});");
                }

                codeBuilder.AddLine($"for (int {indexVariable} = 0; {indexVariable} < {fieldName}.Length; {indexVariable}++) {{");
                codeBuilder.Indent();

                emitMethods.WriteConstructMethod(codeBuilder);

                codeBuilder.Unindent();
                codeBuilder.AddLine("}");
                if (emitData.CanHaveNeighbors) {
                    codeBuilder.EndBlock();
                }
            };

            return new((code) => deserialize(code, true), serialize, (code) => deserialize(code, false));
        }
        {
            var serialize = (CodeBuilder codeBuilder) => {
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
            };
            var deserialize = (CodeBuilder codeBuilder, bool isConstruct) => {
                // So our stored field size doesn't have name conflicts, add a code block
                if (isConstruct) {
                    // haha fuck me
                    var innerWithoutParams = innerTypeName;
                    var introParams = "";
                    while (innerWithoutParams.EndsWith("[]")) {
                        introParams += "[]";
                        innerWithoutParams = innerWithoutParams.Substring(0, innerTypeName.Length-2);
                    }
                    codeBuilder.AddLine($"{fieldName} = new {innerWithoutParams}[");
                    codeBuilder.Indent();
                    for (int i = 0; i < rank; i++) {
                        codeBuilder.AddLine($"{BinaryBundleGenerator.ReadSizeMethodName}(reader) " + (i + 1 == rank ? "" : ","));
                    }

                    codeBuilder.Unindent();
                    codeBuilder.AddLine($"]{introParams};");
                } else {
                    codeBuilder.AddLine(
                        $"{fieldName} = BinaryBundle.BinaryBundleHelpers.CreateArrayIfSizeDiffers({fieldName},");
                    codeBuilder.Indent();
                    for (int i = 0; i < rank; i++) {
                        codeBuilder.AddLine($"{BinaryBundleGenerator.ReadSizeMethodName}(reader) " + (i + 1 == rank ? "" : ","));
                    }

                    codeBuilder.Unindent();
                    codeBuilder.AddLine(");");
                }

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
            };

            return new(code => deserialize(code, true), serialize, code => deserialize(code, false));
        }
    }

    private string GetDepthNr(int depth) {
        return depth == 0 ? "" : depth.ToString();
    }
}