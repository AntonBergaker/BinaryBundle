namespace UnitTests.CustomClasses {
	partial class ComplexTypesTest {
		partial class ArrayClass : UnitTests.CustomClasses.ICustomInterface {
			public virtual void Serialize(UnitTests.CustomClasses.StringWriter writer) {
				BinaryBundle.BinaryBundleHelpers.WriteCollectionSize(writer, this.IntArray.Length);
				for (int i = 0; i < this.IntArray.Length; i++) {
					writer.WriteInt32(this.IntArray[i]);
				}
			}
			public virtual void Deserialize(UnitTests.CustomClasses.StringReader reader) {
				this.IntArray = BinaryBundle.BinaryBundleHelpers.CreateArrayIfSizeDiffers(this.IntArray, BinaryBundle.BinaryBundleHelpers.ReadCollectionSize(reader));
				for (int i = 0; i < this.IntArray.Length; i++) {
					this.IntArray[i] = reader.ReadInt32();
				}
			}
		}
	}
}
