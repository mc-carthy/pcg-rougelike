using UnityEngine;
using System;
using System.Collections.Generic; 		//Allows us to use Lists.
using Random = UnityEngine.Random; 		//Tells Random to use the Unity Engine random number generator.

	
public class BoardManager : MonoBehaviour
{
	// Using Serializable allows us to embed a class with sub properties in the inspector.
	[Serializable]
	public class Count
	{
		public int minimum; 			//Minimum value for our Count class.
		public int maximum; 			//Maximum value for our Count class.
		
		
		//Assignment constructor.
		public Count (int min, int max)
		{
			minimum = min;
			maximum = max;
		}
	}

	public int columns = 5;
	public int rows = 5;
	public GameObject[] floorTiles;
	public GameObject[] wallTiles;
	public GameObject[] outerWallTiles;
	public GameObject chestTile;
	public GameObject exit;
	public GameObject enemy;
	private Transform boardHolder;
	private Dictionary<Vector2, Vector2> gridPositions = new Dictionary<Vector2, Vector2>();
	private Transform dungeonBoardHolder;
	private Dictionary<Vector2, Vector2> dungeonGridPositions;

	public bool checkValidTile(Vector2 pos) {
		if (gridPositions.ContainsKey (pos)) {
			return true;
		}
		return false;
	}

	public void BoardSetup() {
		boardHolder = new GameObject ("Board").transform;

		for (int x = 0; x < columns; x++) {
			for (int y = 0; y < rows; y++) {
				gridPositions.Add (new Vector2 (x, y), new Vector2 (x, y));

				GameObject toInstantiate = floorTiles [Random.Range (0, floorTiles.Length)];

				GameObject instance = Instantiate (toInstantiate, new Vector3 (x, y, 0f), Quaternion.identity) as GameObject;

				instance.transform.SetParent (boardHolder);
			}
		}
	}

	private void AddTiles(Vector2 tileToAdd) {
		if (!gridPositions.ContainsKey (tileToAdd)) {
			gridPositions.Add (tileToAdd, tileToAdd);
			GameObject toInstantiate = floorTiles [Random.Range (0, floorTiles.Length)];
			GameObject instance = Instantiate (toInstantiate, new Vector3 (tileToAdd.x, tileToAdd.y, 0f), Quaternion.identity) as GameObject;

			instance.transform.SetParent (boardHolder);

			// Choose at random a wall tile to lay
			if (Random.Range (0, 3) == 1) {
				toInstantiate = wallTiles [Random.Range (0, wallTiles.Length)];
				instance = Instantiate (toInstantiate, new Vector3 (tileToAdd.x, tileToAdd.y, 0f), Quaternion.identity) as GameObject;
				instance.transform.SetParent (boardHolder);
			} else if (Random.Range (0, 100) == 1) {
				toInstantiate = exit;
				instance = Instantiate (toInstantiate, new Vector3 (tileToAdd.x, tileToAdd.y, 0f), Quaternion.identity) as GameObject;
				instance.transform.SetParent (boardHolder);
			} else if (Random.Range(0, 20) == GameManager.instance.enemySpawnRatio) {
				toInstantiate = enemy;
				instance = Instantiate (toInstantiate, new Vector3 (tileToAdd.x, tileToAdd.y, 0f), Quaternion.identity) as GameObject;
				//instance.transform.SetParent (boardHolder);
			}

		}
	}

	public void AddToBoard(int horizontal, int vertical) {
		if (horizontal == 1) {
			// Check if tiles exist
			int x = (int)Player.position.x;
			int sightX = x + 2;
			for (x += 1; x <= sightX; x++) {
				int y = (int)Player.position.y;
				int sightY = y + 1;
				for (y -= 1; y <= sightY; y++) {
					AddTiles (new Vector2 (x, y));
				}
			}
		} else if (horizontal == -1) {
			int x = (int)Player.position.x;
			int sightX = x - 2;
			for (x -= 1; x >= sightX; x--) {
				int y = (int)Player.position.y;
				int sightY = y + 1;
				for (y -= 1; y <= sightY; y++) {
					AddTiles (new Vector2 (x, y));
				}
			}
		} else if (vertical == 1) {
			int y = (int)Player.position.y;
			int sightY = y + 2;
			for (y += 1; y <= sightY; y++) {
				int x = (int)Player.position.x;
				int sightX = x + 1;
				for (x -= 1; x <= sightX; x++) {
					AddTiles (new Vector2 (x, y));
				}
			}
		} else if (vertical == -1) {
			int y = (int)Player.position.y;
			int sightY = y - 2;
			for (y -= 1; y >= sightY; y--) {
				int x = (int)Player.position.x;
				int sightX = x + 1;
				for (x -= 1; x <= sightX; x++) {
					AddTiles (new Vector2 (x, y));
				}
			}
		}
	}

	public void SetDungeonBoard (Dictionary<Vector2, TileType> dungeonTiles, int bound, Vector2 endPos) {
		boardHolder.gameObject.SetActive (false);
		dungeonBoardHolder = new GameObject ("Dungeon").transform;
		GameObject toInstantiate, instance;

		foreach (KeyValuePair<Vector2, TileType> tile in dungeonTiles) {
			toInstantiate = floorTiles [Random.Range (0, floorTiles.Length)];
			instance = Instantiate (toInstantiate, new Vector3 (tile.Key.x, tile.Key.y, 0f), Quaternion.identity) as GameObject;
			instance.transform.SetParent (dungeonBoardHolder);

			if (tile.Value == TileType.chest) {
				toInstantiate = chestTile;
				instance = Instantiate (toInstantiate, new Vector3 (tile.Key.x, tile.Key.y, 0f), Quaternion.identity) as GameObject;
				//instance.transform.SetParent (dungeonBoardHolder);
			} else if (tile.Value == TileType.enemy) {
				toInstantiate = enemy;
				instance = Instantiate (toInstantiate, new Vector3 (tile.Key.x, tile.Key.y, 0f), Quaternion.identity) as GameObject;
				//instance.transform.SetParent (dungeonBoardHolder);
			}
		}

		for (int x = -1; x < bound + 1; x++) {
			for (int y = -1; y < bound + 1; y++) {
				if (!dungeonTiles.ContainsKey (new Vector2 (x, y))) {
					toInstantiate = outerWallTiles [Random.Range (0, outerWallTiles.Length)];
					instance = Instantiate (toInstantiate, new Vector3 (x, y, 0f), Quaternion.identity) as GameObject;
					instance.transform.SetParent (dungeonBoardHolder);
				}
			}
		}
		toInstantiate = exit;
		instance = Instantiate (toInstantiate, new Vector3 (endPos.x, endPos.y, 0f), Quaternion.identity) as GameObject;
		instance.transform.SetParent (dungeonBoardHolder);
	}

	public void SetWorldBoard() {
		Destroy (dungeonBoardHolder.gameObject);
		boardHolder.gameObject.SetActive (true);
	}
}
