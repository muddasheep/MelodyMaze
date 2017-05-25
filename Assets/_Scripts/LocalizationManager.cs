using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using ArabicSupport;

public class LocalizationManager: MonoBehaviour {

	public static LocalizationManager instance;
	public string defaultLoadLocalizedText;

	private Dictionary<string, string> localizedText;
	private Dictionary<string, Font> localizedFont;
	private bool isArabic = false;
	private bool isReady = false;
	private string missingTextString = "Localized text string NOT found";

	private void Awake() {

		LoadLocalizedText(defaultLoadLocalizedText);
	}

	public void LoadLocalizedText(string fileName) {
		localizedText = new Dictionary<string, string>();
		localizedFont = new Dictionary<string, Font>();
		string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

		if (File.Exists(filePath)) {
			string data_Json = File.ReadAllText(filePath);
			LocalizationData loadedData = JsonUtility.FromJson<LocalizationData>(data_Json);

			for (int i = 0; i < loadedData.localizationItems.Length; i++) {
                if (loadedData.localizationItems[i].key != "") {
					// set "localizedText" dictionary value to the text in json
					localizedText.Add(loadedData.localizationItems[i].key, loadedData.localizationItems[i].value);

					// set "localizedFont" dictionary value to the font in json
					Font tempFont = loadedData.localizationItems[i].overrideLanguageFont;
					if (tempFont == null) {
						tempFont = loadedData.defaultLanguageFont;
					}
					localizedFont.Add(loadedData.localizationItems[i].key, tempFont);

				}
			}
			// is arabic
			if (fileName == "Arabic.json") {
				isArabic = true;
			} else {
				isArabic = false;
			}

		} else {
			Debug.LogError("Cannot find file!");
		}

		isReady = true;
	}

	public Font GetLocalizedFontValue(string key) {
		Font result = null;
		if (localizedText.ContainsKey(key)) {
			result = localizedFont[key];
		}

		return result;
	}

	public string GetLocalizedTextValue(string key) {
		string result = missingTextString;
		if (localizedText.ContainsKey(key)) {
			result = localizedText[key];
		}
		
		if (isArabic == true) {
			result = ArabicFixer.Fix(result, true, true);
		}

		return result;
	}

	public bool GetIsReady() {
		return isReady;
	}
}
