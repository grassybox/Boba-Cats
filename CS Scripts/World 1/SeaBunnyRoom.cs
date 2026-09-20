using Godot;
using System;
using System.Threading.Tasks;

public partial class SeaBunnyRoom : Node2D
{
	private bool transitioning = false;
	private Player Player;
	private Seabunny SeaBunny;
	private bool cameraGliding = false;
	private bool waitingForRespawn = false;
	private AnimationPlayer AnimationP;
	private AnimationPlayer gateAnim;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Player =  GetNode<Player>("GroundPlayer");
		SeaBunny = GetNode<Seabunny>("Seabunny");
		AnimationP = GetNode<AnimationPlayer>("AnimationPlayer");
		gateAnim = GetNode<AnimationPlayer>("GateAnimationPlayer");
		gateAnim.Play("gate_open");

		if (GlobalScript.CQ("short") == "GetBoat")
		{
			GetBoatCutscene();	
		}
		if (GlobalScript.IsAfterQuest("Seabunny"))
		{
			GetNode<GrowableVine>("GrowableVineLeft").Eaten();
			var rightVine = GetNode<GrowableVine>("GrowableVine");
			rightVine.Grown();
			rightVine.Position = new Vector2(572, 143);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override async void _Process(double delta)
	{
		//MOVED: see OnPlayerRespawn()

		if (!transitioning)
		{
			await NextRoomCheck();
		}
		
		if (cameraGliding) {
			if (IsInstanceValid(Player)) //trying to fix the "Cannot access a disposed object" exception
			{
				var camera = Player.GetNode<Camera2D>("Camera2D");
				if (camera.GetScreenCenterPosition().X >= 470) {
					camera.PositionSmoothingEnabled = false;
					cameraGliding = false;
				}
			}
		}
	}

	private void OnVineTimerTimeout()
	{
		GetNode<AnimatedSprite2D>("Sparkle").Show();
		var anim = GetNode<AnimationPlayer>("AnimationPlayer");
		anim.Play("vine_appear");	
	}

	public void OnBossTriggerEntered(Node2D player)
	{
		if (GlobalScript.CQ("short") == "ParvaCave")
		{
			GlobalScript.QuestNum ++;	
		}
		if (GlobalScript.CQ("short") == "Seabunny")
		{
			if (!SeaBunny.InFight && !Player.respawning && !SeaBunny.InCutscene) {
				gateAnim.Play("gate_close");
				GetNode<AnimatedSprite2D>("Sparkle").Show();
				//camera.SetLimit(Side.Left, 320);
				//reset second vine timer
				if (SeaBunny.Hp == 1
					&& !(GetNode<AnimatedSprite2D>("Sparkle").Visible))
				{
					GetNode<Timer>("VineTimer").Start();
				}
					
				SeaBunny.StartFight();
			}
		}
	}

	private void OnCameraTriggerEntered(Node2D player)
	{
		var camera = player.GetNode<Camera2D>("Camera2D");
		camera.PositionSmoothingEnabled = true;
		camera.PositionSmoothingSpeed = 5.0f;
		cameraGliding = true;
		camera.GlobalPosition = new Vector2(470, camera.GlobalPosition.Y);
	}

	private void OnPlayerDied()
	{
		SeaBunny.InFight = false;
	}

	private async void OnPlayerRespawn()
	{
		GD.Print("Respawn stuff");//=
		
		var camera1 = Player.GetNode<Camera2D>("Camera2D");
		camera1.Position = Vector2.Zero;
		GD.Print("opening gate...");//=
		//gateAnim.Play("gate_open");
		gateAnim.Play("RESET");
		//await ToSignal(AnimationP, AnimationPlayer.SignalName.AnimationFinished);

		//reset fight
		//=maybe revert the save file instead? would that be better? idk
		GD.Print("resetting fight... ");//=
		SeaBunny.ResetMiniboss();
		GetNode<Node2D>("Sparkle").Position = new Vector2(400, 130);
		var rightVine = GetNode<GrowableVine>("GrowableVine");
		rightVine.Ungrown();
		rightVine.Position = new Vector2(572, 0);
		GetNode<GrowableVine>("GrowableVineLeft").Ungrown();
		GD.Print("Again Position " + SeaBunny.Position);
	}

	public async void OnLeftVineTriggerEntered(Node2D player)
	{
		if (SeaBunny.Hp == 2 && GetNode<GrowableVine>("GrowableVineLeft").isGrown)
		{
			if (!SeaBunny.InCutscene && player is Player p)
			{
				// p.SetDisableMovement(true);
				// p.invulnerable = true;
				await SeaBunny.EatLeftVine();
				GetNode<Timer>("VineTimer").Start();
				// p.SetDisableMovement(false);
				// p.invulnerable = false;
				var sparkle = GetNode<AnimatedSprite2D>("Sparkle");
				sparkle.Hide();
				sparkle.Position = new Vector2(572, 130);
			}
		}
	}
	public async void EndFight(Node2D player)
	{
		if (GlobalScript.CQ("short") == "Seabunny")
		{
			GetNode<AnimatedSprite2D>("Sparkle").Hide();
			player.GetNode<AnimatedSprite2D>("AnimatedSprite2D").Stop();

			if (!Player.respawning)
			{
				Player.invulnerable = true;
				Player.SetDisableMovement(true);

				SeaBunny.InFight = false;
				await SeaBunny.EndFight();

				if (Player is GroundPlayer gp)
				{
					gp.Climbing = true;
				}
				Player.SetDisableMovement(false);
				Player.invulnerable = false;

				GlobalScript.QuestNum ++;
			}
		}
	}

	private async void GetBoatCutscene()
	{
		var azucatBoba = GetNode<Sprite2D>("BOBA");
		azucatBoba.Position = new Vector2(218, 75);
		var aText = GetNode<TextBox>("BOBA/Azucat/TextBox");
		var dText = GetNode<TextBox>("GroundPlayer/TextBox");
		dText.SetLabel("Dash");
		aText.SetLabel("Azucat");
		aText.Known(true);
		dText.Known(true);
		var anim = GetNode<AnimationPlayer>("AnimationPlayer");
		Player.Position = new Vector2(573, 232);
		Player.GetNode<AnimatedSprite2D>("AnimatedSprite2D").Animation = "sit_left";
		Player.SetDisableMovement(true);
		await aText.ShowText("Thanks for your help, Dash! Here's your new ship!");
		
		anim.Play("ship_deployed");
		await ToSignal(anim, AnimationPlayer.SignalName.AnimationFinished);
		GetNode<GlobalSound>("/root/GlobalSound").PlaySound("bounce");
		anim.Play("ship_bounce");
		await ToSignal(anim, AnimationPlayer.SignalName.AnimationFinished);

		await dText.ShowText("...");
		await aText.ShowText("...Well, I better get going! See ya!");

		anim.Play("azucat_leaves");
		await ToSignal(anim, AnimationPlayer.SignalName.AnimationFinished);
		GlobalScript.QuestNum ++;
		Player.SetDisableMovement(false);
	}

	private async Task NextRoomCheck() {
		var player = GetNode<CharacterBody2D>("GroundPlayer");
		var GlobalSceneChange = GetNode<GlobalSceneChange>("/root/GlobalSceneChange");
		Vector2 pos = player.Position;
		if (pos.X < -8) {
			transitioning = true;
			await GlobalSceneChange.ChangeRoom(new Vector2(300, 142), "enter_sea_bunny_room", false);
		}
		else if (pos.X > 648)
		{
			transitioning = true;
			await GlobalSceneChange.ChangeRoom(new Vector2(20, 140), "treasure_room", true);
		}
		else if (pos.Y < -20)
		{
			transitioning = true;
			await GlobalSceneChange.ChangeRoom(Vector2.Zero, "world_end_screen", true);
		}

	}
}
