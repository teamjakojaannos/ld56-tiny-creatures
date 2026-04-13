class_name HubWisp
extends Node2D

@export var location: String = ""

var _saved: bool = false


func _ready() -> void:
	visible = false
	Signals.try_connect(Persistent.wisp_saved, _on_saved)


func _on_saved(where: String) -> void:
	if _saved or where != location:
		return

	_saved = true
	visible = true
