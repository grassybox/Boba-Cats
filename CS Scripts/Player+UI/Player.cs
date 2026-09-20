using Godot;
using System;
using System.Data.SqlTypes;
using System.Threading.Tasks;

public partial class Player : CharacterBody2D
{
	[Signal]
	public delegate void HitEventHandler(int hp);

	[Signal]
	public delegate void DiedEventHandler();

	/** emitted during the fade to black **/
	[Signal]
	public delegate void RespawnedEventHandler();

	public int Speed{get; set;}
	
	public float Gravity{get; set;}
	
	public float Mass = 4.54f; //in kg

	private int hp;

	public bool respawning;
	
	public bool respawnFadingIn;

	//if true, player can't get hit
	public bool invulnerable;

	//used in cutscenes (implemented in the GroundPlayer and UnderwaterPlayer classes)
	private bool disableMovement;
	public bool InputEnabled{get;set;} = true;

	//true if respawnOverride is taking over respawning
	private bool respawnOverride;

	//player flashing animation (when hit)
	public bool Flash{get; set;}
	
	//determines player sitting position
	public bool FacingRight{get; set;}

	//stores velocity modifiers such as wind/tube coral pull
	public Vector2 VelocityModifier{get; set;}
	
	public GlobalSound globalSound;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		disableMovement = false;
		respawning = false;
		respawnOverride = false;
		//ScreenSize = GetViewportRect().Size;
		VelocityModifier = Vector2.Zero;
		SetHP(GlobalScript.HealthHP);
		invulnerable = false;
		Flash = false;
		
		globalSound = GetNode<GlobalSound>("/root/GlobalSound");

