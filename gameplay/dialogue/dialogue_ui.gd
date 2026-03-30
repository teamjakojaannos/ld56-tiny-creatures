@tool
extends Control

@export_group("Debug controls")
@export_tool_button("Next line")
var debug_next_button = _debug_next
@export_tool_button("Reset")
var debug_reset_button = _debug_reset

@onready var _smoke_layers: Control = $SmokeLayers


func _debug_next() -> void:
	await _slide_in_smoke()


func _debug_reset() -> void:
	for smoke_layer in _smoke_layers.get_children():
		smoke_layer.anchor_top = 1.0
		smoke_layer.anchor_bottom = 1.0


func _slide_in_smoke() -> void:
	var tween := create_tween()
	tween = create_tween()
	tween.set_ease(Tween.EASE_IN_OUT)
	tween.set_trans(Tween.TRANS_QUAD)
	tween.set_parallel(true)

	for smoke_layer in _smoke_layers.get_children():
		tween.tween_property(smoke_layer, "anchor_top", 0.0, 2.0)

	await tween.finished
