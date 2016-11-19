using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MazeMan : MonoBehaviour {

	public GameObject maze_field;
	public GameObject maze_wall;
	public GameObject maze_note;

	public int maze_border_left   = 0;
	public int maze_border_right  = 0;
	public int maze_border_top    = 0;
	public int maze_border_bottom = 0;

	public int maze_initialized = 0;

	public Dictionary<int,GameObject> maze_field_coordinates_hash = new Dictionary<int,GameObject>();
	public Dictionary<string,GameObject> maze_walls_coordinates_hash = new Dictionary<string,GameObject>();
	public List<GameObject> maze_notes = new List<GameObject>();
	
	public bool maze_deconstruction = false;
	public List<GameObject> maze_destruction_animator = new List<GameObject>();

	public GameObject create_maze_field (int col_count, int row_count) {
		
		int pos_x = col_count;// - 7;
		int pos_y = row_count;// - 4;
		
		GameObject new_maze_field = (GameObject)Instantiate(maze_field, new Vector3(pos_x, pos_y, -4F), Quaternion.identity);
		maze_field_script new_maze_field_script = gameentity.get_maze_field_script_from_game_object(new_maze_field);
		new_maze_field_script.coord_x = col_count;
		new_maze_field_script.coord_y = row_count;
		maze_field_coordinates_hash.Add (coordinates_to_array_index(col_count, row_count), new_maze_field);
		//maze_field_coordinates.Insert(coordinates_to_array_index(col_count, row_count), new_maze_field);

		if (pos_x > maze_border_right) {
			maze_border_right = pos_x;
		}

		if (pos_x < maze_border_left) {
			maze_border_left = pos_x;
		}

		if (pos_y > maze_border_top) {
			maze_border_top = pos_y;
		}

		if (pos_y < maze_border_bottom) {
			maze_border_bottom = pos_y;
		}

		return new_maze_field;
	}
	
	public GameObject find_or_create_field_at_coordinates (int x, int y) {
		
		int array_index = coordinates_to_array_index(x, y);
		
		if (maze_field_coordinates_hash.ContainsKey(array_index)) {
			
			return maze_field_coordinates_hash[array_index];
		}
		else {
			
			return create_maze_field(x, y);
		}
	}
	
	public GameObject create_maze_wall (float pos_x, float pos_y) {
		
		string array_index = wall_hash_index(pos_x, pos_y);
		GameObject new_maze_wall = (GameObject)Instantiate(maze_wall, new Vector3(pos_x, pos_y, -4F), Quaternion.identity);
		maze_walls_coordinates_hash.Add (array_index, new_maze_wall);

		// check if it's a horizontal wall
		if (Mathf.Floor(pos_y) != pos_y) {
			new_maze_wall.transform.localRotation = Quaternion.Euler (0, 0, -90F);
		}

		maze_wall_script next_maze_wall_script = gameentity.get_maze_wall_script_from_game_object(new_maze_wall);
		next_maze_wall_script.FadeOut();
		
		return new_maze_wall;
	}
	
	public GameObject find_or_create_wall_at_coordinates (float x, float y) {
		
		if (wall_at_coordinates_exists(x, y)) {
			
			return maze_walls_coordinates_hash[wall_hash_index(x, y)];
		}
		else {
			
			return create_maze_wall(x, y);
		}
	}
	
	public string wall_hash_index (float x, float y) {
		string array_index = x.ToString() + '-' + y.ToString();
		
		return array_index;
	}
	
	public bool wall_at_coordinates_exists (float x, float y) {
		if (maze_walls_coordinates_hash.ContainsKey(wall_hash_index(x, y))) {
			
			return true;
		}
		
		return false;
	}
	
	public bool field_at_coordinates_exists(int x, int y) {
		
		int array_index = coordinates_to_array_index(x, y);
		
		return maze_field_coordinates_hash.ContainsKey(array_index);
	}

	public void quake_from_current_position(float delay) {
		
		int radius = 4;
		int walk_x = gameentity.player_coord_x - radius;
		int walk_y = gameentity.player_coord_x - radius;
		int max_x  = gameentity.player_coord_x + radius;
		int max_y  = gameentity.player_coord_y + radius;
		
		while (walk_y < max_y) {
			
			if (field_at_coordinates_exists(walk_x, walk_y)) {
				GameObject maze_field_target = find_or_create_field_at_coordinates(walk_x, walk_y);
				maze_field_script gotten_maze_script = gameentity.get_maze_field_script_from_game_object(maze_field_target);
				
				// delay = max difference to player_coord
				int diff_x = Mathf.Abs(walk_x - gameentity.player_coord_x);
				int diff_y = Mathf.Abs(walk_y - gameentity.player_coord_y);
				
				gotten_maze_script.quake(1F, delay + (Mathf.Max(diff_x + diff_y) / 5F));
			}
			
			walk_x++;
			
			if (walk_x > max_x) {
				walk_x = gameentity.player_coord_x - radius;
				walk_y++;
			}
		}
	}
	
	public int coordinates_to_array_index(int coord_x, int coord_y) {
		return 100 + coord_y * 100 + coord_x;
	}
	
	public void animate_maze_destruction() {
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
				Vector3 target_position = gameentity.get_random_offscreen_position(maze_field_target.transform.position.z);
				gameentity.smooth_move(maze_field_target.transform.position, target_position, Random.Range (0, 1F), Random.Range (0, 1F), maze_field_target);
			}
			
			maze_deconstruction = false;
		}
	}
	
	public void build_maze() {
		while (maze_initialized < 4) {
			GameObject last_maze_field;
			int difficulty_steps = 10;
			last_maze_field = draw_maze_tunnel(maze_initialized, 0, 0, difficulty_steps);
			maze_field_script last_maze_field_script = gameentity.get_maze_field_script_from_game_object(last_maze_field);
			
			GameObject new_note = (GameObject)Instantiate(maze_note, new Vector3(last_maze_field.transform.position.x, last_maze_field.transform.position.y, last_maze_field.transform.position.z - 1F), Quaternion.identity);
			maze_notes.Add(new_note);
			new_note.transform.parent = last_maze_field.transform;
			
			maze_note_script new_note_script = gameentity.get_maze_note_script_from_game_object(new_note);
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

	public void highlight_walls_around_maze_field(maze_field_script given_maze_field, bool fadeout) {
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
				maze_wall_script found_wall_script = gameentity.get_maze_wall_script_from_game_object(found_wall);
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
	
	public void draw_maze_corners() {
		if (maze_initialized == 4) {
			
			// figure out boundaries of maze
			int lowest_x = 0;
			int lowest_y = 0;
			int highest_x = 0;
			int highest_y = 0;
			
			foreach (KeyValuePair<int,GameObject> field in maze_field_coordinates_hash) {
				maze_field_script field_script = gameentity.get_maze_field_script_from_game_object(field.Value);
				
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
			
			GameObject last_created_tunnel = gameentity.base_note;
			
			while (row_count <= highest_y) {
				while (col_count <= highest_x) {
					last_created_tunnel = draw_maze_tunnel(col_count % 4, col_count, row_count, 30);
					col_count++;
				}
				
				col_count = lowest_x;
				row_count++;
			}
			
			// place base note
			gameentity.base_note_ingame = (GameObject)Instantiate(
				gameentity.base_note, new Vector3(
					last_created_tunnel.transform.position.x,
					last_created_tunnel.transform.position.y,
					last_created_tunnel.transform.position.z - 1F
				), Quaternion.identity
			);
			gameentity.base_note_ingame.transform.Rotate(270F, 0F, 0F);
			gameentity.player_sphere = gameentity.base_note_ingame;
			gameentity.player_coord_x = (int)last_created_tunnel.transform.position.x;
			gameentity.player_coord_y = (int)last_created_tunnel.transform.position.y;
			gameentity.adjust_camera();
			
			maze_initialized++;
		}
	}
	
	public void draw_maze_walls() {
		
		int counter = 0;
		
		foreach (KeyValuePair<int,GameObject> field in maze_field_coordinates_hash) {
			maze_field_script field_script = gameentity.get_maze_field_script_from_game_object(field.Value);
			
			// field_script.coord_x
			// field_script.coord_y
			
			counter++;
			
			if (counter > 0) {
				
				if (!field_script.removed_top()) {
					find_or_create_wall_at_coordinates(field_script.coord_x * 1.0F, (field_script.coord_y * 1.0F) + 0.5F);
				}
				if (!field_script.removed_bottom()) {
					find_or_create_wall_at_coordinates(field_script.coord_x * 1.0F, (field_script.coord_y * 1.0F) - 0.5F);
				}
				if (!field_script.removed_left()) {
					find_or_create_wall_at_coordinates((field_script.coord_x * 1.0F) - 0.5F, field_script.coord_y * 1.0F);
				}
				if (!field_script.removed_right()) {
					find_or_create_wall_at_coordinates((field_script.coord_x * 1.0F) + 0.5F, field_script.coord_y * 1.0F);
				}
			}
		}
	}

	public void destroy_wall_at_coordinates(float x, float y) {
		if (wall_at_coordinates_exists(x, y)) {
			GameObject found_wall = find_or_create_wall_at_coordinates(x, y);
			maze_walls_coordinates_hash.Remove(wall_hash_index(x, y));
			Destroy(found_wall);
		}
	}

	public void destroy_field_at_coordinates(float x, float y) {
		int int_x = Mathf.RoundToInt(x);
		int int_y = Mathf.RoundToInt(y);

		if (field_at_coordinates_exists(int_x, int_y)) {
			GameObject found_field = find_or_create_field_at_coordinates(int_x, int_y);
			maze_field_coordinates_hash.Remove(coordinates_to_array_index(int_x, int_y));
			Destroy(found_field);
		}
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

	public class TakenPath {
		public int coord_x { get; set; }
		public int coord_y { get; set; }
		public int taken_direction { get; set; }
	}

	bool path_contains_coordinates(List<TakenPath> saved_paths, TakenPath taken_path) {
		int found = saved_paths.FindIndex( x => x.coord_x == taken_path.coord_x && x.coord_y == taken_path.coord_y);
		
		if (found >= 0) {
			return true;
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

	GameEntity gameentity;

	// Use this for initialization
	void Start () {
		gameentity = GetComponent<GameEntity>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
	
	}
}
