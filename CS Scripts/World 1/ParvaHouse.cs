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
			await pT.ShowText("something about how I have ingredients for yet another experiment to make brown sugar boba!");
			parva.FlipH = true;
			GetNode<Node2D>("%SingleBoba").Show();
			await pT.ShowText("Boba!");
			parva.FlipH = false;
			GetNode<Node2D>("%Sugar").Show();
			await pT.ShowText("Sugar!");
			await dT.ShowText("something about how that's not how it works. "
				+" brown sugar boba needs brown sugar. not just regular sugar");
			await pT.ShowText("confused");
			parva.FlipH = true;
			await pT.ShowText("brown...");
			parva.FlipH = false;
			await pT.ShowText("sugar...");
			parva.FlipH = true;
			await pT.ShowText("boba...");
			parva.FlipH = false;
			await pT.ShowText("something about actually no you're wrong lemme show you");
			animationPlayer.Play("make_goo");
			await ToSignal(animationPlayer, AnimationPlayer.SignalName.AnimationFinished);
			pT.Position = new Vector2(pT.Position.X, pT.Position.Y + 20);
			await pT.ShowText("aw man it just made goo again");

			await pT.ShowText("A [i]visitor[/i]. Well, I must say I'm surprised you got past the vines.");
			await pT.ShowText("You don't seem like one of those...[i]town cats[/i]. Why don't you come have a seat?");

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

			await pT.ShowText("That's more like it. My name is Parva. Now, what have you come all this way for, brown cat?");

			pT.Known(true);
			await dT.ShowText("I'm Dash. I've been on a quest to find the fabled brown sugar boba.");
			await dT.ShowText("Unfortunately, my ship crashed and I ended up underwater.");
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
