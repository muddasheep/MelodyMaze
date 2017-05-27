using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalizedText: MonoBehaviour {

	public string key;

	private void Start () {
		SetToLocalizedText();
	}
	public void SetToLocalizedText() {

		GetComponent<Text>().font = LocalizationManager.instance.GetLocalizedFontValue(key);
		GetComponent<Text>().text = LocalizationManager.instance.GetLocalizedTextValue(key);
	}

}
