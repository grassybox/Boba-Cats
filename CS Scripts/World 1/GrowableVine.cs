using Godot;
using System;
using System.Threading.Tasks;

public partial class GrowableVine : Area2D
{
	[Export]
	public bool GrownByDefault {get; set;} = false;

	public bool isGrown;

	private CollisionShape2D grownCollisionShape;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		isGrown = GrownByDefault;
		grownCollisionShape = GetNode<CollisionShape2D>("GrownCollisionShape");
		var animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		if (!GrownByDefault)
		{
			animatedSprite2D.Animation = "ungrown";
			grownCollisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
		}
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void Eaten()
	{
		GetNode<CollisionShape2D>("GrownCollisionShape").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
		GetNode<PointLight2D>("PointLight2D").Hide();
		GetNode<AnimatedSprite2D>("AnimatedSprite2D").Animation = "eaten";
	}

	public void Grown()
	{
		var animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		animatedSprite2D.Animation = "grown";
		grownCollisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
		var light = GetNode<PointLight2D>("PointLight2D");
		light.Position = new Vector2(0, 46);
		light.Show();
		isGrown = true;
	}

	public void Ungrown()
	{
		GD.Print("ungrowing " + Name);//=
		var animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		animatedSprite2D.Animation = "ungrown";
		GetNode<AnimationPlayer>("AnimationPlayer").Play("RESET");
		GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
		grownCollisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
		GetNode<PointLight2D>("PointLight2D").Position = new Vector2(0, -39);
		isGrown = false;
	}

	public async void OnAreaEntered(Area2D flashlight) {
		var animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		if (animatedSprite2D.Animation == "ungrown") {
			//the collision shape now enables before it fully grows (in case you want to speedrun)
			GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
			grownCollisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
			animatedSprite2D.Animation = "growing";
			animatedSprite2D.Play();
			isGrown = true;

			GetNode<AnimationPlayer>("AnimationPlayer").Play("grow");

			await ToSignal(animatedSprite2D, AnimatedSprite2D.SignalName.AnimationFinished);
			animatedSprite2D.Animation = "grown";
		}
	}

	public void OnBodyEntered(Node2D player) {
		if (player is GroundPlayer groundPlayer) {
			groundPlayer.Climbing = true;
		}
	}
	public void OnBodyExited(Node2D player) {
		if (player is GroundPlayer groundPlayer) {
			groundPlayer.Climbing = false;
		}
	}
}
