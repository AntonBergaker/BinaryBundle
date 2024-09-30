using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using TupleElementData = (BinaryBundle.Generator.FieldTypeData typeData, string typeName);

namespace BinaryBundle.Generator.TypeGenerators;
internal class TypeGeneratorTuple : TypeGenerator<TypeGeneratorTuple.TupleTypeData> {
    internal record TupleTypeData(string FieldName, TupleElementData[] Elements) : FieldTypeData(FieldName);

    private readonly TypeGeneratorCollection _generators;

    public TypeGeneratorTuple(TypeGeneratorCollection generators) {
        this._generators = generators;
    }



    public override bool TryGetFieldData(CurrentFieldData currentField, FieldDataContext context, out TupleTypeData? result) {
        if (currentField.Type is not INamedTypeSymbol namedType) {
            result = null;
            return false;
        }

        if (namedType.IsTupleType == false) {
            result = null;
            return false;
        }

        List<TupleElementData> elements = new();

        foreach (var tupleElement in namedType.TupleElements) {
            string name = tupleElement.Name + GetTempVariable(currentField.Depth);
            if (_generators.TryGetFieldData(new(tupleElement.Type, name, currentField.Depth + 1, true), context, out var elementResult)) {
                elements.Add((elementResult!, tupleElement.Type.ToDisplayString()));
            } else {
                result = null;
                return false;
            }
        }

        result = new(currentField.FieldName, elements.ToArray());
        return true;
    }

    public override SerializationMethods EmitMethods(TupleTypeData typeData, CurrentEmitData emitData, EmitContext context) {
        var (fieldName, elements) = typeData;
        var depth = emitData.Depth;

        var serialize = (CodeBuilder code) => {
            if (emitData.CanHaveNeighbors) {
                code.StartBlock();
            }
            code.AddLine($"var ({string.Join(", ", elements.Select(x => x.typeData.FieldName))}) = {fieldName};");
            foreach (var element in elements) {
                var methods = _generators.EmitMethods(element.typeData, new(depth + 1, true), context);
                methods.WriteSerializeMethod(code);
            }
            if (emitData.CanHaveNeighbors) {
                code.EndBlock();
            }
        };
        var deserialize = (CodeBuilder code) => {
            if (emitData.CanHaveNeighbors) {
                code.StartBlock();
            }
            foreach (var element in elements) {
                code.AddLine($"{element.typeName} {element.typeData.FieldName} = default;");
                var methods = _generators.EmitMethods(element.typeData, new(depth + 1, true), context);
                methods.WriteDeserializeMethod(code);
            }
            code.AddLine($"{fieldName} = ({string.Join(", ", elements.Select(x => x.typeData.FieldName))});");
            if (emitData.CanHaveNeighbors) {
                code.EndBlock();
            }
        };

        return new(deserialize, serialize, deserialize);
    }
}
