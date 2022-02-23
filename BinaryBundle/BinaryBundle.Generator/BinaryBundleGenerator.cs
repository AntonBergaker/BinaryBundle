using System.Collections.Generic;
using System.Diagnostics;
using BinaryBundle.Generator.FieldGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BinaryBundle.Generator {

    [Generator]
    public class BinaryBundleGenerator : ISourceGenerator {
        public const string AttributeName = "BinaryBundle.BinaryBundleAttribute";
        public const string InterfaceName = "BinaryBundle.IBundleSerializable";
        public const string WriterName = "BinaryBundle.BufferWriter";
        public const string ReaderName = "BinaryBundle.BufferReader";
        public const string TypeExtensionSerializationName = "BinaryBundle.BundleSerializeTypeExtension";
        public const string TypeExtensionDeserializationName = "BinaryBundle.BundleDeserializeTypeExtension";
        public const string IgnoreAttributeName = "BinaryBundle.BundleIgnoreAttribute";
        private class SyntaxReceiver : ISyntaxReceiver {
            public readonly List<TypeDeclarationSyntax> ClassReferences = new();
            public readonly List<MethodDeclarationSyntax> MethodReferences = new();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode) {
                if (syntaxNode is ClassDeclarationSyntax @class) {
                    ClassReferences.Add(@class);
                }

                if (syntaxNode is StructDeclarationSyntax @struct) {
                    ClassReferences.Add(@struct);
                }

                if (syntaxNode is MethodDeclarationSyntax method) {
                    MethodReferences.Add(method);
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

            Dictionary<string, (string? serializeMethod, string? deserializeMethod)> extensionTypeMethods = new();

            // Scan for all method extension types
            foreach (MethodDeclarationSyntax method in syntaxReceiver.MethodReferences) {
                SemanticModel model = context.Compilation.GetSemanticModel(method.SyntaxTree);
                var methodTypeSymbol = model.GetDeclaredSymbol(method);
                if (methodTypeSymbol == null) {
                    continue;
                }

                bool isSerializationTypeExtension =
                    Utils.HasAttribute(methodTypeSymbol, TypeExtensionSerializationName);
                bool isDeserializeTypeExtension =
                    Utils.HasAttribute(methodTypeSymbol, TypeExtensionDeserializationName);

                if (isSerializationTypeExtension == false &&
                    isDeserializeTypeExtension == false) {
                    continue;
                }

                string typeName;

                if (isSerializationTypeExtension) {
                    typeName= methodTypeSymbol.Parameters[1].ToString();
                }
                else {
                    typeName = methodTypeSymbol.ReturnType.ToString();
                }

                if (extensionTypeMethods.TryGetValue(typeName, out var methods) == false) {
                    methods = (null, null);
                }

                string methodName = methodTypeSymbol.ToString();
                // Remove everything after (, if you know a better way to get the full method name please let me know
                methodName = methodName.Substring(0, methodName.IndexOf('('));

                if (isSerializationTypeExtension) {
                    methods.serializeMethod = methodName;
                }
                else {
                    methods.deserializeMethod = methodName;
                }

                extensionTypeMethods[typeName] = methods;
            }

            Dictionary<string, SerializableClass> serializableClasses = new();

            foreach (TypeDeclarationSyntax @class in syntaxReceiver.ClassReferences) {
                SemanticModel model = context.Compilation.GetSemanticModel(@class.SyntaxTree);

                var classTypeSymbol = model.GetDeclaredSymbol(@class) as ITypeSymbol;
                if (classTypeSymbol == null) {
                    continue;
                }

                if (Utils.HasAttribute(classTypeSymbol, AttributeName) == false) {
                    continue;
                }

                serializableClasses.Add(classTypeSymbol.ToString(),
                    new SerializableClass(model, classTypeSymbol, @class));

            }

            List<FieldGenerator> fieldGenerators = new();

            fieldGenerators.AddRange(new FieldGenerator[] {
            new FieldGeneratorPrimitive(),
                new FieldGeneratorEnum(),
                new FieldGeneratorSerializable(),
                new FieldGeneratorTypeExtension(extensionTypeMethods),
                new FieldGeneratorArray(fieldGenerators),
            });

            foreach (var classData in serializableClasses.Values) {
                var @class = classData.Declaration;
                var classTypeSymbol = classData.Symbol;

                bool inheritsSerializable = (classTypeSymbol.BaseType != null &&
                                             (Utils.TypeImplements(classTypeSymbol.BaseType, InterfaceName) ||
                                              serializableClasses.ContainsKey(classTypeSymbol.BaseType.ToString())));

                List<TypeMethods> fields = new();

                FieldContext fieldContext = new FieldContext(classData.Model, new(serializableClasses.Keys));

                foreach (var member in @class.Members) {
                    if (member is not FieldDeclarationSyntax field) {
                        continue;
                    }

                    // Check for ignore attribute
                    if (field.AttributeLists.Count > 0) {
                        foreach (VariableDeclaratorSyntax variable in field.Declaration.Variables) {
                            var fieldSymbol = classData.Model.GetDeclaredSymbol(variable);
                            if (Utils.HasAttribute(fieldSymbol, IgnoreAttributeName)) {
                                goto outer_continue;
                            }
                        }
                    }

                    var fieldTypeInfo = classData.Model.GetTypeInfo(field.Declaration.Type).Type;

                    if (fieldTypeInfo == null) {
                        continue;
                    }

                    foreach (FieldGenerator fieldGenerator in fieldGenerators) {
                        if (fieldGenerator.TryMatch(fieldTypeInfo, field.Declaration.Variables.First().Identifier.Text, 0, fieldContext, out TypeMethods? result)) {
                            fields.Add(result!);
                        }
                    }
                    
                    outer_continue: ;
                }

                string @namespace = classTypeSymbol.ContainingNamespace.ToString();

                string fullName = classTypeSymbol.ToString();

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
                    methods.WriteSerializeMethod(code);
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
                    methods.WriteDeserializeMethod(code);
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

                context.AddSource(fullName+".g", code.ToString());
            }


        }
    }
}
