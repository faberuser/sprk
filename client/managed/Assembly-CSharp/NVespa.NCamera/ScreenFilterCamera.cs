using NShader;
using NVespa.NGlobal;
using NVespa.NGui;
using NVespa.NSingleton;
using UnityEngine;

namespace NVespa.NCamera;

public class ScreenFilterCamera : MonoBehaviour, IInspectorGuiDrawer
{
	public const float FilterDepthDefault = 100f;

	public float FilterDepth => CachedCamera.depth;

	public Camera CachedCamera { get; set; }

	public RenderTexture RT { get; set; }

	public void Open(bool isRebuild)
	{
		if (!isRebuild)
		{
			Camera[] array = Object.FindObjectsOfType<Camera>();
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(array); i++)
			{
				if (!(array[i] == null) && Math.Approximately(array[i].depth, 100f))
				{
					CachedCamera = array[i];
					CachedCamera.enabled = true;
					UIWidget.DebugAlert = true;
					break;
				}
			}
		}
		if (CachedCamera == null)
		{
			CachedCamera = Vespa_Unity_GameObject_Extension_Method.GetComponentAuto<Camera>(base.gameObject);
		}
		CachedCamera.depth = 100f;
		CachedCamera.orthographic = true;
		CachedCamera.cullingMask = 0;
		CachedCamera.clearFlags = CameraClearFlags.Color;
		CachedCamera.backgroundColor = Color.black;
		RT = FilterBase.CreateScreenRenderTarget();
		_ = Mathf.Min(Screen.height, 1080) * 9 / 10;
		GlobalAutoBehaviour<GuiManager>.instance.Add("ScreenFilterCamera", this, delegate
		{
			if (CachedCamera != null)
			{
				using (new BlockManager().Horizontal())
				{
					GUILayout.Label("ScreenFilterCamera Depth=" + CachedCamera.depth);
					if (GUILayout.Button("Toggle"))
					{
						CachedCamera.depth = ((CachedCamera.depth > 10f) ? 3.5f : 100f);
					}
				}
			}
		});
	}

	public void Close()
	{
		if (RT != null)
		{
			VespaObject.DestroyObject(RT);
			RT = null;
		}
	}

	private void LateUpdate()
	{
	}

	private void OnPostRender()
	{
		if (GlobalAutoBehaviour<GlobalCameraManager>.instance.HasScreenFilterTargetCamera())
		{
			FilterBase.Blit(RT, null, FilterBase.BlitMaterial);
		}
	}
}
