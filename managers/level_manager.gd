@tool
extends Node2D

var world_root: Node2D:
	get:
		return self


func _ready() -> void:
	if Engine.is_editor_hint():
		return

	_kidnap_initial_scene.call_deferred()


func _kidnap_initial_scene() -> void:
	var scene := get_tree().current_scene
	var key := _key_for_scene(scene)

	print("Kidnapping initial scene \"%s\" (\"%s\")" % [scene.name, key])

	var _loader := LevelLoader.new(self, key, scene)
	_refresh_refcounter(scene)


func _key_for_scene(root: Node) -> String:
	return root.scene_file_path


func _get_loader_for(key: String) -> LevelLoader:
	var loader = get_node_or_null(key)
	if loader is not LevelLoader:
		loader = LevelLoader.new(self, key, null)
		loader.name = key

		world_root.add_child(loader)

	return loader


func _refresh_refcounter(_scene: Node) -> void:
	pass
