extends Node2D

@onready var WispTarget: WispTargetPosition = $WispTargetPosition

func _OnPlayerTeleported() -> void:
	ResetWispPosition();
	
func _OnInteractOrInspectFinished() -> void:
	ResetWispPosition();

func ResetWispPosition() -> void:
	WispTarget.ResetIdlePosition()
