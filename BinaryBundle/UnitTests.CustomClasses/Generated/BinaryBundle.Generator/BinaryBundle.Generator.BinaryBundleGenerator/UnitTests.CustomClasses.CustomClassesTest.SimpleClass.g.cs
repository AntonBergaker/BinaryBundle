namespace UnitTests.CustomClasses {
	partial class CustomClassesTest {
		partial class SimpleClass : UnitTests.CustomClasses.ICustomInterface {
			public virtual void Serialize(UnitTests.CustomClasses.StringWriter writer) {
				writer.WriteString(this.StringField);
			}
			public virtual void Deserialize(UnitTests.CustomClasses.StringReader reader) {
				this.StringField = reader.ReadString();
			}
		}
	}
}
