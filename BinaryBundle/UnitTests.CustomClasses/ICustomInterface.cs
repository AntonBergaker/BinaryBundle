using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinaryBundle;

namespace UnitTests.CustomClasses; 

[BundleDefaultInterface]
internal interface ICustomInterface : IBundleSerializableBase<StringWriter, StringReader> {
}