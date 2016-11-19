using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameEntity : MonoBehaviour {
	public float start_time = 0;
	public float current_time = 0;

	public GameObject player_sphere_prefab;
	public GameObject base_note;
	public Camera maze_cam;

	bool base_note_reached_center = false;
	public GameObject base_note_ingame;
	public GameObject player_sphere;
	int collected_notes = 0;
	int player_spheres = 0;
	public int player_coord_x = 0;
	public int player_coord_y = 0;
	Vector3 player_target_position;
	bool player_sphere_moving = false;
	bool player_can_move = true;
	float base_note_speed_x = 0F;
	float base_note_speed_y = 0F;

	bool game_running = false;
	bool editor_running = false;

	public class TakenPath {
		public int coord_x { get; set; }
		public int coord_y { get; set; }
		public int taken_direction { get; set; }
	}

	public class TakenPaths {
		public List<TakenPath> taken_path  { get; set; }
		public int redrawn_counter = 0;
	}

	List<TakenPath> player_saved_path_coordinates = new List<TakenPath>();
	List<TakenPaths> player_saved_paths = new List<TakenPaths>();

	MazeMan mazeman;
	MenuMan menuman;
	EditorMan editorman;

	// Use this for initialization
	void Start () {

		mazeman   = GetComponent<MazeMan>();
		menuman   = GetComponent<MenuMan>();
		editorman = GetComponent<EditorMan>();

		start_time = Time.time;
	}

	// Update is called once per frame
	void FixedUpdate () {
		current_time = Time.time - start_time;

		if (game_running) {

			if (mazeman.maze_deconstruction == true) {
				deconstruct_maze();
			}
			else {

				// move player
				if (player_spheres == 0 && base_note_reached_center == true && player_pressed_action_once()) {
					base_note_script next_base_note_script = get_base_note_script_from_game_object(base_note_ingame);
					next_base_note_script.send_next_note_flying();

					spawn_player_sphere(0.3F);
					player_can_move = true;
				}
				else {
					check_base_note_reach_center();
					player_movement();
				}
			}
		}
		else if (editor_running) {

			editorman.editor_movement();
		}
		else {
			if (menuman.displaying_menu == false) {
				menuman.display_menu();
				menuman.displaying_menu = true;
			}
		}
	}

	public void start_random_game() {
		mazeman.build_maze();
		game_running = true;
	}
	
	public void start_editor() {
		editorman.prepare_editor();
		editor_running = true;
	}
	
	void check_base_note_reach_center() {
		if (!player_sphere) {
			return;
		}

		if (player_pressed_action_once () &&
			player_coord_x == 0 && player_coord_y == 0 &&
		    player_sphere_moving == false && base_note_reached_center == false) {

			base_note_reached_center = true;
			mazeman.quake_from_current_position(0.8F);
			base_note_script next_base_note_script = get_base_note_script_from_game_object(base_note_ingame);
			next_base_note_script.set_up_base_camp();

			smooth_move(player_sphere.transform.position, new Vector3(0F, 0F, -4.7F), 0.5F, 0F, player_sphere);
		}
	}

	public void adjust_camera() {

		if (!player_sphere) {
			return;
		}
		maze_cam.transform.position = new Vector3(player_sphere.transform.position.x, player_sphere.transform.position.y, -20.8F);
	}

	public int maze_path_redraw_rate_counter = 0;
	public GameObject redrawing_field;
	void deconstruct_maze() {
		if (player_saved_paths.Count > 0) {
			if (maze_path_redraw_rate_counter < 2) {
				maze_path_redraw_rate_counter++;
			}
			else {
				// redraw paths
				List<TakenPaths> player_saved_paths_altered = new List<TakenPaths>();
				
				foreach (TakenPaths path in player_saved_paths) {
					if (path.redrawn_counter < path.taken_path.Count) {
						TakenPath current_path = path.taken_path[ path.redrawn_counter ];
						
						GameObject redrawing_maze_field = mazeman.find_or_create_field_at_coordinates(current_path.coord_x, current_path.coord_y);
						
						GameObject redrawn_field = (GameObject)Instantiate(redrawing_field, new Vector3(redrawing_maze_field.transform.position.x, redrawing_maze_field.transform.position.y, redrawing_maze_field.transform.position.z - 1F), Quaternion.identity);
						redrawn_field.transform.parent = redrawing_maze_field.transform;
						
						path.redrawn_counter++;
						player_saved_paths_altered.Add(path);
					}
				}
				player_saved_paths = player_saved_paths_altered;
				maze_path_redraw_rate_counter = 0;
			}
		}
		else {
			if (maze_path_redraw_rate_counter < 4) {
				maze_path_redraw_rate_counter++;
			}
			else {

				maze_path_redraw_rate_counter = 0;
			}
			
			mazeman.animate_maze_destruction();
		}
	}

	public Vector3 get_random_offscreen_position(float target_z) {
		float target_x = Random.Range(-16F, 16F);
		float target_y = 7F;
		
		if (Random.Range(0, 50F) > 25F) {
			target_y = -7F;
		}
		
		if (target_x > 11F || target_x < -11F) {
			target_y = Random.Range(-7F, 7F);
		}

		return new Vector3(target_x, target_y, target_z);
	}

	public void smooth_move (Vector3 startpos, Vector3 endpos, float seconds, float delay_seconds, GameObject moving_object) {
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

			if (moving_object == base_note_ingame) {
				adjust_camera();
			}

			yield return null;
		}
	}

	bool check_note_collection() {
		if (base_note_reached_center == false) {
			return false;
		}

		maze_field_script current_maze_field = get_maze_field_script(player_coord_x, player_coord_y);
		int check_coord_x = current_maze_field.coord_x;
		int check_coord_y = current_maze_field.coord_y;

		foreach (GameObject note in mazeman.maze_notes) {
			maze_note_script note_script = get_maze_note_script_from_game_object(note);

			if (note_script.coord_x == check_coord_x && note_script.coord_y == check_coord_y && note_script.collected == false) {
				note_script.collected = true;
				collected_notes++;

				player_sphere.transform.parent = current_maze_field.gameObject.transform;

				if (collected_notes < mazeman.maze_notes.Count) {
					// let another player sphere spawn
					player_can_move = false;

					finish_note_collection();
				}
				else {
					mazeman.maze_deconstruction = true;
				}

				// save currently recorded path
				player_saved_paths.Add (new TakenPaths {
					taken_path = new List<TakenPath>(player_saved_path_coordinates)
				});

				// reset player array path
				player_saved_path_coordinates.Clear();

				return true;
			}
		}

		return false;
	}

	IEnumerator finish_note_collection_delay(float delay_seconds) {
		float t = 0.0F;
		while (t <= 1.0F) {
			t += Time.deltaTime/delay_seconds;
			yield return null;
		}

		player_spheres--;
		player_sphere = base_note_ingame;
		adjust_camera();
		player_sphere = null;
	}

	void finish_note_collection() {
		StartCoroutine(finish_note_collection_delay(1F));
	}

	public maze_note_script get_maze_note_script_from_game_object(GameObject maze_note_object) {
		maze_note_script next_note_script = (maze_note_script)maze_note_object.GetComponent(typeof(maze_note_script));
		return next_note_script;
	}

	public maze_field_script get_maze_field_script_from_game_object(GameObject maze_field_object) {
		maze_field_script next_maze_script = (maze_field_script)maze_field_object.GetComponent(typeof(maze_field_script));
		return next_maze_script;
	}

	public maze_wall_script get_maze_wall_script_from_game_object(GameObject maze_wall_object) {
		maze_wall_script next_wall_script = (maze_wall_script)maze_wall_object.GetComponent(typeof(maze_wall_script));
		return next_wall_script;
	}

	public base_note_script get_base_note_script_from_game_object(GameObject base_note_object) {
		base_note_script next_base_script = (base_note_script)base_note_object.GetComponent(typeof(base_note_script));
		return next_base_script;
	}

	public maze_field_script get_maze_field_script(int coord_x, int coord_y) {
		GameObject next_maze_field = mazeman.find_or_create_field_at_coordinates(coord_x, coord_y);
		maze_field_script next_maze_script = get_maze_field_script_from_game_object(next_maze_field);

		return next_maze_script;
	}

	void player_movement() {
		if (!player_sphere) {
			return;
		}

		if (!player_can_move) {
			return;
		}

		if (base_note_reached_center == false) {
			move_base_note();
		}
		else {
			move_current_note();
		}
	}

	void move_base_note() {

		if (base_note_reached_center) {
			return;
		}

		// accelerate and decelerate
		float player_z = player_sphere.transform.position.z;
		float player_y = player_sphere.transform.position.y;
		float player_x = player_sphere.transform.position.x;

		float speed_limit = 0.05F;

		bool pressed_x = false;
		bool pressed_y = false;

		if (player_pressed_up()) {
			base_note_speed_y += 0.001F;
			pressed_y = true;
		}
		if (player_pressed_down()) {
			base_note_speed_y -= 0.001F;
			pressed_y = true;
		}
		if (player_pressed_left()) {
			base_note_speed_x -= 0.001F;
			pressed_x = true;
		}
		if (player_pressed_right()) {
			base_note_speed_x += 0.001F;
			pressed_x = true;
		}

		if (pressed_x == false) {
			if (base_note_speed_x < 0) {
				base_note_speed_x += 0.001F;
			}
			if (base_note_speed_x > 0) {
				base_note_speed_x -= 0.001F;
			}
			if (Mathf.Abs(base_note_speed_x) < 0.001F) {
				base_note_speed_x = 0;
			}
		}
		if (pressed_y == false) {
			if (base_note_speed_y < 0) {
				base_note_speed_y += 0.001F;
			}
			if (base_note_speed_y > 0) {
				base_note_speed_y -= 0.001F;
			}
			if (Mathf.Abs(base_note_speed_y) < 0.001F) {
				base_note_speed_y = 0;
			}
		}

		if (base_note_speed_x > speed_limit) {
			base_note_speed_x = speed_limit;
		}
		if (base_note_speed_x < 0 - speed_limit) {
			base_note_speed_x = 0 - speed_limit;
		}
		if (base_note_speed_y > speed_limit) {
			base_note_speed_y = speed_limit;
		}
		if (base_note_speed_y < 0 - speed_limit) {
			base_note_speed_y = 0 - speed_limit;
		}

		player_x += base_note_speed_x;
		player_y += base_note_speed_y;
		player_z = 0 - 5.0F - (0.5F * Mathf.Sin(current_time));

		float correction_player_x = check_base_note_horizontal_limit(player_x);
		float correction_player_y = check_base_note_vertical_limit(player_y);

		if (correction_player_x != player_x) {
			base_note_speed_x = 0;
			player_x = correction_player_x;
		}

		if (correction_player_y != player_y) {
			base_note_speed_y = 0;
			player_y = correction_player_y;
		}

		player_sphere.transform.position = new Vector3(player_x, player_y, player_z);

		// highlight field walls based on rounded coords
		player_coord_x = Mathf.RoundToInt(player_x);
		player_coord_y = Mathf.RoundToInt(player_y);

		adjust_camera();
	}

	float check_base_note_horizontal_limit(float x) {

		if (x < mazeman.maze_border_left) {
			return mazeman.maze_border_left;
		}
		
		if (x > mazeman.maze_border_right) {
			return mazeman.maze_border_right;
		}

		return x;
	}

	float check_base_note_vertical_limit(float y) {

		if (y > mazeman.maze_border_top) {
			return mazeman.maze_border_top;
		}
		
		if (y < mazeman.maze_border_bottom) {
			return mazeman.maze_border_bottom;
		}

		return y;
	}

	void move_current_note() {

		if (base_note_ingame == player_sphere) {
			return;
		}
		
		bool allow_movement = true;
		bool note_collected = false;
		
		if (player_sphere_moving == true) {
			bool direction_pressed = false;
			if (player_sphere.transform.position.z >= -4.3F) {
				if (player_pressed_right() || player_pressed_left() || player_pressed_down() || player_pressed_up()) {
					direction_pressed = true;
				}
			}

			// MOVE TIME

			Vector3 new_position = Vector3.Lerp(player_sphere.transform.position, player_target_position, Time.deltaTime * 10);

			if (direction_pressed == true) {
				Vector3 pressed_target_position = new Vector3(player_coord_x, player_coord_y, -4.3F);
				new_position = Vector3.MoveTowards(player_sphere.transform.position, pressed_target_position, 0.1F);
			}

			player_sphere.transform.position = new_position;

			float distance = Vector3.Distance(new_position, player_target_position);
			
			allow_movement = false;
			adjust_camera();
			
			if (distance < 0.1F) {
				allow_movement = true;
				note_collected = check_note_collection();
				
				if (distance < 0.01F) {
					player_sphere_moving = false;
					player_sphere.transform.position = player_target_position;
				}
			}
		}
		
		if (allow_movement && !note_collected) {
			// CONTROL TIME
			maze_field_script current_maze_field = get_maze_field_script(player_coord_x, player_coord_y);
			bool direction_pressed = false;
			
			if (player_pressed_right()) {
				if (current_maze_field.removed_right() == true) {
					player_coord_x++;
					player_sphere_moving = true;
					direction_pressed = true;
				}
			}
			if (player_pressed_down() && !player_sphere_moving) {
				if (current_maze_field.removed_bottom() == true) {
					player_coord_y--;
					player_sphere_moving = true;
					direction_pressed = true;
				}
			}
			if (player_pressed_up() && !player_sphere_moving) {
				if (current_maze_field.removed_top() == true) {
					player_coord_y++;
					player_sphere_moving = true;
					direction_pressed = true;
				}
			}
			if (player_pressed_left() && !player_sphere_moving) {
				if (current_maze_field.removed_left() == true) {
					player_coord_x--;
					player_sphere_moving = true;
					direction_pressed = true;
				}
			}

			if (player_sphere_moving == true && direction_pressed == true) {
				if (base_note_reached_center == true) {
					player_saved_path_coordinates.Add(new TakenPath { coord_x = player_coord_x, coord_y = player_coord_y });
				}
				player_target_position = new Vector3(player_coord_x, player_coord_y, -4.3F);
				
				mazeman.highlight_walls_around_maze_field(current_maze_field, true);
				
				maze_field_script next_maze_field = get_maze_field_script(player_coord_x, player_coord_y);
				mazeman.highlight_walls_around_maze_field(next_maze_field, false);
			}
		}
	}

	public void detectPressedKeyOrButton() {
		foreach(KeyCode kcode in System.Enum.GetValues(typeof(KeyCode))) {
			if (Input.GetKeyDown(kcode))
				Debug.Log("KeyCode down: " + kcode);
		}
	}

	public bool player_action_button_down = false;

	public bool player_pressed_action_once() {
		if (Input.GetKey (KeyCode.Space) || Input.GetButton("Fire1")) {
			if (player_action_button_down == false) {
				player_action_button_down = true;
				return true;
			}
		}
		else {
			player_action_button_down = false;
		}

		return false;
	}

	public bool player_pressed_action() {
		if (Input.GetKey (KeyCode.Space) || Input.GetButton("Fire1")) {
			return true;
		}
		
		return false;
	}

	public bool player_pressed_action2() {
		if (Input.GetKey (KeyCode.Delete) || Input.GetButton("Fire2")) {
			return true;
		}
		
		return false;
	}

	public bool player_pressed_up() {
		if (Input.GetKey (KeyCode.UpArrow) || Input.GetAxis("Vertical") > 0.1F) {
			return true;
		}

		return false;
	}

	public bool player_pressed_down() {
		if (Input.GetKey (KeyCode.DownArrow) || Input.GetAxis("Vertical") < -0.1F) {
			return true;
		}

		return false;
	}

	public bool player_pressed_right() {
		if (Input.GetKey (KeyCode.RightArrow) || Input.GetAxis("Horizontal") > 0.1F) {
			return true;
		}

		return false;
	}

	public bool player_pressed_left() {
		if (Input.GetKey (KeyCode.LeftArrow) || Input.GetAxis("Horizontal") < -0.1F) {
			return true;
		}

		return false;
	}

	bool repeated_once = false;
	Dictionary<string,IEnumerator> player_button_press_coroutines = new Dictionary<string,IEnumerator>();
	Dictionary<string,bool> player_button_pressed_down_once = new Dictionary<string,bool>();

	private IEnumerator player_button_press_coroutine;
	public IEnumerator player_press_button_repeat(float waitTime, string direction) {
		yield return new WaitForSeconds(waitTime);

		player_button_pressed_down_once[direction] = false;

		repeated_once = true;
	}

	bool check_and_repeat_player_button_press(bool pressed, string direction) {
		if (pressed) {
			bool player_button_down;
			player_button_pressed_down_once.TryGetValue(direction, out player_button_down);
			if (player_button_down != true) {
				player_button_pressed_down_once[direction] = true;
				
				if (repeated_once == false) {
					player_button_press_coroutines[direction] = player_press_button_repeat(0.3F, direction);
				}
				else {
					player_button_press_coroutines[direction] = player_press_button_repeat(0.1F, direction);
				}
				StartCoroutine(player_button_press_coroutines[direction]);
				
				return true;
			}
		}
		else {
			IEnumerator coroutine_for_direction;
			player_button_press_coroutines.TryGetValue(direction, out coroutine_for_direction);
			if (coroutine_for_direction != null) {
				StopCoroutine(coroutine_for_direction);
				player_button_press_coroutines[direction] = null;
				repeated_once = false;
			}
			player_button_pressed_down_once[direction] = false;
		}
		
		return false;
	}

	public bool player_pressed_action2_once() {
		bool pressed = player_pressed_action2();
		
		return check_and_repeat_player_button_press(pressed, "action2");
	}

	public bool player_pressed_up_once() {
		bool pressed = player_pressed_up();

		return check_and_repeat_player_button_press(pressed, "up");
	}

	public bool player_pressed_down_once() {
		bool pressed = player_pressed_down();
		
		return check_and_repeat_player_button_press(pressed, "down");
	}

	public bool player_pressed_left_once() {
		bool pressed = player_pressed_left();

		return check_and_repeat_player_button_press(pressed, "left");
	}

	public bool player_pressed_right_once() {
		bool pressed = player_pressed_right();
		
		return check_and_repeat_player_button_press(pressed, "right");
	}
	
	IEnumerator spawn_player_sphere_routine(float delay_seconds) {
		float t = 0.0F;
		while (t <= 1.0F) {
			t += Time.deltaTime/delay_seconds;
			yield return null;
		}

		player_sphere = (GameObject)Instantiate(
			player_sphere_prefab,
			new Vector3(0, 0, -44.3F),
			Quaternion.identity
		);

		player_sphere_moving = true;
		player_target_position = new Vector3(0F, 0.0F, -4.3F);
		player_coord_x = 0;
		player_coord_y = 0;
	}

	public void spawn_player_sphere(float delay_seconds) {
		StartCoroutine(spawn_player_sphere_routine(delay_seconds));
		player_spheres++;
	}
}
