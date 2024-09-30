using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using BinaryBundle.Generator.TypeGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BinaryBundle.Generator; 

[Generator]
public partial class BinaryBundleGenerator : IIncrementalGenerator {
    public const string IgnoreAttributeName = "BinaryBundle.BundleIgnoreAttribute";
    
    public const string LimitAttributeName = "BinaryBundle.BundleLimitAttribute";

    public const string BundleAttributeName = "BinaryBundle.BinaryBundleAttribute";

    public const string TypeExtensionSerializationName = "BinaryBundle.BundleSerializeTypeExtensionAttribute";
    public const string TypeExtensionDeserializationName = "BinaryBundle.BundleDeserializeTypeExtensionAttribute";

    private readonly TypeGeneratorCollection _typeGenerators;

    public BinaryBundleGenerator() {
        _typeGenerators = new();

        _typeGenerators.AddRange(new ITypeGenerator[] {
            new TypeGeneratorNullable(_typeGenerators),
            new TypeGeneratorPrimitive(),
            new TypeGeneratorEnum(_typeGenerators),
            new TypeGeneratorSerializable(),
            new TypeGeneratorTypeExtension(),
            new TypeGeneratorArray(_typeGenerators),
            new TypeGeneratorList(_typeGenerators),
            new TypeGeneratorDictionary(_typeGenerators),
            new TypeGeneratorTuple(_typeGenerators),
        });
    }

    public void Initialize(IncrementalGeneratorInitializationContext context) {
        //Debugger.Launch();
        
        var serializationMethodsWithAttribute = context.SyntaxProvider.ForAttributeWithMetadataName(TypeExtensionSerializationName,
            SerializationMethodsPredicate, static (c, t) => SerializationMethodsTransform(c, t, SerializationMethodType.Serialization))
            .WhereNotNull();
        var deserializationMethodsWithAttribute = context.SyntaxProvider.ForAttributeWithMetadataName(TypeExtensionDeserializationName,
            SerializationMethodsPredicate, static (c, t) => SerializationMethodsTransform(c, t, SerializationMethodType.Deserialization))
            .WhereNotNull();

        var methodDictionary = serializationMethodsWithAttribute.Collect().Combine(deserializationMethodsWithAttribute.Collect()).Select(SerializationMethodsCombine!);

        var interfaceWithAttribute = context.SyntaxProvider.ForAttributeWithMetadataName(DefaultInterfaceAttributeName, DefaultInterfacePredicate, DefaultInterfaceTransform)
            .WhereNotNull().Collect().Select(DefaultInterfaceCollect);

        var interfaceAndMethod = interfaceWithAttribute.Combine(methodDictionary);

        var typesWithAttribute = context.SyntaxProvider.ForAttributeWithMetadataName(BundleAttributeName, TypeSyntaxPredicate, TypeSyntaxTransform).WhereNotNull();
        
        var bundledTypes = typesWithAttribute.Combine(interfaceAndMethod).Select((x, c) => BundledTypeTransform(x.Left, x.Right.Left, x.Right.Right, c)).WhereNotNull();

        var everything = bundledTypes.Collect().Combine(interfaceWithAttribute);

        context.RegisterSourceOutput(everything, GenerateCode);
    }


    private void GenerateCode(SourceProductionContext context, (
            ImmutableArray<BundledType> bundledTypes,
            DefaultInterfaceDeclaration @interface)
        data) {

        var (bundledTypes, @interface) = data;

        string interfaceName = @interface.Name;
        string writerName = @interface.WriterName;
        string readerName = @interface.ReaderName;
        var fieldContext = new EmitContext(interfaceName, writerName, readerName, bundledTypes.ToDictionary(x => x.GetFullName()));

        foreach (var @class in bundledTypes) {

            var code = new CodeBuilder();

            code.StartBlock($"namespace {@class.Namespace}");

            foreach (var parentClass in @class.ParentClasses) {
                code.StartBlock($"partial {GetIdentifierForClassType(parentClass.classType)} {parentClass.name}");
            }

            code.StartBlock($"partial {GetIdentifierForClassType(@class.ClassType)} {@class.Name} : {interfaceName}");


            string writerAndParameter = $"{writerName} writer";

            if (@class.InheritsSerializable) {
                code.StartBlock($"public override void Serialize({writerAndParameter})");
                code.AddLine($"base.Serialize(writer);");
            } else if ((@class.ClassType is BundleClassType.Class or BundleClassType.Record) && @class.IsSealed == false) {
                code.StartBlock($"public virtual void Serialize({writerAndParameter})");
            } else {
                code.StartBlock($"public void Serialize({writerAndParameter})");
            }

            foreach (var members in @class.Members) {
                var methods = _typeGenerators.EmitMethods(members, new(0, true), fieldContext);
                methods.WriteSerializeMethod(code);
            }

            code.EndBlock();

            string readerAndParameter = $"{readerName} reader";
            if (@class.InheritsSerializable) {
                code.StartBlock($"public override void Deserialize({readerAndParameter})");
                code.AddLine($"base.Deserialize(reader);");
            } else if (@class.ClassType is BundleClassType.Class or BundleClassType.Record && @class.IsSealed == false) {
                code.StartBlock($"public virtual void Deserialize({readerAndParameter})");
            } else {
                code.StartBlock($"public void Deserialize({readerAndParameter})");
            }

            foreach (var members in @class.Members) {
                var methods = _typeGenerators.EmitMethods(members, new(0, true), fieldContext);
                methods.WriteDeserializeMethod(code);
            }

            code.EndBlock();

            if (@class.ConstructorType == BundleConstructorType.FieldConstructor) {
                var memberNames = @class.Members.ToDictionary(x => x.FieldName.ToLowerInvariant().Trim('_'));
                var constructorParameters = @class.ConstructorParameters!.ToDictionary(x => x.Name.ToLowerInvariant().Trim('@'));

                code.StartBlock($"public static {@class.Name} ConstructFromBuffer({readerAndParameter})");
                foreach (var member in @class.Members) {
                    var methods = _typeGenerators.EmitMethods(member, new(0, true), fieldContext);
                    var parameter = constructorParameters[member.FieldName.ToLowerInvariant().Trim('_')];
                    code.AddLine($"{parameter.Type} {member.FieldName};");
                    methods.WriteConstructMethod(code);
                }

                code.AddLine($"return new {@class.Name}(");
                code.Indent();

                for (int i = 0; i < @class.ConstructorParameters!.Length; i++) {
                    (string name, string type) = @class.ConstructorParameters[i];
                    var memberName = memberNames[name.ToLowerInvariant().Trim('@')];
                    code.AddLine(memberName.FieldName + (i == @class.ConstructorParameters.Length - 1 ? "" : ","));
                    
                }

                code.Unindent();
                code.AddLine(");");

                code.EndBlock();
            }

            // End of class
            code.EndBlock();

            foreach (var _ in @class.ParentClasses) {
                code.EndBlock();
            }

            // End of namespace
            code.EndBlock();

            context.AddSource($"{@class.GetFullName()}.g", code.ToString());
        }
    }

    private string GetIdentifierForClassType(BundleClassType classType) => classType switch {
        BundleClassType.Struct => "struct",
        BundleClassType.Record => "record",
        BundleClassType.RecordStruct => "record struct",
        _ => "class",
    };
}
