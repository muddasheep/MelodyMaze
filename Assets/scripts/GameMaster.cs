using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameMaster : MonoBehaviour {

    public GameObject base_note_ingame;
    public GameObject player_sphere;

    public GameEntity gameentity { get; set; }
    public InputMan inputman { get; set; }
    public MazeMan mazeman { get; set; }

    int collected_notes = 0;
    int player_spheres = 0;
    public int player_coord_x = 0;
    public int player_coord_y = 0;
    float base_note_speed_x = 0F;
    float base_note_speed_y = 0F;

    Vector3 player_target_position;

    public bool base_note_reached_center = false;
    bool player_sphere_moving = false;
    public bool player_can_move = true;

    public class TakenPath {
        public int coord_x { get; set; }
        public int coord_y { get; set; }
        public int taken_direction { get; set; }
    }

    public class TakenPaths {
        public List<TakenPath> taken_path { get; set; }
        public int redrawn_counter = 0;
    }

    public List<TakenPath> player_saved_path_coordinates = new List<TakenPath>();
    public List<TakenPaths> player_saved_paths = new List<TakenPaths>();

    public void game_update() {

        // move player
        if (player_spheres == 0 && base_note_reached_center == true && inputman.player_pressed_action_once()) {
            base_note_script next_base_note_script = gameentity.get_base_note_script_from_game_object(base_note_ingame);
            next_base_note_script.send_next_note_flying();

            spawn_player_sphere(0.3F);
            player_can_move = true;
        }
        else {
            check_base_note_reach_center();
            player_movement();
        }
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
                if (inputman.player_pressed_right() || inputman.player_pressed_left()
                    || inputman.player_pressed_down() || inputman.player_pressed_up()) {

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
            gameentity.adjust_camera();

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
            maze_field_script current_maze_field = gameentity.get_maze_field_script(player_coord_x, player_coord_y);
            bool direction_pressed = false;

            if (inputman.player_pressed_right()) {
                if (current_maze_field.removed_right() == true) {
                    player_coord_x++;
                    player_sphere_moving = true;
                    direction_pressed = true;
                }
            }
            if (inputman.player_pressed_down() && !player_sphere_moving) {
                if (current_maze_field.removed_bottom() == true) {
                    player_coord_y--;
                    player_sphere_moving = true;
                    direction_pressed = true;
                }
            }
            if (inputman.player_pressed_up() && !player_sphere_moving) {
                if (current_maze_field.removed_top() == true) {
                    player_coord_y++;
                    player_sphere_moving = true;
                    direction_pressed = true;
                }
            }
            if (inputman.player_pressed_left() && !player_sphere_moving) {
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

                maze_field_script next_maze_field = gameentity.get_maze_field_script(player_coord_x, player_coord_y);
                mazeman.highlight_walls_around_maze_field(next_maze_field, false);

                gameentity.play_maze_field_note(next_maze_field);
            }
        }
    }

    IEnumerator spawn_player_sphere_routine(float delay_seconds) {
        float t = 0.0F;
        while (t <= 1.0F) {
            t += Time.deltaTime / delay_seconds;
            yield return null;
        }

        player_sphere = gameentity.summon_player_sphere();
        gameentity.camera_target = player_sphere;

        player_sphere_moving = true;
        player_target_position = new Vector3(0F, 0.0F, -4.3F);
        player_coord_x = 0;
        player_coord_y = 0;
    }

    public void spawn_player_sphere(float delay_seconds) {
        gameentity.start_coroutine(spawn_player_sphere_routine(delay_seconds));
        player_spheres++;
    }

    void check_base_note_reach_center() {
        if (!player_sphere) {
            return;
        }

        if (!mazeman.field_at_coordinates_exists(player_coord_x, player_coord_y)) {
            return;
        }

        GameObject hover_field = mazeman.find_or_create_field_at_coordinates(player_coord_x, player_coord_y);
        maze_field_script hover_field_script = gameentity.get_maze_field_script_from_game_object(hover_field);

        if (inputman.player_pressed_action_once() &&
            hover_field_script.is_base_note &&
            player_sphere_moving == false && base_note_reached_center == false) {

            base_note_reached_center = true;
            mazeman.quake_from_current_position(0.8F);
            base_note_script next_base_note_script = gameentity.get_base_note_script_from_game_object(base_note_ingame);
            next_base_note_script.set_up_base_camp();

            gameentity.smooth_move(player_sphere.transform.position, new Vector3(0F, 0F, -4.7F), 0.5F, 0F, player_sphere);
            gameentity.smooth_adjust_camera(hover_field.transform.position.x, hover_field.transform.position.y);
        }
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

        if (inputman.player_pressed_up()) {
            base_note_speed_y += 0.001F;
            pressed_y = true;
        }
        if (inputman.player_pressed_down()) {
            base_note_speed_y -= 0.001F;
            pressed_y = true;
        }
        if (inputman.player_pressed_left()) {
            base_note_speed_x -= 0.001F;
            pressed_x = true;
        }
        if (inputman.player_pressed_right()) {
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
        player_z = 0 - 5.0F - (0.5F * Mathf.Sin(gameentity.current_time));

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

        gameentity.adjust_camera();
    }

    bool check_note_collection() {
        if (base_note_reached_center == false) {
            return false;
        }

        maze_field_script current_maze_field = gameentity.get_maze_field_script(player_coord_x, player_coord_y);
        int check_coord_x = current_maze_field.coord_x;
        int check_coord_y = current_maze_field.coord_y;

        foreach (GameObject note in mazeman.maze_notes) {
            maze_note_script note_script = gameentity.get_maze_note_script_from_game_object(note);

            if (note_script.coord_x == check_coord_x && note_script.coord_y == check_coord_y && note_script.collected == false) {
                note_script.collected = true;
                collected_notes++;

                mazeman.highlight_walls_around_maze_field(current_maze_field, true);

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
                player_saved_paths.Add(new TakenPaths {
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
            t += Time.deltaTime / delay_seconds;
            yield return null;
        }

        player_spheres--;
        player_sphere = base_note_ingame;
        gameentity.camera_target = base_note_ingame;
        gameentity.adjust_camera();
        player_sphere = null;
    }

    void finish_note_collection() {
        gameentity.smooth_adjust_camera(base_note_ingame.transform.position.x, base_note_ingame.transform.position.y);
        gameentity.start_coroutine(finish_note_collection_delay(1F));
    }

    float check_base_note_horizontal_limit(float x) {

        if (x < mazeman.maze_boundary_lowest_x) {
            return mazeman.maze_boundary_lowest_x;
        }

        if (x > mazeman.maze_boundary_highest_x) {
            return mazeman.maze_boundary_highest_x;
        }

        return x;
    }

    float check_base_note_vertical_limit(float y) {

        if (y > mazeman.maze_boundary_highest_y) {
            return mazeman.maze_boundary_highest_y;
        }

        if (y < mazeman.maze_boundary_lowest_y) {
            return mazeman.maze_boundary_lowest_y;
        }

        return y;
    }

    int maze_path_redraw_rate_counter = 0;
    bool maze_deconstruction_initialized = false;
    public void deconstruct_maze() {
        if (maze_deconstruction_initialized == false) {

            gameentity.smooth_adjust_camera(base_note_ingame.transform.position.x, base_note_ingame.transform.position.y, -25F);
            gameentity.start_coroutine(redraw_found_melody_iterator(1.5F));

            maze_deconstruction_initialized = true;
        }
    }

    IEnumerator redraw_found_melody_iterator(float delay_seconds) {
        float t = 0.0F;
        while (t <= 1.0F) {
            t += Time.deltaTime / delay_seconds;
            yield return null;
        }

        while (player_saved_paths.Count > 0) {

            if (maze_path_redraw_rate_counter < 2) {
                maze_path_redraw_rate_counter++;
            }
            else {
                // redraw paths
                List<TakenPaths> player_saved_paths_altered = new List<TakenPaths>();

                foreach (TakenPaths path in player_saved_paths) {
                    if (path.redrawn_counter < path.taken_path.Count) {
                        TakenPath current_path = path.taken_path[path.redrawn_counter];

                        GameObject redrawing_maze_field = mazeman.find_or_create_field_at_coordinates(
                            current_path.coord_x, current_path.coord_y
                        );

                        GameObject redrawn_field = (GameObject)Instantiate(
                            gameentity.redrawing_field,
                            new Vector3(
                                redrawing_maze_field.transform.position.x,
                                redrawing_maze_field.transform.position.y,
                                redrawing_maze_field.transform.position.z - 1F
                            ),
                            Quaternion.identity
                        );
                        redrawn_field.transform.parent = redrawing_maze_field.transform;

                        path.redrawn_counter++;
                        player_saved_paths_altered.Add(path);
                    }
                }
                player_saved_paths = player_saved_paths_altered;
                maze_path_redraw_rate_counter = 0;
            }

            yield return null;
        }

        if (player_saved_paths.Count == 0) {
            if (maze_path_redraw_rate_counter < 4) {
                maze_path_redraw_rate_counter++;
            }
            else {

                maze_path_redraw_rate_counter = 0;
            }

            mazeman.animate_maze_destruction();
            gameentity.turn_on_the_fog_machine();
            base_note_script next_base_note_script = gameentity.get_base_note_script_from_game_object(base_note_ingame);
            next_base_note_script.unset_base_camp();
            gameentity.smooth_move(
                base_note_ingame.transform.position,
                new Vector3(base_note_ingame.transform.position.x, base_note_ingame.transform.position.y, 30F),
                3F, 2F, base_note_ingame
            );
        }
    }
}
