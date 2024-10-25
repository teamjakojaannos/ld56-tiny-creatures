using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Godot;

public static class ExportProp {
	public static void SetExportProperty<T>(this Node node, ref T field, T value, bool notifyPropertyListChanged = false) {
		field = value;
		node.UpdateConfigurationWarnings();

		if (notifyPropertyListChanged) {
			node.NotifyPropertyListChanged();
		}
	}

	/// <summary>
	/// Gets the value of the property, hiding the nullability of the backing
	/// field. This is useful for e.g. any exported properties that are
	/// required/expected to be configured in-editor.
	/// 
	/// Method throws in-game in case the value of the backing field is still
	/// null. Due to this, care should be taken to not to read these fields in
	/// e.g. `_Ready()` of `[Tool]`-scripts, as that might result in surprising
	/// null reference exceptions on a seemingly non-null field.
	/// 
	/// The trade-off is to avoid the need for null-checks on any properties
	/// known to be not-null at runtime, but still allow the initial value to
	/// be null (before property is set in-editor). This cuts down boilerplate
	/// from runtime usage of the property, but introduces a footgun for
	/// initializers.
	/// 
	/// In context where null-checks are needed, the backing field should be
	/// used directly. This includes e.g. any read accesses in `_Ready()` or
	/// `_EnterTree()` methods of `[Tool]` scripts.
	/// </summary>
	/// <returns>The value of the backing field, with null type masked in-editor</returns>
	public static T GetNotNullExportPropertyWithNullableBackingField<T>(this Node node, T? backingField) where T : class {
		return backingField ?? node.AssertNotNullOutsideEditor<T>();
	}

	public static T AssertNotNullOutsideEditor<T>(this Node node) where T : class {
		if (!Engine.IsEditorHint()) {
			throw new System.InvalidOperationException($"Node \"{node.Name}\" is not fully configured: Value of a required property is missing");
		}

		// HACK: Hides the nullability in-editor. The field is assumed to be
		//       configured via editor and configuration warnings are shown if
		//       it is not. However, the field may be missing its value before
		//       the configuration occurs. Yes, we this is a very blatant and
		//       obvious lie we are telling to the compiler.
		return null!;
	}

	public static IEnumerable<string> CheckCommonConfigurationWarnings(this Node node) {
		return node
			.GetType()
			.GetProperties()
			.Where(f => f.GetCustomAttribute<ExportAttribute>() is not null)
			.Where(f => f.GetCustomAttribute<MustSetInEditorAttribute>() is not null)
			.Where(field => field.GetValue(node) is null)
			.Select(field => $"\"{field.Name}\" on \"{node.Name}\" is required but not set!")
			.ToArray();
	}
}