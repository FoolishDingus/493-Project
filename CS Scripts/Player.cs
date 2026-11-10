using Godot;
//using System;

/*
Changes:
-Increased player HP to 100
-Added Box map, shrunk projectiles, increased fire rate
-Added parry system, reflects projectiles, damages players, has a 1 sec cooldown
-Added debug controls so P2 can be moved without controllers (arrow keys, right shift, and , . /)
-added a SpringArm3D to CameraPivot, giving the cameras collision with the world
-updated button layout for controllers, see changes below
-players emit smoke particles at 1/4 HP
-Fixed analog input, increased player speed

// add check for projectiles colliding with Stage Projectiles (make sure they don't get destroyed if they aren't supposed to)

Use for inputs: $"move_left_{playerID}"
{playerID} is the controller #, so for player 1 this is treated like "move_left_1"

Things we can add (Split into new branch for each category):
Ranking system:
-Set player's ranking when they die
-Once everyone else has died, declare the remaining player as the winner and end the game

UI:
-Item slot: shows image of an item when collected

Items:
-Create a floating item crate that gives a random item to the player who flies into it
-Homing rocket: (code based on projectile script) when used, fires a rocket, launches forward until another player is in range, lock on to that player and rotate towards them. Deals noticeably more damage than regular shots
-Health pack: instant heal, gets used immediately when collected
-Bomb: lobs a cartoon bomb into the air, creating a large explosion that can damage anyone, including the user
(note: projectiles are currently not affected by gravity)
-Shield: reduces damage taken

Player:
-change player script to use a state machine
-complete UseItem() once items are added

Other:
-add a script to the death plane and place it in test room (if player enters death plane area, trigger player's PitRespawn code)
-add invisible walls to test room
-add controller rumble when hit?: Input.StartJoyVibration(int device, float weakMagnitude, float strongMagnitude, [float duration = 0]);
-pause menu? GetTree().Paused = true;	ProcessMode = Node.ProcessModeEnum.Pausable;	// Process the node (and its children in Inherit mode) only when the game is not paused.
*/

/* CONTROLS
// keyboard layout:
// wasd to move
// q and e to turn left/right
// shoot with j, use item with k, air brake with l, parry with i
// esc to close game

// new debug controls for P2 on keyboard:
Right Shift to rise
arrow keys to move
/ to fire
. for item
, for parry

new xbox controls:
// strafe with left stick, turn with right stick
// left and right on d-pad now turn
// lift with LT, air brake with LB
// shoot with RT/X
// items with RB/A
// parry with Y
// B, select, and clicking control sticks in are currently unused
*/

public partial class Player : CharacterBody3D
{
	[Export] public int playerID = 0;	// used to determine which controller controls each player
	private Window currentScene;		// get current stage so projectiles can be loaded into it

	// movement variables
	// add acceleration/deceleration
	public const float Speed = 10.0f;
	public const float MaxRiseSpeed = 10f;	//4.5f
	public Vector3 forwardVector;			// direction player is facing
	public Node3D cameraController;			// movement direction is based on the camera's direction

	public int hp = 100;
	public int maxHP = 100;
	// maybe add extra lives?
	public int parryDamage = 15;
	private bool canParry = true;	// can only parry when this is true

	// child nodes
	private PackedScene projectile = GD.Load<PackedScene>("res://Scenes/projectile.tscn");	// Stores location of projectile scene, replace GD.load to use ResourceLoader?
	private Area3D parryHitbox;
	private Timer reloadTimer;	// time before you can shoot another default projectile
	private Timer parryTimer;	// time before you can use a parry again
	private GpuParticles3D smokeParticles;	// emits smoke when low on HP

	// team variables
	[Export(PropertyHint.Enum, "RED,BLUE,GREEN,YELLOW")]// sets enum values for next variable (teamColor)
	public string teamColor = "invalid";				// jump/float determines jumping or floating upwards
	//[ExportGroup("Team Color")] public string teamColor = "invalid team color";
	

	public Vector3 respawnPosition;	// gets set by MultiplayerHandler.cs
	Godot.Collections.Array<int> controllerID = new Godot.Collections.Array<int>();	// stores all connected controllers
	
	public override void _Ready()
	{
		// get child nodes (children of children are ignored)
    cameraController = GetChild<Node3D>(0);
		parryHitbox = GetChild<Area3D>(1);
		reloadTimer = GetChild<Timer>(2);
		parryTimer = GetChild<Timer>(3);
		smokeParticles = GetChild<GpuParticles3D>(4);

		currentScene = GetTree().GetRoot(); // get current level so projectiles can be loaded into it

		if(teamColor == "invalid")
			GD.PrintErr("Player missing team color");

		//if(Input.GetConnectedJoypads().Count > 0)
		controllerID = Input.GetConnectedJoypads();
		//GD.Print(controllerID);
	}

	
	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;
		Vector3 rotationDegrees = RotationDegrees;

		// Add the gravity.
		if (!IsOnFloor())
			velocity += GetGravity() * (float)delta;

		// rising and braking
		if(Input.IsActionPressed($"air_brake_{playerID}"))
		{
			if(velocity.Y > 0)
				velocity.Y = (float)Mathf.MoveToward(velocity.Y, 0, 0.6);
		}
		else if (Input.IsActionPressed($"lift_{playerID}"))
		{
			velocity.Y = (float)Mathf.MoveToward(velocity.Y, MaxRiseSpeed, 0.4);
			// while rising, slowly rotate player upright if sideways (if we turn player into physics object)
		}


