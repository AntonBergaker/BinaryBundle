using System;

namespace BinaryBundle; 

/// <summary>
/// Marks a class or struct as a target for the BinaryBundle source generator.
/// This will create a Serialize and Deserialize method at runtime.
/// Types marked with this attribute have to be marked partial.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class BinaryBundleAttribute : Attribute { }

/// <summary>
/// Specify a field as being ignored by the generated Serialize and Deserialize method.
/// The field will not be serialized and will not contribute to the binary size.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class BundleIgnoreAttribute : Attribute { }

/// <summary>
/// Specify the method as being an extension method for the BinaryBundle Serialize methods.
/// Whenever the type used by the method is encountered in a class use this method to serialize it.
/// Method needs to have format: void Method(BufferWriter writer, Type field)
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class BundleSerializeTypeExtension : Attribute { }

/// <summary>
/// Specify the method as being an extension method for the BinaryBundle Deserialize methods.
/// Whenever the type used by the method is encountered in a class it will use this method to deserialize it.
/// Method needs to have format: Type Method(BufferReader reader)
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class BundleDeserializeTypeExtension : Attribute { }

