extends HBoxContainer

@onready var numbers = $Numbers
@onready var options = $Options
@onready var selection = $Selection

func _ready() -> void:
	setup_numbers()
	select_option(0)

func setup_numbers() -> void:
	var option_count = options.text.count("\n") + 1
	
	numbers.text = "";
	for i in option_count:
		numbers.text += "%d: " % (i + 1)

		if i != option_count - 1:
			numbers.text += "\n"

func _unhandled_input(event: InputEvent) -> void:
	if event.is_action_pressed("dialogue_option_1"):
		select_option(0)
	elif event.is_action_pressed("dialogue_option_2"):
		select_option(1)
	elif event.is_action_pressed("dialogue_option_3"):
		select_option(2)

func select_option(option: int) -> void:
	var option_count = options.text.count("\n") + 1
	
	selection.text = "";
	for i in option_count:
		if i == option:
			selection.text += ">"
		else:
			selection.text += " "

		if i != option_count - 1:
			selection.text += "\n"
