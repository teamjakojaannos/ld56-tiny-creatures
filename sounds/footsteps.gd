class_name Footsteps
extends Node2D

enum SurfaceMaterial {
	GRASS,
	WET,
}

@export var sounds: Dictionary[SurfaceMaterial, RandomSoundPlayer2D] = { }

var _current_surface: SurfaceMaterial = SurfaceMaterial.GRASS

@onready var timer: Timer = $FootstepTimer


func _play(surface: SurfaceMaterial) -> void:
	var audio_player := sounds[surface]
	if not audio_player:
		var args = [self.get_path(), surface]
		push_warning("%s is missing footstep sounds for %s" % args)
		return

	audio_player.play_random()


func _on_player_moving_start() -> void:
	timer.start()


func _on_player_moving_stop() -> void:
	timer.stop()


func _on_footstep_timer_timeout() -> void:
	_play(_current_surface)
