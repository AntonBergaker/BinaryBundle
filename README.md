


# BinaryBundle

BinaryBundle allows you to generate serialization and deserialization methods for your C# classes and structs using source generators.  
Because the serialization methods are created at compile time, it is highly portable and efficient, as it does not rely on any reflection or any sort of runtime generation. If used correctly, BinaryBundle will not generate any additional garbage.

Good use cases for this library includes network packets, where you know both sides are guaranteed to be on the same version, like a multiplayer game. This library is not recommended for use cases where you need forwards or backwards compatibility as the serialized data is highly dependent on the field layout of the classes.

Currently supported types:
* All .NET primitive types
* Arrays, both jagged and multidimensional
* Enums
* Properties and fields
* Lists and dictionaries
* Tuples

# Getting started

## Importing
[Import the package to your project using NuGet.](https://www.nuget.org/packages/BinaryBundle/)

## Marking a type for serialization
To inform BinaryBundle that it should add `Serialize()` and `Deserialize()` methods to a class, record or struct, mark it with the `[BinaryBundle]` attribute. The type also needs to have a `partial` modifier so that code can be added to it.  
```csharp
[BinaryBundle]
public partial class SimpleClass {
    public int IntField;
}
```
BinaryBundle will create a `Serialize()` and `Deserialize()` method for this type, as well make the type implement `IBundleSerializable`. Since these fields are set from normal C# methods, they can not be marked `readonly`.

## Writing a serializable type to a binary format
You can now write the class to a binary format using the generated methods with the included `BundleDefaultWriter` and `BundleDefaultReader` classes. These are small wrappers around the standard library `BinaryWriter` and `BinaryReader` classes. If you want to use your own custom reader/writer, that's outlined here: [Using your own Reader, Writer and Interface](#Using-your-own-reader-writer-and-interface)
```csharp
var bytes = new byte[0xFF];
var @class = new SimpleClass() {
    IntField = 42,
};
var bundleWriter = new BundleDefaultWriter(bytes);
@class.Serialize(buffer);
var deserializedClass = new SimpleClass();
var bundleReader = new BundleDefaultReader(bytes);
deserializedClass.Deserialize(buffer);

Console.WriteLine(@class.IntField); // 42
Console.WriteLine(deserializedClass.IntField); // 42
```

## Serializable fields
If any fields or properties have types that implement the IBundleSerializable, either manually or from the generator, the serialization methods will be called on them as well. The `IBundleSerializable` type will not be instantiated by the deserialization method, make sure it's created before `Deserialize` is called!
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
    public static void WriteVector2(this BundleDefaultWriter writer, Vector2 vector) {
        writer.WriteFloat(vector.X);
        writer.WriteFloat(vector.Y);
    }

    [BundleDeserializeTypeExtension]
    public static Vector2 ReadVector2(this BundleDefaultReader reader) {
        float x = reader.ReadFloat();
        float y = reader.ReadFloat();
        return new Vector2(x, y);
    }
}
```
Defining these methods as extension methods isn't necessary but it looks nice.

# Attributes that can be used on fields
There are some attributes that BinaryBundle will recognize and will generate different code for.
## BundleIgnore
Fields with the `BundleIgnore` attribute will be skipped entirely.
## BundleLimit
Collections with the `BundleLimit` attribute will be limited to the size defined in the first parameter. This can be useful to prevent huge allocations when receiving untrusted data.  
The second parameter is optional and decides the behavior on collections that exceed the size to either clamp the size down when sent, or throw an exception. Deserializing a size that is read as too big will always throw an exception as this would be invalid data.

# Using your own Reader, Writer and Interface
While the provided `BundleDefaultWriter` and `BundleDefaultReader` classes are fairly fast and flexible, they are not perfect for every project. For that reason you can specify custom Writer and Reader classes to use for the serialization methods. This is done by specifying the classes used by the serializable interface.

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
writer.WriteByte((byte)array.Length); // (For representative purposes writes a byte here, in reality it uses 1-4 bytes depending on the size of the array)
for (int i = 0;i < array.Length; i++) {
    writer.WriteInt32(array[i]);
}
```
BinaryBundle is made with binary data formats in mind, for this reason it's not possible to serialize to something like JSON with this.  In the above code you'll notice there's no callback to close the array which would be necessary to serialize to a JSON list.

# Considerations and limitations

## Reference types
BinaryBundle does not know how to instantiate types, and can therefore not create reference types. In practice this means reference types can only exist on the top level where they can be instantiated in the constructor you write.

Lists and Arrays are excepted from this, who have been manually made to support being nested inside other things for convenience. However this will allocate new objects and create garbage.

## Properties
BinaryBundle will serialize properties only if they are auto properties. If the property has a get or set implementation it will not be serialized.
```csharp
[BinaryBundle]
partial class MyClass {
    // This property will be serialized
    public int AutoProperty { get; set; }
    // This property will also be serialized
    public int AutoPrivateProperty { get; private set; }

    // This field will be serialized
    private int backedProperty;
    // This property is not serialized, since it's data already exists in backedProperty
    public int BackedProperty { get => backedProperty; set => backedProperty = value; }
}
```


## Strings
By default strings are written in a null terminated UTF8 format. You can change this by [overriding the BundleDefaultWriter/Reader](#Using-your-own-reader-writer-and-interface). In the default implementation null strings are not supported, and will be interpreted as an empty string.

## Enums
Enums are serialized using their underlying C# type.
```csharp
// Serialized with an int
enum MyEnum {
    Field0,
    Field1
}
// Serialized with a byte
enum MyByteEnum : byte {
    Field0,
    Field1
}
```
