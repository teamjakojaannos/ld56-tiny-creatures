@tool
extends Node2D

signal initial_scene_ready

var world_root: Node2D:
	get:
		return self
var current_scene: Node2D


func _ready() -> void:
	if Engine.is_editor_hint():
		return

	_kidnap_initial_scene.call_deferred()


func change_level():
	pass


func _kidnap_initial_scene() -> void:
	var scene := get_tree().current_scene
	var key := _key_for_scene(scene)

	print("Kidnapping initial scene \"%s\" (\"%s\")" % [scene.name, key])
	var loader := LevelLoader.new(key, scene)
	loader.name = key
	add_child(loader, true)

	current_scene = scene
	initial_scene_ready.emit()


func _key_for_scene(root: Node) -> String:
	return root.scene_file_path


func _get_loader_for(key: String) -> LevelLoader:
	var loader = get_node_or_null(key)
	if loader is not LevelLoader:
		loader = LevelLoader.new(key, null)
		loader.name = key
		add_child(loader, true)

		world_root.add_child(loader)

	return loader
