class_name MusicArea
extends Area2D

@export var track: BGMusic.Track = BGMusic.Track.HUB


func _ready() -> void:
	Signals.try_connect(body_entered, _body_entered)


func _body_entered(body: Node2D) -> void:
	if body is not Player:
		return

	Jukebox.switch_track(track)

	# HACK: change footstep sounds to wet variant when moving to swamp
	var player := Persistent.player
	if track == Jukebox.Track.SWAMP:
		player.is_wet = true
	else:
		player.is_wet = false
