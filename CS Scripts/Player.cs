using Godot;
//using System;

/*
Things we can add (Split into new branch for each category):
Ranking system:
-Set player's ranking when they die
-Once everyone else has died, declare the remaining player as the winner and end the game

UI:
-Health bar or health percentage (note: HP is currently super low for testing)
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
-complete ShootItem() once items are added
-move player cameras up so it is easier to see in front of you
-add analog input support (mess with inputDir?)

Other:
-add a script to the death plane and place it in test room (if player enters death plane area, trigger player's PitRespawn code)
-add invisible walls to test room
-add rumble when hit?: Input.StartJoyVibration(int device, float weakMagnitude, float strongMagnitude, [float duration = 0]);
-pause menu? GetTree().Paused = true;	ProcessMode = Node.ProcessModeEnum.Pausable;	// Process the node (and its children in Inherit mode) only when the game is not paused.
*/

// keyboard layout:
// wasd to move
// q and e to turn left/right
// shoot with j, use item with k, air brake with l
// esc to close game

// xbox controls:
// strafe with left stick, turn with right or LB/RB
// lift with RT, air brake with LT (put on A and make LT shoot instead?)
// shoot with X
// items with B

public partial class Player : CharacterBody3D
{
	[Export] public int playerID = 0;	// used to determine which controller controls each player
	private Window currentScene;		// get current stage so projectiles can be loaded into it

	// movement variables
	// add acceleration/deceleration
	public const float Speed = 5.0f;
	public const float MaxRiseSpeed = 10f;	//4.5f
	public Vector3 forwardVector;
	public Node3D cameraController;						// movement direction is based on the camera's direction

	public int hp = 3;									// change to 100 later
	// maybe add extra lives?
	private PackedScene projectile = GD.Load<PackedScene>("res://Scenes/projectile.tscn");	// Stores location of projectile scene, replace GD.load to use ResourceLoader?
	private Timer reloadTimer;							// time before you can shoot another default projectile
	[Export(PropertyHint.Enum, "RED,BLUE,GREEN,YELLOW")]// sets enum values for next variable (teamColor)
    public string teamColor = "invalid";				// jump/float determines jumping or floating upwards
	//[ExportGroup("Team Color")] public string teamColor = "invalid team color";
	

	public Vector3 respawnPosition;	// gets set by MultiplayerHandler.cs
	Godot.Collections.Array<int> controllerID = new Godot.Collections.Array<int>();	// stores all connected controllers
    
