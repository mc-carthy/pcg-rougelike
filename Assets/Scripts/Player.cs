using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;	//Allows us to use UI.

//Player inherits from MovingObject, our base class for objects that can move, Enemy also inherits from this.
public class Player : MovingObject
{
	public static bool isFacingRight;
	public int wallDamage = 1;					//How much damage a player does to a wall when chopping it.
	public Text healthText;						//UI Text to display current player health total.
	private Animator animator;					//Used to store a reference to the Player's animator component.
	private int health;							//Used to store player health points total during level.
	public static Vector2 position;
	
	public bool onWorldBoard;
	public bool dungeonTransition;

	public Image glove;
	public Image boot;
	private Weapon weapon;
	public Image weaponComp1, weaponComp2, weaponComp3;

	public int attackMod = 0, defenseMod = 0;
	private Dictionary<string, Item> inventory;



	protected override void Start () {		
		animator = GetComponent<Animator>();

		health = GameManager.instance.healthPoints;
		
		healthText.text = "Health: " + health;

		position.x = position.y = 2;

		onWorldBoard = true;
		dungeonTransition = false;

		inventory = new Dictionary<string, Item> ();
		
		base.Start ();
	}
	
	private void Update ()
	{
		//If it's not the player's turn, exit the function.
		if(!GameManager.instance.playersTurn) return;
		
		int horizontal = 0;  	//Used to store the horizontal move direction.
		int vertical = 0;		//Used to store the vertical move direction.

		bool canMove = false;
		
		//Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction
		horizontal = (int) (Input.GetAxisRaw ("Horizontal"));
		
		//Get input from the input manager, round it to an integer and store in vertical to set y axis move direction
		vertical = (int) (Input.GetAxisRaw ("Vertical"));
		
		//Check if moving horizontally, if so set vertical to zero.
		if(horizontal != 0)
		{
			vertical = 0;
		}

		//Check if we have a non-zero value for horizontal or vertical
		if(horizontal != 0 || vertical != 0) {
			if (!dungeonTransition) {
				if (onWorldBoard) {
					canMove = AttemptMove<Wall> (horizontal, vertical);
				} else {
					canMove = AttemptMove<Chest> (horizontal, vertical);
				}

				if (canMove && onWorldBoard) {
					position.x += horizontal;
					position.y += vertical;
					GameManager.instance.UpdateBoard (horizontal, vertical);
				}
			}
		}
	}
	
	//AttemptMove overrides the AttemptMove function in the base class MovingObject
	//AttemptMove takes a generic parameter T which for Player will be of the type Wall, it also takes integers for x and y direction to move in.
	protected override bool AttemptMove <T> (int xDir, int yDir)
	{	
		if (xDir == 1 && !isFacingRight) {
			isFacingRight = true;
		} else if (xDir == -1 && isFacingRight) {
			isFacingRight = false;
		}
		//Call the AttemptMove method of the base class, passing in the component T (in this case Wall) and x and y direction to move.
		bool hit = base.AttemptMove <T> (xDir, yDir);
		
		//Set the playersTurn boolean of GameManager to false now that players turn is over.
		GameManager.instance.playersTurn = false;

		return hit;
	}
	
	
	//OnCantMove overrides the abstract function OnCantMove in MovingObject.
	//It takes a generic parameter T which in the case of Player is a Wall which the player can attack and destroy.
	protected override void OnCantMove <T> (T component) {
		print (typeof(T));
		if (typeof(T) == typeof(Wall)) {
			Wall blockingObj = component as Wall;
		
			//Call the DamageWall function of the Wall we are hitting.
			blockingObj.DamageWall (wallDamage);
		} else if (typeof(T) == typeof(Chest)) {
			Chest blockingObj = component as Chest;
			print ("OPEN!");
			blockingObj.Open ();
		}
		//Set the attack trigger of the player's animation controller in order to play the player's attack animation.
		animator.SetTrigger ("playerChop");
		if (weapon) {
			weapon.UseWeapon ();
		}
	}
	
	//LoseHealth is called when an enemy attacks the player.
	//It takes a parameter loss which specifies how many points to lose.
	public void LoseHealth (int loss)
	{
		//Set the trigger for the player animator to transition to the playerHit animation.
		animator.SetTrigger ("playerHit");
		
		//Subtract lost health points from the players total.
		health -= loss;
		
		//Update the health display with the new total.
		healthText.text = "-"+ loss + " Health: " + health;
		
		//Check to see if game has ended.
		CheckIfGameOver ();
	}
	
	
	//CheckIfGameOver checks if the player is out of health points and if so, ends the game.
	private void CheckIfGameOver ()
	{
		//Check if health point total is less than or equal to zero.
		if (health <= 0) 
		{	
			//Call the GameOver function of GameManager.
			GameManager.instance.GameOver ();
		}
	}

	private void GoDungeonPortal() {
		if (onWorldBoard) {
			onWorldBoard = false;
			GameManager.instance.EnterDungeon ();
			transform.position = DungeonManager.startPos;
		} else {
			onWorldBoard = true;
			GameManager.instance.ExitDungeon ();
			transform.position = position;
		}
	}

	private void UpdateInventory(Collider2D item) {
		Item itemData = item.GetComponent<Item> ();
		switch (itemData.type) {
		case itemType.glove:
			if (!inventory.ContainsKey ("glove")) {
				inventory.Add ("glove", itemData);
			} else {
				inventory ["glove"] = itemData;
			}
			glove.color = itemData.level;
			break;
		case itemType.boot:
			if (!inventory.ContainsKey ("boot")) {
				inventory.Add ("boot", itemData);
			} else {
				inventory ["boot"] = itemData;
			}
			boot.color = itemData.level;
			break;
		}

		attackMod = 0;
		defenseMod = 0;

		foreach (KeyValuePair<string, Item> gear in inventory) {
			attackMod += gear.Value.attackMod;
			defenseMod += gear.Value.defenseMod;
		}

		if (weapon) {
			wallDamage = attackMod + 3;
		}
	}

	private void UpdateHealth(Collider2D item) {
		if (health < 100) {
			if (item.tag == "Food") {
				health += Random.Range (1, 4);
			} else {
				health += Random.Range (4, 11);
			}
			GameManager.instance.healthPoints = health;
			healthText.text = "Health: " + health;
		}
	}

	private void OnTriggerEnter2D (Collider2D other) {
		if (other.tag == "Exit") {
			dungeonTransition = true;
			Invoke ("GoDungeonPortal", 0.5f);
			Destroy (other.gameObject);
		} else if (other.tag == "Food" || other.tag == "Soda") {
			UpdateHealth (other);
			Destroy (other.gameObject);
		} else if (other.tag == "Item") {
			UpdateInventory (other);
			Destroy (other.gameObject);
		} else if (other.tag == "Weapon") {
			if (weapon) {
				Destroy (transform.GetChild (0).gameObject);
			}
			other.enabled = true;
			other.transform.parent = transform;
			weapon = other.GetComponent<Weapon> ();
			weapon.AcquireWeapon ();
			weapon.inPlayerInventory = true;
			weapon.EnableSpriteRenderer (false);
			wallDamage = attackMod + 3;
			weaponComp1.sprite = weapon.getComponentImage (0);
			weaponComp2.sprite = weapon.getComponentImage (1);
			weaponComp3.sprite = weapon.getComponentImage (2);
		}
	}
}

