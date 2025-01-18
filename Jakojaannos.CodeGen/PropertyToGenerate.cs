namespace Jakojaannos.CodeGen;

public readonly record struct PropertyToGenerate(string Name, string TypeName, string ClassName, string? Namespace) {
	public readonly string ClassName = ClassName;
	public readonly string? Namespace = Namespace;

	public readonly string Name = Name;
	public readonly string TypeName = TypeName;
	public readonly bool Nullable = false;
}
