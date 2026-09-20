using Godot;
using System;
using System.Threading.Tasks;

public partial class Seabunny : CharacterBody2D
{
	[Export]
	public int dashSpeed {get; set;} = 200;
	public bool InFight;
	public bool InCutscene; //used so that startboss trigger doesn't trigger during the left vine cutscene
	public int Hp;

	public Vector2 StartPos;
	private static int LeftVineX = 386;
	private static int RightVineX = 555;
	private AnimatedSprite2D animatedSprite;
	private Godot.Timer idleTimer;
	public Boolean facingLeft;
	private PackedScene bullet;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Hp = 2;
		StartPos = new Vector2(485, 205);
		facingLeft = true;
		Velocity = Vector2.Zero;
		bullet = GD.Load<PackedScene>("res://Scenes/World 1/seabunnybullet.tscn");
		GD.Randomize(); //=
		
		animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

		idleTimer = GetNode<Godot.Timer>("IdleTimer");
		InFight = false;
		InCutscene = false;

		if (GlobalScript.IsAfterQuest("ReturnBoba")) //number
		{
			Position = new Vector2(460, 205);
			GetNode<Hitbox>("Hitbox").SetDisabled(true);
			animatedSprite.Animation = "sleep";
			animatedSprite.Play();

			//display bouncy icon
			GetNode<Sprite2D>("BounceIcon").Show();
			GetNode<AnimationPlayer>("AnimationPlayer").Play("arrow_move");
		}
		else
		{
			GetNode<Sprite2D>("BounceIcon").Hide();
			animatedSprite.Animation = "idle";
			animatedSprite.Play();
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		MoveAndSlide();

		//prevent it from leaving the arena
		Position = new Vector2(
			x: Mathf.Clamp(Position.X, 333, 600),
			y: Position.Y
		);
		// GD.Print(animatedSprite.Animation);//=
	}

	public async void StartFight()
	{
		Position = StartPos;

		if (!InFight)
		{
			InFight = true;
			idleTimer.Start(); 
			await ToSignal(idleTimer, Godot.Timer.SignalName.Timeout);
		}
		else
		{
			InFight = false;
		}
	}

	public async Task EatLeftVine()
	{
		InFight = false;
		InCutscene = true;
		Velocity = Vector2.Zero;
		GetNode<Hitbox>("Hitbox").SetDisabled(true);
		var anim = GetParent().GetNode<AnimationPlayer>("AnimationPlayer");
		Velocity = Vector2.Zero;
		
		GD.Print("calling cutsene dash...");//=
		await CutsceneDash(true);
		GD.Print("cutscene dash finished");//=

		Position = new Vector2(LeftVineX, 205);
		animatedSprite.FlipH = false;
		animatedSprite.Animation = "start_climb";
		GD.Print("Starting climb...");//=
		animatedSprite.Play();
		animatedSprite.Animation = "start_climb";
		await ToSignal(animatedSprite, AnimatedSprite2D.SignalName.AnimationFinished);

		animatedSprite.Animation = "climbing";
		animatedSprite.Play();
		anim.Play("climb_left_vine");
		GD.Print("waiting");//=
		await ToSignal(anim, AnimationPlayer.SignalName.AnimationFinished);
		GD.Print("finished climb=====================================");//=

		GetParent().GetNode<GrowableVine>("GrowableVineLeft").Eaten();

		GD.Print("switching animation==========================");//=
		animatedSprite.Animation = "end_climb";
		animatedSprite.Play();
		GD.Print("Switched animation");//=
		anim.Play("fall_left_vine");
		await ToSignal(anim, AnimationPlayer.SignalName.AnimationFinished);

		Hp --;
		GetNode<Hitbox>("Hitbox").SetDisabled(false);
		facingLeft = true;
		animatedSprite.FlipH = false;
		InFight = true;
		InCutscene = false;
		OnIdleTimerTimeout();
	}

	//=
	public override void _Input(InputEvent @event)
	{
		if (Input.IsActionJustPressed("yes"))
		{
			GD.Print(Position);//=
		}
	}

	public async Task EndFight()
	{
		Hp --;
		InFight = false;
		InCutscene = true;
		GetNode<Hitbox>("Hitbox").SetDisabled(true);
		var anim = GetParent().GetNode<AnimationPlayer>("AnimationPlayer");
		Velocity = Vector2.Zero;
		
		await CutsceneDash(false);

		Position = new Vector2(RightVineX, 205);
		animatedSprite.Animation = "start_climb";
		animatedSprite.Play();
		await ToSignal(animatedSprite, AnimatedSprite2D.SignalName.AnimationFinished);

		animatedSprite.Animation = "climbing";
		animatedSprite.Play();
		anim.Play("climb_vine");
		await ToSignal(anim, AnimationPlayer.SignalName.AnimationFinished);

		animatedSprite.FlipH = false;
		animatedSprite.Animation = "end_climb";
		animatedSprite.Play();
		anim.Play("fall");
		await ToSignal(anim, AnimationPlayer.SignalName.AnimationFinished);

		animatedSprite.Animation = "sleep";
		animatedSprite.Play();
		InCutscene = false;
	}

