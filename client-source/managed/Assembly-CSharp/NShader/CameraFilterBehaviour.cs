using NGameAsset;
using NVespa;
using UnityEngine;

namespace NShader;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class CameraFilterBehaviour : MonoBehaviour
{
	public ChainFilter Filter { get; set; }

	public static bool EnableCameraFilter => SystemOptionData.Spec != ESpecType.Low;

	private void Start()
	{
		if (!SystemInfo.supportsImageEffects)
		{
			base.enabled = false;
		}
		else if (!EnableCameraFilter)
		{
			base.enabled = false;
		}
	}

	private void LateUpdate()
	{
		if (GetComponent<Camera>() == null || GetComponent<Camera>().targetTexture == null)
		{
			base.enabled = false;
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (Filter == null)
		{
			FilterBase.Blit(source, destination, FilterBase.BlitMaterial);
		}
		else
		{
			Filter.ImageProcess(source, destination, discardDest: false);
		}
	}
}
