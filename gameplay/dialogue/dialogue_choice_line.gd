@tool
class_name DialogueChoiceLine2
extends DialogueLine2

enum NumberOfChoices {
	TWO,
	THREE,
}

@export var number_of_choices: NumberOfChoices = NumberOfChoices.TWO
@export_group("Choice A")
@export var choice_text_a: String
@export var choice_branch_a: DialogueConversation
@export_group("Choice B")
@export var choice_text_b: String
@export var choice_branch_b: DialogueConversation
@export_group("Choice C")
@export var choice_text_c: String
@export var choice_branch_c: DialogueConversation
