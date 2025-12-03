@tool
extends Node2D

@export_custom(PROPERTY_HINT_NODE_TYPE, "Sprite2D,AnimatedSprite2D")
var sprite: Node2D:
	get:
		return sprite
	set(value):
		if not Objects.is_null(value) and \
				value is not Sprite2D and \
				value is not AnimatedSprite2D:
			push_error(""""%s" is not a 2D sprite!""" % value)
			return

		if not Objects.is_null(sprite):
			sprite.item_rect_changed.disconnect(queue_redraw)

		sprite = value
		sprite.item_rect_changed.connect(queue_redraw)
		queue_redraw()
		update_configuration_warnings()

@export_range(0.0, 1.0) var percentage_visible: float = 1.0:
	get:
		return percentage_visible
	set(value):
		percentage_visible = value
		queue_redraw()

@export var left_to_right = true:
	get:
		return left_to_right
	set(value):
		left_to_right = value
		queue_redraw()


const DEFAULT_BOUNDS: Rect2 = Rect2(-16, -16, 32, 32)

var bounds: Rect2:
	get:
		if not Objects.is_null(sprite):
			var size: Vector2 = Vector2.ZERO
			var pos: Vector2 = Vector2.ZERO
			if sprite is Sprite2D:
				size = sprite.get_rect().size
				pos = sprite.get_rect().position
			elif sprite is AnimatedSprite2D:
				var anim = sprite.animation
				var frame = sprite.frame
				var tex = sprite.sprite_frames.get_frame_texture(anim, frame)
				size = tex.get_size()

				if sprite.centered:
					pos = size * -0.5
				else:
					pos = Vector2.ZERO

			return Rect2(pos, size)

		return DEFAULT_BOUNDS

func _get_configuration_warnings() -> PackedStringArray:
	var errors = PackedStringArray()
	if Objects.is_null(sprite):
		errors.push_back("Target sprite is not set!")
	elif sprite is not Sprite2D and sprite is not AnimatedSprite2D:
		errors.push_back(""""%s" is not a 2D sprite!""" % sprite)

	return errors

# Ensure clipping is set *by-default* when constructing...
func _init() -> void:
	clip_children = CanvasItem.CLIP_CHILDREN_ONLY

# ...but don't *enforce* clipping in-editor. Only force clip mode in-game.
func _ready() -> void:
	if not Engine.is_editor_hint():
		clip_children = CanvasItem.CLIP_CHILDREN_ONLY

# Need to draw something on screen to generate the clipping mask. The mask
# consists of any pixels drawn here.
func _draw() -> void:
	var adjusted = Rect2(bounds)
	if left_to_right:
		adjusted.size.x *= percentage_visible
	else:
		var total_width = bounds.size.x
		var modified_width = bounds.size.x * percentage_visible
		var offset_x = total_width - modified_width
		adjusted.position.x += offset_x
		adjusted.size.x = modified_width

	draw_rect(adjusted, Color.RED, true)
