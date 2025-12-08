@tool
class_name TouchTrigger
extends Node
## Proximity action trigger.
##
## Triggers actions when something enters its parent Area2D. Must be placed as a
## child of an Area2D.

signal fire(cause: Node2D)

## How the trigger behaves.
enum Mode {
	## Perform the actions when the area is entered.
	ON_ENTER,

	## Perform the actions when the area is exited.
	ON_EXIT,
}

## Only execute the actions once.
@export var one_shot: bool = false
## Remove the area after executing the actions.
@export var remove_after_firing: bool = false
## Operation mode for this trigger.
@export var mode: Mode = Mode.ON_ENTER

var _trigger_count: int = 0


func _ready() -> void:
	var area: Area2D = get_parent() as Area2D
	if not area:
		return

	Signals.try_connect(area.body_entered, _on_body_entered)
	Signals.try_connect(area.body_exited, _on_body_exited)


func _on_body_entered(body: Node2D) -> void:
	# FIXME: why is this triggered early
	if Engine.is_editor_hint():
		return

	if mode == Mode.ON_ENTER:
		_trigger(body)


func _on_body_exited(body: Node2D) -> void:
	if Engine.is_editor_hint():
		return

	if mode == Mode.ON_EXIT:
		_trigger(body)


func _trigger(cause: Node2D) -> void:
	if one_shot and _trigger_count > 0:
		return

	_trigger_count += 1

	fire.emit(cause)

	if one_shot and remove_after_firing:
		var area: Area2D = get_parent() as Area2D
		if area and not Engine.is_editor_hint():
			area.queue_free()
