using System;
using Godot;

public partial class Player : CharacterBody2D {
	[Export]
	public float Speed = 300.0f;

	[Export]
	public float Friction = 10.0f;

	[Export]
	public AnimationPlayer? Animation;

	public override void _Ready() {
		base._Ready();
		var spawns = GetTree().GetNodesInGroup("PlayerSpawn");

		if (spawns.PickRandom() is Node2D spawn) {
			CallDeferred(MethodName.TeleportAt, spawn);
		}
	}

	private void TeleportAt(Node2D target) {
		if (!target.IsInsideTree() || target.IsQueuedForDeletion()) {
			throw new InvalidOperationException("Tried teleporting to a non-existent/deleted node!");
		}

		GetParent()?.RemoveChild(this);
		target.AddChild(this);

		GlobalPosition = target.GlobalPosition;
	}

	public override void _PhysicsProcess(double _delta) {
		var delta = (float)_delta;

		var direction = Input.GetVector("left", "right", "up", "down");
		var animationDirection = direction.X < 0.0
			? "left"
			: direction.X > 0.0
			? "right"
			: direction.Y < 0.0
			? "left"
			: "right";
		if (direction.LengthSquared() > 0.001f) {
			Velocity = direction * Speed * delta;

			Animation?.Play($"walk_{animationDirection}");
		} else {
			var currentSpeed = Velocity.Length();
			Velocity = new(
				Mathf.MoveToward(Velocity.X, 0, currentSpeed * Friction * delta),
				Mathf.MoveToward(Velocity.Y, 0, currentSpeed * Friction * delta)
			);
		}

		if (Velocity.LengthSquared() < 0.01f) {
			Animation?.Play("idle");
		}
		MoveAndCollide(Velocity);
	}
}
