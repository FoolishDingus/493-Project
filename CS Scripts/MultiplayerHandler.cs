//using System.Diagnostics;
//using System.Linq;
using Godot;
//using System;

// sets up splitscreen depending on # of players, gets their spawnpoints from Stage.cs
public partial class MultiplayerHandler : GridContainer
{
	public int controllerCount = 0;	// tracks how many players are using controllers
	public SubViewportContainer[] viewports = new SubViewportContainer[4];

	PlayerSpawner playerSpawner;
	Player[] players = new Player[4];					// each player has their own viewport
	
	[Export] Vector3[] spawnPositions = new Vector3[4];	// set currentStage spawn positions in editor
	CameraPivot[] spectatorCameras = new CameraPivot[4]; 
	Vector3[] spectatorCameraPositions = new Vector3[4];// stores the positions of each spectator camera

	//string[] teamColors = new string[4];				// set player color for team matches (low priority)

	public override void _Ready()
	{
		//GD.Print(Owner.Name);
		playerSpawner = (PlayerSpawner)Owner;	// used to get player spawn points and where spectator cameras go
		
		//controllerCount = Input.GetConnectedJoypads().Count;	// count # of controllers connected, determines how many players spawn
		controllerCount = 4;	// debug, sets # of players to test splitscreen
		GD.Print(controllerCount + " controllers detected");

		// get each child viewport
		for(int i = 0; i < viewports.Length; i++)
			viewports[i] = GetChild<SubViewportContainer>(i);

		// if multiple controllers detected (aka playing multiplayer), set splitscreen to match # of players
		if(controllerCount > 0) // 1
		{
			GetPlayers();
			SetSplitscreen();
			SpawnPlayers();
			SetCameraPositions();
		}
		else	// delete splitscreen object entirely if playing single player (needs to spawn a new Player)
			GetParent().QueueFree();
		
	}

	// adjust viewports based on local player count
	private void SetSplitscreen()
	{
		if(controllerCount == 2)
		{
			Columns = 1;	// horizontal splitscreen, set to 2 for vertical
			// delete viewports for P3 and P4
			viewports[2].QueueFree();
			viewports[3].QueueFree();
		}
		else if(controllerCount == 3)
		{
			// only delete P4 viewport
			viewports[3].QueueFree();
		}
	}

	// place each player at their spawn point
	private void SpawnPlayers()
	{
		//GD.Print("Spawning players");
		spawnPositions = playerSpawner.GetSpawnPositions();
		for(int i = 0; i < controllerCount; i++)
		{
			players[i].GlobalPosition = spawnPositions[i];
			players[i].respawnPosition = spawnPositions[i];
			//players[i].teamColor = teamColors[i];
		}
	}

	// get each human player
	private void GetPlayers()
	{
		for(int i = 0; i < controllerCount; i++)
			players[i] = GetChild<SubViewportContainer>(i).GetChild<SubViewport>(0).GetChild<Player>(0);
	}

	// get the positions of each spectator camera and apply to each
	private void SetCameraPositions()
	{
		// get each viewport's camera
		for(int i = 0; i < spectatorCameras.Length; i++)
			spectatorCameras[i] = GetChild<SubViewportContainer>(i).GetChild<SubViewport>(0).GetChild<CameraPivot>(1);

		// get stage positions of each camera and apply
		spectatorCameraPositions = playerSpawner.GetCameraPositions();
		for(int i = 0; i < spectatorCameraPositions.Length; i++)
			spectatorCameras[i].GlobalPosition = spectatorCameraPositions[i];
	}


}
