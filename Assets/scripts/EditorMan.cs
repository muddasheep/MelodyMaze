using UnityEngine;
using System.Collections;

public class EditorMan : MonoBehaviour {

	GameEntity gameentity;
	MazeMan mazeman;

	int pos_x;
	int pos_y;
	int previous_pos_x;
	int previous_pos_y;

	GameObject editing_maze_field;
	maze_field_script editing_maze_field_script;

	// Use this for initialization
	void Start () {
		gameentity = GetComponent<GameEntity>();
		mazeman    = GetComponent<MazeMan>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		gameentity.detectPressedKeyOrButton();
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
			editor_action_2();
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
		// if no field at current pos, create field
		if (!mazeman.field_at_coordinates_exists(pos_x, pos_y)) {
			mazeman.find_or_create_field_at_coordinates(pos_x, pos_y);

			// add 4 walls
			create_editor_wall_at_coordinates(pos_x + 0.5F, pos_y);
			create_editor_wall_at_coordinates(pos_x - 0.5F, pos_y);
			create_editor_wall_at_coordinates(pos_x, pos_y + 0.5F);
			create_editor_wall_at_coordinates(pos_x, pos_y - 0.5F);

			return;
		}

		// if field at current pos, go into edit mode (walls + notes)
		editing_maze_field = mazeman.find_or_create_field_at_coordinates(pos_x, pos_y);
		editing_maze_field_script = gameentity.get_maze_field_script(pos_x, pos_y);
	}

	void editor_action_2() {
		// if field at current pos, destroy field
		if (mazeman.field_at_coordinates_exists(pos_x, pos_y)) {
			mazeman.destroy_field_at_coordinates(pos_x, pos_y);
			editing_maze_field = null;
			editing_maze_field_script = null;

			// add walls if adjecent fields exist, remove walls if fields don't exist
			if(mazeman.field_at_coordinates_exists(pos_x + 1, pos_y)) {
				create_editor_wall_at_coordinates(pos_x + 0.5F, pos_y);
			}
			else {
				mazeman.destroy_wall_at_coordinates(pos_x + 0.5F, pos_y);
			}

			if(mazeman.field_at_coordinates_exists(pos_x - 1, pos_y)) {
				create_editor_wall_at_coordinates(pos_x - 0.5F, pos_y);
			}
			else {
				mazeman.destroy_wall_at_coordinates(pos_x - 0.5F, pos_y);
			}

			if(mazeman.field_at_coordinates_exists(pos_x, pos_y + 1)) {
				create_editor_wall_at_coordinates(pos_x, pos_y + 0.5F);
			}
			else {
				mazeman.destroy_wall_at_coordinates(pos_x, pos_y + 0.5F);
			}

			if(mazeman.field_at_coordinates_exists(pos_x, pos_y - 1)) {
				create_editor_wall_at_coordinates(pos_x, pos_y - 0.5F);
			}
			else {
				mazeman.destroy_wall_at_coordinates(pos_x, pos_y - 0.5F);
			}

			return;
		}
	}

	void create_editor_wall_at_coordinates(float x, float y) {
		GameObject new_wall;
		maze_wall_script new_wall_script;

		new_wall = mazeman.find_or_create_wall_at_coordinates(x, y);
		new_wall_script = gameentity.get_maze_wall_script_from_game_object(new_wall);
		new_wall_script.FadeIn();
	}
}
