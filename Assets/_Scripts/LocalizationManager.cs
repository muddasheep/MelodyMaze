using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class LocalizationManager: MonoBehaviour {

	public static LocalizationManager instance;
	public string defaultLoadLocalizedText;

	private Dictionary<string, string> localizedText;
	private bool isReady = false;
	private string missingTextString = "Localized text string NOT found";

	private void Awake() {
		LoadLocalizedText(defaultLoadLocalizedText);
	}

	public void LoadLocalizedText(string fileName) {
		localizedText = new Dictionary<string, string>();
		string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

		if (File.Exists(filePath)) {
			string data_Json = File.ReadAllText(filePath);
			LocalizationData loadedData = JsonUtility.FromJson<LocalizationData>(data_Json);

			for (int i = 0; i < loadedData.localizationItems.Length; i++) {
                if (loadedData.localizationItems[i].key != "") {
                    localizedText.Add(loadedData.localizationItems[i].key, loadedData.localizationItems[i].value);
                }
			}

		} else {
			Debug.LogError("Cannot find file!");
		}

		isReady = true;
	}

	public string GetLocalizedValue(string key) {
		string result = missingTextString;
		if (localizedText.ContainsKey(key)) {
			result = localizedText[key];
		}

		return result;
	}

	public bool GetIsReady() {
		return isReady;
	}
}
