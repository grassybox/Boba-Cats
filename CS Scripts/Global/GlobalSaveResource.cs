/* This class stores all of the player's save data
*  Saving and loading will be implemented in the GlobalScript
*/
using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;

[Tool]
[GlobalClass]
public partial class GlobalSaveResource : Resource
{
	
	[Export]
	public string DateSaved{get; set;}

	//configurations for a new game
	[Export]
	public int numCoins = 0;

	[Export]
	public int HealthHP = 2; //yes I know this stands for "HealthHealthPoints"

	[Export]
	public Godot.Collections.Array<String> Inventory {get; set;} = new Array<String>(); //=

	[Export]
	public int QuestNum{get; set;} = 5; //0; //=

	[Export]
	public string CurrentRoom{get; set;} = "first_room";

	[Export]
	public int WorldNum{get; set;} = 1;

	[Export]
	public Godot.Collections.Array<bool> CoinsCollected {get; set;} = new Array<bool>(); //true if collected

	[Export]
	public Godot.Collections.Array<bool> ClamsCollected {get; set;} = new Array<bool>(); //true if collected

	[Export]
	public int NumPearls = 0;

	[Export]
	public int OliveVisitNum{get; set;} = 0;
	[Export]
	public bool GeyserOpened{get; set;} = false;

	[Export]
	public int FishGameHighScore{get; set;} = 0;
	
	[Export]
	public bool Azulcat{get; set;} = false;

	[Export]
	public bool JellyfishRockBroken{get; set;} = false;

	[Export]
	public bool ViewedDarnCutscene{get; set;} = false;
	
	[Export]
	public double CatssavaStoryNum = 0;
	
	[Export]
	public double AzucatStoryNum = 0;
	
	[Export]
	public int CarEncNum = 0;
	
	[Export]
	public int Wisdom = 0;
	
	[Export]
	public int Courage = 0;
	
	[Export]
	public int Loyalty = 0;
	
	[Export]
	public int Compassion = 0;
	
	[Export]
	public int Stupid = 0;
}
