using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameEntity : MonoBehaviour {
	public GameObject wall_of_doom;
	public GameObject player_sphere_prefab;
	public GameObject maze_field;
	public GameObject maze_note;
	public GameObject redrawing_field;

	List<GameObject> maze_field_coordinates = new List<GameObject>();
	Dictionary<int,GameObject> maze_field_coordinates_hash = new Dictionary<int,GameObject>();
	List<GameObject> maze_notes = new List<GameObject>();

	int collected_notes = 0;

	int maze_initialized = 0;
	bool maze_has_failed_to_construct = false;
	bool maze_deconstruction = false;
	int maze_path_redraw_rate_counter = 0;
	int maze_wave_radius_counter = 0;
	List<GameObject> maze_destruction_animator = new List<GameObject>();

	GameObject player_sphere;
	int player_spheres = 0;
	int player_coord_x = 0;
	int player_coord_y = 0;
	Vector3 player_target_position;
	bool player_sphere_moving = false;
	
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
		// build maze fields
		/*float pos_x = -7F;
		float pos_y = -4F;

		int row_count = 0;
		int col_count = 0;

		while (row_count < 9) {
			while (col_count < 15) {
				create_maze_field(col_count, row_count);

				pos_x++;
				col_count++;
			}

			pos_x = -7F;
			pos_y += 1F;

			col_count = 0;
			row_count++;
		}*/
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

	bool field_at_coordinates_exists(int x, int y) {

		int array_index = coordinates_to_array_index(x, y);

		return maze_field_coordinates_hash.ContainsKey(array_index);
	}

	// Update is called once per frame
	void Update () {

		if (maze_deconstruction == true) {
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
					if (maze_wave_radius_counter < 14) {
						// wave_counter increases: every time, start to make bigger "rectangle" and add objects to wave_animator
						int wave_start_x = 0 - maze_wave_radius_counter;
						int wave_start_y = 0 - maze_wave_radius_counter;
						int wave_end_x = 0 + maze_wave_radius_counter;
						int wave_end_y = 0 + maze_wave_radius_counter;

						if (wave_start_x < -8) {
							wave_start_x = -8;
						}
						if (wave_start_y < -9) {
							wave_start_y = -9;
						}
						if (wave_end_x > 8) {
							wave_end_x = 8;
						}
						if (wave_end_y > 9) {
							wave_end_y = 9;
						}

						int wave_counter_x = wave_start_x;
						int wave_counter_y = wave_start_y;

						maze_destruction_animator = new List<GameObject>();

						while(wave_counter_y <= wave_end_y) {
							while(wave_counter_x <= wave_end_x) {
								GameObject maze_field_target = find_or_create_field_at_coordinates(wave_counter_x, wave_counter_y);
								maze_destruction_animator.Add(maze_field_target);

								wave_counter_x++;
							}

							wave_counter_x = wave_start_x;
							wave_counter_y++;
						}

						maze_wave_radius_counter++;
					}

					maze_path_redraw_rate_counter = 0;
				}

				animate_maze_destruction();
			}
		}
		else {

			build_maze();

			// move player
			if (player_spheres == 0) {
				spawn_player_sphere();
			}
			else {
				player_movement();
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
		if (maze_initialized < 4) {
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
		
		// draw corners
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

			while (row_count <= highest_y) {
				while (col_count <= highest_x) {
					draw_maze_tunnel(col_count % 4, col_count, row_count, 30);
					col_count++;
				}
				
				col_count = lowest_x;
				row_count++;
			}
			maze_initialized++;
		}
	}

	bool check_note_collection() {
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

	maze_field_script get_maze_field_script(int coord_x, int coord_y) {
		GameObject next_maze_field = find_or_create_field_at_coordinates(coord_x, coord_y);
		maze_field_script next_maze_script = get_maze_field_script_from_game_object(next_maze_field);

		return next_maze_script;
	}

	void player_movement() {
		float player_z = player_sphere.transform.position.z;
		
		bool allow_movement = true;
		bool note_collected = false;
		
		if (player_sphere_moving == true) {
			// MOVE TIME
			Vector3 new_position = Vector3.Lerp(player_sphere.transform.position, player_target_position, Time.deltaTime * 10);
			player_sphere.transform.position = new_position;
			
			float distance = Vector3.Distance(new_position, player_target_position);
			
			allow_movement = false;
			
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

			if (Input.GetKey (KeyCode.RightArrow) || Input.GetAxis("Horizontal") > 0.1F) {
				if (current_maze_field.removed_right() == true) {
					player_coord_x++;
					player_sphere_moving = true;
					direction_pressed = true;
				}
			}
			else {
				if (Input.GetKey (KeyCode.DownArrow) || Input.GetAxis("Vertical") < -0.1F) {
					if (current_maze_field.removed_bottom() == true) {
						player_coord_y--;
						player_sphere_moving = true;
						direction_pressed = true;
					}
				}
				else {
					if (Input.GetKey (KeyCode.UpArrow) || Input.GetAxis("Vertical") > 0.1F) {
						if (current_maze_field.removed_top() == true) {
							player_coord_y++;
							player_sphere_moving = true;
							direction_pressed = true;
						}
					}
					else {
						if (Input.GetKey (KeyCode.LeftArrow) || Input.GetAxis("Horizontal") < -0.1F) {
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
				player_saved_path_coordinates.Add(new TakenPath { coord_x = player_coord_x, coord_y = player_coord_y });
				player_target_position = new Vector3(player_coord_x, player_coord_y, player_z);
			}
		}
	}

	void spawn_player_sphere() {
		player_sphere = (GameObject)Instantiate(player_sphere_prefab, new Vector3(0, 0, -44.3F), Quaternion.identity);
		player_sphere_moving = true;
		player_target_position = new Vector3(0F, 0.0F, -4.3F);
		player_coord_x = 0;
		player_coord_y = 0;
		player_spheres++;
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
