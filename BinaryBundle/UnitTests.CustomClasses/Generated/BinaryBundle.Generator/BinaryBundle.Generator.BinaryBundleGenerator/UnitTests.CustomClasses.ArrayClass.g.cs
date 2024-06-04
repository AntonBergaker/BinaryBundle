namespace UnitTests.CustomClasses {
	partial class ComplexTypesTest {
		partial class ArrayClass : UnitTests.CustomClasses.ICustomInterface {
			public virtual void Serialize(UnitTests.CustomClasses.StringWriter writer) {
				BinaryBundle.BinaryBundleHelpers.WriteCollectionSize(writer, IntArray.Length);
				for (int i = 0; i < IntArray.Length; i++) {
					writer.WriteInt32(IntArray[i]);
				}
			}
			public virtual void Deserialize(UnitTests.CustomClasses.StringReader reader) {
				IntArray = BinaryBundle.BinaryBundleHelpers.CreateArrayIfSizeDiffers(IntArray, BinaryBundle.BinaryBundleHelpers.ReadCollectionSize(reader));
				for (int i = 0; i < IntArray.Length; i++) {
					IntArray[i] = reader.ReadInt32();
				}
			}
		}
	}
}
