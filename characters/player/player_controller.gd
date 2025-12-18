class_name PlayerController
extends Node2D

@onready var wisp_target: WispFollowTarget = $Player/WispFollowTarget
@onready var player: Player = $Player
@onready var wisp: Wisp = $Wisp
@onready var movement: MovementController = $MovementController


func _on_player_teleported() -> void:
	wisp_target.reset_idle_position(true)
	wisp.global_position = wisp_target.global_position
	wisp.reset_physics_interpolation()


func _on_Player_teleported() -> void:
	_on_player_teleported()


func _on_InteractionController_interaction_finished() -> void:
	wisp_target.reset_idle_position()


func _on_InteractionController_inspection_finished() -> void:
	wisp_target.reset_idle_position()
