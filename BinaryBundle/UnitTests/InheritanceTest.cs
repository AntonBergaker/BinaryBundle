using BinaryBundle;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests;

internal partial class InheritanceTest {

    [BinaryBundle]
    partial class ParentClass {
        public int A;
    }

    [BinaryBundle]
    partial class ChildClass : ParentClass {
        public int B;
    }

    [Test]
    public void Inherited() {
        var childClass = new ChildClass() {
            A = 1,
            B = 2
        };

        var deserializedClass = TestUtils.MakeSerializedCopy(childClass);
        Assert.AreEqual(childClass.A, deserializedClass.A);
        Assert.AreEqual(childClass.B, deserializedClass.B);
    }


    [BinaryBundle]
    abstract partial class AbstractParentClass {
        public int A;

    }

    [BinaryBundle]
    partial class ChildOfAbstractClass : AbstractParentClass {
        public int B;
    }


    [Test]
    public void AbstractParent() {
        var childClass = new ChildOfAbstractClass() {
            A = 1,
            B = 2
        };

        var deserializedClass = TestUtils.MakeSerializedCopy(childClass);
        Assert.AreEqual(childClass.A, deserializedClass.A);
        Assert.AreEqual(childClass.B, deserializedClass.B);
    }

    class ExplicitInterfaceClass : IBundleSerializable {
        public int A;
        public virtual void Deserialize(BundleDefaultReader reader) {
            A = reader.ReadInt32();
        }

        public virtual void Serialize(BundleDefaultWriter writer) {
            writer.WriteInt32(A);
        }
    }


    [BinaryBundle]
    partial class ChildOfExplicitClass : ExplicitInterfaceClass {
        public int B;
    }

    [Test]
    public void ChildOfExplicitInterface() {
        var childClass = new ChildOfExplicitClass() {
            A = 1,
            B = 2
        };

        var deserializedClass = TestUtils.MakeSerializedCopy(childClass);
        Assert.AreEqual(childClass.A, deserializedClass.A);
        Assert.AreEqual(childClass.B, deserializedClass.B);
    }

    [BinaryBundle]
    sealed partial class SealedClass {
        public string Nyeh = "";
    }

    [Test]
    public void Sealed() {
        var @class = new SealedClass() {
            Nyeh = "Bleh"
        };

        var deserializedClass = TestUtils.MakeSerializedCopy(@class);
        Assert.AreEqual(@class.Nyeh, deserializedClass.Nyeh);
    }
}