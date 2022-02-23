using System;

namespace BinaryBundle; 

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class BinaryBundleAttribute : Attribute {

}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class BundleIgnoreAttribute : Attribute {

}

[AttributeUsage(AttributeTargets.Method)]
public class BundleSerializeTypeExtension : Attribute { }

[AttributeUsage(AttributeTargets.Method)]
public class BundleDeserializeTypeExtension : Attribute { }

