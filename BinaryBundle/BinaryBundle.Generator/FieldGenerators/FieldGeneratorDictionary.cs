using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace BinaryBundle.Generator.FieldGenerators;

internal class FieldGeneratorDictionary : FieldGenerator {
    private readonly List<FieldGenerator> generators;

    public FieldGeneratorDictionary(List<FieldGenerator> generators) {
        this.generators = generators;
    }

    public override bool TryMatch(ITypeSymbol type, string fieldName, int depth, FieldContext context, out TypeMethods? result) {
        if (type is not INamedTypeSymbol namedType) {
            result = null;
            return false;
        }

        if (namedType.OriginalDefinition.ToString() != "System.Collections.Generic.Dictionary<TKey, TValue>") {
            result = null;
            return false;
        }

        string indexVariable = depth == 0 ? "i" : "i"+ depth;
        string iteratorVariable = depth == 0 ? "pair" : "pair" + depth;

        string keyTempVariable = "key" + GetTempVariable(depth);
        TypeMethods? innerKeyMethods = null;
        ITypeSymbol innerKeyType = namedType.TypeArguments[0];
        foreach (FieldGenerator generator in generators) {
            if (generator.TryMatch(innerKeyType, keyTempVariable, depth + 1, context, out innerKeyMethods)) {
                break;
            }
        }

        string valueTempVariable = "value"+GetTempVariable(depth);
        TypeMethods? innerValueMethods = null;
        ITypeSymbol innerValueType = namedType.TypeArguments[1];
        foreach (FieldGenerator generator in generators) {
            if (generator.TryMatch(innerValueType, valueTempVariable, depth + 1, context, out innerValueMethods)) {
                break;
            }
        }

        if (innerKeyMethods == null || innerValueMethods == null) {
            result = null;
            return false;
        }

        string sizeVariable = depth == 0 ? "size" : "size" + depth;
        result = new((code) => {
            code.AddLine($"{BinaryBundleGenerator.WriteSizeMethodName}(writer, {fieldName}.Count);");
            code.StartBlock($"foreach (var {iteratorVariable} in {fieldName})");

            code.AddLine($"{innerKeyType} {keyTempVariable} = {iteratorVariable}.Key;");
            innerKeyMethods.WriteSerializeMethod(code);

            code.AddLine($"{innerValueType} {valueTempVariable} = {iteratorVariable}.Value;");
            innerValueMethods.WriteSerializeMethod(code);

            code.EndBlock();
        }, (code) => {
            code.StartBlock();

            code.AddLine($"int {sizeVariable} = {BinaryBundleGenerator.ReadSizeMethodName}(reader);");
            code.AddLine($"{fieldName}.Clear();");
            code.StartBlock($"for (int {indexVariable} = 0; {indexVariable} < {sizeVariable}; {indexVariable}++)");
            
            code.AddLine($"{innerKeyType} {keyTempVariable} = default;");
            innerKeyMethods.WriteDeserializeMethod(code);

            code.AddLine($"{innerValueType} {valueTempVariable} = default;");
            innerValueMethods.WriteDeserializeMethod(code);

            code.AddLine($"{fieldName}.Add({keyTempVariable}, {valueTempVariable});");
            code.EndBlock();

            code.EndBlock();
        });
        return true;
    }
}