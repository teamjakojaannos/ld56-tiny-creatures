@tool
class_name SpiritLantern
extends PlayerTriggerArea2D

@export var location: String = ""
@export var animation: String = "out"

var is_saved: bool:
	get:
		return Persistent.is_wisp_saved(location)
var _conversation: DialogueConversation = preload("uid://cjhwaf55p1o2g")

@onready var _animations: AnimationPlayer = $SpiritLantern/AnimationPlayer


func _ready() -> void:
	Signals.try_connect(player_entered, _on_player_entered)
	Signals.try_connect(player_exited, _on_player_exited)


func _on_player_entered(_player: Player, _wisp: Wisp) -> void:
	if is_saved:
		return

	Signals.try_connect(Conversation.option_chosen, _on_option_chosen)
	Conversation.begin(_conversation)


func _on_player_exited(_player: Player, _wisp: Wisp) -> void:
	Signals.try_disconnect(Conversation.option_chosen, _on_option_chosen)


func _on_option_chosen(option: int) -> void:
	if option != 0:
		return

	_animations.play(animation)
	await _animations.animation_finished

	Persistent.save_wisp(location)
