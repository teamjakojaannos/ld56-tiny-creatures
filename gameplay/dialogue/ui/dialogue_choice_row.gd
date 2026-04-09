@tool
class_name DialogueChoiceRow
extends DialogueComponent

@export_tool_button("Highlight A")
var debug_highlight_a = highlight_option.bind(0)
@export_tool_button("Highlight B")
var debug_highlight_b = highlight_option.bind(1)
@export_tool_button("Highlight C")
var debug_highlight_c = highlight_option.bind(2)
var highlighted_option: int = 0
var max_option: int = 2

@onready var indicator: Control = $HBoxContainer/Indicator
@onready var option_a: Label = $HBoxContainer/Options/A
@onready var option_b: Label = $HBoxContainer/Options/B
@onready var option_c: Label = $HBoxContainer/Options/C
@onready var number_c: Label = $HBoxContainer/Numbers/C
@onready var indicator_c: Label = $HBoxContainer/Numbers/C


func _ready() -> void:
	highlight_option.call_deferred(0)


func set_options(a: String, b: String, c: String) -> void:
	option_a.text = a
	option_b.text = b
	option_c.text = c

	if c.is_empty():
		max_option = 1
		option_c.hide()
		number_c.hide()
		indicator_c.hide()
	else:
		max_option = 2
		option_c.show()
		number_c.show()
		indicator_c.show()


func highlight_option(option: int) -> void:
	option = clampi(option, 0, max_option)
	highlighted_option = option

	for child_idx in 3:
		var indicator_option = indicator.get_child(child_idx)
		indicator_option.text = ">" if child_idx == option else ""
