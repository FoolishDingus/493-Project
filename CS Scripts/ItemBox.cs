using System;
using Godot;
//using System;

/*
stores an array of items

When body entered:
if player, give random item
if projectile/stage projectile, disable hitbox and turn invisible, start respawn countdown
*/
public partial class ItemBox : Area3D
{
	string[] items = {"heal", "rocket", "bomb", "shield", "speed"};	// items could possibly be turned into their own class
	//string[] items = {"rocket", "bomb", "shield", "speed"};
	MeshInstance3D crate;		// crate model
	GpuParticles3D particles;	// explosion of particles when box collides with something
	Timer respawnTimer;			// time before box reappears
	bool respawning = true;		// if false, the item box is deleted after being broken

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		crate = GetChild<CollisionShape3D>(0).GetChild<MeshInstance3D>(0);
		particles = GetChild<GpuParticles3D>(1);
		respawnTimer = GetChild<Timer>(2);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}


	private void HitboxCollision(Node3D body)
	{
		switch(body.GetClass())
		{
			// players
			case "CharacterBody3D":
				GiveItem((Player)body);
				break;
			// projectiles
			case "RigidBody3D":
				BreakBox();
				break;
			default:
				GD.PrintErr("Item box collided with something it shouldn't");
				break;
		}
	}

	// give random item to player
	private void GiveItem(Player player)
	{
		// choose a random item, set that item to player's itemSlot
		Random rng = new Random();								// create RNG value
		string itemValue = items[rng.Next(0, items.Length)];	// get one of the items
		if(itemValue == "heal")									// immediately use healing items to prevent hoarding
			player.HealDamage(20);
		else
		{
			player.itemSlot = itemValue;						// set player's item
			player.itemSlotLabel.Text = itemValue;				// update item panel on HUD
		}
		BreakBox();
	}

	// destroy box and start respawn timer
	private void BreakBox()
	{
		SetDeferred("monitoring", false);	// disable the hitbox
		particles.Emitting = true;			// launch box pieces
		crate.Visible = false;				// box turns invisible until it respawns
		respawnTimer.Start();
	}

	// make item box visible and enable hitbox again
	private void RespawnTimeout()
	{
		if(respawning)
		{
			//GD.Print("box respawn");
			crate.Visible = true;
			Monitoring = true;
		}
		else
			QueueFree();
	}
}
