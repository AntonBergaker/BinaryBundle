using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BinaryBundle.Generator {
    internal class SerializableClass {
        public readonly SemanticModel Model;
        public readonly ITypeSymbol Symbol;
        public readonly TypeDeclarationSyntax Declaration;

        public SerializableClass(SemanticModel model, ITypeSymbol symbol, TypeDeclarationSyntax declaration) {
            Model = model;
            Declaration = declaration;
            Symbol = symbol;
        }
    }
}
