@tool
class_name PlayerSprite
extends Node2D

@export var player: Player
@export_group("Animation Parameters")
## Facing when performing actions (getting up or opening a lantern)
@export var action_facing: Facing.Horizontal = Facing.Horizontal.RIGHT
@export_subgroup("Directional Movement")
@export var facing: Facing.Direction = Facing.Direction.DOWN:
	get:
		return facing
	set(value):
		facing = value
		if Facing.is_h(value):
			action_facing = Facing.as_h(value)
@export var is_moving: bool = false
@export_subgroup("Prone")
@export var is_prone: bool = false:
	get:
		return is_prone
	set(value):
		var was_already_prone := is_prone
		is_prone = value

		if value and not was_already_prone:
			_set_state_to_prone.call_deferred()
@export_subgroup("Cutscenes and actions")
@export var is_in_intro: bool = false
## HACK: Function callback tracks are not executed in-editor (prevents preview)
##       Exporting as write-only proerty circumvents this limitiation.
@export_custom(PROPERTY_HINT_ONESHOT, "") var open_lantern: bool = false:
	get:
		return false
	set(value):
		if value:
			_open_lantern()

@export_tool_button("Open lantern")
var debug_open_lantern_action = _open_lantern
var is_controlled_by_cutscene: bool:
	get:
		return is_in_intro
var _anim_state_machine: AnimationNodeStateMachinePlayback:
	get:
		var playback = anim_tree.get("parameters/playback")
		var state_machine := playback as AnimationNodeStateMachinePlayback
		assert(state_machine, "Failed to access player animation state machine!")

		return state_machine

@onready var anim_tree: AnimationTree = $AnimationTree


func _ready() -> void:
	if not player or Engine.is_editor_hint():
		return

	Signals.try_connect(player.dead, _on_player_dead)


func _process(_delta: float) -> void:
	if not player or Engine.is_editor_hint():
		return

	if is_controlled_by_cutscene:
		return

	facing = player.facing
	is_moving = player.is_moving
	is_prone = player.is_prone


func _open_lantern() -> void:
	_anim_state_machine.travel("open_lantern")


func _on_player_dead() -> void:
	_set_state_to_prone()


func _set_state_to_prone() -> void:
	_anim_state_machine.travel("prone")
