using UnityEngine;
using System.Collections;

public class maze_wall_script : MonoBehaviour {
	public GameObject wall_top;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	/****** DARTH FADER ******/

	// publically editable speed
	public float fadeDelay = 0.0f; 
	public float fadeTime = 0.5f; 

	// store colours
	private Color[] colors; 
	
	// check the alpha value of most opaque object
	float MaxAlpha() {
		float maxAlpha = 0.0f; 
		Renderer[] rendererObjects = wall_top.GetComponentsInChildren<Renderer>(); 
		foreach (Renderer item in rendererObjects) {
			maxAlpha = Mathf.Max (maxAlpha, item.material.color.a); 
		}
		return maxAlpha; 
	}
	
	// fade sequence
	IEnumerator FadeSequence (float fadingOutTime)
	{
		// log fading direction, then precalculate fading speed as a multiplier
		bool fadingOut = (fadingOutTime < 0.0f);
		float fadingOutSpeed = 1.0f / fadingOutTime; 
		
		// grab all child objects
		Renderer[] rendererObjects = wall_top.GetComponentsInChildren<Renderer>(); 
		if (colors == null)
		{
			//create a cache of colors if necessary
			colors = new Color[rendererObjects.Length]; 
			
			// store the original colours for all child objects
			for (int i = 0; i < rendererObjects.Length; i++)
			{
				colors[i] = rendererObjects[i].material.color; 
			}
		}
		
		// make all objects visible
		for (int i = 0; i < rendererObjects.Length; i++)
		{
			rendererObjects[i].enabled = true;
		}
		
		
		// get current max alpha
		float alphaValue = MaxAlpha();  
		
		
		// iterate to change alpha value 
		while ( (alphaValue >= 0.0f && fadingOut) || (alphaValue <= 1.0f && !fadingOut)) 
		{
			alphaValue += Time.deltaTime * fadingOutSpeed; 
			
			for (int i = 0; i < rendererObjects.Length; i++)
			{
				Color newColor = (colors != null ? colors[i] : rendererObjects[i].material.color);
				newColor.a = Mathf.Min ( newColor.a, alphaValue ); 
				newColor.a = Mathf.Clamp (newColor.a, 0.0f, 1.0f); 				
				rendererObjects[i].material.SetColor("_Color", newColor) ; 
			}
			
			yield return null; 
		}
		
		// turn objects off after fading out
		if (fadingOut)
		{
			for (int i = 0; i < rendererObjects.Length; i++)
			{
				rendererObjects[i].enabled = false; 
			}
		}
		
	}

	public void FadeIn ()
	{
		FadeIn (fadeTime); 
	}
	
	public void FadeOut () {
		FadeOut (fadeTime); 		
	}
	
	void FadeIn (float newFadeTime) {
		StopAllCoroutines(); 
		StartCoroutine("FadeSequence", newFadeTime); 
	}
	
	void FadeOut (float newFadeTime) {
		StopAllCoroutines(); 
		StartCoroutine("FadeSequence", -newFadeTime); 
	}

}
