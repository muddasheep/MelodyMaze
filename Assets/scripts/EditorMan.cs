using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EditorMan : MonoBehaviour {

	GameEntity gameentity;
	MazeMan mazeman;
	MenuMan menuman;

	int pos_x;
	int pos_y;
	int previous_pos_x;
	int previous_pos_y;

	GameObject editing_maze_field;
	maze_field_script editing_maze_field_script;

	public GameObject piano_key_white_prototype;
	public GameObject piano_key_black_prototype;

	public class Coords {
		public float wall_coord_x { get; set; }
		public float wall_coord_y { get; set; }
		public int field_coord_x { get; set; }
		public int field_coord_y { get; set; }
	}

	// Use this for initialization
	void Start () {
		gameentity = GetComponent<GameEntity>();
		mazeman    = GetComponent<MazeMan>();
		menuman    = GetComponent<MenuMan>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		gameentity.detectPressedKeyOrButton();

		gameentity.center_camera_within_maze_bounds();
	}
	
	public void prepare_editor() {
		gameentity.spawn_player_sphere(0);
	}

	public void editor_movement() {
		if (!gameentity.player_sphere) {
			return;
		}

		if (gameentity.player_pressed_action_once()) {
			editor_action();
			return;
		}

		if (gameentity.player_pressed_action2_once()) {
			editor_cancel();
			return;
		}

		if (gameentity.player_pressed_action3_once()) {
			editor_settings();
			return;
		}

		if (editing_maze_field != null) {
			move_settings_cursor();
			return;
		}

		float pos_z = -4.4F;

		if (gameentity.player_pressed_up_once()) {
			pos_y++;
		}
		if (gameentity.player_pressed_down_once()) {
			pos_y--;
		}
		if (gameentity.player_pressed_right_once()) {
			pos_x++;
		}
		if (gameentity.player_pressed_left_once()) {
			pos_x--;
		}

		check_paint_mode();

		Vector3 new_position = new Vector3(pos_x, pos_y, pos_z);

		gameentity.player_sphere.transform.position = new_position;

		previous_pos_x = pos_x;
		previous_pos_y = pos_y;
	}

	public int init_settings_x_counter;
	int settings_pos_x = 0;
	int settings_pos_y = 0;

	void move_settings_cursor() {
		if (gameentity.player_pressed_up_once()) {
			settings_pos_y++;
		}
		if (gameentity.player_pressed_down_once()) {
			settings_pos_y--;
		}
		if (gameentity.player_pressed_right_once()) {
			settings_pos_x++;
		}
		if (gameentity.player_pressed_left_once()) {
			settings_pos_x--;
		}

		if (settings_pos_x >= current_piano.notes.Count) {
			settings_pos_x = 0;
		}
		if (settings_pos_x < 0) {
			settings_pos_x = current_piano.notes.Count - 1;
		}

		menuman.highlighted_menu_item = current_piano.notes[settings_pos_x].piano_key;
		menuman.show_highlighted_menu_item();
	}

	void check_paint_mode() {
		// paint mode! if player kept pressing action, destroy wall between last and this one and call editor_action()
		if (gameentity.player_action_button_down && (pos_x != previous_pos_x || pos_y != previous_pos_y)) {
			editor_action();
			if (pos_x < previous_pos_x) {
				mazeman.destroy_wall_at_coordinates(pos_x + 0.5F, pos_y);
			}
			if (pos_x > previous_pos_x) {
				mazeman.destroy_wall_at_coordinates(pos_x - 0.5F, pos_y);
			}
			if (pos_y < previous_pos_y) {
				mazeman.destroy_wall_at_coordinates(pos_x, pos_y + 0.5F);
			}
			if (pos_y > previous_pos_y) {
				mazeman.destroy_wall_at_coordinates(pos_x, pos_y - 0.5F);
			}
		}
	}

	void editor_action() {
		// choose currently selected settings
		if (editing_maze_field != null) {
			choose_field_settings();
			return;
		}
		
		// if no field at current pos, create field
		if (!mazeman.field_at_coordinates_exists(pos_x, pos_y)) {
			mazeman.find_or_create_field_at_coordinates(pos_x, pos_y);

			// add 4 walls
			foreach (Coords coords in find_coordinates_around_pos(pos_x, pos_y)) {
				create_editor_wall_at_coordinates(coords.wall_coord_x, coords.wall_coord_y);
			}
		}
	}

	void editor_cancel() {
		// hide settings if settings are active
		if (editing_maze_field != null) {
			hide_field_settings();
			return;
		}

		// if field at current pos, destroy field
		if (mazeman.field_at_coordinates_exists(pos_x, pos_y)) {
			mazeman.destroy_field_at_coordinates(pos_x, pos_y);
			editing_maze_field = null;
			editing_maze_field_script = null;

			// add walls if adjecent fields exist, remove walls if fields don't exist
			foreach (Coords coords in find_coordinates_around_pos(pos_x, pos_y)) {

				if(mazeman.field_at_coordinates_exists(coords.field_coord_x, coords.field_coord_y)) {
					create_editor_wall_at_coordinates(coords.wall_coord_x, coords.wall_coord_y);
				}
				else {
					mazeman.destroy_wall_at_coordinates(coords.wall_coord_x, coords.wall_coord_y);
				}
			}
		}
	}

	void editor_settings() {
		// if we're already editing, ignore this
		if (editing_maze_field != null) {
			return;
		}

		// if field at current pos, show settings
		if (mazeman.field_at_coordinates_exists(pos_x, pos_y)) {

			show_field_settings(pos_x, pos_y);
		}
	}

	public class Piano {
		public List<PianoNote> notes = new List<PianoNote>();
	}

	public class PianoNote {
		public string note { get; set; }
		public GameObject piano_key { get; set; }
		public bool active { get; set; }
	}

	public List<string> notes = new List<string> {
		"c", "cis", "d", "dis", "e", "f", "fis", "g", "gis", "a", "b", "h",
		"c2", "cis2", "d2", "dis2", "e2", "f2", "fis2", "g2", "gis2", "a2", "b2", "h2",
		"c3"
	};

	public List<string> instruments = new List<string> {
		"piano"
	};

	Piano current_piano;

	void show_field_settings(float x, float y) {
		hide_field_settings();

		editing_maze_field = mazeman.find_or_create_field_at_coordinates(Mathf.RoundToInt(x), Mathf.RoundToInt(y));
		editing_maze_field_script = gameentity.get_maze_field_script(Mathf.RoundToInt(x), Mathf.RoundToInt(y));
		
		// build piano
		current_piano = new Piano();

		float piano_white_x = x - 5F;
		float piano_white_y = y;
		float piano_white_z = -5F;
		float piano_pos_x = piano_white_x;
		float piano_pos_y = piano_white_y;
		float piano_pos_z = piano_white_z;
		init_settings_x_counter = 0;

		foreach (string note in notes) {
			GameObject key_prototype = piano_key_white_prototype;

			bool white = true;

			if (note.Contains("b") || note.Contains("is")) {
				key_prototype = piano_key_black_prototype;
				white = false;
			}
			else {
				piano_white_x += 0.6F;
			}

			float delay = init_settings_x_counter*0.01F;

			if (white) {
				piano_pos_x = piano_white_x;
				piano_pos_y = piano_white_y;
				piano_pos_z = piano_white_z;
			}
			else {
				piano_pos_x = piano_pos_x   + 0.30F;
				piano_pos_y = piano_white_y + 0.74F;
				piano_pos_z = piano_white_z - 0.1F;
				delay += 0.1F;
			}

			if (note == editing_maze_field_script.note) {
				settings_pos_x = init_settings_x_counter;
			}

			GameObject new_piano_key = (GameObject)Instantiate(key_prototype,
				new Vector3(piano_pos_x, piano_pos_y + 8F, piano_pos_z - 15F), Quaternion.identity
			);

			current_piano.notes.Add(new PianoNote {
				note 	  = note,
				piano_key = new_piano_key,
				active    = false
			});

			gameentity.smooth_move(new_piano_key.transform.position,
				new Vector3(piano_pos_x, piano_pos_y, piano_pos_z), 0.3F, delay, new_piano_key
			);

			init_settings_x_counter++;
		}
	}

	void choose_field_settings() {

		editing_maze_field_script.note = current_piano.notes[settings_pos_x].note;
		hide_field_settings();
	}

	void hide_field_settings() {
		// destroy piano

		if (current_piano == null) {
			return;
		}

		foreach(PianoNote note in current_piano.notes) {
			Destroy (note.piano_key);
		}

		current_piano = null;
		editing_maze_field = null;
		editing_maze_field_script = null;

		menuman.remove_menu_highlighter();
	}

	public List<Coords> find_coordinates_around_pos(float x, float y) {
		List<Coords> coordinates = new List<Coords>();

		coordinates.Add(new Coords {
			wall_coord_x  = x + 0.5F, wall_coord_y  = y,
			field_coord_x = Mathf.RoundToInt(x + 1F), field_coord_y = Mathf.RoundToInt(y)
		});

		coordinates.Add(new Coords {
			wall_coord_x  = x - 0.5F, wall_coord_y  = y,
			field_coord_x = Mathf.RoundToInt(x - 1F), field_coord_y = Mathf.RoundToInt(y)
		});

		coordinates.Add(new Coords {
			wall_coord_x  = x, wall_coord_y  = y + 0.5F,
			field_coord_x = Mathf.RoundToInt(x), field_coord_y = Mathf.RoundToInt(y + 1F)
		});

		coordinates.Add(new Coords {
			wall_coord_x  = x, wall_coord_y  = y - 0.5F,
			field_coord_x = Mathf.RoundToInt(x), field_coord_y = Mathf.RoundToInt(y - 1F)
		});

		return coordinates;
	}

	void create_editor_wall_at_coordinates(float x, float y) {
		GameObject new_wall;
		maze_wall_script new_wall_script;

		new_wall = mazeman.find_or_create_wall_at_coordinates(x, y);
		new_wall_script = gameentity.get_maze_wall_script_from_game_object(new_wall);
		new_wall_script.FadeIn();
	}
}
