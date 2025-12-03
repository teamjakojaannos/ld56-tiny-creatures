class_name LevelLoader
extends Node2D


func _init(mgr: LevelManager, key: String, scene: Node):
	name = key

	if get_parent():
		reparent(mgr)
	else:
		mgr.add_child(self, true)

	if scene:
		if scene.get_parent() is Node:
			scene.reparent(self)
		else:
			self.add_child(scene, true)
