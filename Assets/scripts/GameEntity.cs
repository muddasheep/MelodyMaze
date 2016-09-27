using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameEntity : MonoBehaviour {
	public float start_time = 0;
	public float current_time = 0;

	public GameObject wall_of_doom;
	public GameObject player_sphere_prefab;
	public GameObject maze_field;
	public GameObject maze_wall;
	public GameObject maze_note;
	public GameObject redrawing_field;
	public GameObject base_note;
	public Camera maze_cam;

	List<GameObject> maze_field_coordinates = new List<GameObject>();
	Dictionary<int,GameObject> maze_field_coordinates_hash = new Dictionary<int,GameObject>();
	Dictionary<string,GameObject> maze_walls_coordinates_hash = new Dictionary<string,GameObject>();
	List<GameObject> maze_notes = new List<GameObject>();

	int collected_notes = 0;

	int maze_initialized = 0;
	bool base_note_reached_center = false;
	GameObject base_note_ingame;
	bool maze_deconstruction = false;
	int maze_path_redraw_rate_counter = 0;
	List<GameObject> maze_destruction_animator = new List<GameObject>();

	GameObject player_sphere;
	int player_spheres = 0;
	int player_coord_x = 0;
	int player_coord_y = 0;
	Vector3 player_target_position;
	bool player_sphere_moving = false;
	float base_note_speed_x = 0F;
	float base_note_speed_y = 0F;

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

	// Use this for initialization
	void Start () {

		start_time = Time.time;
	}

	GameObject create_maze_field (int col_count, int row_count) {

		int pos_x = col_count;// - 7;
		int pos_y = row_count;// - 4;

		GameObject new_maze_field = (GameObject)Instantiate(maze_field, new Vector3(pos_x, pos_y, -4F), Quaternion.identity);
		maze_field_script new_maze_field_script = get_maze_field_script_from_game_object(new_maze_field);
		new_maze_field_script.coord_x = col_count;
		new_maze_field_script.coord_y = row_count;
		maze_field_coordinates_hash.Add (coordinates_to_array_index(col_count, row_count), new_maze_field);
		//maze_field_coordinates.Insert(coordinates_to_array_index(col_count, row_count), new_maze_field);
		
		return new_maze_field;
	}

	GameObject find_or_create_field_at_coordinates (int x, int y) {

		int array_index = coordinates_to_array_index(x, y);

		if (maze_field_coordinates_hash.ContainsKey(array_index)) {

			return maze_field_coordinates_hash[array_index];
		}
		else {

			return create_maze_field(x, y);
		}
	}

	GameObject create_maze_wall (float pos_x, float pos_y) {

		string array_index = wall_hash_index(pos_x, pos_y);
		GameObject new_maze_wall = (GameObject)Instantiate(maze_wall, new Vector3(pos_x, pos_y, -4F), Quaternion.identity);
		maze_walls_coordinates_hash.Add (array_index, new_maze_wall);

		maze_wall_script next_maze_wall_script = get_maze_wall_script_from_game_object(new_maze_wall);
		next_maze_wall_script.FadeOut();

		return new_maze_wall;
	}

	GameObject find_or_create_wall_at_coordinates (float x, float y) {

		if (wall_at_coordinates_exists(x, y)) {
			
			return maze_walls_coordinates_hash[wall_hash_index(x, y)];
		}
		else {

			return create_maze_wall(x, y);
		}
	}

	string wall_hash_index (float x, float y) {
		string array_index = x.ToString() + '-' + y.ToString();

		return array_index;
	}

	bool wall_at_coordinates_exists (float x, float y) {
		if (maze_walls_coordinates_hash.ContainsKey(wall_hash_index(x, y))) {
			
			return true;
		}

		return false;
	}

	bool field_at_coordinates_exists(int x, int y) {

		int array_index = coordinates_to_array_index(x, y);

		return maze_field_coordinates_hash.ContainsKey(array_index);
	}

	// Update is called once per frame
	void FixedUpdate () {
		current_time = Time.time - start_time;

		if (maze_deconstruction == true) {
			deconstruct_maze();
		}
		else {

			build_maze();

			// move player
			if (player_spheres == 0 && base_note_reached_center == true) {
				spawn_player_sphere();
			}
			else {
				check_base_note_reach_center();
				player_movement();
			}
		}
	}

	void check_base_note_reach_center() {
		if (!player_sphere) {
			return;
		}

		if (player_coord_x == 0 && player_coord_y == 0 && player_sphere_moving == false && base_note_reached_center == false) {
			base_note_reached_center = true;
			quake_from_current_position(0.8F);
			base_note_script next_base_note_script = get_base_note_script_from_game_object(base_note_ingame);
			next_base_note_script.set_up_base_camp();

			smooth_move(player_sphere.transform.position, new Vector3(0F, 0F, -4.7F), 0.5F, 0F, player_sphere);
		}
	}

	void adjust_camera() {
		if (!player_sphere) {
			return;
		}
		maze_cam.transform.position = new Vector3(player_sphere.transform.position.x, player_sphere.transform.position.y, -20.8F);
	}

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
						
						GameObject redrawing_maze_field = find_or_create_field_at_coordinates(current_path.coord_x, current_path.coord_y);
						
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
			
			animate_maze_destruction();
		}
	}

	void quake_from_current_position(float delay) {
		int maze_wave_radius_counter = 0;

		maze_field_script current_maze_field = get_maze_field_script(player_coord_x, player_coord_y);

		int radius = 4;
		int walk_x = player_coord_x - radius;
		int walk_y = player_coord_x - radius;
		int max_x  = player_coord_x + radius;
		int max_y  = player_coord_y + radius;

		while (walk_y < max_y) {

			if (field_at_coordinates_exists(walk_x, walk_y)) {
				GameObject maze_field_target = find_or_create_field_at_coordinates(walk_x, walk_y);
				maze_field_script gotten_maze_script = get_maze_field_script_from_game_object(maze_field_target);

				// delay = max difference to player_coord
				int diff_x = Mathf.Abs(walk_x - player_coord_x);
				int diff_y = Mathf.Abs(walk_y - player_coord_y);

				gotten_maze_script.quake(1F, delay + (Mathf.Max(diff_x + diff_y) / 5F));
			}

			walk_x++;

			if (walk_x > max_x) {
				walk_x = player_coord_x - radius;
				walk_y++;
			}
		}
	}

	int coordinates_to_array_index(int coord_x, int coord_y) {
		return 100 + coord_y * 100 + coord_x;
	}

	void animate_maze_destruction() {
		int finished_destruction = 0;
		foreach(GameObject maze_field_target in maze_destruction_animator) {
			Vector3 target_position = new Vector3(maze_field_target.transform.position.x, maze_field_target.transform.position.y, -2F);
			Vector3 new_position = Vector3.Lerp(maze_field_target.transform.position, target_position, Time.deltaTime * 2);
			maze_field_target.transform.position = new_position;
			maze_field_target.transform.Rotate(-1F, 1F, 3F, Space.World);

			float distance = Vector3.Distance(new_position, target_position);
			if (distance < 0.2F) {
				finished_destruction++;
			}
		}

		if (finished_destruction >= maze_destruction_animator.Count && maze_destruction_animator.Count > 100) {
			foreach(GameObject maze_field_target in maze_destruction_animator) {
				Vector3 target_position = get_random_offscreen_position(maze_field_target.transform.position.z);
				smooth_move(maze_field_target.transform.position, target_position, Random.Range (0, 1F), Random.Range (0, 1F), maze_field_target);
			}

			maze_deconstruction = false;
		}
	}

	Vector3 get_random_offscreen_position(float target_z) {
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

	void build_maze() {
		while (maze_initialized < 4) {
			GameObject last_maze_field;
			int difficulty_steps = 10;
			last_maze_field = draw_maze_tunnel(maze_initialized, 0, 0, difficulty_steps);
			maze_field_script last_maze_field_script = get_maze_field_script_from_game_object(last_maze_field);

			GameObject new_note = (GameObject)Instantiate(maze_note, new Vector3(last_maze_field.transform.position.x, last_maze_field.transform.position.y, last_maze_field.transform.position.z - 1F), Quaternion.identity);
			maze_notes.Add(new_note);
			new_note.transform.parent = last_maze_field.transform;
			
			maze_note_script new_note_script = get_maze_note_script_from_game_object(new_note);
			new_note_script.coord_x = last_maze_field_script.coord_x;
			new_note_script.coord_y = last_maze_field_script.coord_y;

			maze_initialized++;
		}

		if (maze_initialized == 4) {
			// draw corners
			draw_maze_corners();

			// draw walls
			draw_maze_walls();
		}
	}

	void draw_maze_corners() {
		if (maze_initialized == 4) {

			// figure out boundaries of maze
			int lowest_x = 0;
			int lowest_y = 0;
			int highest_x = 0;
			int highest_y = 0;

			foreach (KeyValuePair<int,GameObject> field in maze_field_coordinates_hash) {
				maze_field_script field_script = get_maze_field_script_from_game_object(field.Value);

				if (field_script.coord_x < lowest_x) {
					lowest_x = field_script.coord_x;
				}
				if (field_script.coord_y < lowest_y) {
					lowest_y = field_script.coord_y;
				}
				if (field_script.coord_x > highest_x) {
					highest_x = field_script.coord_x;
				}
				if (field_script.coord_y > highest_y) {
					highest_y = field_script.coord_y;
				}
			}

			int row_count = lowest_y;
			int col_count = lowest_x;

			GameObject last_created_tunnel = base_note;

			while (row_count <= highest_y) {
				while (col_count <= highest_x) {
					last_created_tunnel = draw_maze_tunnel(col_count % 4, col_count, row_count, 30);
					col_count++;
				}

				col_count = lowest_x;
				row_count++;
			}

			// place base note
			base_note_ingame = (GameObject)Instantiate(base_note, new Vector3(last_created_tunnel.transform.position.x, last_created_tunnel.transform.position.y, last_created_tunnel.transform.position.z - 1F), Quaternion.identity);
			base_note_ingame.transform.Rotate(270F, 0F, 0F);
			player_sphere = base_note_ingame;
			player_coord_x = (int)last_created_tunnel.transform.position.x;
			player_coord_y = (int)last_created_tunnel.transform.position.y;
			adjust_camera();

			maze_initialized++;
		}
	}

	void draw_maze_walls() {

		int counter = 0;

		foreach (KeyValuePair<int,GameObject> field in maze_field_coordinates_hash) {
			maze_field_script field_script = get_maze_field_script_from_game_object(field.Value);

			// field_script.coord_x
			// field_script.coord_y

			GameObject new_wall;

			counter++;

			if (counter > 0) {

				if (!field_script.removed_top()) {
					new_wall = find_or_create_wall_at_coordinates(field_script.coord_x * 1.0F, (field_script.coord_y * 1.0F) + 0.5F);
					new_wall.transform.localRotation = Quaternion.Euler (0, 0, -90F);
				}
				if (!field_script.removed_bottom()) {
					new_wall = find_or_create_wall_at_coordinates(field_script.coord_x * 1.0F, (field_script.coord_y * 1.0F) - 0.5F);
					new_wall.transform.localRotation = Quaternion.Euler (0, 0, -90F);
				}
				if (!field_script.removed_left()) {
					new_wall = find_or_create_wall_at_coordinates((field_script.coord_x * 1.0F) - 0.5F, field_script.coord_y * 1.0F);
				}
				if (!field_script.removed_right()) {
					new_wall = find_or_create_wall_at_coordinates((field_script.coord_x * 1.0F) + 0.5F, field_script.coord_y * 1.0F);
				}
			}
		}
	}

	bool check_note_collection() {
		if (base_note_reached_center == false) {
			return false;
		}

		maze_field_script current_maze_field = get_maze_field_script(player_coord_x, player_coord_y);
		int check_coord_x = current_maze_field.coord_x;
		int check_coord_y = current_maze_field.coord_y;

		foreach (GameObject note in maze_notes) {
			maze_note_script note_script = get_maze_note_script_from_game_object(note);

			if (note_script.coord_x == check_coord_x && note_script.coord_y == check_coord_y && note_script.collected == false) {
				note_script.collected = true;
				collected_notes++;

				player_sphere.transform.parent = current_maze_field.gameObject.transform;

				if (collected_notes < maze_notes.Count) {
					// spawn another player sphere
					spawn_player_sphere();
				}
				else {
					maze_deconstruction = true;
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

	maze_note_script get_maze_note_script_from_game_object(GameObject maze_note_object) {
		maze_note_script next_note_script = (maze_note_script)maze_note_object.GetComponent(typeof(maze_note_script));
		return next_note_script;
	}

	maze_field_script get_maze_field_script_from_game_object(GameObject maze_field_object) {
		maze_field_script next_maze_script = (maze_field_script)maze_field_object.GetComponent(typeof(maze_field_script));
		return next_maze_script;
	}

	maze_wall_script get_maze_wall_script_from_game_object(GameObject maze_wall_object) {
		maze_wall_script next_wall_script = (maze_wall_script)maze_wall_object.GetComponent(typeof(maze_wall_script));
		return next_wall_script;
	}

	base_note_script get_base_note_script_from_game_object(GameObject base_note_object) {
		base_note_script next_base_script = (base_note_script)base_note_object.GetComponent(typeof(base_note_script));
		return next_base_script;
	}

	maze_field_script get_maze_field_script(int coord_x, int coord_y) {
		GameObject next_maze_field = find_or_create_field_at_coordinates(coord_x, coord_y);
		maze_field_script next_maze_script = get_maze_field_script_from_game_object(next_maze_field);

		return next_maze_script;
	}

	void player_movement() {
		if (!player_sphere) {
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

		player_sphere.transform.position = new Vector3(player_x, player_y, player_z);

		// highlight field walls based on rounded coords
		player_coord_x = Mathf.RoundToInt(player_x);
		player_coord_y = Mathf.RoundToInt(player_y);

		adjust_camera();
	}

	void move_current_note() {
		
		float player_z = player_sphere.transform.position.z;
		
		bool allow_movement = true;
		bool note_collected = false;
		
		if (player_sphere_moving == true) {
			// MOVE TIME
			Vector3 new_position = Vector3.Lerp(player_sphere.transform.position, player_target_position, Time.deltaTime * 10);
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
			else {
				if (player_pressed_down()) {
					if (current_maze_field.removed_bottom() == true) {
						player_coord_y--;
						player_sphere_moving = true;
						direction_pressed = true;
					}
				}
				else {
					if (player_pressed_up()) {
						if (current_maze_field.removed_top() == true) {
							player_coord_y++;
							player_sphere_moving = true;
							direction_pressed = true;
						}
					}
					else {
						if (player_pressed_left()) {
							if (current_maze_field.removed_left() == true) {
								player_coord_x--;
								player_sphere_moving = true;
								direction_pressed = true;
							}
						}
					}
				}
			}
			
			if (player_sphere_moving == true && direction_pressed == true) {
				if (base_note_reached_center == true) {
					player_saved_path_coordinates.Add(new TakenPath { coord_x = player_coord_x, coord_y = player_coord_y });
				}
				player_target_position = new Vector3(player_coord_x, player_coord_y, player_z);
				
				highlight_walls_around_maze_field(current_maze_field, true);
				
				maze_field_script next_maze_field = get_maze_field_script(player_coord_x, player_coord_y);
				highlight_walls_around_maze_field(next_maze_field, false);
			}
		}
	}

	bool player_pressed_up() {
		if (Input.GetKey (KeyCode.UpArrow) || Input.GetAxis("Vertical") > 0.1F) {
			return true;
		}

		return false;
	}

	bool player_pressed_down() {
		if (Input.GetKey (KeyCode.DownArrow) || Input.GetAxis("Vertical") < -0.1F) {
			return true;
		}

		return false;
	}

	bool player_pressed_right() {
		if (Input.GetKey (KeyCode.RightArrow) || Input.GetAxis("Horizontal") > 0.1F) {
			return true;
		}

		return false;
	}

	bool player_pressed_left() {
		if (Input.GetKey (KeyCode.LeftArrow) || Input.GetAxis("Horizontal") < -0.1F) {
			return true;
		}

		return false;
	}

	void highlight_walls_around_maze_field(maze_field_script given_maze_field, bool fadeout) {
		float start_x = given_maze_field.coord_x;
		float start_y = given_maze_field.coord_y;

		float check_x = start_x;
		float check_y = start_y;

		int count = 0;

		while ( count < 4 ) {

			check_x = start_x;
			check_y = start_y;

			if (count == 0) {
				check_x -= 0.5F;
			}
			if (count == 1) {
				check_x += 0.5F;
			}
			if (count == 2) {
				check_y += 0.5F;
			}
			if (count == 3) {
				check_y -= 0.5F;
			}

			if (wall_at_coordinates_exists(check_x, check_y)) {
				GameObject found_wall = find_or_create_wall_at_coordinates(check_x, check_y);
				maze_wall_script found_wall_script = get_maze_wall_script_from_game_object(found_wall);
				if (fadeout == true) {
					found_wall_script.FadeOut();
				}
				else {
					found_wall_script.FadeIn();
				}
			}

			count++;
		}
	}

	void spawn_player_sphere() {
		player_sphere = (GameObject)Instantiate(player_sphere_prefab, new Vector3(0, 0, -44.3F), Quaternion.identity);
		player_sphere_moving = true;
		player_target_position = new Vector3(0F, 0.0F, -4.3F);
		player_coord_x = 0;
		player_coord_y = 0;
		player_spheres++;
		adjust_camera();
	}

	GameObject draw_maze_tunnel(int current_direction, int start_x, int start_y, int steps) {
		// build maze
		// current_direction: 0 N, 1 E, 2 S, 3 W
		// int start_x = 7;
		// int start_y = 4;
		GameObject current_field = find_or_create_field_at_coordinates(start_x, start_y);
		GameObject previous_field;
		int temp_x;
		int temp_y;
		int initial_x = start_x;
		int initial_y = start_y;
		int avoid_direction = opposite_direction(current_direction);

		List<TakenPath> tunnel_saved_path_coordinates = new List<TakenPath>();

		int counter = 0;
		int insanity_counter = 0;

		// make a theoretical array of coordinates

		while (counter < steps && insanity_counter < 50) {
			temp_x = start_x;
			temp_y = start_y;

			if (avoid_direction == current_direction) {
				Debug.Log("GOING THE WRONG WAY T_T");
			}

			start_x = change_x_via_direction(start_x, current_direction);
			start_y = change_y_via_direction(start_y, current_direction);

			TakenPath new_taken_path = new TakenPath { coord_x = start_x, coord_y = start_y, taken_direction = current_direction };

			if (field_at_coordinates_exists(start_x, start_y) || path_contains_coordinates(tunnel_saved_path_coordinates, new_taken_path)) {

				current_direction = choose_random_direction_with_exceptions(new int[] { current_direction, avoid_direction });

				insanity_counter++;
				start_x = temp_x;
				start_y = temp_y;
				continue;
			}

			// add coordinates
			tunnel_saved_path_coordinates.Add(new_taken_path);

			current_direction = choose_random_direction_with_exceptions(new int[] { avoid_direction });

			counter++;
		}

		// create fields from coordinates
		previous_field = current_field;
		foreach (TakenPath path in tunnel_saved_path_coordinates) {
			current_field = find_or_create_field_at_coordinates(path.coord_x, path.coord_y);
			maze_field_script current_script = (maze_field_script)current_field.GetComponent(typeof(maze_field_script));

			if(previous_field) {
				maze_field_script previous_script = (maze_field_script)previous_field.GetComponent(typeof(maze_field_script));

				if (path.taken_direction == 0) {	
					previous_script.remove_top();
					current_script.remove_bottom();
				}
				if (path.taken_direction == 1) {
					previous_script.remove_right();
					current_script.remove_left();
				}
				if (path.taken_direction == 2) {
					previous_script.remove_bottom();
					current_script.remove_top();
				}
				if (path.taken_direction == 3) {
					previous_script.remove_left();
					current_script.remove_right();
				}
			}

			previous_field = current_field;
		}

		return current_field;
	}

	bool path_contains_coordinates(List<TakenPath> saved_paths, TakenPath taken_path) {
		int found = saved_paths.FindIndex( x => x.coord_x == taken_path.coord_x && x.coord_y == taken_path.coord_y);

		if (found >= 0) {
			return true;
		}
		else {
			return false;
		}

		return false;
	}

	int change_x_via_direction(int x, int direction) {
		if (direction < 0) {
			direction = 3;
		}
		if (direction > 3) {
			direction = 0;
		}

		// direction: 0 N, 1 E, 2 S, 3 W
		if (direction == 1) {
			x++;
		}
		if (direction == 3) {
			x--;
		}

		return x;
	}

	int change_y_via_direction(int y, int direction) {
		if (direction < 0) {
			direction = 3;
		}
		if (direction > 3) {
			direction = 0;
		}
		
		// direction: 0 N, 1 E, 2 S, 3 W
		if (direction == 0) {
			y++;
		}
		if (direction == 2) {
			y--;
		}

		return y;
	}

	int opposite_direction(int direction) {
		if (direction == 0) {
			return 2;
		}
		if (direction == 1) {
			return 3;
		}
		if (direction == 2) {
			return 0;
		}
		if (direction == 3) {
			return 1;
		}

		return 0;
	}

	int choose_random_direction_with_exceptions(int[] direction_exceptions) {
		List<int> new_directions = new List<int>();
		
		int direction_counter = 0;
		
		while (direction_counter < 4) {

			bool direction_found = false;

			for (int i = 0; i < direction_exceptions.Length; i++) {
				if (direction_exceptions[i] == direction_counter) {
					direction_found = true;
				}
			}

			if (!direction_found) {
				new_directions.Add(direction_counter);
			}

		    direction_counter++;
	    }

		return new_directions[ Random.Range( 0, new_directions.Count ) ];
    }
}
