using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

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
    public class Maze {

        public string level_name;
        public int level_number;

        public MazeField[] fields;
        public MazeWall[] walls;
    }

    [Serializable]
    public class MazeField {
        public float x;
        public float y;
        public string note;
        public bool base_note;
        public bool target_note;
    }

    [Serializable]
    public class MazeWall {
        public float x;
        public float y;
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
        List<MazeWall> maze_walls = new List<MazeWall>();

        foreach (KeyValuePair<int, GameObject> field in mazeman.maze_field_coordinates_hash) {

            maze_field_script maze_script = gameentity.get_maze_field_script_from_game_object(field.Value);

            maze_fields.Add(new MazeField {
                x = field.Value.transform.position.x,
                y = field.Value.transform.position.y,
                note = maze_script.note,
                base_note = maze_script.is_base_note,
                target_note = maze_script.is_target_note,
            });
        }

        foreach (KeyValuePair<string, GameObject> field in mazeman.maze_walls_coordinates_hash) {

            maze_walls.Add(new MazeWall {
                x = field.Value.transform.position.x,
                y = field.Value.transform.position.y,
            });
        }

        maze.fields = maze_fields.ToArray();
        maze.walls = maze_walls.ToArray();

        string json = JsonUtility.ToJson(maze);

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(path_to_level(level_number.ToString()));
        bf.Serialize(file, json);
        file.Close();
    }

    public Maze load_from_json(int level_number) {
        Maze maze = new Maze();

        if (File.Exists(path_to_level(level_number.ToString()))) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(path_to_level(level_number.ToString()), FileMode.Open);
            string json = (string)bf.Deserialize(file);
            file.Close();

            maze = JsonUtility.FromJson<Maze>(json);
        }

        return maze;
    }

    public string path_to_level(string level_number) {
        return Application.dataPath + "/levels/level_" + level_number + ".json";
    }

    public bool level_file_exists(int level_number) {
        return File.Exists(path_to_level(level_number.ToString()));
    }
}