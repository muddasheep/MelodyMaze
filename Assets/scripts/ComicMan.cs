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

        public void initialize() {
            if (panel == null) {
                panel = (GameObject)Instantiate(default_comic_panel);
            }

            panel.transform.position = new Vector3(x, y, z);
            GameObject background = panel.transform.GetChild(0).gameObject;
            background.transform.localScale = new Vector3(width, height, default_comic_panel.transform.localScale.z);
            background.transform.localPosition = new Vector3(0 + (width / 2), 0 - (height / 2), -0.66F);
        }

        // attaches image of character x with emotion y, i.e. son_rise.jpg, dad_serious.jpg
        public void add_character(string character_name, string character_emote) {

        }
    }

    public ComicPanel create_comic_panel(float x, float y, float z, float width, float height) {
        ComicPanel new_panel = new ComicPanel { x = x, y = y, z = z, width = width, height = height, default_comic_panel = default_comic_panel };
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

    void DrawLine(Vector3 start, Vector3 end, Color color, GameObject panel) {
        GameObject myLine = new GameObject();
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.SetPosition(0, myLine.transform.InverseTransformPoint(start));
        lr.SetPosition(1, myLine.transform.InverseTransformPoint(end));
        lr.useWorldSpace = false;
        lr.numCapVertices = 90;
        myLine.transform.parent = panel.transform;
    }
}
