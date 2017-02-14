using UnityEngine;
using System.Collections;

public class keyboard_button_script : MonoBehaviour {
    public bool active { get; set; }
    public Sprite button_on;
    public Sprite button_off;
    public GameObject button;
    public TextMesh button_text;

    SpriteRenderer sprite_renderer;

    // Use this for initialization
    void Awake () {
        active = false;

        sprite_renderer = button.GetComponent<SpriteRenderer>();
    }
	
    public void turn_on() {
        sprite_renderer.sprite = button_on;
        active = true;
    }

    public void turn_off() {
        sprite_renderer.sprite = button_off;
        active = false;
    }

    public void toggle() {
        active = !active;
        sprite_renderer.sprite = active ? button_on : button_off;
    }
}
