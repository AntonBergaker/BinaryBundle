using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryBundle; 

/// <summary>
/// Default interface for BinaryBundle serializable classes.
/// </summary>
public interface IBundleSerializable : IBundleSerializableBase<BufferWriter, BufferReader> { }