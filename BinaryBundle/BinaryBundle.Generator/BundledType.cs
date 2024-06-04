using System;
using System.Collections.Generic;
using System.Text;
using static BinaryBundle.Generator.BundledType;

namespace BinaryBundle.Generator;

record BundledType(string Name, string? Namespace, bool InheritsSerializable, BundleClassType ClassType, (string name, BundleClassType classType)[] ParentClasses, FieldTypeData[] Members) {
    public enum BundleClassType {
        Class,
        Struct,
    }
}
