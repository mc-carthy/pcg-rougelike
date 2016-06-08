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

	public bool enemiesFaster = false;
	public bool enemiesSmarter = false;
	public int enemySpawnRatio = 20;

	private BoardManager boardScript;
	private List<Enemy> enemies;							//List of all Enemy units, used to issue them move commands.
	private bool enemiesMoving;								//Boolean to check if enemies are moving.
	private DungeonManager dungeonScript;
	private Player playerScript;
	private bool playerInDungeon;




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

		playerInDungeon = false;
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
	IEnumerator MoveEnemies() {
		enemiesMoving = true;
		yield return new WaitForSeconds(turnDelay);
		if (enemies.Count == 0) {
			yield return new WaitForSeconds(turnDelay);
		}
		List<Enemy> enemiesToDestroy = new List<Enemy>();
		for (int i = 0; i < enemies.Count; i++) {
			if (playerInDungeon) {
				if ((!enemies[i].GetSpriteRenderer().isVisible)) {
					if (i == enemies.Count - 1) {
						yield return new WaitForSeconds(enemies[i].moveTime); 
					}
					continue;
				}
			} else {
				if ((!enemies[i].GetSpriteRenderer().isVisible) || (!boardScript.checkValidTile (enemies[i].transform.position))) {
					enemiesToDestroy.Add(enemies[i]);
					continue;
				}
			}

			enemies [i].MoveEnemy ();

			yield return new WaitForSeconds (enemies [i].moveTime);
		}
		playersTurn = true;
		enemiesMoving = false;

		for (int i = 0; i < enemiesToDestroy.Count; i++) {
			enemies.Remove (enemiesToDestroy[i]);
			Destroy (enemiesToDestroy [i].gameObject);
		}
		enemiesToDestroy.Clear ();
	}

	public void UpdateBoard(int horizontal, int vertical) {
		boardScript.AddToBoard (horizontal, vertical);
	}

	public void AddEnemyToList(Enemy script) {
		enemies.Add (script);
		SoundManager.instance.FormAudio (true);
	}

	public void RemoveEnemyFromList(Enemy script) {
		enemies.Remove (script);
		if (enemies.Count == 0) {
			SoundManager.instance.FormAudio (false);
		}
	}
		











	public void EnterDungeon() {
		dungeonScript.StartDungeon ();
		boardScript.SetDungeonBoard (dungeonScript.gridPositions, dungeonScript.maxBound, dungeonScript.endPos);
		playerScript.dungeonTransition = false;
		playerInDungeon = true;

		for (int i = 0; i < enemies.Count; i++) {
			Destroy (enemies [i].gameObject);
		}
		enemies.Clear ();
	}

	public void ExitDungeon() {
		boardScript.SetWorldBoard ();
		playerScript.dungeonTransition = false;
		playerInDungeon = false;
		enemies.Clear();

		SoundManager.instance.FormAudio (false);
	}
}