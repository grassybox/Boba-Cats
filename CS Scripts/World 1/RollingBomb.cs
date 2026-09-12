using Godot;
using System;

public partial class RollingBomb : RigidBody2D
{
	public float bombMass{get;} = 6.8f;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Mass = bombMass; //in kilograms
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionPressed("reset_bomb")) {
			var bomb = GetNode<Sprite2D>("Bomb");
			bomb.GlobalPosition = new Vector2(205, 119);
			var shape = GetNode<CollisionShape2D>("CollisionShape2D");
			shape.GlobalPosition = new Vector2(205, 119);
		}
	}

	private async void OnTimerTimeout()
	{
		GetNode<Node2D>("BOOM").Show();
		await ToSignal(GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);
		Hide();
		SetDeferred("freeze", true);
	}
}
