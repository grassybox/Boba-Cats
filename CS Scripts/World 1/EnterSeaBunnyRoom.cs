using Godot;
using System;
using System.Threading.Tasks;

public partial class EnterSeaBunnyRoom : Node2D
{
	private bool transitioning = false;
	private TextBox dashT;
	private TextBox parvaT;
	//private static bool fedToBunny = false;

	// Called when the node enters the scene tree for the first time.
	public override async void _Ready()
	{
		dashT = GetNode<TextBox>("GroundPlayer/TextBox");
		parvaT = GetNode<TextBox>("Parva/TextBox");
		
		dashT.SetLabel("Dash");
		parvaT.SetLabel("Parva");
		dashT.Known(true);
		parvaT.Known(true);

		if (GlobalScript.QuestNum <= GlobalScript.MainQuests.IndexOf("ReturnBoba")) //number
		{
			GetNode<Sprite2D>("Ladder").Position = new Vector2(0, 250); //offscreen
		}

		//Quest before escape from the sea bunny
		//note: alternatively could do GlobalScript.MainQuests[QuestNum] == quest name
		if (GlobalScript.CQ("short") == "Trapdoor") {
			GetNode<AnimatedSprite2D>("Parva").Show();
			await StartDialogue();
		}
		else {
			GetNode<AnimatedSprite2D>("Parva").Hide();
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
	
	private async Task StartDialogue() {
		var parvaAni = GetNode<AnimatedSprite2D>("Parva");
		parvaAni.Animation = "default";
		var player = GetNode<GroundPlayer>("GroundPlayer");
		//this way still affected by gravity
		player.SetDisableControl(true);
		await dashT.ShowText("How'd you get an entire stash of boba?");
		await parvaT.ShowText("Ah, just stole them from the town's boba shop.");
		await dashT.ShowText("...");
		await parvaT.ShowText("No need to stare like that, it wasn't that hard. They keep their doors open at night, pfft.");
		
		var choice = await dashT.Ask("1. Why steal? It's selfish.\n2. Parva is impressive!", "cou", "w");
		if (choice == "1") {
			await dashT.ShowText("I don't understand. What about the townscats who need boba and gold? Why not just buy it from the shop?");
			await parvaT.ShowText("Why should I care what happens to those town cats?");
			await parvaT.ShowText("But you... you're looking for brown sugar boba too, right? You could help me steal boba each month for my experiments.");
			await parvaT.ShowText("I'll even share some with you in return. What do you say?");
			choice = await dashT.Ask("1. There's something you're hiding.\n2. I'm loyal to the cats of Bubbly Town!", "w", "l");
			if (choice == "1") {
				await dashT.ShowText("It doesn't make sense. There must be something you're not telling me.");
				await dashT.ShowText("You have something against the cats of Bubbly Town, don't you...");
				parvaAni.Animation = "mad";
				await parvaT.ShowText("What would YOU know! There's NOTHING, you hear, NOTHING! You think you can waltz in and try and dig up my past -");
				await parvaT.ShowText("It's NOT HAPPENING! NOT HAPPENING!");
				await dashT.ShowText("...");
				choice = await dashT.Ask("1. Dig deeper\n2. Back off", "s", "w");
				if (choice == "1") {
					await dashT.ShowText("Parva, I won't help you if you don't tell me the truth.");
					await parvaT.ShowText("YOU CAN HELP ME BY STEALING MORE BOBA FOR ME!");
					await parvaT.ShowText("BUT IT SEEMS YOU'D RATHER DECEIVE ME INTO BECOMING WEAK!");
					await parvaT.ShowText("I'M LEAVING. YOU CAN ROT HERE FOR ALL I CARE.");
				}
				else {
					await dashT.ShowText("Okay, okay! I suppose it's not my business. Still, I'm not helping you.");
					await parvaT.ShowText("You're right. It isn't your business. And for that, you'll be staying here for the rest of your short life.");
					await dashT.ShowText("...!");
				}
			}
			else {
				await dashT.ShowText("I've already promised to help the cats of Bubbly Town get their boba back.");
				await dashT.ShowText("A sailor never goes back on his word.");
				parvaAni.Animation = "mad";
				await parvaT.ShowText("Those cats never cared about you! They were USING YOU!");
				choice = await dashT.Ask("1. They would never do that\n2. It is an exchange", "l", "w");
				if (choice == "1") {
					await dashT.ShowText("I've met them and the cats of Bubbly Town aren't like that.");
					await dashT.ShowText("It was my own choice to help them, and I will stand by it.");
					await parvaT.ShowText("You FOOL! I've fought for everything I have! While those town cats get everything for nothing!");
					await dashT.ShowText("Perhaps it's because they have each other. Don't you think that by relying solely on yourself, you've made things harder for youself?");
					await parvaT.ShowText("How dare you! You have NO IDEA what I've been through.");
					await parvaT.ShowText("You're all alone. Let's see where your [i]friends[/i] are when you meet your end!");
				}
				else {
					await dashT.ShowText("They aren't using me. I agreed to help them in exchange for a new boat.");
					await parvaT.ShowText("AND I'M OFFERING YOU BOBA IN EXCHANGE FOR HELPING ME STEAL IT!");
					await dashT.ShowText("I'm sorry, but I need a new boat more. And I won't help you steal.");
					await parvaT.ShowText("AARGHH why are you cats all so annoying?! Fine, rot down here for all I care!");
				}
			}
		}
		else {
			await dashT.ShowText("Wow, traveling all the way to the town and stealing their boba is impressive, I must admit.");
			await dashT.ShowText("After my journey through the ocean, I know from experience it's not easy!");
			await parvaT.ShowText("I'm pleased you think so. I have something to propose.");
			await parvaT.ShowText("If you help me steal boba from the town each month, I'll let you share some of my stash!");
			choice = await dashT.Ask("1. Can I also get a new boat?\n2. I've already agreed to help the cats of Bubbly Town", "w", "l");
			if (choice == "1") {
				await dashT.ShowText("Was that a submarine diagram I saw in your house? Perhaps you could make me a new boat as well?");
				await parvaT.ShowText("My offer was generous enough. I won't let you take advantage of me.");
				await dashT.ShowText("I can't take it, then.");
				parvaAni.Animation = "mad";
				await parvaT.ShowText("Then I'm sorry for what comes next.");
			}
			else {
				await dashT.ShowText("What I need is a new boat. The cats of Bubbly Town have promised me one given I find their boba.");
				parvaAni.Animation = "mad";
				await parvaT.ShowText("So you're their lackey? You disgust me.");
				choice = await dashT.Ask("1. I like them.\n2. I'm independent.", "l", "cou");
				if (choice == "1") {
					await dashT.ShowText("I like the cats of Bubbly Town, and decided to help them on my own accord.");
					await dashT.ShowText("I can't say the same about you.");
					await parvaT.ShowText("Those blasted town cats get everything just for being and looking nice!");
					await parvaT.ShowText("You've fallen into their trap. And now you've fallen into mine.");
				}
				else {
					await dashT.ShowText("I help whom I please. And I'm in desperate need of a new boat.");
					parvaAni.Animation = "mad";
					await parvaT.ShowText("That's unfortunate. But what you might not know - I'm independent too."); 
					await parvaT.ShowText("I don't [i]need[/i] your help. And right now I'm thinking trapping you down here is the way to go.");
				}
			}
		}
		

		// var parva = GetNode<AnimatedSprite2D>("Parva");
		// parva.Hide();
		var anim = GetNode<AnimationPlayer>("AnimationPlayer");
		anim.Play("parva_leaves");
		await ToSignal(anim, AnimationPlayer.SignalName.AnimationFinished);
		await dashT.ShowText("Where did he go? Seems like I'm stuck here...");
		player.SetDisableControl(false);
		//Next quest: investigate the cave
		GlobalScript.QuestNum++;
	}

	private async Task NextRoomCheck() {
		var player = GetNode<CharacterBody2D>("GroundPlayer");
		var GlobalSceneChange = GetNode<GlobalSceneChange>("/root/GlobalSceneChange");
		Vector2 pos = player.Position;
		
		if (pos.Y < -8 && GlobalScript.IsAfterQuest("GetBoat"))
		{
			transitioning = true;
			await GlobalSceneChange.ChangeRoom(new Vector2(270, 140), "parva_house", false);
		}
		
		if (pos.X > 328)
		{
			transitioning = true;
			await GlobalSceneChange.ChangeRoom(new Vector2(20, 231), "sea_bunny_room", true);
		}

	}
}
