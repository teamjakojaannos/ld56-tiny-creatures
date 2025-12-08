class_name LevelLoader
extends Node2D

var _key: String


func _init(key: String, scene: Node):
	_key = key

	if scene:
		if scene.get_parent() is Node:
			scene.reparent(self)
		else:
			add_child(scene, true)


func get_linked_levels() -> Array[LevelLink]:
	return []
