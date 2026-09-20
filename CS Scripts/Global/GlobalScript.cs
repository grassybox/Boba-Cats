using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;

public partial class GlobalScript : Node
{
	
	public static GlobalSaveResource GameData{get; set;} = new GlobalSaveResource();
	//so that it doesn't break code accessing from GlobalScript.variable
	public static int Coins {
		get => GameData.numCoins;
		set {
			GameData.numCoins = value;
			SaveGame();
		}
	}

	public static int HealthHP
	{
		get => GameData.HealthHP;
		set {
			GameData.HealthHP = value;
			SaveGame();
		}
	}

	public static Godot.Collections.Array<string> Inventory {
		get => GameData.Inventory;
		set {
			GameData.Inventory = value; //for changing entire list, .Add doesn't use set
			SaveGame();
		}
	}
	public static int QuestNum { //temp // todo: change to 0 later after fixing "that's my boat" cutscene
		get => GameData.QuestNum;
		set {
			GameData.QuestNum = value;
			SaveGame();
		}
	}

	public static string CurrentRoom {
		get => GameData.CurrentRoom;
		set {
			GameData.CurrentRoom = value;
			SaveGame();
		}
	}

	public static int WorldNum
	{
		get => GameData.WorldNum;
		set {
			GameData.WorldNum = value;
			SaveGame();
		}
	}

	/// <summary>
	/// Stores whether each coin has been collected. True if collected.
	/// </summary>
	public static Godot.Collections.Array<bool> CoinsCollected
	{
		get => GameData.CoinsCollected;
		set {
			GameData.CoinsCollected = value;
			SaveGame();
		}
	}

	public static Godot.Collections.Array<bool> ClamsCollected
	{
		get => GameData.ClamsCollected;
		set {
			GameData.ClamsCollected = value;
			if (value.Count == 0)
			{
				value.Resize(6);
				value.Fill(false);
			}
			SaveGame();
		}
	}
	
	public static int NumPearls {
		get => GameData.NumPearls;
		set {
			GameData.NumPearls = value;
			SaveGame();
		}
	}

	public static int OliveVisitNum
	{
		get => GameData.OliveVisitNum;
		set { 
			GameData.OliveVisitNum = value;
			SaveGame();
		}
	}
	
	public static int CarEncNum {
		get => GameData.CarEncNum;
		set {
			GameData.CarEncNum = value;
			SaveGame();
		}
	}
	public static bool GeyserOpened {
		get => GameData.GeyserOpened;
		set {
			GameData.GeyserOpened = value;
			SaveGame();
		}
	}

	public static bool JellyfishRockBroken {
		get => GameData.JellyfishRockBroken;
		set {
			GameData.JellyfishRockBroken = value;
			SaveGame();
		}
	}

	public static bool ViewedDarnCutscene
	{
		get => GameData.ViewedDarnCutscene;
		set {
			GameData.ViewedDarnCutscene = value;
			SaveGame();
		}
	}

	public static bool Azulcat {
		get => GameData.Azulcat;
		set {
			GameData.Azulcat = value;
			SaveGame();
		}
	}

	public static int FishGameHighScore {
		get => GameData.FishGameHighScore;
		set {
			GameData.FishGameHighScore = value;
			SaveGame();
		}
	}
	
	public static double CatssavaStoryNum {
		get => GameData.CatssavaStoryNum;
		set {
			GameData.CatssavaStoryNum = value;
			SaveGame();
		}
	}
	
	public static double AzucatStoryNum {
		get => GameData.AzucatStoryNum;
		set {
			GameData.AzucatStoryNum = value;
			SaveGame();
		}
	}
	
	public static int Wisdom {
		get => GameData.Wisdom;
		set {
			GameData.Wisdom = value;
			SaveGame();
		}
	}
	
	public static int Courage {
		get => GameData.Courage;
		set {
			GameData.Courage = value;
			SaveGame();
		}
	}
	
	public static int Loyalty {
		get => GameData.Loyalty;
		set {
			GameData.Loyalty = value;
			SaveGame();
		}
	}
	
