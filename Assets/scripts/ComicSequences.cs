using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComicSequences {

    public ComicMan comicman { get; set; }

    public void display_comic_sequence_for_level(int level_number) {

        // just .. do it this way for now XD
        if (level_number == 1) {
            comic_sequence_for_level_1();
        }
    }

    void comic_sequence_for_level_1() {
        ComicMan.ComicSequence test_sequence = comicman.create_comic_sequence().initialize();
        ComicMan.ComicStrip test_strip = test_sequence.create_comic_strip().initialize();
        test_strip.create_comic_panel(0F, 2.1F, -9F, 2F, 2F).add_character("son").character_says(0, "Hey dad, I know what you did last summer");
        test_strip.create_comic_panel(0F, 0F, -9F, 1F, 2F).add_character("dad").character_says(0, "oh... that's ok.");
        test_strip.create_comic_panel(2.1F, 2.1F, -9F, 1F, 2F).add_character("dad");
        test_strip.create_comic_panel(1.1F, 0F, -9F, 2F, 2F).add_character("son").character_says(0, "cool");
        ComicMan.ComicStrip test_strip2 = test_sequence.create_comic_strip().initialize();
        test_strip2.create_comic_panel(0F, 2.1F, -9F, 4F, 4F).add_character("son").character_says(0, "I guess");
    }
}