    public override void _Ready()
    {
		// get child nodes (children of children are ignored)
        cameraController = GetChild<Node3D>(0);
		reloadTimer = GetChild<Timer>(1);
		currentScene = GetTree().GetRoot(); // get current level so projectiles can be loaded into it

		if(teamColor == "invalid")
			GD.PrintErr("Player missing team color");

		//if(Input.GetConnectedJoypads().Count > 0)
		controllerID = Input.GetConnectedJoypads();
		//GD.Print(controllerID);

		// if no controllers found, set player 1's ID to 16 (the device ID for keyboard inputs) and add the keyboard as a controller ID
		if(controllerID.Count == 0 && playerID == 0)
		{
			controllerID.Add((int)Godot.InputEvent.DeviceIdKeyboard);
			//GD.Print(controllerID[0]);
			playerID = (int)Godot.InputEvent.DeviceIdKeyboard;	// default to keyboard device (16 is ID for keyboard in Godot 4.7)
		}
    }

	
	public override void _PhysicsProcess(double delta)
	{
		// match controller inputs to playerID (note: keyboard and controller 1 sometimes share an ID of 0, this might change if you open the project without connecting a controller)
		for(int i = 0; i < controllerID.Count; i++)	// there might be a better way to do this, but this works for now
		{
			if(playerID == controllerID[i])	// find a way for this to work properly when no controllers are connected
			{
				Vector3 velocity = Velocity;
				Vector3 rotationDegrees = RotationDegrees;

				// Add the gravity.
				if (!IsOnFloor())
					velocity += GetGravity() * (float)delta;
				
				// rising and braking
				if(Input.IsActionPressed("air_brake"))
				{
					if(velocity.Y > 0)
						velocity.Y = (float)Mathf.MoveToward(velocity.Y, 0, 0.6);
				}
				else if (Input.IsActionPressed("lift"))
				{
					velocity.Y = (float)Mathf.MoveToward(velocity.Y, MaxRiseSpeed, 0.4);
					// while rising, slowly rotate player upright if sideways (if we turn player into physics object)
				}
				

				// Get the input direction and handle the movement/deceleration.
				Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
				// change inputdir to use analog support (seems to only return 0 or 1), GetActionStrenth()?
				Vector3 moveDirection = (cameraController.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized(); // allows you to move relative to camera instead of player
				forwardVector = GlobalTransform.Basis.Z.Normalized();	// get forward direction


				// allow movement & turning only when off the ground
				if(!IsOnFloor())
				{
					// turning
					if(Input.IsActionPressed("turn_right"))
						rotationDegrees.Y -= 2;
					if(Input.IsActionPressed("turn_left"))
						rotationDegrees.Y += 2;
					
					// placeholder movement (make movement gradual with velocity.MoveToward)
					if (moveDirection != Vector3.Zero)
						velocity = velocity with {X = moveDirection.X * Speed, Z = moveDirection.Z * Speed};
					else
						velocity = velocity.MoveToward(velocity with {X = moveDirection.X * Speed, Z = moveDirection.Z * Speed}, 0.2f);			
				}
				else	// stop moving if on ground
					velocity = velocity.MoveToward(velocity with {X = 0, Z = 0}, 0.2f);		
				//*/

				Velocity = velocity;
				RotationDegrees = rotationDegrees;
				MoveAndSlide();

				if(Input.IsActionPressed("shoot") && reloadTimer.IsStopped())
					ShootDefault();
			}
		}
	}

	// handle IsActionJustPressed inputs (using items)
    public override void _Input(InputEvent playerInput)
    {
        //GD.Print(playerInput.Device); check input IDs
    }

	// create default projectile
	private void ShootDefault()
	{
		reloadTimer.Start();
		Projectile shotInstance = projectile.Instantiate<Projectile>();	// create projectile object
		currentScene.AddChild(shotInstance);							// add instance to parent (aka the current stage)
		shotInstance.GlobalRotation = GlobalRotation;					// shot matches player rotation
		// shot spawns at player position (slightly up on y axis so the shot isn't in the ground)
		shotInstance.GlobalPosition = GlobalPosition with {Y = GlobalPosition.Y + 1.25f};//(GlobalPosition.X, GlobalPosition.Y + 1.25f, GlobalPosition.Z);
		shotInstance.teamColor = teamColor;	// set projectile to player's team color (prevents friendly fire/shooting yourself)
	}

	// similar to shoot, but uses up held item (rocket, bomb, shield, etc.), doesn't affect reloadTimer
	private void ShootItem()
	{
		
	}

	// subtract damage and check if dead
	public void TakeDamage(int damage)
	{
		hp -= damage;
		//GD.Print("Player " + playerID + " took damage");
		if(hp < 1)
			QueueFree(); // replace to set playerState = "dead"
	}

	// if you fall to low, respawn with some damage taken
	public void PitRespawn()
	{
		GlobalPosition = respawnPosition;
		TakeDamage(2);
	}
}

/*
From GodotDocs:
Array[int] get_connected_joypads()
Returns an Array containing the device IDs of all currently connected joypads.

Note: The order of connected joypads can not be guaranteed to be the same after a project and/or the editor is restarted,
because Godot doesn't save the order of joypad connections. Joypads are registered in the order they are discovered by Godot.
*/
