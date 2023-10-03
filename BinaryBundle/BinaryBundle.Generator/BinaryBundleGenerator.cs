using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using BinaryBundle.Generator.FieldGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BinaryBundle.Generator; 

[Generator]
public partial class BinaryBundleGenerator : IIncrementalGenerator {
    public const string IgnoreAttributeName = "BinaryBundle.BundleIgnoreAttribute";

    public const string WriteSizeMethodName = "BinaryBundle.BinaryBundleHelpers.WriteCollectionSize";
    public const string ReadSizeMethodName = "BinaryBundle.BinaryBundleHelpers.ReadCollectionSize";

    public void Initialize(IncrementalGeneratorInitializationContext context) {
        //Debugger.Launch();        
        var methodsWithAttribute = context.SyntaxProvider.CreateSyntaxProvider(SerializationMethodsPredicate, SerializationMethodsTransform)
            .Where(x => x is not null).Collect();

        var interfaceWithAttribute = context.SyntaxProvider.CreateSyntaxProvider(DefaultInterfacePredicate, DefaultInterfaceTransform)
            .Where(x => x is not null).Collect().Select(DefaultInterfaceCollect!);

        var typesWithAttribute = context.SyntaxProvider.CreateSyntaxProvider(TypeSyntaxPredicate, TypeSyntaxTransform)
            .Where(x => x is not null).Collect();

        var interfaceAndMethod = interfaceWithAttribute.Combine(methodsWithAttribute);

        var everything = typesWithAttribute.Combine(interfaceAndMethod).Select((x, _) => (x.Left, x.Right.Left, x.Right.Right));

        context.RegisterSourceOutput(everything, GenerateCode!);
    }

    private void GenerateCode(SourceProductionContext context, (
            ImmutableArray<SerializableClass> classes,
            DefaultInterfaceDeclaration @interface,
            ImmutableArray<SerializationMethod> methods)
        data) {


        Dictionary<string, SerializableClass> serializableClasses = data.classes.ToDictionary(x => x.Symbol.ToString());

        string interfaceName = data.@interface.Name;
        string writerName = data.@interface.WriterName;
        string readerName = data.@interface.ReaderName;

        Dictionary<string, (string serializeMethod, string deserializeMethod)> extensionTypeMethods = new();
        {
            Dictionary<string, (string? serializeMethod, string? deserializeMethod)> incompleteMethods = new();
            foreach (var method in data.methods) {
                var typeName = method.TypeName;
                
                if (incompleteMethods.TryGetValue(typeName, out var tuple) == false) {
                    tuple = (null, null);
                }

                if (method.Type == SerializationMethodType.Serialization) {
                    tuple.serializeMethod = method.MethodName;
                } else {
                    tuple.deserializeMethod = method.MethodName;
                }

                incompleteMethods[typeName] = tuple;
            }

            foreach (var pair in incompleteMethods) {
                var (serialize, deserialize) = pair.Value;
                if (serialize != null && deserialize != null) {
                    extensionTypeMethods.Add(pair.Key, (serialize, deserialize));
                }
            }
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
