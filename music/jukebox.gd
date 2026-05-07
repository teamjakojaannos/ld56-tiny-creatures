class_name BGMusic
extends Node

enum Track {
	FOREST,
	HUB,
	DANGER,
	SWAMP,
	PSYCHOSIS,
	CREDITS,
}

const ALL_TRACKS: Array[Track] = [
	Track.FOREST,
	Track.HUB,
	Track.DANGER,
	Track.SWAMP,
	Track.PSYCHOSIS,
	Track.CREDITS,
]

@export var fade_in_speed := 0.05
@export var fade_out_speed := 0.5

var _current_track: Track = Track.HUB
var _is_in_combat = false

@onready var _track_forest: AudioStreamPlayer = $Forest
@onready var _track_hub: AudioStreamPlayer = $Hub
@onready var _track_danger: AudioStreamPlayer = $Danger
@onready var _track_swamp: AudioStreamPlayer = $Swamp
@onready var _track_psychosis: AudioStreamPlayer = $Psychosis
@onready var _track_credits: AudioStreamPlayer = $Credits


func _ready() -> void:
	for track in ALL_TRACKS:
		var stream := _get_stream_player(track)
		if not track:
			printerr("Missing track: %s" % track)
			continue

		stream.volume_db = linear_to_db(0.0)
		if track == _current_track:
			stream.play()


func _process(delta: float) -> void:
	for track in ALL_TRACKS:
		var stream = _get_stream_player(track)
		if not stream:
			continue

		var is_current_track := not _is_in_combat and track == _current_track

		var target_volume := 1.0 if is_current_track else 0.0
		var fade_speed = fade_in_speed if is_current_track else fade_out_speed

		var volume := db_to_linear(stream.volume_db)
		var adjusted := move_toward(volume, target_volume, fade_speed * delta)
		stream.volume_db = linear_to_db(adjusted)

		if track != _current_track and adjusted < 0.001:
			stream.stop()

	var combat_target_vol := 1.0 if _is_in_combat else 0.0
	var combat_fade_spd := fade_in_speed if _is_in_combat else fade_out_speed

	var combat_vol := db_to_linear(_track_danger.volume_db)
	var combat_adjusted := move_toward(combat_vol, combat_target_vol, combat_fade_spd * delta)
	_track_danger.volume_db = linear_to_db(combat_adjusted)

	if not _is_in_combat and combat_adjusted < 0.0001:
		_track_danger.stop()


func switch_track(track: Track) -> void:
	if _current_track == Track.CREDITS:
		return

	_current_track = track

	for t in ALL_TRACKS:
		var stream = _get_stream_player(t)
		if not stream:
			printerr("Missing track: %s" % track)
			continue

		if track == t and not stream.playing:
			stream.play()


func start_chase() -> void:
	_is_in_combat = true
	if not _track_danger.playing:
		_track_danger.play()


func stop_chase() -> void:
	_is_in_combat = false


func _get_stream_player(track: Track) -> AudioStreamPlayer:
	match track:
		Track.FOREST:
			return _track_forest
		Track.HUB:
			return _track_hub
		Track.DANGER:
			return _track_danger
		Track.SWAMP:
			return _track_swamp
		Track.PSYCHOSIS:
			return _track_psychosis
		Track.CREDITS:
			return _track_credits
		_:
			printerr("Unimplemented track %s" % track)
			return null
