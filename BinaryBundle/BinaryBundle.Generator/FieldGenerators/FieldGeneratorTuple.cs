using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BinaryBundle.Generator.FieldGenerators;
internal class FieldGeneratorTuple : FieldGenerator {

    private readonly FieldGeneratorCollection generators;

    public FieldGeneratorTuple(FieldGeneratorCollection generators) {
        this.generators = generators;
    }

    public override bool TryMatch(ITypeSymbol type, string fieldName, int depth, bool isAccessor, FieldContext context, out TypeMethods? result) {
        
        if (type is not INamedTypeSymbol namedType) {
            result = null;
            return false;
        }

        if (namedType.IsTupleType == false) {
            result = null;
            return false;
        }

        List<(string name, string type, TypeMethods methods)> elements = new();

        foreach (var tupleElement in namedType.TupleElements) {
            string name = tupleElement.Name + GetTempVariable(depth);
            if (generators.TryMatch(tupleElement.Type, name, depth+1, true, context, out var elementResult)) {
                elements.Add((name, tupleElement.Type.ToString(), elementResult!));
            } else {
                result = null;
                return false;
            }
        }

        result = new(
        (code) => {
            code.StartBlock();
            code.AddLine($"var ({string.Join(", ", elements.Select(x => x.name))}) = {fieldName};");
            foreach (var element in elements) {
                element.methods.WriteSerializeMethod(code);
            }
            code.EndBlock();
        }, 
        (code) => {
            code.StartBlock();
            foreach (var element in elements) {
                code.AddLine($"{element.type} {element.name};");
                element.methods.WriteDeserializeMethod(code);
            }
            code.AddLine($"{fieldName} = ({string.Join(", ", elements.Select(x => x.name))});");
            code.EndBlock();
        }
        );

        return true;
    }
}
