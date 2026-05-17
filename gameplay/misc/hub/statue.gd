@tool
extends PlayerTriggerArea2D

var _first_time_conversation: DialogueConversation = preload("uid://cdm6mmqql1n6")
var _victory_conversation: DialogueConversation = preload("uid://dluw81ekbnloc")
var _is_first_time: bool = true

@onready var sprite: AnimatedSprite2D = $Sprite


func _ready() -> void:
	Signals.try_connect(player_entered, _on_player_entered)
	Signals.try_connect(player_exited, _on_player_exited)


func _on_player_entered(_player: Player, _wisp: Wisp) -> void:
	sprite.play("wisp_inside")

	var is_objective_complete: bool = Persistent.wisps_saved == 7
	if is_objective_complete:
		Jukebox.switch_track(Jukebox.Track.CREDITS)
		Persistent.player_controller.movement.is_allowed = false

		Conversation.begin(_victory_conversation)
		await Conversation.finished

		await Persistent.main_camera.fade_to_black()
		# FIXME: can we drop this?
		Jukebox.switch_track(Jukebox.Track.CREDITS)

		await get_tree().create_timer(3.0).timeout

		var win: Node2D = get_tree().get_first_node_in_group("ViineriTpTarget")
		Persistent.player.teleport(win)

		await Persistent.main_camera.fade_to_visible()
		Persistent.player_controller.movement.is_allowed = true

	elif _is_first_time:
		_is_first_time = false
		Conversation.begin(_first_time_conversation)


func _on_player_exited(_player: Player, _wisp: Wisp) -> void:
	sprite.play("idle")
