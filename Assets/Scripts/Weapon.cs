using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {

	public bool inPlayerInventory = false;

	private Player player;
	private WeaponComponents[] weaponComps;
	private bool weaponUsed = false;

	public void AcquireWeapon() {
		player = GetComponentInParent<Player> ();
		weaponComps = GetComponentsInChildren<WeaponComponents> ();
	}

	void Update() {
		if (inPlayerInventory) {
			transform.position = player.transform.position;
			if (weaponUsed == true) {
				float degreeY = 0, degreeZ = -90f, degreeZMax = 275f;
				Vector3 returnVector = Vector3.zero;
				if (Player.isFacingRight) {
					degreeY = 0;
					returnVector = Vector3.zero;
				} else if (!Player.isFacingRight) {
					degreeY = 180;
					returnVector = new Vector3 (0, 180, 0);
				}
				transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.Euler (0, degreeY, degreeZ), Time.deltaTime * 20f);
				if (transform.eulerAngles.z <= degreeZMax) {
					transform.eulerAngles = returnVector;
					weaponUsed = false;
					EnableSpriteRenderer(false);
				}
			}
		}
	}

	public void UseWeapon() {
		EnableSpriteRenderer (true);
		weaponUsed = true;
	}

	public void EnableSpriteRenderer(bool isEnabled) {
		foreach (WeaponComponents comp in weaponComps) {
			comp.getSpriteRenderer ().enabled = isEnabled;
		}
	}

	public Sprite getComponentImage(int index) {
		return weaponComps[index].getSpriteRenderer().sprite;
	}
}
