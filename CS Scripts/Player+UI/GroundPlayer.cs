using Godot;
using System;

public partial class GroundPlayer : Player
{
	//public new float Speed = 70.0f;
	public const float JumpVelocity = -300; //-330.0f;
	public bool Climbing{get; set;} = false;

	private AnimatedSprite2D animatedSprite;
	private Godot.Timer hurtTimer;
	public override void _Ready() {
		base._Ready();
		base.Speed = 120;
		base.Gravity = 0.0003F;
		InputEnabled = true;

		hurtTimer = GetNode<Godot.Timer>("HurtTimer");
		animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		animatedSprite.Animation = "sit_right";
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!MovementIsDisabled())
		{
			if (!Climbing) {
				Vector2 velocity = Velocity;

				// Add the gravity.
				if (!IsOnFloor())
				{
					velocity += GetGravity() * (float)delta;
				}

				// Handle Jump.
				if (InputEnabled) {
					if (Input.IsActionJustPressed("move_up") && IsOnFloor())
					{
						velocity.Y = JumpVelocity;
					}

					// Get the input direction and handle the movement/deceleration.
					// As good practice, you should replace UI actions with custom gameplay actions.
					var direction = Vector2.Zero;
					if (Input.IsActionPressed("move_right"))
					{
						direction.X = 1;
					}
					else if (Input.IsActionPressed("move_left"))
					{
						direction.X = -1;
					}

					if (direction != Vector2.Zero)
					{
						velocity.X = direction.X * Speed;
						if (direction.X < 0) {
							animatedSprite.Animation = "walk_left";
							FacingRight = false;
						}
						else if (direction.X > 0) {
							animatedSprite.Animation = "walk_right";
							FacingRight = true;
						}
						animatedSprite.Play();
					}
					else //if velocity == zero
					{
						velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
						if (FacingRight) {
							animatedSprite.Animation = "sit_right";
						}
						else {
							animatedSprite.Animation = "sit_left";
						}
						animatedSprite.Stop();
					}

					if (velocity.Y != 0) {
						if (FacingRight) {
							animatedSprite.Animation = "jump_right";
						}
						else {
							animatedSprite.Animation ="jump_left";
						}
					}
				}

				Velocity = velocity;
				//MoveAndSlide();
			}
			else { //climbing
				var dir = Vector2.Zero;
				if (InputEnabled) {
					if (Input.IsActionPressed("move_up")) {
						dir.Y -= 1;
					}
					if (Input.IsActionPressed("move_down")) {
						dir.Y += 1;
					}
					if (Input.IsActionPressed("move_right")) {
						dir.X += 1;
					}
					if (Input.IsActionPressed("move_left")) {
						dir.X -= 1;
					}
				}
				if (dir.Length() > 0) {
					dir *= Speed; //dir.Normalized() * Speed; ///wait why normalized
				}

				animatedSprite.Animation = "climb";
				
				Velocity = dir;
			}
		} //end of if movement not disabled

		//flashing when hurt
		if (Flash)
		{
			if (hurtTimer.TimeLeft % 0.2 < 0.1)
			{
				animatedSprite.SelfModulate = new Color(1, 1, 1, 1);
			}
			else
			{
				animatedSprite.SelfModulate = new Color(100, 1, 1, 1);
			}
		}

		//Sea bunny bounce
		//Finished quest 7: get treasure
		if (GlobalScript.QuestNum >= GlobalScript.MainQuests.IndexOf("GetBoat")) //number
		{
			int count = GetSlideCollisionCount();
			for (int i = 0; i < count; i++) {
				var info = GetSlideCollision(i);
				var collider = info.GetCollider();
				if (collider is Seabunny boss) {
					//direction it bounces and how fast
					Velocity = new Vector2(0, -400);
					//how high up it can go
					//Position += new Vector2(0, -70);
					SetDisableControlBounce(true);
					globalSound.PlaySound("bounce");
					TimeBounce();
					break;
				}
			}
		}
		MoveAndSlide();
		OOBCheck();
	}
	
	private async void TimeBounce() {
		await ToSignal(GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);
		SetDisableMovement(false);
	}

	//change player back to normal after flashing animation
	new private void OnHurtTimerTimeout()
	{
		base.OnHurtTimerTimeout();
		animatedSprite.Modulate = new Color(1, 1, 1, 1);
	}

	private void OnPlayerDied()
	{
		animatedSprite.Animation = "died";
	}

	//can change the code to go to different room instead
	//= can this be deleted?
	private void OOBCheck() {
		var pos = GlobalPosition;
		// if (GetParent().Name == "EnterCaveRoom" && pos.Y > 180) {
		// 	base.Respawn(); //=change this
		// }
	}
}
