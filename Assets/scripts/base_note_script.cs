using UnityEngine;
using System.Collections;

public class base_note_script : MonoBehaviour {

	public GameObject note_wing_top;
	public GameObject note_wing_left;
	public GameObject note_wing_right;
	public GameObject note_wing_bottom;

	public GameObject base_note_1;
	public GameObject base_note_2;
	public GameObject base_note_3;
	public GameObject base_note_4;

	int current_base_note_sent = 0;
	public GameObject current_base_note = null;

	bool set_up_camp = false;

	// Use this for initialization
	void Start () {
	
	}

	IEnumerator RotateObj(GameObject target_object, Vector3 byAngles, float inTime) {
		var fromAngle = target_object.transform.rotation;
		var toAngle = Quaternion.Euler(target_object.transform.eulerAngles + byAngles);
		for(var t = 0f; t < 1; t += Time.deltaTime/inTime) {
			target_object.transform.rotation = Quaternion.Lerp(fromAngle, toAngle, t);
			yield return null;
		}
	}

	// Update is called once per frame
	void Update () {
		if (current_base_note) {
			if(current_base_note.transform.position.z > -25F) {
				Vector3 target_position = new Vector3(current_base_note.transform.position.x, current_base_note.transform.position.y, -25F);
				Vector3 new_position = Vector3.Lerp(current_base_note.transform.position, target_position, Time.deltaTime * 10);
				current_base_note.transform.position = new_position;
			}
			else {
				current_base_note = null;
			}
		}
	}

	public void send_next_note_flying() {
		current_base_note_sent++;

		if (current_base_note_sent == 1) {
			current_base_note = base_note_1;
		}
		if (current_base_note_sent == 2) {
			current_base_note = base_note_2;
		}
		if (current_base_note_sent == 3) {
			current_base_note = base_note_3;
		}
		if (current_base_note_sent == 4) {
			current_base_note = base_note_4;
		}
	}

	public void set_up_base_camp() {
		if (set_up_camp == false) {
			StartCoroutine(RotateObj(note_wing_top, Vector3.left * 90, 1));
			StartCoroutine(RotateObj(note_wing_left, Vector3.up * 90, 1));
			StartCoroutine(RotateObj(note_wing_right, Vector3.down * 90, 1));
			StartCoroutine(RotateObj(note_wing_bottom, Vector3.right * 90, 1));
			
			set_up_camp = true;
		}
	}
}
