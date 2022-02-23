namespace BinaryBundle;

/// <summary>
/// Base interface for BinaryBundle serialization.
/// To provide your own interface for BinaryBundle implement this and add the <see cref="BundleDefaultInterfaceAttribute"/> attribute.
/// </summary>
/// <typeparam name="TWriter"></typeparam>
/// <typeparam name="TReader"></typeparam>
public interface IBundleSerializableBase<TWriter, TReader> where TReader : IBundleReader where TWriter : IBundleWriter {
    void Serialize(TWriter writer);

    void Deserialize(TReader reader);
}