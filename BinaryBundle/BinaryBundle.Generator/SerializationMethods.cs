using System;
using System.Collections.Generic;
using System.Text;

namespace BinaryBundle.Generator;
public class SerializationMethods : IEquatable<SerializationMethods?> {
    public readonly string SerializationMethodName;
    public readonly string DeserializationMethodName;

    public SerializationMethods(string serializationName, string deserializationName) {
        SerializationMethodName = serializationName;
        DeserializationMethodName = deserializationName;
    }

    public bool Equals(SerializationMethods? other) {
        return other is not null &&
               SerializationMethodName == other.SerializationMethodName &&
               DeserializationMethodName == other.DeserializationMethodName;
    }
}