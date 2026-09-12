using Godot;
using System;
using System.Threading.Tasks;

public partial class EnterCaveRoom : Node2D
{
	private bool transitioning = false;
	private Node2D Player;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Player = GetNode<CharacterBody2D>("GroundPlayer");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override async void _Process(double delta)
	{
		if (!transitioning)
		{
			await NextRoomCheck();
		}
		if (Player.Position.Y < 85 && !GlobalScript.Inventory.Contains("Flashlight"))
		{
			TextBox pText = Player.GetNode<TextBox>("TextBox");
			pText.SetLabel("Dash");
			await pText.ShowText("That ledge is too high for me to reach!");
			await pText.ShowText("Maybe I can find something back in the town that can help me...");
		}
	}

	private void OnVineGrown(Node2D flashlight)
	{
		GetNode<Label>("Label").Show();
	}

	private async Task NextRoomCheck() {
		var player = GetNode<CharacterBody2D>("GroundPlayer");
		var GlobalSceneChange = GetNode<GlobalSceneChange>("/root/GlobalSceneChange");
		Vector2 pos = player.Position;
		if (pos.X > 328) {
			transitioning = true;
			await GlobalSceneChange.ChangeRoom(new Vector2(10, 90), "cave_room", true);
		}
		else if (pos.Y > 140) {
			transitioning = true;
			//fall behind water
			GetNode<Player>("GroundPlayer").ZIndex = -5; //= why not just use the player variable
			await GlobalSceneChange.ChangeRoom(new Vector2(160, 20), "tall_tube_coral_room", false);
		}
	}
}
