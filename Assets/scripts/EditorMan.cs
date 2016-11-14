using UnityEngine;
using System.Collections;

public class EditorMan : MonoBehaviour {

	GameEntity gameentity;
	MazeMan mazeman;

	int pos_x;
	int pos_y;

	GameObject editing_maze_field;

	// Use this for initialization
	void Start () {
		gameentity = GetComponent<GameEntity>();
		mazeman    = GetComponent<MazeMan>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void prepare_editor() {
		gameentity.spawn_player_sphere(0);
	}

	public void editor_movement() {
		if (!gameentity.player_sphere) {
			return;
		}

		if (gameentity.player_pressed_action()) {
			editor_action();
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

		Vector3 new_position = new Vector3(pos_x, pos_y, pos_z);

		gameentity.player_sphere.transform.position = new_position;
	}

	void editor_action() {

		// if no field at current pos, create field
		if (!mazeman.field_at_coordinates_exists(pos_x, pos_y)) {
			mazeman.find_or_create_field_at_coordinates(pos_x, pos_y);

			// add 4 walls
			mazeman.find_or_create_wall_at_coordinates(pos_x + 0.5F, pos_y);
			mazeman.find_or_create_wall_at_coordinates(pos_x - 0.5F, pos_y);
			mazeman.find_or_create_wall_at_coordinates(pos_x, pos_y + 0.5F);
			mazeman.find_or_create_wall_at_coordinates(pos_x, pos_y - 0.5F);

			return;
		}

		// if field at current pos, go into edit mode (walls + notes)
		editing_maze_field = mazeman.find_or_create_field_at_coordinates(pos_x, pos_y);
		maze_field_script next_maze_field = gameentity.get_maze_field_script(pos_x, pos_y);
		mazeman.highlight_walls_around_maze_field(next_maze_field, true);
	}
}
