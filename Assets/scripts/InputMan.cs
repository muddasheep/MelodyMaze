using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InputMan : MonoBehaviour {

    /* INPUT MAN
     * 
     * ===========
     * 
     * Once an assistant to a journalist, Jim officially died when trying to bring coffee to
     * a gathering of fellow peers at a press conference. The fall wasn't what killed him.
     * He spilled cups of hot coffee on his chest and face, and when trying to grab onto nearby
     * journalists, they too stumbled, sending their photo cameras flying, which shattered on Jim's
     * body and immediately electrocuted him.
     * He didn't die though; he had gained a new power, but nobody would talk about it in fear of
     * having to explain themselves or ridiculing their reputation into oblivion.
     * Before his new life had begun, Jim was known to give great input when writing reports.
     * With his newly found powers - recognizing input from others - he would immediately seek out
     * Philipp and let him know when a player presses certain combinations of buttons and keys.
     * 
     */

    // Use this for initialization
    void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void detectPressedKeyOrButton() {
        foreach (KeyCode kcode in System.Enum.GetValues(typeof(KeyCode))) {
            if (Input.GetKeyDown(kcode))
                Debug.Log("KeyCode down: " + kcode);
        }
    }

    public bool player_action_button_down = false;

    public bool player_pressed_action_once() {
        if (Input.GetButton("Fire1")) {
            if (player_action_button_down == false) {
                player_action_button_down = true;
                return true;
            }
        }
        else {
            player_action_button_down = false;
        }

        return false;
    }

    public bool player_pressed_escape() {
        if (Input.GetButton("Cancel")) {
            return true;
        }

        return false;
    }


    public bool player_pressed_action() {
        if (Input.GetButton("Fire1")) {
            return true;
        }

        return false;
    }

    public bool player_pressed_action2() {
        if (Input.GetKey(KeyCode.Delete) || Input.GetButton("Fire2")) {
            return true;
        }

        return false;
    }

    public bool player_pressed_action3() {
        if (Input.GetButton("Fire3")) {
            return true;
        }

        return false;
    }

    public bool player_pressed_action4() {
        if (Input.GetButton("Jump")) {
            return true;
        }

        return false;
    }

    public bool player_pressed_up() {
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetAxis("Vertical") > 0.1F
            || Input.mouseScrollDelta.y > 0) {
            return true;
        }

        return false;
    }

    public bool player_pressed_down() {
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetAxis("Vertical") < -0.1F
            || Input.mouseScrollDelta.y < 0) {
            return true;
        }

        return false;
    }

    public bool player_pressed_right() {
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetAxis("Horizontal") > 0.1F) {
            return true;
        }

        return false;
    }

    public bool player_pressed_left() {
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetAxis("Horizontal") < -0.1F) {
            return true;
        }

        return false;
    }

    bool repeated_once = false;
    Dictionary<string, IEnumerator> player_button_press_coroutines = new Dictionary<string, IEnumerator>();
    Dictionary<string, bool> player_button_pressed_down_once = new Dictionary<string, bool>();

    private IEnumerator player_button_press_coroutine;
    public IEnumerator player_press_button_repeat(float waitTime, string direction) {
        yield return new WaitForSeconds(waitTime);

        player_button_pressed_down_once[direction] = false;

        repeated_once = true;
    }

    bool check_and_repeat_player_button_press(bool pressed, string direction) {
        if (pressed) {
            bool player_button_down;
            player_button_pressed_down_once.TryGetValue(direction, out player_button_down);
            if (player_button_down != true) {
                player_button_pressed_down_once[direction] = true;

                if (repeated_once == false) {
                    player_button_press_coroutines[direction] = player_press_button_repeat(0.3F, direction);
                }
                else {
                    player_button_press_coroutines[direction] = player_press_button_repeat(0.1F, direction);
                }
                StartCoroutine(player_button_press_coroutines[direction]);

                return true;
            }
        }
        else {
            IEnumerator coroutine_for_direction;
            player_button_press_coroutines.TryGetValue(direction, out coroutine_for_direction);
            if (coroutine_for_direction != null) {
                StopCoroutine(coroutine_for_direction);
                player_button_press_coroutines[direction] = null;
                repeated_once = false;
            }
            player_button_pressed_down_once[direction] = false;
        }

        return false;
    }

    public bool player_pressed_action2_once() {
        bool pressed = player_pressed_action2();

        return check_and_repeat_player_button_press(pressed, "action2");
    }

    public bool player_pressed_action3_once() {
        bool pressed = player_pressed_action3();

        return check_and_repeat_player_button_press(pressed, "action3");
    }

    public bool player_pressed_action4_once() {
        bool pressed = player_pressed_action4();

        return check_and_repeat_player_button_press(pressed, "jump");
    }

    public bool player_pressed_up_once() {
        bool pressed = player_pressed_up();

        return check_and_repeat_player_button_press(pressed, "up");
    }

    public bool player_pressed_down_once() {
        bool pressed = player_pressed_down();

        return check_and_repeat_player_button_press(pressed, "down");
    }

    public bool player_pressed_left_once() {
        bool pressed = player_pressed_left();

        return check_and_repeat_player_button_press(pressed, "left");
    }

    public bool player_pressed_right_once() {
        bool pressed = player_pressed_right();

        return check_and_repeat_player_button_press(pressed, "right");
    }

    public Vector3 get_mouse_position() {
        /*
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100)) {
            Debug.Log(hit.collider.gameObject);
            Debug.DrawLine(ray.origin, hit.point);
        }
        */
        var v3 = Input.mousePosition;
        v3.z = Mathf.Abs(Camera.main.transform.position.z) - 4F;
        v3 = Camera.main.ScreenToWorldPoint(v3);
        //Debug.DrawLine(ray.origin, v3);

        return v3;
    }

    public bool has_mouse_moved() {
        return (Input.GetAxis("Mouse X") != 0) || (Input.GetAxis("Mouse Y") != 0);
    }
}
