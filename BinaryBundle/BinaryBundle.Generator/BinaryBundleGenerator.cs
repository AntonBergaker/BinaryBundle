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
        public const string TypeExtensionSerializationName = "BinaryBundle.BundleSerializeTypeExtensionAttribute";
        public const string TypeExtensionDeserializationName = "BinaryBundle.BundleDeserializeTypeExtensionAttribute";
        public const string IgnoreAttributeName = "BinaryBundle.BundleIgnoreAttribute";
        public const string DefaultInterfaceAttributeName = "BinaryBundle.BundleDefaultInterfaceAttribute";
        
        public const string DefaultInterfaceBaseName = "BinaryBundle.IBundleSerializableBase<TWriter, TReader>";

        public const string WriteSizeMethodName = "BinaryBundle.BinaryBundleHelpers.WriteCollectionSize";
        public const string ReadSizeMethodName = "BinaryBundle.BinaryBundleHelpers.ReadCollectionSize";

        private class SyntaxReceiver : ISyntaxReceiver {
            public readonly List<TypeDeclarationSyntax> ClassReferences = new();
            public readonly List<MethodDeclarationSyntax> MethodReferences = new();
            public readonly List<InterfaceDeclarationSyntax> InterfaceReferences = new();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode) {
                if (syntaxNode is ClassDeclarationSyntax @class) {
                    if (@class.AttributeLists.Count > 0) {
                        ClassReferences.Add(@class);
                    }
                }

                if (syntaxNode is StructDeclarationSyntax @struct) {
                    if (@struct.AttributeLists.Count > 0) {
                        ClassReferences.Add(@struct);
                    }
                }

                if (syntaxNode is MethodDeclarationSyntax method) {
                    if (method.AttributeLists.Count > 0) {
                        MethodReferences.Add(method);
                    }
                }

                if (syntaxNode is InterfaceDeclarationSyntax @interface) {
                    if (@interface.AttributeLists.Count > 0) {
                        InterfaceReferences.Add(@interface);
                    }
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

            string interfaceName = "BinaryBundle.IBundleSerializable";
            string writerName = "BinaryBundle.BufferWriter";
            string readerName = "BinaryBundle.BufferReader";

            foreach (InterfaceDeclarationSyntax @interface in syntaxReceiver.InterfaceReferences) {
                SemanticModel model = context.Compilation.GetSemanticModel(@interface.SyntaxTree);

                var classTypeSymbol = model.GetDeclaredSymbol(@interface);
                if (classTypeSymbol == null) {
                    continue;
                }

                AttributeData? attributeInterface = null;
                var attributes = classTypeSymbol.GetAttributes();
                foreach (var attribute in attributes) {
                    if (attribute.AttributeClass?.ToString() == DefaultInterfaceAttributeName) {
                        attributeInterface = attribute;
                        break;
                    }
                }

                if (attributeInterface == null) {
                    continue;
                }

                if (attributeInterface.ConstructorArguments.Length > 0) {
                    var argument = attributeInterface.ConstructorArguments[0];
                    var typeSymbol = argument.Value as INamedTypeSymbol;
                    if (typeSymbol != null) {
                        classTypeSymbol = typeSymbol;
                    }
                }

                foreach (INamedTypeSymbol implementedInterface in classTypeSymbol.Interfaces) {
                    if (implementedInterface.OriginalDefinition.ToString() == DefaultInterfaceBaseName) {
                        interfaceName = classTypeSymbol.ToString();
                        var types = implementedInterface.TypeArguments;
                        writerName = types[0].ToString();
                        readerName = types[1].ToString();
                        break;
                    }
                }
                break;
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
                new FieldGeneratorEnum(fieldGenerators),
                new FieldGeneratorSerializable(),
                new FieldGeneratorTypeExtension(extensionTypeMethods),
                new FieldGeneratorArray(fieldGenerators),
                new FieldGeneratorList(fieldGenerators),
                new FieldGeneratorDictionary(fieldGenerators),
            });

            foreach (var classData in serializableClasses.Values) {
                var @class = classData.Declaration;
                var classTypeSymbol = classData.Symbol;

                bool inheritsSerializable = (classTypeSymbol.BaseType != null &&
                                             (Utils.TypeImplements(classTypeSymbol.BaseType, interfaceName) ||
                                              serializableClasses.ContainsKey(classTypeSymbol.BaseType.ToString())));

                List<TypeMethods> fields = new();

                FieldContext fieldContext = new FieldContext(classData.Model, new(serializableClasses.Keys), interfaceName, writerName, readerName);

                foreach (var member in @class.Members) {

                    MemberDeclarationSyntax? memberSyntax = null;
                    string variableName = "";
                    ITypeSymbol? fieldTypeInfo = null;
                    bool isAccessor = false;

                    if (member is FieldDeclarationSyntax field) {
                        // Check for ignore attribute
                        if (field.AttributeLists.Count > 0) {
                            foreach (VariableDeclaratorSyntax variable in field.Declaration.Variables) {
                                var fieldSymbol = classData.Model.GetDeclaredSymbol(variable);
                                if (Utils.HasAttribute(fieldSymbol, IgnoreAttributeName)) {
                                    goto outer_continue;
                                }
                            }
                        }

                        fieldTypeInfo = classData.Model.GetTypeInfo(field.Declaration.Type).Type;
                        memberSyntax = field;
                        variableName = field.Declaration.Variables.First().Identifier.Text;
                    }

                    if (member is PropertyDeclarationSyntax property) {
                        var propertySymbol = classData.Model.GetDeclaredSymbol(property);

                        if (Utils.HasAttribute(propertySymbol, IgnoreAttributeName)) {
                            continue;
                        }

                        // Is expression bodied, skip
                        if (property.ExpressionBody != null) {
                            continue;
                        }

                        // Ignore properties that aren't auto-implemented.
                        // We know they are auto-implemented if the setter or getter contains a body
                        if (property.AccessorList != null) {
                            foreach (var accessor in property.AccessorList.Accessors) {
                                if (accessor.Body != null || accessor.ExpressionBody != null) {
                                    goto outer_continue;
                                }
                            }
                        }

                        isAccessor = true;
                        memberSyntax = property;
                        variableName = property.Identifier.Text;
                        fieldTypeInfo = classData.Model.GetTypeInfo(property.Type).Type;
                    }

                    if (memberSyntax == null || fieldTypeInfo == null) {
                        continue;
                    }


                    foreach (FieldGenerator fieldGenerator in fieldGenerators) {
                        if (fieldGenerator.TryMatch(fieldTypeInfo, "this." + variableName, 0, isAccessor, fieldContext, out TypeMethods? result)) {
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
                code.AddLines($"partial {classType} {@class.Identifier.Text} : {interfaceName} {{");
                code.Indent();


                string writerAndParameter = $"{writerName} writer";

                if (inheritsSerializable) {
                    code.AddLine($"public override void Serialize({writerAndParameter}) {{");
                    code.Indent();
                    code.AddLine($"base.Serialize(writer);");
                }
                else if (@class is ClassDeclarationSyntax) {
                    code.AddLine($"public virtual void Serialize({writerAndParameter}) {{");
                    code.Indent();
                }
                else {
                    code.AddLine($"public void Serialize({writerAndParameter}) {{");
                    code.Indent();
                }

                foreach (TypeMethods methods in fields) {
                    methods.WriteSerializeMethod(code);
                }

                code.Unindent();
                code.AddLine("}");

                string readerAndParameter = $"{readerName} reader";
                if (inheritsSerializable) {
                    code.AddLine($"public override void Deserialize({readerAndParameter}) {{");
                    code.Indent();
                    code.AddLine($"base.Deserialize(reader);");
                } else if (@class is ClassDeclarationSyntax) {
                    code.AddLine($"public virtual void Deserialize({readerAndParameter}) {{");
                    code.Indent();
                }
                else {
                    code.AddLine($"public void Deserialize({readerAndParameter}) {{");
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
