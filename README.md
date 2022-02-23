
## WORK IN PROGRESS NOT PUBLISHED YET

BinaryBundle allows you to generate serialization and deserialization methods for your C# classes and structs using source generators.  
Because the serialization methods are created at compile time, it is highly portable and efficient, as it does not rely on any reflection or any sort of runtime generation. If used correctly, BinaryBundle will not generate any additional garbage.

Good use cases for this library includes network packets, where you know both sides are guaranteed to be on the same version, like a multiplayer game. This library is not recommended for use cases where you need forwards or backwards compatibility as the serialized data is highly dependent on the field layout of the classes.

Currently supported types:
* All .NET primitive types
* Arrays, both jagged and multidimensional
* Enums
* Lists and dictionaries

# Getting started

## Importing
TBA when released

## Marking a class for serialization
To inform BinaryBundle that it should add `Serialize()` and `Deserialize()` methods to a class or struct, mark it with the `[BinaryBundle]` attribute. The type also needs to have a `partial` modifier so that code can be added to it.  
```csharp
[BinaryBundle]
public partial class SimpleClass {
    public int IntField;
}
```
BinaryBundle will create a `Serialize()` and `Deserialize()` method for this class, as well make the class implement `IBundleSerializable`. Since these fields are set from normal C# methods, they can not be marked `readonly`.

## Writing a serializable class to a binary format
You can now write the class to a binary format using the generated methods with the included `BufferWriter` and `BufferReader` classes. These are small wrappers around the standard library `BinaryWriter` and `BinaryReader` classes. If you want to use your own custom reader/writer, that's outlined here: [Using your own Reader, Writer and Interface](#Using-your-own-reader-writer-and-interface)
```csharp
var bytes = new byte[0xFF];
var @class = new SimpleClass() {
    IntField = 42,
};
var bufferWriter = new BufferWriter(bytes);
@class.Serialize(buffer);
var deserializedClass = new SimpleClass();
var bufferReader = new BufferReader(bytes);
deserializedClass.Deserialize(buffer);

Console.WriteLine(@class.IntField); // 42
Console.WriteLine(deserializedClass.IntField); // 42
```

## Serializable fields
If any fields implement the IBundleSerializable, either manually or from the generator, the serialization methods will be called on them as well. The `IBundleSerializable` type will not be instantiated by the deserialization method, make sure it's created before `Deserialize` is called!
```csharp
[BinaryBundle]
public partial class NestedClass {
    // Deserialize() will be called on InnerClass, make sure it's instantiated before Deserialization!
    public InnerClass ClassField = new InnerClass();
    // For structs you don't need to worry about this, since they always have a valid default value
    public InnerStruct StructField;
}
[BinaryBundle]
public partial class InnerClass {
    public int IntField;
}
[BinaryBundle]
public partial struct InnerStruct {
    public int IntField;
}
```


# Extending serialization support to any type
Often you want to be able to serialize types outside your own project. BinaryBundle allows you to define TypeExtension methods that can be used to serialize any type. To mark a pair of methods as TypeExtensions add the `[BundleSerializeTypeExtension]` and `[BundleDeserializeTypeExtension]` attributes to them respectively.
```csharp
static class VectorSerializeExtension {
    [BundleSerializeTypeExtension]
    public static void WriteVector2(this BufferWriter writer, Vector2 vector) {
        writer.WriteFloat(vector.X);
        writer.WriteFloat(vector.Y);
    }

    [BundleDeserializeTypeExtension]
    public static Vector2 ReadVector2(this BufferReader reader) {
        float x = reader.ReadFloat();
        float y = reader.ReadFloat();
        return new Vector2(x, y);
    }
}
```
Defining these methods as extension methods isn't necessary but it looks nice.

# Using your own Reader, Writer and Interface
While the provided BufferWriter and BufferReader classes are fairly fast and flexible, they are not perfect for every project. For that reason you can specify custom Writer and Reader classes to use for the serialization methods. This is done by specifying the classes used by the serializable interface.

## Defining types used by the entire project
To indicate that an interface should be the one used for serializable classes you need to mark it with the `[BundleDefaultInterface]` attribute, as well as implement `IBundleSerializableBase<TWriter, TReader>`. The types used by TWriter and TReader are the ones that will be used for serialization.
```csharp
[BundleDefaultInterface]
interface IMySerializable: IBundleSerializableBase<MyWriter, MyReader> { }
```

Your custom Reader and Writer types have to implement `IBundleReader` and `IBundleWriter` respectively.

## How Reader and Writer is used
To use a custom Reader and Writer, they have to implement the corresponding interfaces, `IBundleReader` and `IBundleWriter`. These interfaces require you to define methods for writing and reading all .NET primitive types. These primitive calls are used by BinaryBundle to serialize more complex types like arrays, dictionaries and the serializable objects themselves.   
For example the code generated for serializing an array looks like this:
```csharp
writer.WriteInt16((short)array.Length);
for (int i = 0;i < array.Length; i++) {
    writer.WriteInt32(array[i]);
}
```
BinaryBundle is made with binary data formats in mind, for this reason it's not possible to write something like JSON with this.  In the above code you'll notice there's no callback to close the array which would be necessary to serialize to a JSON list.
