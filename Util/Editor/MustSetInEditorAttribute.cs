namespace Jakojaannos.WisperingWoods.Util.Editor;

[System.AttributeUsage(System.AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
sealed class MustSetInEditorAttribute : System.Attribute {
	public MustSetInEditorAttribute() {
	}
}