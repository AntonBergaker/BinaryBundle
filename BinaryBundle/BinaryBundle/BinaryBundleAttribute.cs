using System;

namespace BinaryBundle;

/// <summary>
/// Marks a class or struct as a target for the BinaryBundle source generator.
/// This will create a <see cref="IBundleSerializableBase{TWriter,TReader}.Serialize"/> and <see cref="IBundleSerializableBase{TWriter,TReader}.Deserialize"/> method at runtime.
/// Types marked with this attribute have to be marked partial.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class BinaryBundleAttribute : Attribute { }

/// <summary>
/// Specify a field as being ignored by the generated <see cref="IBundleSerializableBase{TWriter,TReader}.Serialize"/> and <see cref="IBundleSerializableBase{TWriter,TReader}.Deserialize"/> methods.
/// The field will not be serialized and will not contribute to the type's binary size.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class BundleIgnoreAttribute : Attribute { }

/// <summary>
/// Specify the method as being an extension method for the <see cref="IBundleSerializableBase{TWriter,TReader}.Serialize"/> methods.
/// Whenever the type used by the method is encountered in a class use this method to serialize it.
/// Method needs to have format: <code>void Method(BufferWriter writer, Type field)</code>
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class BundleSerializeTypeExtensionAttribute : Attribute { }

/// <summary>
/// Specify the method as being an extension method for the BinaryBundle <see cref="IBundleSerializableBase{TWriter,TReader}.Deserialize"/> methods.
/// Whenever the type used by the method is encountered in a class it will use this method to deserialize it.
/// Method needs to have format: <code>Type Method(BufferReader reader)</code>
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class BundleDeserializeTypeExtensionAttribute : Attribute { }

/// <summary>
/// Mark an interface with this attribute to make it the interface used by BinaryBundle.
/// The Reader and Writer will be used as the classes used as parameters for any generated <see cref="IBundleSerializableBase{TWriter,TReader}.Serialize"/> and <see cref="IBundleSerializableBase{TWriter,TReader}.Deserialize"/> methods
/// </summary>
[AttributeUsage(AttributeTargets.Interface)]
public class BundleDefaultInterfaceAttribute : Attribute { }