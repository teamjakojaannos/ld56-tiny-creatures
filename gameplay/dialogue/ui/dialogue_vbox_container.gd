@tool
class_name DialogueVBoxContainer
extends Container

var anim_offset: float = 0.0:
	get:
		return anim_offset
	set(value):
		anim_offset = value
		queue_sort()


func _notification(n):
	if n == NOTIFICATION_SORT_CHILDREN:
		var total_height: float = 0.0
		for child in get_children():
			total_height += 64.0

		for child in get_children():
			if child is not DialogueComponent:
				continue

			var pos := Vector2(child.offset_x, position.y - total_height + child.anim_offset)
			var s := Vector2(self.size.x, 64.0)
			child.position = pos
			child.size = s

			total_height -= 64.0
