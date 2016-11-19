using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuMan : MonoBehaviour {

	public bool displaying_menu = false;
	public GameObject menu_item;
	public GameObject menu_item_highlighter;
	public GameObject highlighted_menu_item;

	GameObject menu_item_highlighter_object;

	public class MenuItem {
		public string text { get; set; }
		public GameObject my_object { get; set; }
		public bool highlighted { get; set; }
	}

	public class StartMenu {
		public List<MenuItem> menu_items { get; set; }
	}

	List<MenuItem> start_menu = new List<MenuItem>();

	GameEntity gameentity;

	// Use this for initialization
	void Start () {
		gameentity = GetComponent<GameEntity>();

		menu_item_highlighter_object = (GameObject)Instantiate(menu_item_highlighter);

		start_menu.Add(new MenuItem { text = "New Game" });
		start_menu.Add(new MenuItem { text = "Random" });
		start_menu.Add(new MenuItem { text = "Create" });
		start_menu.Add(new MenuItem { text = "Credits" });
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (!displaying_menu) {
			return;
		}

		if (gameentity.player_pressed_action_once()) {
			int index = destroy_menu();

			MenuItem selected_item = start_menu[index];

			if (selected_item.text == "Random") {
				gameentity.start_random_game();
			}
			if (selected_item.text == "Create") {
				gameentity.start_editor();
			}

			displaying_menu = false;

			return;
		}

		if (highlighted_menu_item) {
			show_highlighted_menu_item();

			if (gameentity.player_pressed_down_once()) {
				highlight_next_menu_item(1);
			}
			if (gameentity.player_pressed_up_once()) {
				highlight_next_menu_item(-1);
			}
		}
	}

	public void display_menu() {

		int index = 0;

		float count_y = 0F;

		foreach (MenuItem item in start_menu) {

			item.my_object = (GameObject)Instantiate(
				menu_item, new Vector3(0, count_y, 1F), Quaternion.identity
			);

			menu_item_script menu_script = (menu_item_script)item.my_object.GetComponent(typeof(menu_item_script));

			menu_script.menu_text.text = item.text;

			if (index == 0) {
				highlighted_menu_item = item.my_object;
			}

			index++;
			count_y -= 1F;
		}
	}

	public int destroy_menu() {
		int highlighted_index = 0;
		int index = 0;

		foreach (MenuItem item in start_menu) {
			if (item.highlighted == true) {
				highlighted_index = index;
			}

			Destroy(item.my_object);
			index++;
		}

		highlighted_menu_item = null;

		menu_item_highlighter_object.transform.position = new Vector3(0, 0, -100F);

		return highlighted_index;
	}

	void highlight_next_menu_item(int increase) {
		int highlighted_index = 0;
		int index = 0;

		foreach (MenuItem item in start_menu) {

			if (item.highlighted == true) {
				highlighted_index = index;
				item.highlighted = false;
			}

			index++;
		}

		highlighted_index += increase;

		if (highlighted_index >= start_menu.Count) {
			highlighted_index = 0;
		}

		if (highlighted_index < 0) {
			highlighted_index = start_menu.Count - 1;
		}

		start_menu[ highlighted_index ].highlighted = true;
		highlighted_menu_item = start_menu[ highlighted_index ].my_object;
	}

	void show_highlighted_menu_item() {
		Vector3 target_position = new Vector3(
			highlighted_menu_item.transform.position.x,
			highlighted_menu_item.transform.position.y,
			highlighted_menu_item.transform.position.z - 0.3F
		);

		Vector3 new_position = Vector3.Lerp(menu_item_highlighter_object.transform.position, target_position, Time.deltaTime * 20);

		menu_item_highlighter_object.transform.position = new_position;
	}
}
