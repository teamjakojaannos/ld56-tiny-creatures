extends Node2D

var Persistent = load("res://Persistent.cs")

func _physics_process(_delta: float) -> void:
	global_position = Persistent.Instance(self).Player.global_position
