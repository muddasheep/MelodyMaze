using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameEntity : MonoBehaviour {
	public float start_time = 0;
	public float current_time = 0;

	public GameObject player_sphere_prefab;
	public GameObject base_note;
	public Camera maze_cam;

    Vector3 initial_cam_position;
    public GameObject camera_target;

	public bool game_running = false;
    public bool editor_running = false;
    public int current_level = 0; // -1 = random

    public List<string> instrument_names = new List<string> {
        "piano", "guitar"
    };

    public List<string> notes = new List<string> {
        "c", "cis", "d", "dis", "e", "f", "fis", "g", "gis", "a", "b", "h",
        "c2", "cis2", "d2", "dis2", "e2", "f2", "fis2", "g2", "gis2", "a2", "b2", "h2",
        "c3"
    };

    InputMan inputman;
	MazeMan mazeman;
	MenuMan menuman;
	EditorMan editorman;
    GameMaster gamemaster;
    SoundMan soundman;
    StorageMan storageman;

	// Use this for initialization
	void Start () {

        inputman   = GetComponent<InputMan>();
        mazeman    = GetComponent<MazeMan>();
		menuman    = GetComponent<MenuMan>();
        editorman  = GetComponent<EditorMan>();
        soundman   = GetComponent<SoundMan>();
        storageman = GetComponent<StorageMan>();

        initial_cam_position = gameObject.transform.position;

        start_time = Time.time;
	}

	// Update is called once per frame
	void FixedUpdate () {
		current_time = Time.time - start_time;

        if (game_running) {

            if (mazeman.maze_deconstruction == true) {
                gamemaster.deconstruct_maze();
            }
            else {

                if (!menuman.displaying_menu) {
                    gamemaster.game_update();
                }

                if (menuman.displaying_menu == false && inputman.player_pressed_escape()) {
                    menuman.display_menu("pause");
                    menuman.displaying_menu = true;
                }
            }
        }
        else if (editor_running) {

            if (!menuman.displaying_menu) {
                editorman.editor_movement();
            }

            if (menuman.displaying_menu == false && inputman.player_pressed_escape()) {
                menuman.display_menu("pause");
                menuman.displaying_menu = true;
            }
        }
        else {
            if (menuman.displaying_menu == false) {
                show_title_screen();
                menuman.display_menu("start");
                menuman.displaying_menu = true;
            }

            int title_coord_x = Mathf.RoundToInt(camera_target.transform.position.x);
            int title_coord_y = Mathf.RoundToInt(camera_target.transform.position.y);

            if (mazeman.field_at_coordinates_exists(title_coord_x - 1, title_coord_y)) {
                mazeman.highlight_walls_around_maze_field(get_maze_field_script(title_coord_x - 1 , title_coord_y), true);
            }

            if (mazeman.field_at_coordinates_exists(title_coord_x, title_coord_y)) {
                mazeman.highlight_walls_around_maze_field(get_maze_field_script(title_coord_x, title_coord_y), false);
            }
        }
	}

    public void destroy_title_screen() {
        if (camera_target) {
            Destroy(camera_target);
        }
        gamemaster = null;

        mazeman.clean_maze();

        StopAllCoroutines();
    }

    public void show_title_screen() {
        int count = -5;

        List<string> mm_title = new List<string> { "M", "E", "L", "O", "D", "Y", " ", "M", "A", "Z", "E" };

        while (count <= 5) {
            menuman.create_text_at_coordinates(count, 2F, -4.6F, mm_title[ count + 5 ], mazeman.create_maze_field(count, 2));
            mazeman.create_maze_wall(count, 2.5F, false);
            mazeman.create_maze_wall(count, 1.5F, false);
            count++;
        }

        camera_target = summon_player_sphere();
        camera_target.transform.position = new Vector3(-9F, 2F, -4.6F);
        smooth_move(camera_target.transform.position, new Vector3(9F, 2F, -4.6F), 5F, 0.5F, camera_target);
    }

    public void start_game() {
        destroy_title_screen();
        current_level = 1;

        start_current_level();

        game_running = true;
    }

    public void start_current_level() {
        gamemaster = new GameMaster { inputman = inputman, mazeman = mazeman, gameentity = this };

        StorageMan.Maze maze = storageman.load_from_json(current_level);
        mazeman.build_maze_from_maze_class(maze);
        mazeman.maze_initialized = 4;

        GameObject last_maze_field = mazeman.maze_field_coordinates_hash[mazeman.coordinates_to_array_index(
            mazeman.maze_boundary_highest_x, mazeman.maze_boundary_highest_y
        )];

        summon_base_note(new Vector3(
            last_maze_field.transform.position.x,
            last_maze_field.transform.position.y,
            last_maze_field.transform.position.z - 1F
        ));
    }

    public void start_random_game() {
        destroy_title_screen();
        gamemaster = new GameMaster { inputman = inputman, mazeman = mazeman, gameentity = this };
        mazeman.build_random_maze();
		game_running = true;
        current_level = -1;
    }
	
	public void start_editor() {
        destroy_title_screen();
        editorman.prepare_editor();
		editor_running = true;
	}

    public void end_level() {
        if (current_level == -1) {
            return_to_title();

            return;
        }

        StopAllCoroutines();

        mazeman.maze_initialized = 0;
        destroy_base_note();
        gamemaster = null;
        mazeman.clean_maze();
        reset_camera();
        turn_off_the_fog_machine();

        current_level++;

        while (current_level <= 99) {
            if (editorman.level_file_exists(current_level)) {
                start_current_level();
                return;
            }

            current_level++;
        }

        return_to_title();
    }

    public void return_to_title() {
        game_running = false;
        editor_running = false;
        menuman.displaying_menu = false;
        mazeman.maze_initialized = 0;

        destroy_base_note();

        if (camera_target) {
            Destroy(camera_target);
        }

        gamemaster = null;
        mazeman.clean_maze();
        reset_camera();
        turn_off_the_fog_machine();

        StopAllCoroutines();
    }

    public void summon_base_note(Vector3 new_position) {
        gamemaster.base_note_ingame = (GameObject)Instantiate(
            base_note, new_position, Quaternion.identity
        );
        gamemaster.base_note_ingame.transform.Rotate(270F, 0F, 0F);
        gamemaster.player_sphere = gamemaster.base_note_ingame;
        gamemaster.player_coord_x = (int)new_position.x;
        gamemaster.player_coord_y = (int)new_position.y;
        camera_target = gamemaster.base_note_ingame;
        adjust_camera();
    }

    public void destroy_base_note() {
        if (gamemaster != null && gamemaster.base_note_ingame != null) {
            Destroy(gamemaster.base_note_ingame);
            gamemaster.base_note_ingame = null;
        }
    }

    public GameObject summon_player_sphere() {
        GameObject new_sphere = (GameObject)Instantiate(
            player_sphere_prefab,
            new Vector3(0, 0, -44.3F),
            Quaternion.identity
        );
        return new_sphere;
    }

    public Vector3 get_current_player_coordinates() {

        return camera_target.transform.position;
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

    public IEnumerator rotate_object(GameObject target_object, Vector3 byAngles, float inTime) {
        var fromAngle = target_object.transform.rotation;
        var toAngle = Quaternion.Euler(target_object.transform.eulerAngles + byAngles);
        for (var t = 0f; t < 1; t += Time.deltaTime / inTime) {
            target_object.transform.rotation = Quaternion.Lerp(fromAngle, toAngle, t);
            yield return null;
        }
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
			if (moving_object != null) {
				moving_object.transform.position = Vector3.Lerp(startpos, endpos, Mathf.SmoothStep(0.0F, 1.0F, t));
			}

			yield return null;
		}
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

    public void play_maze_field_note(maze_field_script given_maze_field) {

        soundman.play_instrument_sound(instrument_names[given_maze_field.instrument], given_maze_field.note);
    }

    public void smooth_adjust_camera(float x, float y, float z = -20.8F, float seconds = 1F, float delay_seconds = 0F) {
        smooth_move(
            gameObject.transform.position,
            new Vector3(x, y, z), seconds, delay_seconds, gameObject
        );
    }

    public void reset_camera() {

        gameObject.transform.position = initial_cam_position;
    }

    public void adjust_camera() {
        if (!camera_target) {
            return;
        }
        gameObject.transform.position = new Vector3(camera_target.transform.position.x, camera_target.transform.position.y, -20.8F);
    }

    public void center_camera_within_maze_bounds() {

        if (!camera_target) {
            return;
        }

        int lowest_x = mazeman.maze_boundary_lowest_x - 5;
        int lowest_y = mazeman.maze_boundary_lowest_y - 5;
        int highest_x = mazeman.maze_boundary_highest_x + 5;
        int highest_y = mazeman.maze_boundary_highest_y + 5;

        float cam_x = camera_target.transform.position.x;
        float cam_y = camera_target.transform.position.y;

        if (cam_x < lowest_x + 6) {
            cam_x = lowest_x + 6;
        }
        if (cam_x > highest_x - 6) {
            cam_x = highest_x - 6;
        }
        if (cam_y < lowest_y + 4) {
            cam_y = lowest_y + 4;
        }
        if (cam_y > highest_y - 4) {
            cam_y = highest_y - 4;
        }

        Vector3 new_cam_pos = new Vector3(cam_x, cam_y, gameObject.transform.position.z);

        gameObject.transform.position = new_cam_pos;
    }

    public void start_coroutine(IEnumerator numerator) {
        StartCoroutine(numerator);
    }

    public void turn_on_the_fog_machine() {
        RenderSettings.fog = true;
        start_coroutine(fog_machine_intensifies(8F));
    }

    IEnumerator fog_machine_intensifies(float seconds) {
        float t = 0.0F;
        while (t <= 1.0F) {
            t += Time.deltaTime / seconds;
            RenderSettings.fogDensity = 0.12F * t;
            yield return null;
        }
    }

    public void turn_off_the_fog_machine() {
        RenderSettings.fog = false;
    }
}
