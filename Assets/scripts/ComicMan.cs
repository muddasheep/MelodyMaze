using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComicMan : MonoBehaviour {

    public GameObject default_comic_panel;
    public GameObject comic_bubble_text;

    ComicSequence current_comic_sequence;

    public void start_sequence() {
        current_comic_sequence.next_strip();
    }

    public bool next_strip() {
        return current_comic_sequence.next_strip();
    }

    public class ComicSequence {
        // has comic strips, each strip gets displayed one after another
        public List<ComicStrip> comic_strips { get; set; }
        public ComicMan comicman { get; set; }
        public int current_comic_strip { get; set; }

        public ComicSequence initialize() {
            comic_strips = new List<ComicStrip>();

            current_comic_strip = -1;

            return this;
        }

        public ComicStrip create_comic_strip() {
            ComicStrip comic_strip = new ComicStrip { comicman = comicman };

            comic_strips.Add(comic_strip);

            return comic_strip;
        }

        public bool next_strip() {
            if (current_comic_strip >= 0) {
                comic_strips[current_comic_strip].hide_panels();
            }

            current_comic_strip++;

            if (current_comic_strip >= comic_strips.Count) {
                return false;
            }

            comic_strips[current_comic_strip].show_panels();

            return true;
        }
    }

    public ComicSequence create_comic_sequence() {
        ComicSequence comic_sequence = new ComicSequence { comicman = this };

        current_comic_sequence = comic_sequence;

        return comic_sequence;
    }

    public class ComicStrip {
        // has multiple panels
        public List<ComicPanel> comic_panels { get; set; }
        public ComicMan comicman { get; set; }

        public ComicStrip initialize() {
            comic_panels = new List<ComicPanel>();

            return this;
        }

        public ComicPanel create_comic_panel(float x, float y, float z, float width, float height) {
            ComicPanel new_panel = new ComicPanel {
                x = x, y = y, z = z, width = width, height = height,
                default_comic_panel = comicman.default_comic_panel, comicman = comicman
            };

            new_panel.initialize();

            if (comic_panels == null) {
                comic_panels = new List<ComicPanel>();
            }

            z = z - 0.7f;

            comicman.DrawLine(new Vector3(x, y, z), new Vector3(x + width, y, z), Color.gray, new_panel.panel);
            comicman.DrawLine(new Vector3(x + width, y, z), new Vector3(x + width, y - height, z), Color.gray, new_panel.panel);
            comicman.DrawLine(new Vector3(x + width, y - height, z), new Vector3(x, y - height, z), Color.gray, new_panel.panel);
            comicman.DrawLine(new Vector3(x, y - height, z), new Vector3(x, y, z), Color.gray, new_panel.panel);

            comic_panels.Add(new_panel);
            return new_panel;
        }

        public void show_panels() {
            foreach (ComicPanel panel in comic_panels) {
                panel.panel.SetActive(true);
            }
        }

        public void hide_panels() {
            foreach (ComicPanel panel in comic_panels) {
                panel.panel.SetActive(false);
            }
        }
    }

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

            panel.SetActive(false);
        }

        // attaches image of character x with emotion y, i.e. son_rise.jpg, dad_serious.jpg
        public ComicPanel add_character(string character_name, string character_emote = "normal") {
            var character = new GameObject();
            character.transform.parent = panel.transform;
            character.transform.localPosition = new Vector3(0, 0, -0.75F);

            SpriteRenderer renderer = character.AddComponent<SpriteRenderer>();

            renderer.sprite = Resources.Load("sprites/" + character_name + "_" + character_emote, typeof(Sprite)) as Sprite;

            characters.Add(character);

            return this;
        }

        public void character_says(int character_index, string text) {
            GameObject chosen_character = characters[character_index];

            GameObject bubble = comicman.create_speech_bubble(
                new Vector3(chosen_character.transform.position.x, chosen_character.transform.position.y + 0.5f, chosen_character.transform.position.z - 1f),
                new Vector3(chosen_character.transform.position.x + chosen_character.GetComponent<SpriteRenderer>().sprite.bounds.size.x / 2, chosen_character.transform.position.y, chosen_character.transform.position.z - 1f),
                this, text
            );
        }
    }

    GameObject create_speech_bubble(Vector3 speech_position, Vector3 character_position, ComicPanel panel, string text) {

        float speech_width = 1f;
        float speech_height = 0.5f;

        float bubblez = speech_position.z;

        GameObject bubble = new GameObject();

        GameObject bubble_text = (GameObject)Instantiate(comic_bubble_text);
        bubble_text.transform.parent = bubble.transform;
        bubble_text.transform.position = new Vector3(speech_position.x, speech_position.y, bubblez - 0.30f);
        TextMesh bubble_text_mesh = bubble_text.transform.GetChild(0).gameObject.GetComponent<TextMesh>();
        bubble_text_mesh.text = text;

        speech_width = get_mesh_width(bubble_text_mesh);
        float start_x = speech_position.x - speech_width / 2;
        float end_x = speech_position.x + speech_width / 2;

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

    public float get_mesh_width(TextMesh mesh) {
        float width = 0;
        foreach (char symbol in mesh.text) {
            CharacterInfo info;
            if (mesh.font.GetCharacterInfo(symbol, out info, mesh.fontSize, mesh.fontStyle)) {
                width += info.advance;
            }
        }

        return width * mesh.characterSize * 0.1f * 0.1f + 0.2f;
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
