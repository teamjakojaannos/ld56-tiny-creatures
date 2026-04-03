@tool
class_name DialogueUINew
extends Control

signal input_progress

@export var debug_dialogue_lines: Array[DialogueLine2] = []

@export_group("Debug controls")
@export_tool_button("Start")
var debug_start_button = _debug_start
@export_tool_button("Next line")
var debug_next_button = _debug_next
@export_tool_button("Reset")
var debug_reset_button = reset
var _is_smoke_visible: bool = false
var _is_in_transition: bool = false

@onready var _smoke_layers: Control = $SmokeLayers
@onready var _line_container: DialogueRowContainer = $DialogueRowContainer


func _input(event: InputEvent) -> void:
	if event.is_action_pressed("gui_accept"):
		if _line_container.is_playing_animation:
			return

		input_progress.emit()


func end_dialogue() -> void:
	_line_container.end_dialogue()

	_is_in_transition = true
	await _slide_out_smoke()
	_is_in_transition = false


func show_line(text: String, speaker: DialogueSpeaker, side: DialogueLine2.Side) -> void:
	if not _is_smoke_visible:
		_is_in_transition = true
		await _slide_in_smoke()
		_is_in_transition = false

	_line_container.next(text, speaker, side)


func reset() -> void:
	for smoke_layer in _smoke_layers.get_children():
		smoke_layer.anchor_top = 1.0
		smoke_layer.anchor_bottom = 1.0

	_line_container.reset()
	_is_smoke_visible = false


func _debug_start() -> void:
	preload("uid://bq75metace7hw").show_lines(self, debug_dialogue_lines)


func _debug_next() -> void:
	input_progress.emit()


func _slide_in_smoke() -> void:
	if _is_smoke_visible:
		return

	var pos_tween := create_tween()
	pos_tween.set_ease(Tween.EASE_IN_OUT)
	pos_tween.set_trans(Tween.TRANS_QUAD)
	pos_tween.set_parallel(true)

	var mod_tween := create_tween()
	pos_tween.set_ease(Tween.EASE_IN_OUT)
	pos_tween.set_trans(Tween.TRANS_QUAD)
	pos_tween.set_parallel(true)

	var delay := 0.0
	var layers := _smoke_layers.get_children()
	layers.reverse()
	for smoke_layer in layers:
		smoke_layer.modulate = Color.TRANSPARENT
		pos_tween.tween_property(smoke_layer, "anchor_top", 0.0, 2.0 + delay)
		mod_tween.tween_property(smoke_layer, "modulate", Color.WHITE, 5.0 + delay)
		delay += 0.25

	# HACK: Do not wait for mod tween to allow the darken effect take longer
	await pos_tween.finished
	_is_smoke_visible = true


func _slide_out_smoke() -> void:
	if not _is_smoke_visible:
		return

	var tween := create_tween()
	tween.set_ease(Tween.EASE_IN_OUT)
	tween.set_trans(Tween.TRANS_QUAD)
	tween.set_parallel(true)

	var delay := 0.0
	var layers := _smoke_layers.get_children()
	layers.reverse()
	for smoke_layer in layers:
		smoke_layer.modulate = Color.WHITE
		tween.tween_property(smoke_layer, "anchor_top", 1.0, 2.0 + delay)
		tween.tween_property(smoke_layer, "modulate", Color.TRANSPARENT, 0.5 + delay)
		delay += 0.25

	await tween.finished
	_is_smoke_visible = true
