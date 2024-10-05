using System;
using Godot;

public partial class Player : CharacterBody2D {
	[Export]
	public float Speed = 300.0f;

	[Export]
	public float Friction = 10.0f;

	public override void _Ready() {
		base._Ready();
		var spawns = GetTree().GetNodesInGroup("PlayerSpawn");
		spawns.Shuffle();

		if (spawns[0] is Node2D spawn) {
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
		if (direction.LengthSquared() > 0.001f) {
			Velocity = direction * Speed * delta;
		} else {
			var currentSpeed = Velocity.Length();
			Velocity = new(
				Mathf.MoveToward(Velocity.X, 0, (currentSpeed * Friction) * delta),
				Mathf.MoveToward(Velocity.Y, 0, (currentSpeed * Friction) * delta)
			);
		}

		MoveAndCollide(Velocity);
	}
}
