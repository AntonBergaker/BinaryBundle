using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace BinaryBundle.Generator.FieldGenerators;

internal class FieldGeneratorDictionary : FieldGenerator {
    private readonly FieldGeneratorCollection generators;

    public FieldGeneratorDictionary(FieldGeneratorCollection generators) {
        this.generators = generators;
    }

    public override bool TryMatch(ITypeSymbol type, string fieldName, int depth, bool isAccessor, FieldContext context, out TypeMethods? result) {
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
        ITypeSymbol innerKeyType = namedType.TypeArguments[0];
        if (generators.TryMatch(innerKeyType, keyTempVariable, depth + 1, false, context, out var innerKeyMethods) == false) {
            result = null;
            return false;
        }
        

        string valueTempVariable = "value"+GetTempVariable(depth);
        ITypeSymbol innerValueType = namedType.TypeArguments[1];
        if (generators.TryMatch(innerValueType, valueTempVariable, depth + 1, false, context, out var innerValueMethods) == false) {
            result = null;
            return false;
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