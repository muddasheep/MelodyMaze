using UnityEngine;
using System.Collections;

public class maze_field_script : MonoBehaviour {
	public int coord_x { get; set; }
	public int coord_y { get; set; }

	public GameObject maze_wall;
	GameObject wall_top;
	GameObject wall_left;
	GameObject wall_bottom;
	GameObject wall_right;
	bool wall_top_removed = false;
	bool wall_left_removed = false;
	bool wall_bottom_removed = false;
	bool wall_right_removed = false;

	int removed_sides = 0;

	// Use this for initialization
	void Awake () {
		//Debug.Log("ADDING WALLS *_*");
		wall_top = (GameObject)Instantiate(maze_wall, new Vector3(4.4F, -0.5F, -4.4F), Quaternion.identity);
		wall_top.transform.parent = gameObject.transform;
		wall_top.transform.localRotation = Quaternion.Euler (0, 0, -90F);
		wall_top.transform.localPosition = new Vector3(0F, 0.5F, -4.4F);
		wall_bottom = (GameObject)Instantiate(maze_wall, new Vector3(4.4F, 0.5F, -4.4F), Quaternion.identity);
		wall_bottom.transform.parent = gameObject.transform;
		wall_bottom.transform.localRotation = Quaternion.Euler (0, 0, -90F);
		wall_bottom.transform.localPosition = new Vector3(0F, -0.5F, -4.4F);
		wall_left = (GameObject)Instantiate(maze_wall, new Vector3(-0.5F, 0F, -4.4F), Quaternion.identity);
		wall_left.transform.parent = gameObject.transform;
		wall_left.transform.localPosition = new Vector3(-0.5F, 0F, -4.4F);
		wall_right = (GameObject)Instantiate(maze_wall, new Vector3(0.5F, 0F, -4.4F), Quaternion.identity);
		wall_right.transform.parent = gameObject.transform;
		wall_right.transform.localPosition = new Vector3(0.5F, 0F, -4.4F);
	}
	
	// Update is called once per frame
	void Update () {
	}

	public bool can_remove_sides() {
		if (removed_sides < 2) {
			return true;
		}

		return false;
	}

	public bool removed_bottom() {
		return wall_bottom_removed;
	}
	public bool removed_top() {
		return wall_top_removed;
	}
	public bool removed_left() {
		return wall_left_removed;
	}
	public bool removed_right() {
		return wall_right_removed;
	}

	public void remove_bottom() {
		Destroy( wall_bottom );
//		wall_bottom.transform.localPosition = new Vector3(wall_bottom.transform.localPosition.x, wall_bottom.transform.localPosition.y, 14.4F);
		wall_bottom_removed = true;
		removed_sides++;
	}
	public void remove_top() {
		Destroy( wall_top );
//		wall_top.transform.localPosition = new Vector3(wall_top.transform.localPosition.x, wall_top.transform.localPosition.y, 14.4F);
		wall_top_removed = true;
		removed_sides++;
	}
	public void remove_right() {
		Destroy( wall_right );
//		wall_right.transform.localPosition = new Vector3(wall_right.transform.localPosition.x, wall_right.transform.localPosition.y, 14.4F);
		wall_right_removed = true;
		removed_sides++;
	}
	public void remove_left() {
		Destroy( wall_left );
//		wall_left.transform.localPosition = new Vector3(wall_left.transform.localPosition.x, wall_left.transform.localPosition.y, 14.4F);
		wall_left_removed = true;
		removed_sides++;
	}
}
