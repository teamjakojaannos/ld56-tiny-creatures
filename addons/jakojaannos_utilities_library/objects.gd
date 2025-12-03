@tool
class_name Objects


## tool-safe null check, resilient to non-tool script instances evaluating to
## Object#null -abominations, which seemingly, in some cases, pass the regular
## `node != null` -checks.[br]
## [br]
## Prefer this over regular null checks in tool-scripts.
static func is_null(value: Object) -> bool:
	return value is not Object


static func match_by_class_name(object: Object, clazz: String) -> bool:
	if Objects.is_null(object):
		return false

	# Match built-in classes
	if object.is_class(clazz):
		return true

	# Match by script
	var script: Script = object.get_script()
	if Objects.is_null(script) and object is Script:
		script = object

	# Recurse through inheritance hierarchy
	while script is Script:
		if script.get_global_name() == clazz:
			return true
		script = script.get_base_script()

	# No matches
	return false
