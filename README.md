# BinaryBundle

## WORK IN PROGRESS NOT PUBLISHED YET

BinaryBundle allows you to generate serialization and deserialization methods for your C# classes and structs using source generators.  
Because the library uses source generators, it is highly portable and efficient, as it does not rely on any reflection or any sort of runtime generation.

## Importing
TBA when released

## Using

### Marking a class for serialization
To inform BinaryBundle that it should add `Serialize()` and `Deserialize()` methods to a class or struct, mark it with the `[BinaryBundle]` attribute. The type also needs to have a `partial` modifier so that code can be added to it.  
```csharp
[BinaryBundle]
public partial class SimpleClass {
    public int IntField;
}
```
BinaryBundle will create a `Serialize()` and `Deserialize()` method for this class, as well make the class implement `IBundleSerializable`. Since these fields are set from normal C# methods, they can not be marked readonly.

### Serializable fields
If any fields implement the IBundleSerializable, either manually or from the generator, the serialization methods will be called on them as well. The `IBundleSerializable` type will not be instantiated by the deserialize method, make sure it's created before `Deserialize` is called!
```csharp
[BinaryBundle]
public partial class NestedClass {
    // Deserialize() will be called on InnerClass, make sure it's instantiated before Deserialization!
    public InnerClass ClassField = new InnerClass();
    // For structs you don't need to worry about this, since they always have a valid default value
    public InnerStruct StructField
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


### Extending with other types
Often you want to be able to serialize types outside your own project. BinaryBundle allows you to define TypeExtension methods that will be used to serialize these types. To mark a pair of methods as TypeExtensions add the `[BundleSerializeTypeExtension]` and `[BundleDeserializeTypeExtension]` attributes to them respectively.
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

### Using your own reader/writer and interface
TBA
