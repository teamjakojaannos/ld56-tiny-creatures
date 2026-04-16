@tool
class_name PersistentState
extends Node

## Player has fully respawned and stood up.
signal player_respawned
## Game has faded out after death.
signal player_respawning
## Player releases a wisp at given location.
signal wisp_saved(location: String)

@export var player: Player
@export var player_controller: PlayerController
@export var main_camera: CameraManager
@export var wisp: Wisp

var wisps_saved: int = 0
var state: Array[String] = []


func _ready() -> void:
	# Workaround https://github.com/godotengine/godot/issues/71373
	# TL;DR:
	# Persistent is an autoload, but when [Tool]-script gets autoloaded at
	# editor startup, it ends up being created as a direct child of the editor
	# window. The persistent scene contains the default player controller
	# instance, which in turn has a camera as the child. This camera is then the
	# first one in the hierarchy, taking priority over the editor viewport
	# "camera". This completely breaks the editor.
	if Engine.is_editor_hint() and get_viewport() is Window:
		# ...to fix this, detach the autoload instance from the scene tree.
		# Freeing the instance seems to break the project settings dialog, so
		# the instance is left dangling.
		get_parent().remove_child(self)


func is_wisp_saved(location: String) -> bool:
	return state.has("wisp_%s" % location)


func save_wisp(location: String) -> void:
	wisps_saved += 1
	state.push_back("wisp_%s" % location)
	wisp_saved.emit(location)


func reset_player_to_hub() -> void:
	var spawnpoints := get_tree().get_nodes_in_group("HubSpawn")
	var spawnpoint: Node = null
	if spawnpoints.is_empty():
		print("No hub spawnpoints available: Falling back to scene root!")
		spawnpoint = LevelManager.current_scene
	else:
		spawnpoint = spawnpoints.pick_random()

	if spawnpoint is not Node2D:
		var path = spawnpoint.get_path()
		printerr("Spawnpoint \"%s\" is not valid!" % path)
		return

	player_controller.movement.is_allowed = false
	await main_camera.fade_to_black(2.5)

	player.teleport.call_deferred(spawnpoint)
	await player.teleported

	player_respawning.emit()

	player.is_prone = true
	player.is_in_trouble = false

	await main_camera.fade_to_visible(1.5)
	player.is_prone = false

	# FIXME: await player.standing
	await get_tree().create_timer(2.5).timeout
	player_controller.movement.is_allowed = true
	player.is_dead = false

	player_respawned.emit()
