@tool
class_name NeckAttributes
extends Resource

## Movement speed while in moving state.
@export var speed: float = 45.0
## The rate of detection gain when player is in sight, units per second.
@export var detection_gain: float = 100.0
## The rate of detection decay when player is out of sight, units per second.
@export var detection_decay: float = 60.0
## The amount of time the movement state lasts, in seconds.
@export var move_time: float = 3.0
## The chance neck stops moving when movement state ends.
@export var stop_move_chance: float = 0.4
@export var change_direction_instead_of_stopping_chance: float = 0.1
@export var go_underwater_chance: float = 0.4
@export var min_underwater_time: float = 1.0
@export var max_underwater_time: float = 3.0
@export var min_idle_time: float = 0.5
@export var max_idle_time: float = 2.0
@export var emerge_at_player_chance: float = 0.35
@export var emerge_at_same_location_chance: float = 0.0
## Maximum time the neck can spend alert, in seconds.
@export var alert_time: float = 7.5
## The required detection level until the neck becomes alert.
@export var alert_threshold: float = 40.0
## The required detection level until the neck can attack.
@export var attack_threshold: float = 100.0
## Delay between entering attack state and hand snatching the player. This is
## essentially the time player has to react to the incoming attack.
@export var attack_time: float = 1.0
@export var attack_animation_speed: float = 1.0
@export var emerge_animation_speed: float = 1.0
