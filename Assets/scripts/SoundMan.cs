using UnityEngine;
using System.Collections;

public class SoundMan : MonoBehaviour {
	public GameObject sounds_source;

	AudioSource sound_source;
    AudioSource pitched_sound_source;
    float scale = Mathf.Pow(2f, 1.0f / 12f);

    // Use this for initialization
    void Start () {
		sound_source = sounds_source.GetComponent<AudioSource>();

        pitched_sound_source = sounds_source.AddComponent<AudioSource>();
    }
	
	public void play_sound(string sound, int pitch_amount = 0) {

        AudioSource sound_player = sound_source;

        if (pitch_amount != 0) {
            sound_player = pitched_sound_source;
            sound_player.pitch = Mathf.Pow(scale, pitch_amount);
        }

        AudioClip clip = Resources.Load<AudioClip>("sounds/" + sound);

		sound_player.PlayOneShot(clip);
	}

    public void play_instrument_sound(string instrument_type, string note, int pitch_amount = 0) {
        play_sound("instruments/" + instrument_type + "/" + instrument_type + "_" + note, pitch_amount);
    }
}
