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
        
        HashSet<string> autoBackedProperties = new(
            classTypeSymbol.GetMembers().OfType<IFieldSymbol>().Where(x => x.AssociatedSymbol != null).Select(x => x.AssociatedSymbol!.Name)
        );
        List<FieldTypeData> members = [];

        foreach (var member in classTypeSymbol.GetMembers()) {
            if (Utils.HasAttribute(member, IgnoreAttributeNameWithGlobal) == true) {
                continue;
            }

            if (member is IFieldSymbol field) {
                if (field.AssociatedSymbol != null) {
                    continue;
                }

                var currentData = new CurrentFieldData(field.Type, member.Name, 0, false);
                var context = new FieldDataContext(defaultInterface.Name, serializationMethods);
                if (_typeGenerators.TryGetFieldData(currentData, context, out var result) == true) {
                    members.Add(result!);
                }
            }

            else if (member is IPropertySymbol property) {
                if (autoBackedProperties.Contains(member.Name) == false) {
                    continue;
                }
                var currentData = new CurrentFieldData(property.Type, member.Name, 0, true);
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
                              Utils.TypeOrInheritanceHasAttribute(baseType, BundleAttributeNameWithGlobal));

        var classType = classTypeSymbol.TypeKind == TypeKind.Struct ? BundledType.BundleClassType.Struct : BundledType.BundleClassType.Class;

        List<(string, BundledType.BundleClassType)>? parents = null;
        var containingClass = classTypeSymbol.ContainingType;
        while (containingClass != null) {
            parents ??= [];
            parents.Insert(0, (
                containingClass.Name,
                containingClass.TypeKind == TypeKind.Struct ? BundledType.BundleClassType.Struct : BundledType.BundleClassType.Class));
            containingClass = containingClass.ContainingType;
        }

        return new(classTypeSymbol.Name, @namespace?.ToString(), inheritsSerializable, classType, parents?.ToArray() ?? [], members.ToArray());
    }

}
