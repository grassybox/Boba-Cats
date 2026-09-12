using Godot;
using System;

public partial class TitleScreen : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNode<SaveSlot>("FilePicker/SaveSlot").SlotNum = 1;
		GetNode<SaveSlot>("FilePicker/SaveSlot2").SlotNum = 2;
		GetNode<SaveSlot>("FilePicker/SaveSlot3").SlotNum = 3;
		GetNode<CanvasLayer>("FilePicker").Hide();
	}

	private void OnQuitButtonPressed()
	{
		GetTree().Quit();
	}

	private void OnCreditsButtonPressed()
	{
		GetNode<Control>("Credits").Show();
	}

	private void OnCloseCreditsButtonPressed()
	{
		GetNode<Control>("Credits").Hide();
	}

	public async void OnPlayButtonPressed()
	{
		GetNode<CanvasLayer>("FilePicker").Show();
	}
	
	public void StartUnsavedGame() {
		GlobalScript.GameData = new GlobalSaveResource();
		var GS = GetNode<GlobalScript>("/root/GlobalScript");
		GS.InitializeArrays();
		ShipCrashCutscene();
	}
	public async void ShipCrashCutscene() {
		GetNode<Control>("Buttons").Hide();
		GetNode<CanvasLayer>("FilePicker").Hide();
		var animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		animatedSprite.Animation = "opening";
		animatedSprite.Play();
		var animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		animationPlayer.Play("skip_button_fade_in");
		await ToSignal(animatedSprite, AnimatedSprite2D.SignalName.AnimationFinished);
		ToFirstRoom();
	}

	private async void ToFirstRoom()
	{
		var GlobalSceneChange = GetNode<GlobalSceneChange>("/root/GlobalSceneChange");
		await GlobalSceneChange.ChangeRoom(new Vector2(35, 138), "first_room", true);
	}

	public void OnExitFilePicker() {
		GetNode<CanvasLayer>("FilePicker").Hide();
	}
}
