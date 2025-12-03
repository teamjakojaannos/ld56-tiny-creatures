@tool
class_name Nodes

const GROUP_SAFE_TO_DELETE_IN_EDITOR: String = "I KNOW WHAT I AM DOING THIS NODE IS SAFE TO AUTOMATICALLY FREE IN-EDITOR"


static func find_if_null(
		node: Node,
		value: Node,
		clazz: Variant,
) -> Object:
	if not Objects.is_null(value):
		return value

	return find_by_class(node, clazz, true)


## Finds a node with matching class. Nodes are searched from direct siblings of
## [param node].[br]
## [br]
## If [param node] itself has a matching class and [param match_self] is set to
## [code]true[/code], the [param node] itself is returned.[br]
## [br]
## The [param clazz] must be a class, not an instance. For example, a script
## [code]Health[/code] or a built-in class [code]AnimationPlayer[/code]. More
## specifically, the [param clazz] [b]must[/b] be valid second parameter for
## [method @GDScript.is_instance_of]
static func find_by_class(
		node: Node,
		clazz: Variant,
		match_self: bool = true,
		recursive: bool = false,
) -> Object:
	if Objects.is_null(node):
		return null

	if match_self and is_instance_of(node, clazz):
		return node

	for child in node.get_children():
		if recursive:
			var found = Nodes.find_by_class(child, clazz, true, true)
			if found:
				return found
		else:
			if is_instance_of(child, clazz):
				return child

	return null


## Finds a node with a matching [code]class_name[/code] (or a built-in
## class). Nodes are searched from direct siblings of [param node].[br]
## [br]
## If [param node] is of matching class and [param match_self] is set to
## [code]true[/code], the [param node] itself is returned.
static func find_by_class_name(
		node: Node,
		clazz: String,
		match_self: bool = true,
		recursive: bool = false,
) -> Object:
	if Objects.is_null(node):
		return null

	if match_self and Objects.match_by_class_name(node, clazz):
		return node

	var by_name = node.get_node_or_null(clazz)
	if Objects.match_by_class_name(by_name, clazz):
		return by_name

	for child in node.get_children():
		if recursive:
			var found = Nodes.find_by_class_name(child, clazz, true, true)
			if found:
				return found

		else:
			if Objects.match_by_class_name(child, clazz):
				return child

	return null


## Marks the node as "safe to free in-editor". This is useful for any case when
## short-lived nodes are spawned in tool scripts for preview purposes.[br]
## [br]
## Beware that [Lifetime]-components of safe-to-free nodes are executed and
## [b]will[/b] call [code]queue_free[/code] on their parent nodes in-editor.
static func mark_as_safe_to_free(node: Node) -> void:
	if Objects.is_null(node):
		return

	node.add_to_group(GROUP_SAFE_TO_DELETE_IN_EDITOR)


## Checks if the node is marked as "safe to free in-editor". May evaluate to
## [code]true[/code] only for nodes which have explicitly been marked as safe
## (via [member Nodes.mark_as_safe_to_free]).
static func is_safe_to_free_in_editor(node: Node) -> bool:
	if Objects.is_null(node):
		return false

	# Never free edited scene root
	if Engine.is_editor_hint():
		var edited_scene = EditorInterface.get_edited_scene_root()
		if edited_scene == node:
			return false

		return node.is_in_group(GROUP_SAFE_TO_DELETE_IN_EDITOR)

	return true
