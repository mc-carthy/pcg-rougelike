using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class Wall : MonoBehaviour {
	public Sprite dmgSprite;					//Alternate sprite to display after Wall has been attacked by player.
	public int hp = 3;							//hit points for the wall.
	public GameObject[] foodTiles;

	private SpriteRenderer spriteRenderer;		//Store a component reference to the attached SpriteRenderer.

	void Awake ()
	{
		//Get a component reference to the SpriteRenderer.
		spriteRenderer = GetComponent<SpriteRenderer> ();
	}
	
	
	//DamageWall is called when the player attacks a wall.
	public void DamageWall (int loss) {
		
		spriteRenderer.sprite = dmgSprite;
		hp -= loss;
		
		//If hit points are less than or equal to zero:
		if (hp <= 0) {
			if (Random.Range (0, 5) == 1) {
				GameObject toInstantiate = foodTiles [Random.Range (0, foodTiles.Length)];
				GameObject instance = Instantiate (toInstantiate, new Vector3 (transform.position.x, transform.position.y, 0f), Quaternion.identity) as GameObject;
				instance.transform.SetParent (transform.parent);
			}
			gameObject.SetActive (false);
		}
	}
}