		var devMode = GetNode<DevMode>("/root/DevMode");
		//connect signal
		devMode.ChangeHP += DevChangeHP;
		//gravity = Gravity.Underwater; //todo - change to update based on the player's room
	}
	
	
	public void OnPromptUser(TextBox box, string prompt) {
		GetNode<TextEnterLabel>("Camera2D/TextEnterLabel").FadeIn(box, prompt);
	}
	
	//= i didn't want to get rid of it because idk how to fix the signal
	//= but this is almost the same as SetHP
	private void DevChangeHP(int newHP) {
		SetHP(newHP);
		GD.Print(hp);
		GD.Print(newHP);
	}

	public void SetHP(int newHP) {
		hp = newHP;
		GlobalScript.HealthHP = hp;
		GetNode<PlayerHealth>("%PlayerHealth").ResetHPSprite(hp);
	}

	/// <summary>
	/// Changes hp by the amount given. Probably works with negative numbers
	/// </summary>
	/// <param name="add"></param>
	public void ChangeHP(int add)
	{
		hp += add;
		GlobalScript.HealthHP = hp;
		GetNode<PlayerHealth>("%PlayerHealth").ResetHPSprite(hp);
	}

	public int GetHP()
	{
		return hp;
	}
	
	//player enteres hitbox
	private void OnHurtboxAreaEntered(Node2D area)
	{
		if (!invulnerable)
		{
			GetHit();
		}
	}

	//get hit
	private async Task GetHit()
	{
		ChangeHP(-1);
		Flash = true;
		globalSound.PlaySound("hurt");
		EmitSignal(SignalName.Hit, hp);
		
		//i-frames
		var hurtTimer = GetNode<Godot.Timer>("HurtTimer");
		hurtTimer.Start();
		invulnerable = true;

		if (hp <= 0 || 
		GetParent().Name == "CaveRoom" ||
		GetParent().Name == "LongTubeCoralRoom" ||
		GetParent().Name == "TallTubeCoralRoom") // making you respawn after hit in cave room
		{
			respawning = true;
			EmitSignal(SignalName.Died);
			GD.Print("Died");//==
			await Respawn();
		}
	}
	
	//when invulnerablility ends
	//I don't know why but this method doesn't ever run for me
	public void OnHurtTimerTimeout()
	{
		if (!respawning)
		{
			invulnerable = false;
			Flash = false;

			var insideHurtbox =  GetNode<Area2D>("Hurtbox").GetOverlappingAreas();
			//if player is in hitbox when invulnerability ends
			if (insideHurtbox.Count > 0)
			{
				GetHit();
			}
		}
	}
	
	//Todo - reduce spawn points to: first room, underwater town, cave, 
	// seabunny, enter cave room (when backtracking)
	//we need to make a list
	public Vector2 GetSpawnPoint()
	{
		Vector2 respawnPoint = Vector2.Zero;
		var room = GetParent().Name;
		if (room == "EnterCaveRoom") {
			//coorinates might change if room coordinates change
			respawnPoint = new Vector2(113, 122);
		}
		else if (room == "PlantShop")
		{
			respawnPoint = new Vector2(258, 132);
		}
		else if (room == "FirstRoom" || room == "BoxRoom") {
			respawnPoint = new Vector2(10, 140);
		}
		else if (room == "SeabunnyBossRoom") {
			respawnPoint = new Vector2(100, 144);
		}
		else if (room == "UnderwaterTown") {
			respawnPoint = new Vector2(10, 518);
		}
		else if (room == "BoxRoom")
		{
			respawnPoint = new Vector2(10, 140);
		}
		else if (room == "FishRoom")
		{
			respawnPoint = new Vector2(35, 122);
		}
		else if (room == "LongTubeCoralRoom")
		{
			respawnPoint = new Vector2(276, 138);
		}
		else if (room == "JellyfishRoom")
		{
			respawnPoint = new Vector2(690, 138);
		}
		else if (room == "VineRoom")
		{
			respawnPoint = new Vector2(163, 126);
		}
		else if (room == "TallTubeCoralRoom")
		{
			respawnPoint = new Vector2(160, 557);
		}
		else if (room == "CaveRoom")
		{
			respawnPoint = new Vector2(25, 90);
		}
		else if (room == "SeaBunnyRoom")
		{
			respawnPoint = new Vector2(50, 232);
		}
		else
		{
			respawnPoint = new Vector2(20, 90);//default
		}
		return respawnPoint;
	}

	//Todo - do a scene change instead if the spawn point is in another room
	public async Task Respawn() 
	{
		if (!respawnOverride)
		{
			await RespawnToPoint(GetSpawnPoint());
			SetHP(2);
		}
		respawning = false;
		invulnerable = false;
	}

	/// <summary>
	/// Overrides the player's normal respawn to respawn at a specific point
	/// </summary>
	/// <param name="pos">The position to respawn at
	/// <returns></returns>
	public async Task RespawnOverride(Vector2 pos)
	{
		respawnOverride = true;
		await RespawnToPoint(pos);
		respawnOverride = false;
	}

	/// <summary>
	/// Disables respawn. The player will not gain invulnerability or receive hp.
	/// </summary>
	/// <param name="disable"></param>
	public void SetDisableRespawn(bool disable)
	{
		respawnOverride = disable;
	}

	public async Task RespawnToPoint(Vector2 pos)
	{
		respawning = true;
		respawnFadingIn = true;
		invulnerable = true;
		SetDisableMovement(true);
		var transition = GetNode<Fader>("/root/Fader");
		await ToSignal(GetTree().CreateTimer(1.0f), SceneTreeTimer.SignalName.Timeout);
		await transition.FadeIn(1.0f);

		GlobalPosition = pos;
		SetHP(2);
		EmitSignal(SignalName.Respawned);
		GD.Print("emitted respawn");//=
		
		respawnFadingIn = false;
		FacingRight = true;
		await transition.FadeOut(1.0f);

		invulnerable = false;
		respawning = false;
		disableMovement = false;
	}

	public void SetVelocityModifier(Vector2 vel)
	{
		VelocityModifier = vel;
	}

	/// <summary>
	/// Removes player movement control and disables gravity if true (for bouncing)
	/// </summary>
	/// <param name="disable"></param>
	public void SetDisableControlBounce(bool disable)
	{
		disableMovement = disable;
	}

	/// <summary>
	/// Removes player movement control and sets horizontal velocity to zero if true. Does not disable gravity
	/// </summary>
	/// <param name="disable"></param>
	public void SetDisableControl(bool disable)
	{
		Velocity = new Vector2(0, Velocity.Y);
		InputEnabled = !disable;
	}

	/// <summary>
	/// Removes player movement control and disables movement if true
	/// </summary>
	/// <param name="disable"></param>
	public void SetDisableMovement(bool disable)
	{
		disableMovement = disable;
		if (disable)
		{
			Velocity = Vector2.Zero;
		}
	}
	public bool MovementIsDisabled()
	{
		return disableMovement;
	}

	public bool InputIsDisabled()
	{
		return !InputEnabled;
	}
	
	public void GetItem(string item)
	{
		GetNode<AnimatedSprite2D>("Camera2D/Controls/ItemIcon").Animation = item;
		GetNode<AnimationPlayer>("Camera2D/Controls/AnimationPlayer").Play("item_get");
		if (!GlobalScript.Inventory.Contains(item)) {
			GlobalScript.AddItem(item);
		}
	}
	
	public void SetCameraDrag(string room) {
		var cam = GetNode<Camera2D>("Camera2D");
		if (room == "TallTubeCoralRoom") {
			cam.DragVerticalEnabled = true;
			cam.SetDragMargin(Side.Top, 0.5f);
			cam.SetDragMargin(Side.Bottom, 0.5f);
		}
		else {
			cam.SetDragMargin(Side.Left, 0);
			cam.SetDragMargin(Side.Top, 0);
			cam.SetDragMargin(Side.Bottom, 0);
			cam.SetDragMargin(Side.Right, 0);
		}
	}

	public void SetGlow(bool glow)
	{
		GetNode<Node2D>("PlayerGlow").Visible = glow;
	}
}
