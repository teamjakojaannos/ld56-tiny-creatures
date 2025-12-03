class_name Exports


static func visible_if(property: Dictionary, condition: bool, mask: int) -> void:
	property.usage = Bitflags.cond(condition, property.usage, mask)


static func delegate_get(
		node: Node,
		property: StringName,
		default: Variant,
) -> Variant:
	if node == null:
		return default

	return node.get(property)


static func delegate_set(
		node: Node,
		property: StringName,
		value: Variant,
) -> void:
	if node == null:
		return

	return node.set(property, value)
