using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceGenerators.FieldGenerators;

namespace SourceGenerators {

    [Generator]
    public class BinaryBundleGenerator : ISourceGenerator {
        public const string AttributeName = "BinaryBundle.BinaryBundleAttribute";
        public const string InterfaceName = "BinaryBundle.ISerializable";
        public const string WriterName = "BinaryBundle.BufferWriter";
        public const string ReaderName = "BinaryBundle.BufferReader";

        private class SyntaxReceiver : ISyntaxReceiver {
            public List<TypeDeclarationSyntax> References = new();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode) {
                if (syntaxNode is ClassDeclarationSyntax @class) {
                    References.Add(@class);
                }

                if (syntaxNode is StructDeclarationSyntax @struct) {
                    References.Add(@struct);
                }
                
            }
        }

        public void Initialize(GeneratorInitializationContext context) {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
            //Debugger.Launch();
        }

        public void Execute(GeneratorExecutionContext context) {
            if (context.SyntaxReceiver is not SyntaxReceiver syntaxReceiver) {
                return;
            }

            Dictionary<string, SerializableClass> serializableClasses = new();

            foreach (TypeDeclarationSyntax @class in syntaxReceiver.References) {
                SemanticModel model = context.Compilation.GetSemanticModel(@class.SyntaxTree);

                var classTypeSymbol = model.GetDeclaredSymbol(@class) as ITypeSymbol;
                if (classTypeSymbol == null) {
                    continue;
                }

                if (classTypeSymbol.GetAttributes().Any(x => x.AttributeClass?.ToString() == AttributeName) == false) {
                    continue;
                }

                serializableClasses.Add(classTypeSymbol.ToString(),
                    new SerializableClass(model, classTypeSymbol, @class));

            }

            FieldGenerator[] fieldGenerators = {
                new FieldGeneratorPrimitive(),
                new FieldGeneratorEnum(),
                new FieldGeneratorSerializable(),
            };

            foreach (var classData in serializableClasses.Values) {
                var @class = classData.Declaration;
                var typeSymbol = classData.Symbol;

                bool inheritsSerializable = (typeSymbol.BaseType != null &&
                                             (Utils.TypeImplements(typeSymbol.BaseType, InterfaceName) ||
                                              serializableClasses.ContainsKey(typeSymbol.BaseType.ToString())));

                List<TypeMethods> fields = new();

                FieldContext fieldContext = new FieldContext(classData.Model, new(serializableClasses.Keys));

                foreach (var member in @class.Members) {
                    if (member is not FieldDeclarationSyntax field) {
                        continue;
                    }
                    foreach (FieldGenerator fieldGenerator in fieldGenerators) {
                        if (fieldGenerator.TryMatch(field, fieldContext, out TypeMethods? result)) {
                            fields.Add(result!);
                        }
                    }
                }

                string @namespace = typeSymbol.ContainingNamespace.ToString();

                string fullName = typeSymbol.ToString();

                // Get parent classes for nested classes
                List<string> parentClasses = new();
                {
                    SyntaxNode? node = @class.Parent;
                    while (node is not null) {
                        if (node is ClassDeclarationSyntax parentClass) {
                            parentClasses.Add(parentClass.Identifier.Text);
                        }

                        node = node.Parent;
                    }
                }

                // Flip parent classes, since we read inward and out
                parentClasses.Reverse();


                CodeBuilder code = new CodeBuilder();

                code.AddLine($"namespace {@namespace} {{");
                code.Indent();

                foreach (string parentClass in parentClasses) {
                    code.AddLine($"partial class {parentClass} {{");
                    code.Indent();
                }

                string classType = @class is ClassDeclarationSyntax ? "class" : "struct";
                code.AddLines($"partial {classType} {@class.Identifier.Text} : {InterfaceName} {{");
                code.Indent();


                string writerName = $"{WriterName} writer";

                if (inheritsSerializable) {
                    code.AddLine($"public override void Serialize({writerName}) {{");
                    code.Indent();
                    code.AddLine($"base.Serialize(writer);");
                }
                else if (@class is ClassDeclarationSyntax) {
                    code.AddLine($"public virtual void Serialize({writerName}) {{");
                    code.Indent();
                }
                else {
                    code.AddLine($"public void Serialize({writerName}) {{");
                    code.Indent();
                }

                foreach (TypeMethods methods in fields) {
                    code.AddLine(methods.SerializeMethod);
                }

                code.Unindent();
                code.AddLine("}");

                string readerName = $"{ReaderName} reader";
                if (inheritsSerializable) {
                    code.AddLine($"public override void Deserialize({readerName}) {{");
                    code.Indent();
                    code.AddLine($"base.Deserialize(reader);");
                } else if (@class is ClassDeclarationSyntax) {
                    code.AddLine($"public virtual void Deserialize({readerName}) {{");
                    code.Indent();
                }
                else {
                    code.AddLine($"public void Deserialize({readerName}) {{");
                    code.Indent();
                }

                foreach (TypeMethods methods in fields) {
                    code.AddLine(methods.DeserializeMethod);
                }

                code.Unindent();
                code.AddLine("}");

                // End of class
                code.Unindent();
                code.AddLine("}");

                foreach (string _ in parentClasses) {
                    code.Unindent();
                    code.AddLine("}");
                }

                // End of namespace
                code.Unindent();
                code.AddLine("}");

                context.AddSource(fullName, code.ToString());
            }


        }
    }
}
