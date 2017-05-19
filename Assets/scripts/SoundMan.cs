using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundMan : MonoBehaviour {
	public GameObject sounds_source;

    public float scale = Mathf.Pow(2f, 1.0f / 12f);

    public class AudioSourceData {
        
        public AudioSource audio_source { get; set; }
        public IEnumerator audio_source_numerator { get; set; }
    }

    public List<AudioSourceData> audio_sources = new List<AudioSourceData>();

    int current_audio_source = 0;
    int audio_source_limit = 30;

    // Use this for initialization
    void Start () {

        for (int i = 0; i < audio_source_limit; i++) {
            audio_sources.Add(new AudioSourceData { audio_source = sounds_source.AddComponent<AudioSource>() });
        }
    }
    
    public void play_sound(string sound, int pitch_amount = 0) {

        AudioSourceData previous_sound = audio_sources[current_audio_source];

        if (previous_sound.audio_source_numerator != null) {
            StopCoroutine(previous_sound.audio_source_numerator);
        }

        previous_sound.audio_source_numerator = fade_out_sound(audio_sources[current_audio_source].audio_source);

        StartCoroutine(previous_sound.audio_source_numerator);

        current_audio_source++;

        if (current_audio_source >= audio_source_limit) {
            current_audio_source = 0;
        }

        AudioSource sound_player = audio_sources[current_audio_source].audio_source;
        sound_player.volume = 1f;

        sound_player.pitch = Mathf.Pow(scale, pitch_amount);

        sound_player.Stop();
        AudioClip clip = Resources.Load<AudioClip>("sounds/" + sound);
        sound_player.clip = clip;
        sound_player.Play();
	}

    public string get_instrument_file_path(string instrument_type, string note) {

        return "instruments/" + instrument_type + "/" + instrument_type + "_" + note;
    }

    public void play_instrument_sound(string instrument_type, string note, int pitch_amount = 0) {
        play_sound(get_instrument_file_path(instrument_type, note), pitch_amount);
    }

    IEnumerator fade_out_sound(AudioSource the_source) {
        float t = 1;
        while (t > 0.0f) {
            t -= Time.deltaTime;
            the_source.volume = t;
            yield return new WaitForSeconds(0);
        }
        the_source.volume = 0.0f;
        the_source.Stop();
    }
}
