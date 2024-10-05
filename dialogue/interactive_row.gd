extends HBoxContainer

@onready var numbers = $Numbers
@onready var options = $Options
@onready var selection = $Selection

@export var dialogue_color: Color = Color.DARK_GRAY
@export var selected_dialogue_color: Color = Color.WHITE

var hightlighted_option: int = 0

func _ready() -> void:
	setup_numbers()
	highlight_option(0)

func setup_numbers() -> void:
	var option_count = count_options()
	
	numbers.text = "";
	for i in option_count:
		numbers.text += "%d: " % (i + 1)

		if i != option_count - 1:
			numbers.text += "\n"

func _unhandled_input(event: InputEvent) -> void:
	if event.is_action_pressed("dialogue_option_1"):
		highlight_option(0)
	elif event.is_action_pressed("dialogue_option_2"):
		highlight_option(1)
	elif event.is_action_pressed("dialogue_option_3"):
		highlight_option(2)
	elif event.is_action_pressed("dialogue_select_option"):
		select()

func count_options() -> int:
	return options.get_child_count()

func select() -> void:
	pass

func highlight_option(option: int) -> void:
	var option_count = count_options()
	hightlighted_option = option
	
	selection.text = "";
	for i in option_count:
		var option_label: Label = options.get_child(i)

		if i == option:
			selection.text += ">"
			option_label.label_settings.font_color = selected_dialogue_color
		else:
			selection.text += " "
			option_label.label_settings.font_color = dialogue_color

		if i != option_count - 1:
			selection.text += "\n"