	public static int Compassion {
		get => GameData.Compassion;
		set {
			GameData.Compassion = value;
			SaveGame();
		}
	}
	
	public static int Stupid {
		get => GameData.Stupid;
		set {
			GameData.Stupid = value;
			SaveGame();
		}
	}
	
	//set because maybe we can change name of quest based on player choices
	public static List<string> MainQuests{get; set;} = new List<string> {
		"GoToTown", //0
		"MeetAzucat", //1
		"MeetCatssava", //2
		"ExploreOcean", //3
		"Trapdoor", //4
		"ParvaCave", //5 (after talking to parva) "Cave"
		"Seabunny", //6
		"ReturnBoba", //7
		"GetBoat", //8
		"Surface" //9
	};
	
	
	public static System.Collections.Generic.Dictionary<string, string> QuestNames = new System.Collections.Generic.Dictionary<string, string> {
		{"GoToTown", "Explore the ocean"}, //0
		{"MeetAzucat", "Investigate the shop"}, //1
		{"MeetCatssava", "Visit the boba shop and ask for brown sugar boba"}, //2
		{"ExploreOcean", "Journey into the dangerous ocean to find boba"}, //3
		{"Trapdoor", "Meet Parva in his secret chambers"}, //4
		{"ParvaCave", "Investigate the cave"}, //5 (after talking to parva)
		{"Seabunny", "Escape from the sea bunny"}, //6
		{"ReturnBoba", "Return the boba to Azucat and Catssava"}, //7
		{"GetBoat", "Get your new boat from Azucat"}, //8
		{"Surface", "Head up to the surface"} //9
	};
	
	public static System.Collections.Generic.Dictionary<string, Func<int>> NumItems = new System.Collections.Generic.Dictionary<string, Func<int>> {
		{"Pearl", () => GameData.NumPearls}
	};
	
	public static String savePath{get; set;} //= "user://save_data/tres";

	public void InitializeArrays()
	{
		//todo - change size?
		//did I do this in the right spot
		CoinsCollected.Resize(99);
		CoinsCollected.Fill(false);

		ClamsCollected.Resize(6);
		ClamsCollected.Fill(false);
	}

	//Resource cursor = ResourceLoader.Load("path");
	public static Resource flashlightCursor = ResourceLoader.Load("res://assets/icons/flashlight.png");

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		InitializeArrays();
	}	

	public static void ChangeCursor(String cursor)
	{
		if (cursor.ToLower() == "flashlight")
		{
			Input.SetCustomMouseCursor(flashlightCursor);
		}
	}
	
	public static void LoadGame() {
		if (ResourceLoader.Exists(savePath)) {
			GameData = ResourceLoader.Load<GlobalSaveResource>(savePath);
		}
	}
	
	public static void SaveGame() {
		if (savePath != null) {
			ResourceSaver.Save(GameData, savePath);
		}
	}
	
	/// <summary>
	/// Exclusive (>)
	/// </summary>
	public static bool IsAfterQuest(string shortName) {
		return MainQuests.IndexOf(CQ("short")) > MainQuests.IndexOf(shortName);
	}

	/// <summary>
	/// Exclusive (<)
	/// </summary>
	public static bool IsBeforeQuest(string shortName) {
		return MainQuests.IndexOf(CQ("short")) < MainQuests.IndexOf(shortName);
	}
	//get current quest
	//type = long or short
	public static String CQ(string type) {
		if (type == "long") {
			if (QuestNames.TryGetValue(MainQuests[QuestNum], out string fullName)) {
				return fullName;
			}
			else {
				GD.Print("Error getting current quest from QuestNames Dictionary");
				return null;
			}
		}
		else if (type == "short") {
			return MainQuests[QuestNum];
		}
		else {
			GD.Print("Parameter does not match short or long (GlobalScript.CQ(string))");
			return null;
		}
	}
	public static void AddItem(string item) {
		Inventory.Add(item);
		SaveGame();
	}

	//=handle player pressing 'X'
	// public override void _Notification(int notif)
	// {
	// 	if (notif == NotificationWMCloseRequest)
	// 	{
	// 		GetTree().Quit();
	// 	}
	// }
}
