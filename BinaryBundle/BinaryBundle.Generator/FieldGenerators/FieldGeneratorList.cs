using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace BinaryBundle.Generator.FieldGenerators; 

internal class FieldGeneratorList : FieldGenerator {
    private readonly List<FieldGenerator> generators;

    public FieldGeneratorList(List<FieldGenerator> generators) {
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

        TypeMethods? innerTypeMethods = null;
        ITypeSymbol innerType = namedType.TypeArguments[0];

        foreach (FieldGenerator generator in generators) {
            if (generator.TryMatch(innerType, tempVariable, depth + 1, false, context, out innerTypeMethods)) {
                break;
            }
        }

        if (innerTypeMethods == null) {
            result = null;
            return false;
        }

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
            code.AddLine($"BinaryBundle.BinaryBundleHelpers.ClearListAndPrepareCapacity(ref {fieldName}, {sizeVariable});");
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