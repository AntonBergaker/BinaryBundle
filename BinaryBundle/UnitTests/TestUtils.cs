using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinaryBundle;
using NUnit.Framework;

namespace UnitTests; 

internal static class TestUtils {

    private static readonly byte[] sharedBuffer = new byte[0xFFF];

    public static T MakeSerializedCopy<T>(T instance, byte[]? buffer = null) where T : IBundleSerializable, new() {
        buffer ??= sharedBuffer;
        BundleDefaultWriter writer = new BundleDefaultWriter(buffer);

        instance.Serialize(writer);

        T deserializedClass = new T();
        BundleDefaultReader reader = new BundleDefaultReader(buffer);
        deserializedClass.Deserialize(reader);

        return deserializedClass;
    }

}