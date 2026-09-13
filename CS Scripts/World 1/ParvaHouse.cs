using Godot;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

public partial class ParvaHouse : Node2D
{
	private bool transitioning = false;
	private TextBox pT;
	private TextBox dT;

	private AnimationPlayer animationPlayer;

	// Called when the node enters the scene tree for the first time.
	public override async void _Ready()
	{
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

		dT = GetNode<TextBox>("GroundPlayer/TextBox");
		pT = GetNode<TextBox>("Parva/TextBox");
		
		dT.SetLabel("Dash");
		pT.SetLabel("Parva");
		dT.Known(true);
		pT.Known(GlobalScript.IsAfterQuest("ExploreOcean"));

		if (GlobalScript.CQ("short") != "ExploreOcean")
		{
			animationPlayer.Play("open_trapdoor");
		}

		//darn
		if (GlobalScript.QuestNum >= GlobalScript.MainQuests.IndexOf("Surface") && !GlobalScript.ViewedDarnCutscene) //number
		{
			GetNode<Sprite2D>("BOBA").Position = new Vector2(-20, -25);
			GetNode<AnimatedSprite2D>("Parva").Animation = "mad";
			var aT = GetNode<TextBox>("BOBA/Azucat/TextBox");
			aT.SetLabel("Azucat");
			aT.Known(true);
			await aT.ShowText("Thanks for the boba, Parva!!");
			animationPlayer.Play("Azucat_Catssava_run");
			await ToSignal(animationPlayer, AnimationPlayer.SignalName.AnimationFinished);
			await pT.ShowText("NOOOOO MY BOBA THAT DARN CAT");
			animationPlayer.Play("Parva_chase");
			await ToSignal(animationPlayer, AnimationPlayer.SignalName.AnimationFinished);
			GlobalScript.ViewedDarnCutscene = true;
		}
		else if (GlobalScript.ViewedDarnCutscene)
		{
			GetNode<Node2D>("Parva").Hide();
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
	
	private async void OnExitRoom() {
		var GlobalScene = GetNode<GlobalSceneChange>("/root/GlobalSceneChange");
		
		await GlobalScene.ChangeRoom(new Vector2(440, 197), "cave_room", false);
	}
	
	public async void startDialogue(Node2D player) {
		if (GlobalScript.CQ("short") == "ExploreOcean") {

			//lock player movement?
			//sure
			var dash = player;
			if (dash is Player p)
			{
				p.SetDisableMovement(true);
			}
			player.GetNode<AnimatedSprite2D>("AnimatedSprite2D").Animation = "sit_right";	
			player.Position = new Vector2(78, 132);

			//=new stuff
			var parva = pT.GetParent<AnimatedSprite2D>();
			pT.Position = new Vector2(pT.Position.X, pT.Position.Y - 20);
			await pT.ShowText("At last. I have gathered the ingredients for BROWN SUGAR BOBA!");
			parva.FlipH = true;
			GetNode<Node2D>("%SingleBoba").Show();
			await pT.ShowText("Boba!");
			parva.FlipH = false;
			GetNode<Node2D>("%Sugar").Show();
			await pT.ShowText("Sugar!");
			await dT.ShowText("...");
			await dT.ShowText("Hello - excuse me - I don't think it works like that.");
			await dT.ShowText("Brown sugar boba needs...well...brown sugar. That's just regular sugar.");
			await pT.ShowText("Hrmph! Who are you? What would you know about it?");
			await pT.ShowText("What you said can't possibly be true...right...?");
			parva.FlipH = true;
			await pT.ShowText("brown...");
			parva.FlipH = false;
			await pT.ShowText("sugar...");
			parva.FlipH = true;
			await pT.ShowText("boba...");
			parva.FlipH = false;
			await pT.ShowText("This has to be right. I've been trying to make brown sugar boba for years! I know what I'm doing.");
			await pT.ShowText("OBSERVE AS I MAKE BROWN SUGAR BOBA!");
			animationPlayer.Play("make_goo");
			await ToSignal(animationPlayer, AnimationPlayer.SignalName.AnimationFinished);
			pT.Position = new Vector2(pT.Position.X, pT.Position.Y + 20);
			await pT.ShowText("What are you looking at?!");
			await dT.ShowText("That's...some interesting brown sugar boba.");
			await pT.ShowText("Just admit it alright? It's goo. It's always goo. WHY IS IT ALWAYS GOO???");

			await dT.ShowText("Because brown sugar boba needs brown sugar. I'd know because I'm on a quest to find it too.");
			await pT.ShowText("Hmmm interesting. Very well. Why don't you come have a seat?");

			//move to seat:
			if (dash is Player p1)
			{
				var sprite = GetNode<AnimatedSprite2D>("GroundPlayer/AnimatedSprite2D");
				sprite.Animation = "walk_right";
				sprite.Play();
			
				animationPlayer.Play("sit_down");
				await ToSignal(animationPlayer, AnimationPlayer.SignalName.AnimationFinished);

				sprite.Animation = "sit_right";
			}

			await pT.ShowText("That's more like it. My name is Parva. Now, what has brought you here, brown cat?");
			await pT.ShowText("Perhaps you've heard of my soon-to-be greatness?");

			pT.Known(true);
			await dT.ShowText("I'm Dash. My ship crashed and I ended up underwater.");
			await dT.ShowText("My search for the fabled brown sugar boba led me here. I need boba, after all.");
			//await dT.showText("It seems like this place has every kind of boba except for that. I've looked everywhere.");
			//no they don't have every kind of boba because someone stole it all... lol

			//my idea:
			//parva says something along the lines of "I can help with that"
			await pT.ShowText("Hmm...I might be able to help you with that.");
			await pT.ShowText("You see, I have my own stash. It's top secret though; I trust you wouldn't tell anyone-");
			await pT.ShowText("Not that they would be able to get to it anyways, heh.");
			//trapdoor is revealed
			//opens the trapdoor:
			animationPlayer.Play("open_trapdoor");
			await ToSignal(animationPlayer, AnimationPlayer.SignalName.AnimationFinished);

			//player goes down
			GlobalScript.QuestNum++;
			GlobalScript.QuestNum = 4; //= manual set
			if (dash is Player playerr)
			{
				playerr.SetDisableMovement(false);
				playerr.SetDisableControl(false);
			}
		}
	}

	private async Task NextRoomCheck() {
		var player = GetNode<CharacterBody2D>("GroundPlayer");
		var GlobalSceneChange = GetNode<GlobalSceneChange>("/root/GlobalSceneChange");
		Vector2 pos = player.Position;
		if (pos.Y > 188)
		{
			transitioning = true;
			await GlobalSceneChange.ChangeRoom(new Vector2(138, 125), "enter_sea_bunny_room", true);
		}

	}
}
