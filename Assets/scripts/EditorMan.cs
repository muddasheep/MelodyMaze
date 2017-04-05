using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EditorMan : MonoBehaviour {

    InputMan inputman;
    GameEntity gameentity;
    MazeMan mazeman;
    MenuMan menuman;
    SoundMan soundman;
    StorageMan storageman;

    int pos_x;
    int pos_y;
    int previous_pos_x;
    int previous_pos_y;

    GameObject editing_maze_field;
    maze_field_script editing_maze_field_script;

    public GameObject piano_key_white_prototype;
    public GameObject piano_key_black_prototype;
    public GameObject keyboard_button;
    public GameObject instrument_display;
    public GameObject piano_key_highlighter_prototype;
    GameObject piano_key_highlighter;
    public int init_settings_x_counter;
    int settings_pos_x = 0;
    int settings_pos_y = 0; // 0 == buttons, 1 == piano

    public int current_level = 1;

    public class Coords {
        public float wall_coord_x { get; set; }
        public float wall_coord_y { get; set; }
        public int field_coord_x { get; set; }
        public int field_coord_y { get; set; }
    }

    void Start() {
        gameentity = GetComponent<GameEntity>();
        inputman = GetComponent<InputMan>();
        mazeman = GetComponent<MazeMan>();
        menuman = GetComponent<MenuMan>();
        soundman = GetComponent<SoundMan>();
        storageman = GetComponent<StorageMan>();
    }

    void FixedUpdate() {
        if (gameentity.editor_running) {
            if (!menuman.displaying_menu) {
                inputman.detectPressedKeyOrButton();

                gameentity.center_camera_within_maze_bounds();
            }
            else {
                hide_field_settings();
            }
        }
    }

    public void prepare_editor() {
        gameentity.camera_target = gameentity.summon_player_sphere();
    }

    public void editor_movement() {
        if (!gameentity.camera_target) {
            return;
        }

        if (inputman.player_pressed_action_once()) {
            editor_action();
            return;
        }

        if (inputman.player_pressed_action2_once()) {
            editor_cancel();
            return;
        }

        if (inputman.player_pressed_action3_once()) {
            editor_settings();
            return;
        }

        if (editing_maze_field != null) {
            move_settings_cursor();
            return;
        }

        move_field_cursor();
    }

    void move_field_cursor() {
        float pos_z = -4.4F;

        bool moved = false;

        if (inputman.player_pressed_up_once()) {
            pos_y++;
            moved = true;
        }
        if (inputman.player_pressed_down_once()) {
            pos_y--;
            moved = true;
        }
        if (inputman.player_pressed_right_once()) {
            pos_x++;
            moved = true;
        }
        if (inputman.player_pressed_left_once()) {
            pos_x--;
            moved = true;
        }

        check_paint_mode();

        Vector3 new_position = new Vector3(pos_x, pos_y, pos_z);

        gameentity.camera_target.transform.position = new_position;

        previous_pos_x = pos_x;
        previous_pos_y = pos_y;

        if (moved && mazeman.field_at_coordinates_exists(pos_x, pos_y)) {
            maze_field_script hover_maze_field_script = gameentity.get_maze_field_script(pos_x, pos_y);

            int pitch_amount = 0;
            if (hover_maze_field_script.is_base_note) {
                pitch_amount = -12;
            }
            soundman.play_instrument_sound(
                gameentity.instrument_names[hover_maze_field_script.instrument],
                hover_maze_field_script.note,
                pitch_amount
            );
        }
    }

    void move_settings_cursor() {

        if (inputman.player_pressed_up_once()) {
            settings_pos_y++;
        }
        if (inputman.player_pressed_down_once()) {
            settings_pos_y--;
        }
        if (inputman.player_pressed_right_once()) {
            settings_pos_x++;
        }
        if (inputman.player_pressed_left_once()) {
            settings_pos_x--;
        }

        if (settings_pos_y > 1) {
            settings_pos_y = 0;
        }
        if (settings_pos_y < 0) {
            settings_pos_y = 1;
        }

        if (settings_pos_y == 0) { // keyboard buttons
            if (settings_pos_x >= current_keyboard_buttons.buttons.Count) {
                settings_pos_x = 0;
            }
            if (settings_pos_x < 0) {
                settings_pos_x = current_keyboard_buttons.buttons.Count - 1;
            }

            menuman.highlighted_menu_item = current_keyboard_buttons.buttons[settings_pos_x].button_script.gameObject;
        }

        if (settings_pos_y == 1) { // piano notes
            if (settings_pos_x >= current_piano.notes.Count) {
                settings_pos_x = 0;
            }
            if (settings_pos_x < 0) {
                settings_pos_x = current_piano.notes.Count - 1;
            }

            menuman.highlighted_menu_item = current_piano.notes[settings_pos_x].piano_key;
        }

        menuman.show_highlighted_menu_item();
    }

    void check_paint_mode() {
        // paint mode! if player kept pressing action, destroy wall between last and this one and call editor_action()
        if (inputman.player_action_button_down && (pos_x != previous_pos_x || pos_y != previous_pos_y)) {
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

                if (mazeman.field_at_coordinates_exists(coords.field_coord_x, coords.field_coord_y)) {
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
            soundman.play_instrument_sound(
                gameentity.instrument_names[editing_maze_field_script.instrument],
                editing_maze_field_script.note
            );
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

    public class KeyboardButtons {
        public List<KeyboardButton> buttons = new List<KeyboardButton>();
    }

    public class KeyboardButton {
        public keyboard_button_script button_script { get; set; }
        public string text { get; set; }
    }

    Piano current_piano;
    KeyboardButtons current_keyboard_buttons;
    GameObject current_instrument_display;
    instrument_display_script current_instrument_display_script;

    void show_field_settings(float x, float y) {
        hide_field_settings();

        editing_maze_field = mazeman.find_or_create_field_at_coordinates(Mathf.RoundToInt(x), Mathf.RoundToInt(y));
        editing_maze_field_script = gameentity.get_maze_field_script(Mathf.RoundToInt(x), Mathf.RoundToInt(y));

        build_piano(x, y);

        show_buttons(x, y);

        show_instrument_display(x, y);
    }

    void show_buttons(float x, float y) {
        // build buttons
        current_keyboard_buttons = new KeyboardButtons();

        float button_x = x - 4.1F;

        // button for base note
        GameObject new_button = (GameObject)Instantiate(keyboard_button,
            new Vector3(button_x, y + 2.5F, -5F), Quaternion.identity
        );
        current_keyboard_buttons.buttons.Add(new KeyboardButton {
            button_script = new_button.GetComponent<keyboard_button_script>(),
            text = "Base Note"
        });
        if (editing_maze_field_script.is_base_note) {
            current_keyboard_buttons.buttons[0].button_script.turn_on();
        }

        button_x += 1.2F;

        // button for target note
        new_button = (GameObject)Instantiate(keyboard_button,
            new Vector3(button_x, y + 2.5F, -5F), Quaternion.identity
        );
        current_keyboard_buttons.buttons.Add(new KeyboardButton {
            button_script = new_button.GetComponent<keyboard_button_script>(),
            text = "Target Note"
        });
        if (editing_maze_field_script.is_target_note) {
            current_keyboard_buttons.buttons[1].button_script.turn_on();
        }

        button_x += 5.4F;

        // button for instrument
        new_button = (GameObject)Instantiate(keyboard_button,
            new Vector3(button_x, y + 2.5F, -5F), Quaternion.identity
        );
        current_keyboard_buttons.buttons.Add(new KeyboardButton {
            button_script = new_button.GetComponent<keyboard_button_script>(),
            text = "Instrument"
        });

        button_x += 1.2F;

        // placeholder
        new_button = (GameObject)Instantiate(keyboard_button,
            new Vector3(button_x, y + 2.5F, -5F), Quaternion.identity
        );
        current_keyboard_buttons.buttons.Add(new KeyboardButton {
            button_script = new_button.GetComponent<keyboard_button_script>(),
            text = "Hog"
        });

        // set text
        foreach (KeyboardButton button in current_keyboard_buttons.buttons) {
            button.button_script.button_text.text = button.text;
        }
    }

    void show_instrument_display(float x, float y) {

        current_instrument_display = (GameObject)Instantiate(instrument_display,
            new Vector3(x - 0.2F, y + 2.65F, -5F), Quaternion.identity
        );
        current_instrument_display_script = (instrument_display_script)current_instrument_display.GetComponent(typeof(instrument_display_script));
        current_instrument_display_script.display_text.text = gameentity.instrument_names[editing_maze_field_script.instrument];
    }

    void build_piano(float x, float y) {
        // build piano
        current_piano = new Piano();

        float piano_white_x = x - 5F;
        float piano_white_y = y;
        float piano_white_z = -5F;
        float piano_pos_x = piano_white_x;
        float piano_pos_y = piano_white_y;
        float piano_pos_z = piano_white_z;
        init_settings_x_counter = 0;

        foreach (string note in gameentity.notes) {

            GameObject key_prototype = piano_key_white_prototype;

            bool white = true;

            if (note.Contains("b") || note.Contains("is")) {
                key_prototype = piano_key_black_prototype;
                white = false;
            }
            else {
                piano_white_x += 0.6F;
            }

            float delay = init_settings_x_counter * 0.01F;

            if (white) {
                piano_pos_x = piano_white_x;
                piano_pos_y = piano_white_y;
                piano_pos_z = piano_white_z;
            }
            else {
                piano_pos_x = piano_pos_x + 0.30F;
                piano_pos_y = piano_white_y + 0.74F;
                piano_pos_z = piano_white_z - 0.1F;
                delay += 0.1F;
            }

            GameObject new_piano_key = (GameObject)Instantiate(key_prototype,
                new Vector3(piano_pos_x, piano_pos_y + 8F, piano_pos_z - 15F), Quaternion.identity
            );

            current_piano.notes.Add(new PianoNote {
                note = note,
                piano_key = new_piano_key,
                active = false
            });

            gameentity.smooth_move(new_piano_key.transform.position,
                new Vector3(piano_pos_x, piano_pos_y, piano_pos_z), 0.3F, delay, new_piano_key
            );

            if (note == editing_maze_field_script.note) {
                settings_pos_x = init_settings_x_counter;
                highlight_piano_key(settings_pos_x);
            }

            init_settings_x_counter++;
        }
    }

    void delight_piano_keys() {
        if (piano_key_highlighter != null) {
            Destroy(piano_key_highlighter);
        }
    }

    void highlight_piano_key(int x) {

        if (piano_key_highlighter == null) {
            piano_key_highlighter = (GameObject)Instantiate(piano_key_highlighter_prototype,
                new Vector3(0, 0, 0), Quaternion.identity);
        }

        if (settings_pos_x >= current_piano.notes.Count) {
            return;
        }

        piano_key_highlighter.transform.parent = null;

        GameObject piano_key = current_piano.notes[settings_pos_x].piano_key;
        piano_key_highlighter.transform.position = new Vector3(
            piano_key.transform.position.x,
            piano_key.transform.position.y,
            piano_key.transform.position.z - 0.1F
        );

        piano_key_highlighter.transform.localScale = new Vector3(
            piano_key.transform.localScale.x,
            piano_key.transform.localScale.y,
            piano_key_highlighter.transform.localScale.z
        );

        piano_key_highlighter.transform.parent = piano_key.transform;
    }

    void choose_field_settings() {

        if (settings_pos_y == 0) { // keyboard buttons
            // toggle active
            current_keyboard_buttons.buttons[settings_pos_x].button_script.toggle();

            if (settings_pos_x == 0) { // base note
                editing_maze_field_script.is_base_note = current_keyboard_buttons.buttons[settings_pos_x].button_script.active;

                if (editing_maze_field_script.is_base_note) {
                    mazeman.set_field_to_base_note(editing_maze_field);
                }
                else {
                    mazeman.remove_base_note();
                }
            }

            if (settings_pos_x == 1) { // target note
                editing_maze_field_script.is_target_note = current_keyboard_buttons.buttons[settings_pos_x].button_script.active;

                if (editing_maze_field_script.is_target_note) {
                    mazeman.set_field_to_target_note(editing_maze_field, editing_maze_field_script);
                }
                else {
                    mazeman.remove_target_note_from_field(editing_maze_field_script);
                }
            }

            if (settings_pos_x == 2) { // instrument
                editing_maze_field_script.toggle_instrument();

                current_keyboard_buttons.buttons[settings_pos_x].button_script.turn_off();
                current_instrument_display_script.display_text.text = gameentity.instrument_names[editing_maze_field_script.instrument];
            }
        }

        if (settings_pos_y == 1) { // piano keys
            if (editing_maze_field_script.note == current_piano.notes[settings_pos_x].note) {
                editing_maze_field_script.remove_note();
                delight_piano_keys();
            }
            else {
                editing_maze_field_script.save_note(current_piano.notes[settings_pos_x].note);

                soundman.play_instrument_sound(
                    gameentity.instrument_names[editing_maze_field_script.instrument],
                    current_piano.notes[settings_pos_x].note
                );

                highlight_piano_key(settings_pos_x);
            }
        }
    }

    void hide_field_settings() {
        // destroy piano and buttons

        if (current_piano == null) {
            return;
        }

        foreach (PianoNote note in current_piano.notes) {
            Destroy(note.piano_key);
        }

        foreach (KeyboardButton button in current_keyboard_buttons.buttons) {
            Destroy(button.button_script.gameObject);
        }

        Destroy(current_instrument_display);

        current_piano = null;
        current_keyboard_buttons = null;
        current_instrument_display = null;
        editing_maze_field = null;
        editing_maze_field_script = null;

        menuman.remove_menu_highlighter();
    }

    public List<Coords> find_coordinates_around_pos(float x, float y) {
        List<Coords> coordinates = new List<Coords>();

        coordinates.Add(new Coords {
            wall_coord_x = x + 0.5F, wall_coord_y = y,
            field_coord_x = Mathf.RoundToInt(x + 1F), field_coord_y = Mathf.RoundToInt(y)
        });

        coordinates.Add(new Coords {
            wall_coord_x = x - 0.5F, wall_coord_y = y,
            field_coord_x = Mathf.RoundToInt(x - 1F), field_coord_y = Mathf.RoundToInt(y)
        });

        coordinates.Add(new Coords {
            wall_coord_x = x, wall_coord_y = y + 0.5F,
            field_coord_x = Mathf.RoundToInt(x), field_coord_y = Mathf.RoundToInt(y + 1F)
        });

        coordinates.Add(new Coords {
            wall_coord_x = x, wall_coord_y = y - 0.5F,
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

    public void increase_current_level() {
        current_level++;

        if (current_level > 99) {
            current_level = 1;
        }
    }

    public void decrease_current_level() {
        current_level--;

        if (current_level < 1) {
            current_level = 99;
        }
    }

    public void save_level() {
        storageman.save_as_json("test", current_level);
    }

    public void load_level(int level_number) {
        if (level_file_exists(level_number)) {
           current_level = level_number;
           StorageMan.Maze maze = storageman.load_from_json(level_number);
           build_maze_from_maze_class(maze);
        }
    }

    void build_maze_from_maze_class(StorageMan.Maze maze) {
        mazeman.clean_maze();

        foreach (StorageMan.MazeWall maze_wall in maze.walls) {
            create_editor_wall_at_coordinates(maze_wall.x, maze_wall.y);
        }

        foreach (StorageMan.MazeField maze_field in maze.fields) {
            GameObject new_field = mazeman.find_or_create_field_at_coordinates(Mathf.RoundToInt(maze_field.x), Mathf.RoundToInt(maze_field.y));

            maze_field_script field_script = gameentity.get_maze_field_script_from_game_object(new_field);

            if (maze_field.base_note) {
                mazeman.set_field_to_base_note(new_field);
            }
            if (maze_field.target_note) {
                mazeman.set_field_to_target_note(new_field, field_script);
            }

            field_script.save_note(maze_field.note);

            field_script.instrument = maze_field.instrument;
        }
    }

    public bool level_file_exists(int level_number) {
        return storageman.level_file_exists(level_number);
    }
}
