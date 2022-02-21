﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerators;

internal class Utils {

    public static bool TypeImplements(ITypeSymbol type, string typeName) {
        return (type.ToString() == typeName ||
                (type.AllInterfaces.Any(x => x.ToString() == typeName)));
    }

    public static bool TypeImplements(TypeInfo typeInfo, string typeName) {
        if (typeInfo.Type == null) {
            return false;
        }
        return TypeImplements(typeInfo.Type, typeName);
    }

}
