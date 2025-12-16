@tool
class_name PlayerSprite
extends Node2D

@export var player: Player
@export_group("Debug Animation Parameters")
@export_subgroup("Directional Movement")
@export var facing: Facing.Direction = Facing.Direction.DOWN:
	get:
		return facing
	set(value):
		facing = value
		if Facing.is_h(value):
			prone_facing = Facing.as_h(value)
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
@export var prone_facing: Facing.Horizontal = Facing.Horizontal.RIGHT

@onready var anim_tree: AnimationTree = $AnimationTree


func _ready() -> void:
	if not player or Engine.is_editor_hint():
		return

	Signals.try_connect(player.dead, _on_player_dead)


func _process(_delta: float) -> void:
	if not player or Engine.is_editor_hint():
		return

	facing = player.facing
	is_moving = player.is_moving
	is_prone = player.is_prone


func _on_player_dead() -> void:
	_set_state_to_prone()


func _set_state_to_prone() -> void:
	var playback = anim_tree.get("parameters/playback")
	var state_machine := playback as AnimationNodeStateMachinePlayback
	if not state_machine:
		push_error("Player AnimationTree root isn't a State Machine!")
		return

	state_machine.travel("prone")
