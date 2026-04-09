@tool
class_name DialogueUINew
extends Control

var is_in_transition: bool:
	get:
		return _is_in_transition or _line_container.is_playing_animation
var _is_smoke_visible: bool = false
var _is_in_transition: bool = false

@onready var _smoke_layers: Control = $SmokeLayers
@onready var _line_container: DialogueRowContainer = $DialogueRowContainer


func end_dialogue() -> void:
	_line_container.end_dialogue()

	_is_in_transition = true
	await _slide_out_smoke()
	_is_in_transition = false


func show_line(
		text: String,
		speaker: DialogueSpeaker,
		side: DialogueLine2.Side,
) -> void:
	if not _is_smoke_visible:
		_is_in_transition = true
		await _slide_in_smoke()
		_is_in_transition = false

	_line_container.next(text, speaker, side)


func show_choice(
		speaker: DialogueSpeaker,
		side: DialogueLine2.Side,
		a: String,
		b: String,
		c: String,
) -> void:
	if not _is_smoke_visible:
		_is_in_transition = true
		await _slide_in_smoke()
		_is_in_transition = false

	_line_container.next_choice(speaker, side, a, b, c)


func highlight_option(option: int) -> void:
	_line_container.highlight_option(option)


func reset() -> void:
	for smoke_layer in _smoke_layers.get_children():
		smoke_layer.anchor_top = 1.0
		smoke_layer.anchor_bottom = 1.0

	_line_container.reset()
	_is_smoke_visible = false


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

	# HACK: don't wait for tween to make the fade-out-to-gameplay smoother
	await get_tree().create_timer(0.33 + delay).timeout
	_is_smoke_visible = true
