using Godot;
using System;
using System.Threading.Tasks;

public partial class VineRoom : Node2D
{
	private bool transitioning = false;

	// Called when the node enters the scene tree for the first time.
	public override async void _Ready() 
	{
		if (GetNode<Node2D>("UnderwaterPlayer").Position.X < 200)
		{
			Camera2D camera = GetNode<Camera2D>("UnderwaterPlayer/Camera2D");
			camera.SetLimit(Side.Right, 500);
			camera.SetLimit(Side.Bottom, 180);

			transitioning = true;
			AnimationPlayer anim = GetNode<AnimationPlayer>("AnimationPlayer");
			anim.Play("enter_room");
			await anim.ToSignal(anim, AnimationPlayer.SignalName.AnimationFinished);
			transitioning = false;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override async void _Process(double delta)
	{
		if (!transitioning)
		{
			await NextRoomCheck();
		}
	}

	private void OnGrowableVineAreaLit(Area2D flashlight)
	{
		GetNode<Node2D>("Walls/SecretDoor").Position = new Vector2(360, -50);
	}

	private async Task NextRoomCheck() {
		var player = GetNode<CharacterBody2D>("UnderwaterPlayer");
		var GlobalSceneChange = GetNode<GlobalSceneChange>("/root/GlobalSceneChange");
		Vector2 pos = player.Position;
		if (pos.Y > 188) {
			transitioning = true;
			await GlobalSceneChange.ChangeRoom(new Vector2(187, 138), "jellyfish_room", true);
		}
		else if (pos.X > 508) {
			transitioning = true;
			await GlobalSceneChange.ChangeRoom(new Vector2(40, 558), "tall_tube_coral_room", true);
		
		}
		else if (pos.Y < -8)
		{
			transitioning = true;
			await GlobalSceneChange.ChangeRoom(new Vector2(220, 132), "secret_room_1", true);
		}
	}
}
