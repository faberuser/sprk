using UnityEngine;

namespace NVespa.NCamera;

public class TargetTextureCache : MonoBehaviour
{
	private void OnDestroy()
	{
		Camera component = GetComponent<Camera>();
		if (!(component == null) && component.targetTexture != null)
		{
			component.targetTexture = null;
		}
	}
}
