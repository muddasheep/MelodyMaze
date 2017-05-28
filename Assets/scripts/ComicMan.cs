using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComicMan : MonoBehaviour {

    public GameObject default_comic_panel;
    public List<ComicPanel> comic_panels { get; set; }

    public class ComicPanel {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public float width { get; set; }
        public float height { get; set; }
        public GameObject panel { get; set; }
        public GameObject default_comic_panel { get; set; }
        public List<GameObject> characters { get; set; }
        public ComicMan comicman { get; set; }

        public void initialize() {
            if (panel == null) {
                panel = (GameObject)Instantiate(default_comic_panel);
            }

            panel.transform.position = new Vector3(x, y, z);
            GameObject background = panel.transform.GetChild(0).gameObject;
            background.transform.localScale = new Vector3(width, height, default_comic_panel.transform.localScale.z);
            background.transform.localPosition = new Vector3(0 + (width / 2), 0 - (height / 2), -0.66F);

            characters = new List<GameObject>();
        }

        // attaches image of character x with emotion y, i.e. son_rise.jpg, dad_serious.jpg
        public GameObject add_character(string character_name, string character_emote = "normal") {
            var character = new GameObject();
            character.transform.parent = panel.transform;
            character.transform.localPosition = new Vector3(0, 0, -0.75F);

            SpriteRenderer renderer = character.AddComponent<SpriteRenderer>();

            renderer.sprite = Resources.Load("sprites/" + character_name + "_" + character_emote, typeof(Sprite)) as Sprite;

            characters.Add(character);

            return character;
        }

        public void character_says(int character_index, string text) {
            GameObject chosen_character = characters[character_index];

            comicman.create_speech_bubble(
                new Vector3(chosen_character.transform.position.x, chosen_character.transform.position.y + 0.5f, chosen_character.transform.position.z - 1f),
                new Vector3(chosen_character.transform.position.x + chosen_character.GetComponent<SpriteRenderer>().sprite.bounds.size.x / 2, chosen_character.transform.position.y, chosen_character.transform.position.z - 1f),
                this
            );
        }
    }

    public ComicPanel create_comic_panel(float x, float y, float z, float width, float height) {
        ComicPanel new_panel = new ComicPanel { x = x, y = y, z = z, width = width, height = height, default_comic_panel = default_comic_panel, comicman = this };
        new_panel.initialize();

        if (comic_panels == null) {
            comic_panels = new List<ComicPanel>();
        }

        z = z - 0.7f;

        DrawLine(new Vector3(x, y, z), new Vector3(x + width, y, z), Color.gray, new_panel.panel);
        DrawLine(new Vector3(x + width, y, z), new Vector3(x + width, y - height, z), Color.gray, new_panel.panel);
        DrawLine(new Vector3(x + width, y - height, z), new Vector3(x, y - height, z), Color.gray, new_panel.panel);
        DrawLine(new Vector3(x, y - height, z), new Vector3(x, y, z), Color.gray, new_panel.panel);

        comic_panels.Add(new_panel);
        return new_panel;
    }

    GameObject create_speech_bubble(Vector3 speech_position, Vector3 character_position, ComicPanel panel) {

        float speech_width = 1f;
        float speech_height = 0.5f;

        float start_x = speech_position.x - speech_width / 2;
        float start_y = speech_position.y + speech_height / 2;
        float end_x = speech_position.x + speech_width / 2;
        float end_y = speech_position.y - speech_height / 2;
        float bubblez = speech_position.z;

        GameObject bubble = new GameObject();

        LineRenderer bubble_background = DrawLine(new Vector3(start_x, speech_position.y, bubblez + 0.1f), new Vector3(end_x, speech_position.y, bubblez + 0.1f), Color.white, bubble, speech_height, speech_height);
        bubble_background.numCapVertices = 0;


        LineRenderer bubble_background_shadow = DrawLine(new Vector3(start_x - 0.1f, speech_position.y - 0.1f, bubblez + 0.2f), new Vector3(end_x + 0.1f, speech_position.y + 0.1f, bubblez + 0.2f), Color.black, bubble, speech_height + 0.2f, speech_height + 0.2f);
        bubble_background_shadow.numCapVertices = 0;

        bubble.transform.parent = panel.panel.transform;

        LineRenderer bubble_arrow = DrawLine(new Vector3(speech_position.x, speech_position.y, bubblez - 0.25f), new Vector3(character_position.x, character_position.y, bubblez - 0.25f), Color.white, bubble);
        bubble_arrow.startWidth = 0.2f;
        bubble_arrow.endWidth = 0;

        LineRenderer bubble_arrow_shadow = DrawLine(new Vector3(speech_position.x, speech_position.y, bubblez + 0.2f), new Vector3(character_position.x, character_position.y, bubblez + 0.2f), Color.black, bubble);
        bubble_arrow_shadow.startWidth = 0.5f;
        bubble_arrow_shadow.endWidth = 0;

        return bubble;
    }

    LineRenderer DrawLine(Vector3 start, Vector3 end, Color color, GameObject panel, float start_width = 0.05f, float end_width = 0.05f) {
        GameObject myLine = new GameObject();
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = start_width;
        lr.endWidth = end_width;
        lr.SetPosition(0, myLine.transform.InverseTransformPoint(start));
        lr.SetPosition(1, myLine.transform.InverseTransformPoint(end));
        lr.useWorldSpace = false;
        lr.numCapVertices = 90;
        myLine.transform.parent = panel.transform;
        return lr;
    }
}
