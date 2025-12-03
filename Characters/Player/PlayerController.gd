class_name PlayerController
extends Node2D

@onready var wisp_target: WispTargetPosition = $WispTargetPosition

@onready var player: PlayerCharacter = $Player
@onready var wisp: WispCharacter = $Wisp

func _on_Player_teleported() -> void:
	_reset_wisp_position()

func _on_InteractionController_interaction_finished() -> void:
	_reset_wisp_position()

func _on_InteractionController_inspection_finished() -> void:
	_reset_wisp_position()


func _reset_wisp_position() -> void:
	wisp_target.ResetIdlePosition()
