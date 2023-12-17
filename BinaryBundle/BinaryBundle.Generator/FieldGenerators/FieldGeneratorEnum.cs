using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BinaryBundle.Generator.FieldGenerators;

internal class FieldGeneratorEnum : FieldGenerator {
    private readonly FieldGeneratorCollection generators;

    public FieldGeneratorEnum(FieldGeneratorCollection generators) {
        this.generators = generators;
    }

    public override bool TryMatch(ITypeSymbol type, string fieldName, int depth, bool isAccessor, FieldContext context, out TypeMethods? result) {
        if (type.TypeKind != TypeKind.Enum) {
            result = null;
            return false;
        }

        INamedTypeSymbol? namedType = type as INamedTypeSymbol;

        if (namedType == null || namedType.EnumUnderlyingType == null) {
            result = null;
            return false;
        }

        string tempVariable = GetTempVariable(depth);

        if (generators.TryMatch(namedType.EnumUnderlyingType, tempVariable, depth + 1, false, context, out var methods) == false) {
            result = null;
            return false;
        }
        _ = methods ?? throw new Exception("Stop shouting at me, I can't fix it.");

        string underlying = namedType.EnumUnderlyingType.ToString();

        result = new TypeMethods((code) => {
            code.StartBlock();
            code.AddLine($"{underlying} {tempVariable} = ({underlying}){fieldName};");
            methods.WriteSerializeMethod(code);
            code.EndBlock();
        }, (code) => {
            code.StartBlock();
            code.AddLine($"{underlying} {tempVariable};");
            methods.WriteDeserializeMethod(code);
            code.AddLine($"{fieldName} = ({namedType}){tempVariable};");
            code.EndBlock();
        });
        return true;
    }
}