		// Get the input direction and handle the movement/deceleration.
		Vector2 inputDir = Input.GetVector($"move_left_{playerID}", $"move_right_{playerID}", $"move_forward_{playerID}", $"move_backward_{playerID}");
		// change inputdir to use analog support (seems to only return 0 or 1), GetActionStrenth()?
		Vector3 moveDirection = (cameraController.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized(); // allows you to move relative to camera instead of player
		forwardVector = GlobalTransform.Basis.Z.Normalized();	// get forward direction


		// allow movement & turning only when off the ground
		if(!IsOnFloor())
		{
			// turning
			if(Input.IsActionPressed($"turn_right_{playerID}"))
				rotationDegrees.Y -= 2;
			if(Input.IsActionPressed($"turn_left_{playerID}"))
				rotationDegrees.Y += 2;
			/*if(Input.IsActionPressed($"turn_up_{playerID}"))
				rotationDegrees.X -= 2;
			if(Input.IsActionPressed($"turn_down_{playerID}"))
				rotationDegrees.X += 2;*/

			// placeholder movement (make movement gradual with velocity.MoveToward)
			if (moveDirection != Vector3.Zero)
				velocity = velocity with {X = (moveDirection.X * Speed) * inputDir.Length(), Z = (moveDirection.Z * Speed) * inputDir.Length()};
			else
				velocity = velocity.MoveToward(velocity with {X = moveDirection.X * Speed, Z = moveDirection.Z * Speed}, 0.2f);			
		}
		else	// stop moving if on ground
			velocity = velocity.MoveToward(velocity with {X = 0, Z = 0}, 0.2f);		
		//*/

		Velocity = velocity;
		RotationDegrees = rotationDegrees;
		MoveAndSlide();

		if(Input.IsActionPressed($"shoot_{playerID}") && reloadTimer.IsStopped())
			ShootDefault();
	}


	// handle IsActionPressed inputs (using items)
    public override void _Input(InputEvent playerInput)
    {
    //GD.Print(playerInput.Device); check input IDs
		if(playerInput.IsActionPressed($"parry_{playerID}"))
			HandleParry();
		else if(playerInput.IsActionPressed($"item_{playerID}"))
			UseItem();
    }


	// create default projectile
	private void ShootDefault()
	{
		reloadTimer.Start();
		Projectile shotInstance = projectile.Instantiate<Projectile>();	// create projectile object
		currentScene.AddChild(shotInstance);							// add instance to parent (aka the current stage)
		shotInstance.GlobalRotation = GlobalRotation;					// shot matches player rotation
		shotInstance.forwardVector = forwardVector;						// set shot to point in the direction you are facing
		// shot spawns at player position (slightly up on y axis so the shot isn't in the ground)
		shotInstance.GlobalPosition = GlobalPosition with {Y = GlobalPosition.Y + 1.25f};//(GlobalPosition.X, GlobalPosition.Y + 1.25f, GlobalPosition.Z);
		shotInstance.teamColor = teamColor;	// set projectile to player's team color (prevents friendly fire/shooting yourself)
	}


	// similar to shoot, but uses up held item (rocket, bomb, shield, etc.), doesn't affect reloadTimer
	private void UseItem()
	{
		GD.Print("item button");
		// if player is holding healing item, add HP
		// if holding weapon, spawn projectile weapon
	}


	// parry anything found in parryHitbox
	private void HandleParry()
	{
		// check if parry is available and if you've parried something
		if(parryTimer.IsStopped())// && parryHitbox.HasOverlappingBodies())
		{
			//GD.Print("used parry");
			parryTimer.Start();
			// go through every object found in parryHitbox (should only detect Players, Projectiles, and Stage Projectiles)
			foreach (Node3D body in parryHitbox.GetOverlappingBodies())
			{
				// check what the parry hit and run the corresponding method
				// GD.Print(body.GetClass());
				switch(body.GetClass())
				{
					// hurt enemy players
					case "CharacterBody3D":
						ParryPlayer((Player)body);
						break;
					// reflect projectiles
					case "RigidBody3D":
						ParryProjectile((Projectile)body);
						break;
				}
			}
		}
		//else
		//	GD.Print("can't parry yet");
	}


	// this might return player you are using
	public void ParryPlayer(Player player)
	{
		if(player.teamColor != teamColor)
		{
			player.TakeDamage(parryDamage);			// deal damage to enemy player
			player.Velocity += forwardVector * 15;	// push enemy away
			//GD.Print(player.hp);
			//GD.Print("parried player");
		}
	}


	// if parrying an enemy projectile, reflect it back in the direction you are facing
	private void ParryProjectile(Projectile projectile)
	{
		if(projectile.teamColor != teamColor)
		{
			//GD.Print("parried projectile");
			projectile.teamColor = teamColor;	// allows reflected projectiles to deal damage to original shooter
			projectile.damage *= 2;				// increase damage of parried projectiles (experimental)
			projectile.forwardVector = forwardVector;
		}
	}


	// subtract damage and check if dead
	public void TakeDamage(int damage)
	{
		hp -= damage;
		//GD.Print("Player " + playerID + " took damage");
		if(hp <= 0)
			QueueFree(); // replace to set playerState = "dead"
		else if(hp <= maxHP/4)
			smokeParticles.Emitting = true;	// emit smoke particles if low on heath
	}


	// add HP from healing items
	public void HealDamage(int healing)
	{
		hp += healing;
		//GD.Print("Player " + playerID + " took damage");
		//if(hp > 25)
			//stop emitting smoke
	}


	// if you hit a death plane, respawn with some damage taken
	public void PitRespawn()
	{
		GlobalPosition = respawnPosition;
		TakeDamage(20);
	}
}
