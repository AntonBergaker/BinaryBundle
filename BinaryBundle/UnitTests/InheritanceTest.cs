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
    public void TestInherited() {
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
    partial class ChildOfAbstractClass : ParentClass {
        public int B;
    }
}
