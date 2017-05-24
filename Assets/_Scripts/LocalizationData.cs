
[System.Serializable]
public class LocalizationItem {
	public string key;
	[UnityEngine.TextArea(3, 10)]
	public string value;
}

[System.Serializable]
public class LocalizationData {

	public LocalizationItem[] localizationItems;
}
