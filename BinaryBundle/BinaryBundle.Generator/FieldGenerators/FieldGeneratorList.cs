using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace BinaryBundle.Generator.FieldGenerators; 

internal class FieldGeneratorList : FieldGenerator {
    private readonly FieldGeneratorCollection generators;

    public FieldGeneratorList(FieldGeneratorCollection generators) {
        this.generators = generators;
    }

    public override bool TryMatch(ITypeSymbol type, string fieldName, int depth, bool isAccessor, FieldContext context, out TypeMethods? result) {
        if (type is not INamedTypeSymbol namedType) {
            result = null;
            return false;
        }

        if (namedType.OriginalDefinition.ToString() != "System.Collections.Generic.List<T>") {
            result = null;
            return false;
        }

        string indexVariable = depth == 0 ? "i" : "i" + depth;
        string tempVariable = GetTempVariable(depth);

        ITypeSymbol innerType = namedType.TypeArguments[0];

        if (generators.TryMatch(innerType, tempVariable, depth + 1, false, context, out var innerTypeMethods) == false) {
            result = null;
            return false;
        }
        _ = innerTypeMethods ?? throw new Exception("Stop shouting at me, I can't fix it.");


        string sizeVariable = depth == 0 ? "size" : "size" + depth;
        result = new ((code) => {
            code.AddLine($"{BinaryBundleGenerator.WriteSizeMethodName}(writer, {fieldName}.Count);");
            code.StartBlock($"for (int {indexVariable} = 0; {indexVariable} < {fieldName}.Count; {indexVariable}++)");
            code.AddLine($"{innerType} {tempVariable} = {fieldName}[{indexVariable}];");
            innerTypeMethods.WriteSerializeMethod(code);
            code.EndBlock();
        }, (code) => {
            code.StartBlock();

            code.AddLine($"int {sizeVariable} = {BinaryBundleGenerator.ReadSizeMethodName}(reader);");
            code.AddLine($"{fieldName} = BinaryBundle.BinaryBundleHelpers.ClearListAndPrepareCapacity({fieldName}, {sizeVariable});");
            code.StartBlock($"for (int {indexVariable} = 0; {indexVariable} < {sizeVariable}; {indexVariable}++)");
            code.AddLine($"{innerType} {tempVariable} = default;");
            innerTypeMethods.WriteDeserializeMethod(code);
            code.AddLine($"{fieldName}.Add({tempVariable});");

            code.EndBlock();

            code.EndBlock();
        });
        return true;
    }
}