	/// <summary>
	/// Resets the boss. To be called in SeaBunnyRoom/OnPlayerRepawn
	/// </summary>
	public void ResetMiniboss()
	{
		facingLeft = true;
		animatedSprite.FlipH = false;
		InFight = false;
		Position = StartPos;
		GD.Print(Position + " Should be " + StartPos);//=
		Modulate = new Color(1, 1, 1, 1);
		Hp = 2;
	}

	//every time it is done waiting, do another attack
	private async void OnIdleTimerTimeout()
	{
		if (InFight && !InCutscene) //=
		{
			await DoAttack();
		}
	}

	private async Task DoAttack()
	{
		int attack = GD.RandRange(0, 3);
		if (attack == 0)
		{
			await Dash(1);
		}
		else if (attack == 1)
		{
			await DashAttack(3);
		}
		else
		{
			await Spin();
		}

		animatedSprite.Animation = "idle";
		idleTimer.Start();
	}

	private async Task CutsceneDash(bool left)
	{
		GD.Print("Cutscene dash!");//=
		if (left)
		{
			GD.Print("Starting cutscene dash");//=
			facingLeft = true;
			animatedSprite.FlipH = false;
			int numDashes = (int) Math.Abs(Math.Round((Position.X - LeftVineX) / 84));
			await Dash(numDashes);
		}
		else
		{
			facingLeft = false;
			animatedSprite.FlipH = true;
			int numDashes = (int) Math.Abs(Math.Round((RightVineX - Position.X) / 84));
			GD.Print(numDashes);//=
			await Dash(numDashes);
		}
	}

	private async Task DashAttack(int loops)
	{
		if (InFight)
		{
			await Dash(loops);
		}
		//turn around
		if (facingLeft)
		{
			//turn to right
			facingLeft = false;
			animatedSprite.FlipH = true;
		}
		else
		{
			//turn to left
			facingLeft = true;
			animatedSprite.FlipH = false;
		}
	}

	//loops: how many times to loop the dashing animation
	private async Task Dash(int loops)
	{
		GD.Print("Dash");//=
		if (loops == 0) return;
		animatedSprite.Animation = "start_dash";
		animatedSprite.Play(); //idk if we need to call Play() every time
		await ToSignal(animatedSprite, AnimatedSprite2D.SignalName.AnimationFinished);

		animatedSprite.Animation = "dashing";
		animatedSprite.Play();
		
		if (facingLeft && !InCutscene)
		{
			Velocity = new Vector2(-dashSpeed, 0); //set velocity.x to -dashSpeed
		}
		else
		{
			Velocity = new Vector2(dashSpeed, 0);
		}
		
		//wait for dashing animation to loop a certain number of times
		for (int i = 0; i < loops; i ++)
		{
			await ToSignal(animatedSprite, AnimatedSprite2D.SignalName.AnimationLooped);
			// if (!facingLeft && Position.X > RightVineX)
			// {
			// 	break;
			// }
			if (!InFight && facingLeft && Position.X < 395)
			{
				break;
			}
		}
		
		if (!InCutscene)
		{
			Velocity = Vector2.Zero;
			animatedSprite.Animation = "end_dash";
			animatedSprite.Play();
			await ToSignal(animatedSprite, AnimatedSprite2D.SignalName.AnimationFinished);
		}
	}

	private async Task Spin()
	{
		animatedSprite.Animation = "spin";
		animatedSprite.Play();
		await ToSignal(GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);
		
		//clone bullets, positioned left to right
		AddBullet(new Vector2(0, Rand(12, 32)), new Vector2(-1, 0));
		AddBullet(new Vector2(0, Rand(-8, 8)), new Vector2(-1, -1));
		AddBullet(new Vector2(Rand(0, 16), 0), new Vector2(0, -1));
		AddBullet(new Vector2(Rand(51, 60), Rand(0, 10)), new Vector2(1, -1));
		AddBullet(new Vector2(63, Rand(20, 33)), new Vector2(1, 0));
		
		await ToSignal(animatedSprite, AnimatedSprite2D.SignalName.AnimationFinished);
	}
	
	private static int Rand(int low, int high) {
		var randomizer = new RandomNumberGenerator();
		randomizer.Randomize();
		return randomizer.RandiRange(low, high);
	}
	
	//position: relative to origin of sea bunny (parent)
	private void AddBullet(Vector2 position, Vector2 velocity) {
		Seabunnybullet inst = bullet.Instantiate<Seabunnybullet>();
		inst.Position = position;
		inst.Velocity = velocity;
		AddChild(inst);
	}
}
