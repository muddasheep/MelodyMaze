using UnityEngine;
using System.Collections;

public class maze_field_script : MonoBehaviour {
	public int coord_x { get; set; }
	public int coord_y { get; set; }
    public string note { get; set; }
    public bool is_base_note { get; set; }
    public bool is_target_note { get; set; }
    public GameObject linked_target_note { get; set; }

    public GameObject note_sprite;
    GameObject note_indicator;
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
		//Destroy( wall_bottom );
		wall_bottom_removed = true;
		removed_sides++;
	}
	public void remove_top() {
		//Destroy( wall_top );
		wall_top_removed = true;
		removed_sides++;
	}
	public void remove_right() {
		//Destroy( wall_right );
		wall_right_removed = true;
		removed_sides++;
	}
	public void remove_left() {
		//Destroy( wall_left );
		wall_left_removed = true;
		removed_sides++;
	}

	public void quake(float magnification, float delay) {
		StopAllCoroutines();

		// down
		Vector3 target_position1 = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z + 0.9F);
		smooth_move(gameObject.transform.position, target_position1, 0.5F, 0F + delay, gameObject);

		// up
		Vector3 target_position2 = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z - 0.3F);
		smooth_move(target_position1, target_position2, 0.4F, 0.5F + delay, gameObject);

		// down softer
		Vector3 target_position3 = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z + 0.3F);
		smooth_move(target_position2, target_position3, 0.3F, 0.9F + delay, gameObject);

		// up normal
		Vector3 target_position4 = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z);
		smooth_move(target_position3, target_position4, 0.2F, 1.2F + delay, gameObject);
	}

	void smooth_move (Vector3 startpos, Vector3 endpos, float seconds, float delay_seconds, GameObject moving_object) {
		StartCoroutine(smooth_move_iterator(startpos, endpos, seconds, delay_seconds, moving_object));
	}
	
	IEnumerator smooth_move_iterator (Vector3 startpos, Vector3 endpos, float seconds, float delay_seconds, GameObject moving_object) {
		float t = 0.0F;
		while (t <= 1.0F) {
			t += Time.deltaTime/delay_seconds;
			yield return null;
		}
		
		t = 0.0F;
		while (t <= 1.0F) {
			t += Time.deltaTime/seconds;
			moving_object.transform.position = Vector3.Lerp(startpos, endpos, Mathf.SmoothStep(0.0F, 1.0F, t));
			yield return null;
		}
	}

    public void save_note(string new_note) {
        note = new_note;

        if (new_note == null || new_note == "") {
            return;
        }

        if (note_indicator == null) {
            note_indicator = (GameObject)Instantiate(note_sprite, new Vector3(0, 0, 0), Quaternion.identity);
        }

        note_indicator.transform.parent = gameObject.transform;
        note_indicator.transform.localPosition = new Vector3(0, 0, -4.5F);
    }

    public void remove_note() {
        note = null;

        Destroy(note_indicator);

        note_indicator = null;
    }
}
