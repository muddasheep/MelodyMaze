using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StorageMan : MonoBehaviour {

    /* STORAGE MAN
     * 
     * ===========
     * 
     * Abandoned as a child, Jason grew up in a cardboard box factory.
     * When he was ten, his hand got stuck in the machinery and his whole body got pulled in.
     * Miraculously, he not only survived, but he was able to create cardboard boxes out of thin air.
     * He has aged considerably since then, and he has decided to devote his entire time
     * to help Philipp out and store all levels in his trademarked JSON cardbox format.
     * 
     */

    [Serializable]
    private class Maze {

        public string level_name;
        public int level_number;

        public MazeField[] fields;
    }

    [Serializable]
    public class MazeField {
        public float x;
        public float y;
        public string note;
        public bool base_note;
        public bool target_note;
    }

    MazeMan mazeman;
    GameEntity gameentity;

    void Start() {

        mazeman = GetComponent<MazeMan>();
        gameentity = GetComponent<GameEntity>();
    }

    void Update() {

    }

    public void save_as_json(string level_name, int level_number) {

        Maze maze = new Maze();
        maze.level_name = level_name;
        maze.level_number = level_number;

        List<MazeField> maze_fields = new List<MazeField>();

        foreach (KeyValuePair<int, GameObject> field in mazeman.maze_field_coordinates_hash) {

            maze_field_script maze_script = gameentity.get_maze_field_script_from_game_object(field.Value);

            Debug.Log(field.Key);

            maze_fields.Add(new MazeField {
                x = field.Value.transform.position.x,
                y = field.Value.transform.position.y,
                note = maze_script.note,
                base_note = maze_script.is_base_note,
                target_note = maze_script.is_target_note,
            });
        }

        maze.fields = maze_fields.ToArray();

        string json = JsonUtility.ToJson(maze);

        Debug.Log(json);
    }
}