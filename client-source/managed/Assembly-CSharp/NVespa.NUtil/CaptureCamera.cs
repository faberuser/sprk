using UnityEngine;

namespace NVespa.NUtil;

public class CaptureCamera : MonoBehaviour
{
	public bool RequestCapture;

	public RenderTexture CaptureRender;

	private void Start()
	{
	}

	private void Update()
	{
		CaptureCurrent();
	}

	private void OnPostRender()
	{
	}

	private void CaptureCurrent()
	{
		if (RequestCapture)
		{
			RequestCapture = false;
			if (CaptureRender == null)
			{
				CaptureRender = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
			}
			RenderTexture.active = CaptureRender;
			RenderTexture targetTexture = GetComponent<Camera>().targetTexture;
			GetComponent<Camera>().targetTexture = CaptureRender;
			GetComponent<Camera>().Render();
			GetComponent<Camera>().targetTexture = targetTexture;
			RenderTexture.active = null;
		}
	}
}
