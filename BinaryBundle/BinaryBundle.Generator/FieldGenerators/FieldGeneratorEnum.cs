using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BinaryBundle.Generator.FieldGenerators;

internal class FieldGeneratorEnum : FieldGenerator {
    private readonly List<FieldGenerator> generators;

    public FieldGeneratorEnum(List<FieldGenerator> generators) {
        this.generators = generators;
    }

    public override bool TryMatch(ITypeSymbol type, string fieldName, int depth, FieldContext context, out TypeMethods? result) {
        if (type.TypeKind != TypeKind.Enum) {
            result = null;
            return false;
        }

        INamedTypeSymbol? namedType = type as INamedTypeSymbol;

        if (namedType == null || namedType.EnumUnderlyingType == null) {
            result = null;
            return false;
        }

        TypeMethods? methods = null;
        foreach (FieldGenerator fieldGenerator in generators) {
            if (fieldGenerator.TryMatch(namedType.EnumUnderlyingType, "temp", 1, context, out methods)) {
                break;
            }
        }

        if (methods == null) {
            result = null;
            return false;
        }

        string underlying = namedType.EnumUnderlyingType.ToString();

        result = new TypeMethods((code) => {
            code.AddLine("{");
            code.Indent();
            code.AddLine($"{underlying} temp = ({underlying}){fieldName};");
            methods.WriteSerializeMethod(code);
            code.Unindent();
            code.AddLine("}");
        }, (code) => {
            code.AddLine("{");
            code.Indent();
            code.AddLine($"{underlying} temp;");
            methods.WriteDeserializeMethod(code);
            code.AddLine($"{fieldName} = ({namedType})temp;");
            code.Unindent();
            code.AddLine("}");
        });
        return true;
    }
}
