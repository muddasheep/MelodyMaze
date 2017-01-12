using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class MenuMan : MonoBehaviour {

	public bool displaying_menu = false;
	public GameObject menu_item;
	public GameObject menu_item_highlighter;
    public GameObject title_text_beholder;
    public GameObject highlighted_menu_item;

	GameObject menu_item_highlighter_object;

	public class MenuItem {
		public string text { get; set; }
        public string value { get; set; }
        public GameObject my_object { get; set; }
		public bool highlighted { get; set; }
	}

    List<MenuItem> start_menu = new List<MenuItem>();
    List<MenuItem> pause_menu = new List<MenuItem>();
    List<MenuItem> load_menu  = new List<MenuItem>();
    List<MenuItem> save_menu = new List<MenuItem>();
    List<MenuItem> current_menu;

    GameEntity gameentity;
    EditorMan editorman;

    // Use this for initialization
    void Start () {
		gameentity = GetComponent<GameEntity>();
        editorman = GetComponent<EditorMan>();

		menu_item_highlighter_object = (GameObject)Instantiate(menu_item_highlighter);

		start_menu.Add(new MenuItem { text = "New Game" });
		start_menu.Add(new MenuItem { text = "Random" });
		start_menu.Add(new MenuItem { text = "Create" });
		start_menu.Add(new MenuItem { text = "Credits" });
        start_menu.Add(new MenuItem { text = "Exit" });

        pause_menu.Add(new MenuItem { text = "Save" });
        pause_menu.Add(new MenuItem { text = "Load" });
        pause_menu.Add(new MenuItem { text = "Return to Title" });
        pause_menu.Add(new MenuItem { text = "Exit" });

    }

    // Update is called once per frame
    void FixedUpdate () {
		if (!displaying_menu) {
			return;
		}

		if (gameentity.player_pressed_action_once()) {
			int index = destroy_menu();

			MenuItem selected_item = current_menu[index];

			if (selected_item.text == "Random") {
				gameentity.start_random_game();
			}
			if (selected_item.text == "Create") {
				gameentity.start_editor();
			}

            if (selected_item.text == "Save") {
                display_menu("save");
                return;
            }
            if (selected_item.text.IndexOf(" + ") > -1) {
                editorman.increase_current_level();
                display_menu("save");
                return;
            }
            if (selected_item.text.IndexOf(" - ") > -1) {
                editorman.decrease_current_level();
                display_menu("save", 2);
                return;
            }
            if (selected_item.text.IndexOf("Level #") > -1) {
                // do nothing
                return;
            }
            if (selected_item.text == "Save Level") {
                editorman.save_level();
            }

            if (selected_item.text == "Load") {
                display_menu("load");
                return;
            }
            if (selected_item.text.IndexOf("Level ") > -1 && selected_item.text.IndexOf("Save Level") == -1) {
                editorman.load_level(int.Parse(selected_item.value));
            }
            if (selected_item.text == "Exit") {
                if (!Application.isEditor) {
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                }
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

	public void display_menu(string menu, int highlight_index = 0) {

        if (menu == "start") {
            current_menu = start_menu;
        }
        if (menu == "pause") {
            current_menu = pause_menu;
        }
        if (menu == "save") {
            update_save_menu();
            current_menu = save_menu;
        }
        if (menu == "load") {
            update_load_menu();
            current_menu = load_menu;
        }

        int index = 0;

		float count_y = gameentity.maze_cam.transform.position.y;

		foreach (MenuItem item in current_menu) {

			item.my_object = (GameObject)Instantiate(
				menu_item, new Vector3(gameentity.maze_cam.transform.position.x, count_y, -6F), Quaternion.identity
			);

            item.highlighted = false;

			menu_item_script menu_script = (menu_item_script)item.my_object.GetComponent(typeof(menu_item_script));

			menu_script.menu_text.text = item.text;

			if (index == highlight_index) {
				highlighted_menu_item = item.my_object;
                item.highlighted = true;
			}

			index++;
			count_y -= 1F;
		}
	}

    void update_save_menu() {
        int active_level = editorman.current_level;

        save_menu = new List<MenuItem>();

        save_menu.Add(new MenuItem { text = " + " });
        save_menu.Add(new MenuItem { text = "Level #: " + active_level });
        save_menu.Add(new MenuItem { text = " - " });
        save_menu.Add(new MenuItem { text = "Save Level" });
    }

    void update_load_menu() {
        int level_count = 1;

        load_menu = new List<MenuItem>();

        while (level_count <= 99) {
            if (editorman.level_file_exists(level_count)) {
                load_menu.Add(new MenuItem { text = "Level " + level_count.ToString(), value = level_count.ToString() });
            }
            level_count++;
        }
    }

    public int destroy_menu() {
		int highlighted_index = 0;
		int index = 0;

		foreach (MenuItem item in current_menu) {
			if (item.highlighted == true) {
				highlighted_index = index;
			}

			Destroy(item.my_object);
			index++;
		}

		highlighted_menu_item = null;

		remove_menu_highlighter();

		return highlighted_index;
	}

	void highlight_next_menu_item(int increase) {
		int highlighted_index = 0;
		int index = 0;

		foreach (MenuItem item in current_menu) {

			if (item.highlighted == true) {
				highlighted_index = index;
				item.highlighted = false;
			}

			index++;
		}

		highlighted_index += increase;

		if (highlighted_index >= current_menu.Count) {
			highlighted_index = 0;
		}

		if (highlighted_index < 0) {
			highlighted_index = current_menu.Count - 1;
		}

        current_menu[ highlighted_index ].highlighted = true;
		highlighted_menu_item = current_menu[ highlighted_index ].my_object;
	}

	public void remove_menu_highlighter() {
		menu_item_highlighter_object.transform.position = new Vector3(0, 0, -100F);
	}

	public void show_highlighted_menu_item() {
		Vector3 target_position = new Vector3(
			highlighted_menu_item.transform.position.x,
			highlighted_menu_item.transform.position.y,
			highlighted_menu_item.transform.position.z - 0.3F
		);

        menu_item_highlighter_object.transform.localScale = new Vector3(
            highlighted_menu_item.transform.localScale.x,
            highlighted_menu_item.transform.localScale.y,
            menu_item_highlighter_object.transform.localScale.z
        );

        Vector3 new_position = Vector3.Lerp(menu_item_highlighter_object.transform.position, target_position, Time.deltaTime * 20);

		menu_item_highlighter_object.transform.position = new_position;
	}

    public GameObject create_text_at_coordinates(float x, float y, float z, string text, GameObject parent) {
        GameObject summoned_title_text = (GameObject)Instantiate(title_text_beholder, new Vector3(x, y, z), Quaternion.identity);

        TextMesh summoned_title_text_mesh = summoned_title_text.GetComponentInChildren<TextMesh>();
        summoned_title_text_mesh.text = text;

        summoned_title_text.transform.parent = parent.transform;

        return summoned_title_text;
    }
}
