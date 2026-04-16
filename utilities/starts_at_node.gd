@tool
class_name StartsAtNode
extends Node

## Groups to search the spawnpoints from
@export var spawnpoint_groups: Array[String] = []
## Only run on first time the node enters the tree for the fist time (and not
## e.g. when node is reparented)
@export var is_one_shot: bool = true
## Should the node be repositioned only after the initial scene is done loading.
@export var should_wait_for_initial_scene: bool = true

var _is_first_time: bool = true


func _enter_tree() -> void:
	if Engine.is_editor_hint():
		return

	if not _is_first_time and is_one_shot:
		return
	_is_first_time = false

	var s: Signal
	if should_wait_for_initial_scene:
		s = LevelManager.initial_scene_ready
	else:
		s = ready

	Signals.try_connect(s, _find_spawn_and_teleport, CONNECT_ONE_SHOT)


func _get_configuration_warnings() -> PackedStringArray:
	var errors: PackedStringArray = []
	if not spawnpoint_groups or spawnpoint_groups.is_empty():
		errors.push_back("No spawnpoint groups specified")

	return errors


func _find_spawn_and_teleport() -> void:
	for group in spawnpoint_groups:
		var spawns := get_tree().get_nodes_in_group(group)
		if spawns.is_empty():
			continue

		var spawn: Node = spawns.pick_random()
		if spawn is Node2D:
			_teleport_to_spawn.call_deferred(spawn)
			return

	push_error("No spawns available for \"%s\"" % get_path())


func _teleport_to_spawn(spawn: Node2D) -> void:
	var subject: Node2D = get_parent()
	if not spawn.is_inside_tree() or not is_instance_valid(spawn):
		push_error("Tried spawning at non-valid node %s" % spawn.get_path())
		return

	if subject.get_parent():
		subject.get_parent().remove_child(subject)
	spawn.add_sibling(subject, true)

	subject.global_position = spawn.global_position
	subject.reset_physics_interpolation()
