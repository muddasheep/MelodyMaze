using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MazeMan : MonoBehaviour {

    public GameObject maze_field;
    public GameObject maze_wall;
    public GameObject maze_note;
    public GameObject base_note_marker;

    public int maze_initialized = 0;

    public Dictionary<int, GameObject> maze_field_coordinates_hash = new Dictionary<int, GameObject>();
    public Dictionary<string, GameObject> maze_walls_coordinates_hash = new Dictionary<string, GameObject>();
    public List<GameObject> maze_notes = new List<GameObject>();
    public GameObject base_note;

    public bool maze_deconstruction = false;
    bool maze_deconstruction_initialized = false;

    public GameObject create_maze_field(int col_count, int row_count) {

        int pos_x = col_count;// - 7;
        int pos_y = row_count;// - 4;

        GameObject new_maze_field = (GameObject)Instantiate(maze_field, new Vector3(pos_x, pos_y, -4F), Quaternion.identity);
        maze_field_script new_maze_field_script = gameentity.get_maze_field_script_from_game_object(new_maze_field);
        new_maze_field_script.coord_x = col_count;
        new_maze_field_script.coord_y = row_count;
        maze_field_coordinates_hash.Add(coordinates_to_array_index(col_count, row_count), new_maze_field);

        update_maze_boundaries();

        return new_maze_field;
    }

    public GameObject find_or_create_field_at_coordinates(int x, int y) {

        int array_index = coordinates_to_array_index(x, y);

        if (maze_field_coordinates_hash.ContainsKey(array_index)) {

            return maze_field_coordinates_hash[array_index];
        }
        else {

            return create_maze_field(x, y);
        }
    }

    public void set_field_to_target_note(GameObject maze_field, maze_field_script field_script) {
        GameObject new_note = (GameObject)Instantiate(maze_note,
            new Vector3(
                maze_field.transform.position.x,
                maze_field.transform.position.y,
                maze_field.transform.position.z - 1F
            ), Quaternion.identity);

        maze_notes.Add(new_note);
        new_note.transform.parent = maze_field.transform;
        field_script.is_target_note = true;
        field_script.linked_target_note = new_note;

        maze_note_script new_note_script = gameentity.get_maze_note_script_from_game_object(new_note);
        new_note_script.coord_x = field_script.coord_x;
        new_note_script.coord_y = field_script.coord_y;
    }

    public void remove_target_note_from_field(maze_field_script field_script) {
        GameObject note = field_script.linked_target_note;

        if (note != null) {
            Destroy(field_script.linked_target_note);
            field_script.is_target_note = false;
        }
    }

    public void set_field_to_base_note(GameObject maze_field) {
        remove_base_note();

        base_note = (GameObject)Instantiate(base_note_marker,
            new Vector3(
                maze_field.transform.position.x,
                maze_field.transform.position.y,
                maze_field.transform.position.z + 0F
            ),
            Quaternion.Euler(-90F, 0, 0)
        );
        base_note.transform.parent = maze_field.transform;
        maze_field_script new_maze_field_script = gameentity.get_maze_field_script_from_game_object(maze_field);
        new_maze_field_script.is_base_note = true;
    }

    public void remove_base_note() {
        if (base_note != null) {
            int base_x = Mathf.RoundToInt(base_note.transform.position.x);
            int base_y = Mathf.RoundToInt(base_note.transform.position.y);

            if (field_at_coordinates_exists(base_x, base_y)) {
                GameObject base_note_field = find_or_create_field_at_coordinates(base_x, base_y);
                maze_field_script base_note_script = gameentity.get_maze_field_script_from_game_object(base_note_field);
                base_note_script.is_base_note = false;
            }
            Destroy(base_note);
            base_note = null;
        }
    }

    public GameObject create_maze_wall(float pos_x, float pos_y, bool stay_visible = false) {

        string array_index = wall_hash_index(pos_x, pos_y);
        GameObject new_maze_wall = (GameObject)Instantiate(maze_wall, new Vector3(pos_x, pos_y, -4F), Quaternion.identity);
        maze_walls_coordinates_hash.Add(array_index, new_maze_wall);

        // check if it's a horizontal wall
        if (Mathf.Floor(pos_y) != pos_y) {
            new_maze_wall.transform.localRotation = Quaternion.Euler(0, 0, -90F);
        }

        if (!stay_visible) {
            maze_wall_script next_maze_wall_script = gameentity.get_maze_wall_script_from_game_object(new_maze_wall);
            next_maze_wall_script.FadeOut(0F);
        }

        attach_wall_to_nearest_maze_field(new_maze_wall, pos_x, pos_y);

        if (field_at_coordinates_exists(Mathf.RoundToInt(pos_x), Mathf.RoundToInt(pos_y))) {
            GameObject maze_field = find_or_create_field_at_coordinates(Mathf.RoundToInt(pos_x), Mathf.RoundToInt(pos_y));
            new_maze_wall.transform.parent = maze_field.transform;
        }

        return new_maze_wall;
    }

    public void attach_wall_to_nearest_maze_field(GameObject wall, float x, float y) {

        int left_x = Mathf.FloorToInt(x);
        int right_x = Mathf.CeilToInt(x);
        int bottom_y = Mathf.FloorToInt(y);
        int top_y = Mathf.CeilToInt(y);

        attach_wall_to_field_at_coords(wall, left_x, bottom_y);
        attach_wall_to_field_at_coords(wall, left_x, top_y);
        attach_wall_to_field_at_coords(wall, right_x, bottom_y);
        attach_wall_to_field_at_coords(wall, right_x, top_y);
    }

    public void attach_wall_to_field_at_coords(GameObject wall, int x, int y) {
        if (field_at_coordinates_exists(x, y)) {
            GameObject maze_field = find_or_create_field_at_coordinates(x, y);
            wall.transform.parent = maze_field.transform;
        }
    }

    public GameObject find_or_create_wall_at_coordinates(float x, float y) {

        if (wall_at_coordinates_exists(x, y)) {

            return maze_walls_coordinates_hash[wall_hash_index(x, y)];
        }
        else {

            return create_maze_wall(x, y);
        }
    }

    public string wall_hash_index(float x, float y) {
        string array_index = x.ToString() + '-' + y.ToString();

        return array_index;
    }

    public bool wall_at_coordinates_exists(float x, float y) {
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

        Vector3 player_coordinates = gameentity.get_current_player_coordinates();

        int radius = 4;
        int walk_x = (int)player_coordinates.x - radius;
        int walk_y = (int)player_coordinates.x - radius;
        int max_x = (int)player_coordinates.x + radius;
        int max_y = (int)player_coordinates.y + radius;

        while (walk_y < max_y) {

            if (field_at_coordinates_exists(walk_x, walk_y)) {
                GameObject maze_field_target = find_or_create_field_at_coordinates(walk_x, walk_y);
                maze_field_script gotten_maze_script = gameentity.get_maze_field_script_from_game_object(maze_field_target);

                // delay = max difference to player_coord
                int diff_x = Mathf.Abs(walk_x - (int)player_coordinates.x);
                int diff_y = Mathf.Abs(walk_y - (int)player_coordinates.y);

                gotten_maze_script.quake(1F, delay + (Mathf.Max(diff_x + diff_y) / 5F));
            }

            walk_x++;

            if (walk_x > max_x) {
                walk_x = (int)player_coordinates.x - radius;
                walk_y++;
            }
        }
    }

    public int coordinates_to_array_index(int coord_x, int coord_y) {
        return 100 + coord_y * 100 + coord_x;
    }

    public void animate_maze_destruction() {

        if (maze_deconstruction_initialized == false) {

            List<GameObject> maze_fields_to_destroy = new List<GameObject>();

            foreach (KeyValuePair<int, GameObject> field in maze_field_coordinates_hash) {

                int field_coord_x = Mathf.RoundToInt(field.Value.transform.position.x);
                int field_coord_y = Mathf.RoundToInt(field.Value.transform.position.y);

                if (field_at_coordinates_exists(field_coord_x, field_coord_y)) {

                    maze_fields_to_destroy.Add(find_or_create_field_at_coordinates(field_coord_x, field_coord_y));
                }
            }

            foreach (KeyValuePair<string, GameObject> field in maze_walls_coordinates_hash) {
                if (wall_at_coordinates_exists(field.Value.transform.position.x, field.Value.transform.position.y)) {
                    GameObject found_wall = find_or_create_wall_at_coordinates(field.Value.transform.position.x, field.Value.transform.position.y);
                    maze_wall_script found_wall_script = gameentity.get_maze_wall_script_from_game_object(found_wall);
                    found_wall_script.FadeOut();
                }
            }

            foreach (GameObject maze_field_target in maze_fields_to_destroy) {
                Vector3 target_position = new Vector3(maze_field_target.transform.position.x, maze_field_target.transform.position.y, 10F);
                gameentity.smooth_move(
                    maze_field_target.transform.position, target_position, 1F + Random.Range(0, 4F), Random.Range(0, 1F), maze_field_target
                );

                Vector3 angles =
                    Vector3.up * (20 + Random.Range(0.0f, 70.0f)) +
                    Vector3.down * (20 + Random.Range(0.0f, 70.0f)) +
                    Vector3.left * (20 + Random.Range(0.0f, 70.0f)) +
                    Vector3.right * (20 + Random.Range(0.0f, 70.0f));

                gameentity.start_coroutine(gameentity.rotate_object(maze_field_target, angles, 5F));
                //maze_field_target.transform.Rotate(-1F, 1F, 3F, Space.World);
            }

            finish_maze_destruction(5F);

            maze_deconstruction_initialized = true;
        }
    }
	
	public void build_maze() {
		while (maze_initialized < 4) {
			GameObject last_maze_field;
			int difficulty_steps = 10;
			last_maze_field = draw_maze_tunnel(maze_initialized, 0, 0, difficulty_steps);
			maze_field_script last_maze_field_script = gameentity.get_maze_field_script_from_game_object(last_maze_field);

            set_field_to_target_note(last_maze_field, last_maze_field_script);
			
			maze_initialized++;
		}

        set_field_to_base_note(find_or_create_field_at_coordinates(0, 0));

        if (maze_initialized == 4) {
			// draw corners
			draw_maze_corners();
			
			// draw walls
			draw_maze_walls();
		}
	}

    public void clean_maze() {
        remove_base_note();

        foreach (GameObject note in maze_notes) {
            Destroy(note);
        }
        maze_notes = new List<GameObject>();

        List<EditorMan.Coords> to_delete = new List<EditorMan.Coords>();

        foreach (KeyValuePair<int, GameObject> field in maze_field_coordinates_hash) {
            to_delete.Add(new EditorMan.Coords {
                field_coord_x = Mathf.RoundToInt(field.Value.transform.position.x),
                field_coord_y = Mathf.RoundToInt(field.Value.transform.position.y)
            });
        }
        foreach (EditorMan.Coords coords in to_delete) {
            destroy_field_at_coordinates(coords.field_coord_x, coords.field_coord_y);
        }

        to_delete = new List<EditorMan.Coords>();

        foreach (KeyValuePair<string, GameObject> field in maze_walls_coordinates_hash) {
            to_delete.Add(new EditorMan.Coords {
                wall_coord_x = field.Value.transform.position.x,
                wall_coord_y = field.Value.transform.position.y
            });
        }
        foreach (EditorMan.Coords coords in to_delete) {
            destroy_wall_at_coordinates(coords.wall_coord_x, coords.wall_coord_y);
        }

        maze_field_coordinates_hash = new Dictionary<int, GameObject>();
        maze_walls_coordinates_hash = new Dictionary<string, GameObject>();

        maze_deconstruction = false;
        maze_deconstruction_initialized = false;
    }

    IEnumerator finish_maze_destruction_numerator(float delay_seconds) {
        float t = 0.0F;
        while (t <= 1.0F) {
            t += Time.deltaTime / delay_seconds;
            yield return null;
        }

        StopAllCoroutines();
        clean_maze();
        gameentity.return_to_title();
    }

    public void finish_maze_destruction(float delay_seconds) {
        gameentity.start_coroutine(finish_maze_destruction_numerator(delay_seconds));
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

	public int maze_boundary_lowest_x = 0;
	public int maze_boundary_lowest_y = 0;
	public int maze_boundary_highest_x = 0;
	public int maze_boundary_highest_y = 0;

	public void update_maze_boundaries() {
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

		maze_boundary_lowest_x  = lowest_x;
		maze_boundary_lowest_y  = lowest_y;
		maze_boundary_highest_x = highest_x;
		maze_boundary_highest_y = highest_y;
	}

	public void draw_maze_corners() {
		if (maze_initialized == 4) {

			update_maze_boundaries();

			int lowest_x = maze_boundary_lowest_x;
			int lowest_y = maze_boundary_lowest_y;
			int highest_x = maze_boundary_highest_x;
			int highest_y = maze_boundary_highest_y;
			
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
            gameentity.summon_base_note(new Vector3(
                last_created_tunnel.transform.position.x,
                last_created_tunnel.transform.position.y,
                last_created_tunnel.transform.position.z - 1F
            ));
			
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
            update_maze_boundaries();
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
}
