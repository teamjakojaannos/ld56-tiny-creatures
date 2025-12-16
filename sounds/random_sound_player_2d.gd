class_name RandomSoundPlayer2D
extends Node2D


func play_random() -> void:
	var idle_streams: Array[AudioStreamPlayer2D] = []
	for child in get_children():
		var sound := child as AudioStreamPlayer2D
		if not sound:
			continue

		if not sound.is_playing():
			idle_streams.push_back(sound)

	if idle_streams.is_empty():
		push_warning("No idle audio streams available! Skipping!")
		return

	idle_streams.pick_random().play()
