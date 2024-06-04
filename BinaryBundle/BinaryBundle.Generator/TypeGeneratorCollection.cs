
using BinaryBundle.Generator.TypeGenerators;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace BinaryBundle.Generator;
internal class TypeGeneratorCollection {
    private List<ITypeGenerator> _generators;
    public TypeGeneratorCollection() {
        _generators = [];
    }

    public void AddRange(IEnumerable<ITypeGenerator> generators) {
        this._generators.AddRange(generators);
    }

    public bool TryGetFieldData(CurrentFieldData currentField, FieldDataContext context, out FieldTypeData? result) {
        foreach (var generator in _generators) {
            if (generator.TryGetFieldData(currentField, context, out result)) {
                return true;
            }
        }
        result = null;
        return false;
    }

    public TypeGenerators.SerializationMethods EmitMethods(FieldTypeData typeData, EmitContext context) {
        var type = typeData.GetType();
        foreach (var generator in _generators) {
            if (generator.Handles == type) {
                return generator.EmitMethods(typeData, context);
            }
        }

        throw new Exception($"Intermediate type {type} was not handled by any generator.");
    }
}
