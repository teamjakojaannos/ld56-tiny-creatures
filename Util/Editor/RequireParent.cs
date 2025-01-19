using System;

namespace Jakojaannos.WisperingWoods.Util.Editor;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
sealed class RequireParentAttribute(params Type[] parentTypes) : Attribute {
	public Type[] ParentTypes { get; } = parentTypes;
}
