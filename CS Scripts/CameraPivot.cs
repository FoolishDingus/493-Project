using Godot;
//using System;

// fix quaternion
// get spectate cam from split, if player is null, follow spectate cam

public partial class CameraPivot : Node3D
{
	Camera3D playerCamera;
	Player targetPlayer;
	//MultiplayerHandler splitscreenHandler;
	//Stage currentStage;
	public CameraPivot spectateCam;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        playerCamera = GetChild<Camera3D>(0);
		targetPlayer = GetParentOrNull<Player>();
		if(targetPlayer == null)// probably remove
		{
			//splitscreenHandler = (SplitscreenHandler)GetParent().GetParent().GetParent();
			//spectate = splitscreenHandler.mapSpectateCamera;
			//SceneTree currentScene = GetTree();
			//GD.Print(GetTree());

			//currentStage = (Stage)GetTree().CurrentScene;
		}
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if(targetPlayer != null)
		{
			//Vector3 globalPosition = GlobalPosition;
			Vector3 globalPosition = targetPlayer.GlobalPosition;
			GlobalPosition = globalPosition;
			Vector3 rotationDegrees = RotationDegrees;
			Quaternion quaternion = Quaternion;
			
			// placeholder, turn camera to behind player
			if(rotationDegrees.Y != targetPlayer.RotationDegrees.Y + 180)
				rotationDegrees.Y = targetPlayer.RotationDegrees.Y + 180;
			// create quaternion using targetPlayer.RotationDegrees.Y + 180, set Quaternion to that?


			// change to use slerp with quaternions for smoother movement
			//RotationDegrees = targetPlayer.RotationDegrees;
			//quaternion.Y = targetPlayer.Quaternion.Y;
			//Quaternion = Quaternion.Slerp(quaternion.Normalized(), 0.2f);	// apply smooth camera rotation

			RotationDegrees = rotationDegrees;
		}
		else// splitscreen cam detected, rotate to keep all players on screen?
		{
			
		}
	}
}
