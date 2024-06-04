using BinaryBundle.Generator.TypeGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading;

namespace BinaryBundle.Generator;
public partial class BinaryBundleGenerator {

    private bool TypeSyntaxPredicate(SyntaxNode syntaxNode, CancellationToken token) {
        return (syntaxNode is ClassDeclarationSyntax or StructDeclarationSyntax or RecordDeclarationSyntax);
    }

    private ITypeSymbol? TypeSyntaxTransform(GeneratorAttributeSyntaxContext context, CancellationToken token) {
        return context.TargetSymbol as ITypeSymbol;
    }

    private BundledType? BundledTypeTransform(
        ITypeSymbol classTypeSymbol, 
        DefaultInterfaceDeclaration defaultInterface, 
        Dictionary<string, TypeExtensionMethods> serializationMethods, 
        CancellationToken token) {

        var classType = GetBundleClassType(classTypeSymbol);

        HashSet<string> autoBackedProperties = new(
            classTypeSymbol.GetMembers().OfType<IFieldSymbol>().Where(x => x.AssociatedSymbol != null).Select(x => x.AssociatedSymbol!.Name)
        );
        List<FieldTypeData> members = [];

        foreach (var member in classTypeSymbol.GetMembers()) {
            if (Utils.HasAttribute(member, IgnoreAttributeName)) {
                continue;
            }

            LimitData? limitData;
            var limitAttribute = member.GetAttributes().FirstOrDefault(x => x.AttributeClass?.ToDisplayString() == LimitAttributeName);
            if (limitAttribute != null) {
                var constructor = limitAttribute.ConstructorArguments;
                int limit = (int)constructor[0].Value!;
                BundleLimitBehavior limitBehavior = (BundleLimitBehavior)(int)constructor[1].Value!;
                limitData = new(limit, limitBehavior);
            } else {
                limitData = null;
            }

            if (member is IFieldSymbol field) {
                if (field.AssociatedSymbol != null) {
                    continue;
                }

                var currentData = new CurrentFieldData(field.Type, member.Name,
                    Depth: 0, IsAccessor: false, Limit: limitData, IsReadOnly: field.IsReadOnly);
                var context = new FieldDataContext(defaultInterface.Name, serializationMethods);
                if (_typeGenerators.TryGetFieldData(currentData, context, out var result) == true) {
                    members.Add(result!);
                }
            }

            else if (member is IPropertySymbol property) {
                if (autoBackedProperties.Contains(member.Name) == false) {
                    continue;
                }

                bool readOnly = 
                    property.IsReadOnly ||
                    (property.SetMethod?.ToString().EndsWith("init") ?? false); // There must be a better way to detect init properties...
                
                var currentData = new CurrentFieldData(property.Type, member.Name, 
                    Depth: 0, IsAccessor: true, Limit: limitData, IsReadOnly: readOnly);
                var context = new FieldDataContext(defaultInterface.Name, serializationMethods);
                if (_typeGenerators.TryGetFieldData(currentData, context, out var result) == true) {
                    members.Add(result!);
                }
            }
        }

        StringBuilder? @namespace = null;
        var containingNamespace = classTypeSymbol.ContainingNamespace;
        while (containingNamespace != null) {
            if (@namespace == null) {
                @namespace = new();
            } else if (containingNamespace.Name != "") {
                @namespace.Insert(0, '.');
            }
            @namespace.Insert(0, containingNamespace.Name);
            containingNamespace = containingNamespace.ContainingNamespace;
        }

        var baseType = classTypeSymbol.BaseType;
        bool inheritsSerializable = baseType != null &&
                             (Utils.TypeImplements(baseType, defaultInterface.Name) ||
                              Utils.TypeOrInheritanceHasAttribute(baseType, BundleAttributeName));


        List<(string, BundleClassType)>? parents = null;
        var containingClass = classTypeSymbol.ContainingType;
        while (containingClass != null) {
            parents ??= [];
            parents.Insert(0, (
                containingClass.Name,
                GetBundleClassType(containingClass)));
            containingClass = containingClass.ContainingType;
        }

        return new(classTypeSymbol.Name, @namespace?.ToString(), inheritsSerializable, classType, parents?.ToArray() ?? [], members.ToArray());
    }

    private BundleClassType GetBundleClassType(ITypeSymbol symbol) {
        if (symbol.IsRecord) {
            return symbol.TypeKind == TypeKind.Struct ? 
                BundleClassType.RecordStruct : 
                BundleClassType.Record;
        }
        if (symbol.TypeKind == TypeKind.Struct) {
            return BundleClassType.Struct;
        }
        return BundleClassType.Class;
    }

}
