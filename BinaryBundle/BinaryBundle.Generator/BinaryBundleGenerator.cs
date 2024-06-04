using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using BinaryBundle.Generator.TypeGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BinaryBundle.Generator; 

[Generator]
public partial class BinaryBundleGenerator : IIncrementalGenerator {
    public const string IgnoreAttributeName = "BinaryBundle.BundleIgnoreAttribute";
    public const string IgnoreAttributeNameWithGlobal = $"global::{IgnoreAttributeName}";
    
    public const string LimitAttributeName = "global::BinaryBundle.BundleLimitAttribute";

    public const string WriteSizeMethodName = "BinaryBundle.BinaryBundleHelpers.WriteCollectionSize";
    public const string ReadSizeMethodName = "BinaryBundle.BinaryBundleHelpers.ReadCollectionSize";

    public const string BundleAttributeName = "BinaryBundle.BinaryBundleAttribute";
    public const string BundleAttributeNameWithGlobal = $"global::{BundleAttributeName}";

    public const string TypeExtensionSerializationName = "BinaryBundle.BundleSerializeTypeExtensionAttribute";
    public const string TypeExtensionDeserializationName = "BinaryBundle.BundleDeserializeTypeExtensionAttribute";

    private readonly TypeGeneratorCollection _typeGenerators;

    public BinaryBundleGenerator() {
        _typeGenerators = new();

        _typeGenerators.AddRange(new ITypeGenerator[] {
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

        var everything = bundledTypes.Combine(interfaceWithAttribute);

        context.RegisterSourceOutput(everything, GenerateCode);
    }


    private void GenerateCode(SourceProductionContext context, (
            BundledType @class,
            DefaultInterfaceDeclaration @interface)
        data) {


        string interfaceName = data.@interface.Name;
        string writerName = data.@interface.WriterName;
        string readerName = data.@interface.ReaderName;

        var @class = data.@class;

        var fieldContext = new EmitContext(interfaceName, writerName, readerName);

        CodeBuilder code = new CodeBuilder();

        code.AddLine($"namespace {@class.Namespace} {{");
        code.Indent();

        foreach (var parentClass in @class.ParentClasses) {
            code.AddLine($"partial {GetIdentifierForClassType(parentClass.classType)} {parentClass.name} {{");
            code.Indent();
        }

        code.AddLines($"partial {GetIdentifierForClassType(@class.ClassType)} {@class.Name} : {interfaceName} {{");
        code.Indent();


        string writerAndParameter = $"{writerName} writer";

        if (@class.InheritsSerializable) {
            code.AddLine($"public override void Serialize({writerAndParameter}) {{");
            code.Indent();
            code.AddLine($"base.Serialize(writer);");
        }
        else if (@class.ClassType is BundleClassType.Class or BundleClassType.Record) {
            code.AddLine($"public virtual void Serialize({writerAndParameter}) {{");
            code.Indent();
        }
        else {
            code.AddLine($"public void Serialize({writerAndParameter}) {{");
            code.Indent();
        }

        foreach (var members in @class.Members) {
            var methods = _typeGenerators.EmitMethods(members, fieldContext);
            methods.WriteSerializeMethod(code);
        }

        code.Unindent();
        code.AddLine("}");

        string readerAndParameter = $"{readerName} reader";
        if (@class.InheritsSerializable) {
            code.AddLine($"public override void Deserialize({readerAndParameter}) {{");
            code.Indent();
            code.AddLine($"base.Deserialize(reader);");
        } else if (@class.ClassType is BundleClassType.Class or BundleClassType.Record) {
            code.AddLine($"public virtual void Deserialize({readerAndParameter}) {{");
            code.Indent();
        }
        else {
            code.AddLine($"public void Deserialize({readerAndParameter}) {{");
            code.Indent();
        }

        foreach (var members in @class.Members) {
            var methods = _typeGenerators.EmitMethods(members, fieldContext);
            methods.WriteDeserializeMethod(code);
        }

        code.Unindent();
        code.AddLine("}");

        // End of class
        code.Unindent();
        code.AddLine("}");

        foreach (var _ in @class.ParentClasses) {
            code.Unindent();
            code.AddLine("}");
        }

        // End of namespace
        code.Unindent();
        code.AddLine("}");

        context.AddSource($"{@class.Namespace}.{@class.Name}.g", code.ToString());

    }

    private string GetIdentifierForClassType(BundleClassType classType) => classType switch {
        BundleClassType.Struct => "struct",
        BundleClassType.Record => "record",
        BundleClassType.RecordStruct => "record struct",
        _ => "class",
    };
}
