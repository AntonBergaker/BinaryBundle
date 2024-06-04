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
/// Method needs to have format: <code>void Method(BundleWriter writer, Type field)</code>
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class BundleSerializeTypeExtensionAttribute : Attribute { }

/// <summary>
/// Specify the method as being an extension method for the BinaryBundle <see cref="IBundleSerializableBase{TWriter,TReader}.Deserialize"/> methods.
/// Whenever the type used by the method is encountered in a class it will use this method to deserialize it.
/// Method needs to have format: <code>Type Method(BundleWriter reader)</code>
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class BundleDeserializeTypeExtensionAttribute : Attribute { }

/// <summary>
/// Mark an interface with this attribute to make it the interface used by BinaryBundle.
/// The Reader and Writer will be used as the classes used as parameters for any generated <see cref="IBundleSerializableBase{TWriter,TReader}.Serialize"/> and <see cref="IBundleSerializableBase{TWriter,TReader}.Deserialize"/> methods
/// </summary>
[AttributeUsage(AttributeTargets.Interface)]
public class BundleDefaultInterfaceAttribute : Attribute {
    /// <summary>
    /// Default constructor, will attribute the interface to the decorated interface.
    /// </summary>
    public BundleDefaultInterfaceAttribute() { }

    /// <summary>
    /// Type parameter, allows reference to another interface.
    /// </summary>
    /// <param name="type"></param>
    public BundleDefaultInterfaceAttribute(Type type) { }
}

/// <summary>
/// Behavior when limit is reached on a collection with <see cref="BundleLimitAttribute"/>
/// </summary>
public enum BundleLimitBehavior {
    /// <summary>
    /// Throws an exception when limit is exceeded
    /// </summary>
    ThrowException,
    /// <summary>
    /// Clamps the values so they fit inside the limit, discarding the rest. This is done silently.
    /// </summary>
    Clamp
}

/// <summary>
/// Marks a collection with a maximum number of entries. This can be important to prevent attacks on memory allocations.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class BundleLimitAttribute : Attribute { 
    /// <summary>
    /// Mark a collection with a maximum number of entries. What happens when the limit is reached can be configured with the behavior parameter.
    /// Deserializing a collection with more entries than the count will always throw an exception, as this would be bad data.
    /// </summary>
    /// <param name="count"></param>
    /// <param name="behavior">Behavior when limit is reached. Defaults to ThrowException</param>
    public BundleLimitAttribute(int count, BundleLimitBehavior behavior = BundleLimitBehavior.ThrowException) { }
}