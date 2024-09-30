

using System.Linq;

namespace BinaryBundle.Generator;

record BundledType(string Name, string? Namespace, bool InheritsSerializable, bool IsSealed, 
    BundleClassType ClassType, BundleConstructorType ConstructorType, (string name, BundleClassType classType)[] ParentClasses, 
    FieldTypeData[] Members, (string Name, string Type)[]? ConstructorParameters) {

    public string GetFullName() {
        var @namespace = Namespace != null ? Namespace + "." : "";
        var parentNames = string.Join(".", ParentClasses.Select(x => x.name));
        if (parentNames.Length > 0) {
            parentNames += ".";
        }
        return @namespace + parentNames + Name;
    }
}

public enum BundleClassType {
    Class,
    Struct,
    Record,
    RecordStruct
}

public enum BundleConstructorType {
    NoConstructor,
    EmptyConstructor,
    FieldConstructor,
}