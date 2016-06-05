using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	public float turnDelay = 0.1f;							//Delay between each Player turn.
	public int healthPoints = 100;							//Starting value for Player health points.
	public static GameManager instance = null;				//Static instance of GameManager which allows it to be accessed by any other script.
	[HideInInspector] public bool playersTurn = true;		//Boolean to check if it's players turn, hidden in inspector but public.

	private BoardManager boardScript;
	private List<Enemy> enemies;							//List of all Enemy units, used to issue them move commands.
	private bool enemiesMoving;								//Boolean to check if enemies are moving.
	private DungeonManager dungeonScript;
	private Player playerScript;




	void Awake() {
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy (gameObject);	
		}
		DontDestroyOnLoad(gameObject);
		
		enemies = new List<Enemy>();

		boardScript = GetComponent<BoardManager> ();

		dungeonScript = GetComponent<DungeonManager> ();
		playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<Player> ();
		
		InitGame();
	}
	
	//This is called each time a scene is loaded.
	void OnLevelWasLoaded(int index)
	{
		//Call InitGame to initialize our level.
		InitGame();
	}
	
	//Initializes the game for each level.
	void InitGame()
	{
		//Clear any Enemy objects in our List to prepare for next level.
		enemies.Clear();

		boardScript.BoardSetup ();
	}
	
	//Update is called every frame.
	void Update()
	{
		//Check that playersTurn or enemiesMoving or doingSetup are not currently true.
		if(playersTurn || enemiesMoving)
			
			//If any of these are true, return and do not start MoveEnemies.
			return;
		
		//Start moving enemies.
		StartCoroutine (MoveEnemies ());
	}
	
	//GameOver is called when the player reaches 0 health points
	public void GameOver()
	{
		//Disable this GameManager.
		enabled = false;
	}
	
	//Coroutine to move enemies in sequence.
	IEnumerator MoveEnemies()
	{
		//While enemiesMoving is true player is unable to move.
		enemiesMoving = true;
		
		//Wait for turnDelay seconds, defaults to .1 (100 ms).
		yield return new WaitForSeconds(turnDelay);
		
		//If there are no enemies spawned (IE in first level):
		if (enemies.Count == 0) 
		{
			//Wait for turnDelay seconds between moves, replaces delay caused by enemies moving when there are none.
			yield return new WaitForSeconds(turnDelay);
		}

		playersTurn = true;

		//Enemies are done moving, set enemiesMoving to false.
		enemiesMoving = false;
	}

	public void UpdateBoard(int horizontal, int vertical) {
		boardScript.AddToBoard (horizontal, vertical);
	}















	public void EnterDungeon() {
		dungeonScript.StartDungeon ();
		boardScript.SetDungeonBoard (dungeonScript.gridPositions, dungeonScript.maxBound, dungeonScript.endPos);
		playerScript.dungeonTransition = false;
	}

	public void ExitDungeon() {
		boardScript.SetWorldBoard ();
		playerScript.dungeonTransition = false;
	}
}