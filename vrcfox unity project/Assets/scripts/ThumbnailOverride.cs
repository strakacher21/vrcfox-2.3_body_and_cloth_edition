using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThumbnailOverride : MonoBehaviour
{
	[Header("Should be 4:3 and at least 400x300")]
	[SerializeField] private Texture2D thumbnailTexture;

	private void Start()
	{
		if (GetComponent<Camera>() == null)
		{
			// copy component to camera if not already on one 
			StartCoroutine(LateStart());
		}
	}

	private IEnumerator LateStart()
	{
		yield return new WaitForFixedUpdate();

		GameObject vrcCam = GameObject.Find("VRCCam");

		if (vrcCam != null)
		{
			// add component copy to VRCCam
			ThumbnailOverride setterOnCam = vrcCam.AddComponent<ThumbnailOverride>();
			setterOnCam.thumbnailTexture = thumbnailTexture;
		}
	}

	// override render texture
	private void OnRenderImage (RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit(thumbnailTexture, destination);
	}
}