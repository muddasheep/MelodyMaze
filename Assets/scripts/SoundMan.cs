using UnityEngine;
using System.Collections;

public class SoundMan : MonoBehaviour {
	public GameObject sounds_source;

	AudioSource sound_source;

	// Use this for initialization
	void Start () {
		sound_source = sounds_source.GetComponent<AudioSource>();
	}
	
	public void play_sound(string sound) {

		AudioSource sound_player = sound_source;

		AudioClip clip = Resources.Load<AudioClip>("sounds/" + sound);

		sound_player.PlayOneShot(clip);
	}

    public void play_instrument_sound(string instrument_type, string note) {
        play_sound("instruments/" + instrument_type + "/" + instrument_type + "_" + note);
    }
}
