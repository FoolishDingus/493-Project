using Godot;
//using System;

// place this node in a scene to spawn up to 4 players, set their spawn locations in the inspector
// anything else related to multiplayer is handled in MultiplayerHandler.cs
public partial class PlayerSpawner : Node
{	
	Player[] players = new Player[4];	// track player ranking?

	// export variables can be adjusted in the inspector
	[Export] Vector3 spawnP1 = new Vector3(-20,0,-20);		// red
	[Export] Vector3 spawnP2 = new Vector3(20,0,20);		// blue
	[Export] Vector3 spawnP3 = new Vector3(-20,0,20);		// green
	[Export] Vector3 spawnP4 = new Vector3(20,0,-20);		// yellow


	// spectator camera positions (death -> cut to spectator camera)
	[Export] Vector3 cameraPosition1 = new Vector3(-20,20,-20);	// red
	[Export] Vector3 cameraPosition2 = new Vector3(20,20,20);	// blue
	[Export] Vector3 cameraPosition3 = new Vector3(-20,20,20);	// green
	[Export] Vector3 cameraPosition4 = new Vector3(20,20,-20);	// yellow
	
	

	// possible single player setup? (low priority)
	public override void _Ready()
	{
		// if single player, delete grid container and load a Player object at spawnP1 (low priority, designing this game with only human players currently)
	}

	// currently used to close game, might be useful for a pause menu?
    public override void _Input(InputEvent @event)
    {
		// close game with escape key
		if (Input.IsActionPressed("quit"))
			GetTree().Quit();
    }

	public Vector3[] GetSpawnPositions()
	{
		return [spawnP1, spawnP2, spawnP3, spawnP4];
	}

	public Vector3[] GetCameraPositions()
	{
		return [cameraPosition1, cameraPosition2, cameraPosition3, cameraPosition4];
	}
}
