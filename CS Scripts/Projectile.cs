//using System;
//using System.Collections;
using Godot;

// make HandleCollision method that uses signal? try casting types

// base class for all projectiles, also default shot type
public partial class Projectile : RigidBody3D
{
	public float speed = 20.0f;		// speed of projectile
	Vector3 forwardVector;			// direction shot travels
	public float fallSpeed = 0; //- influence from gravity, higher = falls faster, 0 = flies straight
	public string teamColor = "invalid team color";
	Area3D hitbox;
	private int damage = 1;

    public override void _Ready()
    {
		Vector3 globalPosition = GlobalPosition;
		globalPosition.Y += 5f;
		GlobalPosition = globalPosition;
		hitbox = GetChild<Area3D>(0);
    }


	public override void _PhysicsProcess(double delta)
	{
		// ADD: if affected by gravity (fallspeed != 0), gradually rotate down

		forwardVector = GlobalTransform.Basis.Z.Normalized();	// get forward direction
		LinearVelocity = speed * forwardVector;;				// move forward
	}

	// delete projectile when DespawnTimer ends
	private void DespawnTimeout()
	{
		QueueFree();
	}

	private void DestroyProjectile()
	{
		hitbox.SetDeferred("monitoring", false);// disable hitbox at end of current frame
		GetChild<MeshInstance3D>(1).Hide();		// make projectile invisible 
		// play explosion effect here (GPUparticles3D, either include in projectile or load separate object)
		QueueFree(); 				// destroy projectile after any collision
	}




	public void ProjectileCollision(Node3D body)
	{
		// check what the projectile hit and run the corresponding method
		// GD.Print(body.GetClass());
		switch(body.GetClass())
		{
			// players
			case "CharacterBody3D":
				DamagePlayer((Player)body);
				break;
			// other projectiles
			case "RigidBody3D":
				HitProjectile((Projectile)body);
				//DestroyProjectile();
				break;
			// world objects + destructable objects(?)
			case "StaticBody3D":
				DestroyProjectile();
				break;
			// Destructable objects
			//case "Destructable"// don't use, GetClass() doesn't get custom class for some reason
				//DamageDestructable((Destructable)body);
				//break
			default:
				GD.PrintErr("Projectile collided with something it shouldn't");
				break;
		}

		
	}

	private void DamagePlayer(Player player)
	{
		if(player.teamColor != teamColor)
		{
			player.TakeDamage(damage); // deal damage to player
			//GD.Print(player.hp);
			DestroyProjectile();
		}
	}

	// projectiles destroy each other if they collide
	private void HitProjectile(Projectile projectile)
	{
		if(projectile.teamColor != teamColor)
			DestroyProjectile();
	}

}
