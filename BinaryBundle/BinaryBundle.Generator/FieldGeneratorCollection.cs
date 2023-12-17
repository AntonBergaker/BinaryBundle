
using BinaryBundle.Generator.FieldGenerators;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace BinaryBundle.Generator;
internal class FieldGeneratorCollection {
    private List<FieldGenerator> generators;
    public FieldGeneratorCollection() {
        generators = new();
    }

    public void AddRange(IEnumerable<FieldGenerator> generators) {
        this.generators.AddRange(generators);
    }

    public bool TryMatch(ITypeSymbol type, string fieldName, int depth, bool isAccessor, FieldContext context, out TypeMethods? result) {
        foreach (var generator in generators) {
            if (generator.TryMatch(type, fieldName, depth, isAccessor, context, out result)) {
                return true;
            }
        }
        result = null;
        return false;
    }
}
