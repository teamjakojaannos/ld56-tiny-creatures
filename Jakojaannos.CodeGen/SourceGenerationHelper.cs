#nullable enable
namespace Jakojaannos.CodeGen;

public static class SourceGenerationHelper {
	public static string MarkerAttribute = @"
namespace Jakojaannos.CodeGen;

[global::System.AttributeUsage(global::System.AttributeTargets.Field | global::System.AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public class MustSetInEditorAttribute : global::System.Attribute {
}
".TrimStart();

	public static string GeneratePropertyExtensionClass(PropertyToGenerate property) {
		var fieldName = $"_{property.Name.TrimStart('_').Uncapitalize()}";
		var namespaceDeclaration = property.Namespace is null ? "" : $"namespace {property.Namespace};\n";

		var sourceText = $@"
#nullable enable
{namespaceDeclaration}
// FIXME: should reference something from e.g. `Jakojaannos.CodeGen` instead
using Jakojaannos.WisperingWoods.Util.Editor;

public partial class {property.ClassName} {{
    public partial {property.TypeName} {property.Name} {{
        get => this.GetNotNullExportPropertyWithNullableBackingField({fieldName});
		set => this.SetExportProperty(ref {fieldName}, value);
	}}
	private {property.TypeName}{(property.Nullable ? "?" : "")} {fieldName};
}}
#nullable disable
".TrimStart();

		return sourceText;
	}
}

internal static class StringExt {
	public static string Uncapitalize(this string str) {
		return str switch {
			null => throw new ArgumentNullException(nameof(str)),
			"" => throw new ArgumentException($"{nameof(str)} cannot be empty", nameof(str)),
			_ => string.Concat(str[0].ToString().ToLower(), str.Substring(1))
		};
	}
}
