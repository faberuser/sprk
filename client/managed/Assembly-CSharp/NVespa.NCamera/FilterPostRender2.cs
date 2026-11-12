using UnityEngine;

namespace NVespa.NCamera;

public class FilterPostRender2 : MonoBehaviour
{
	private Camera _camera;

	private void Start()
	{
		_camera = base.gameObject.GetComponent<Camera>();
	}

	private void OnPostRender()
	{
		if (!(_camera == null))
		{
			_ = _camera.targetTexture == null;
		}
	}
}
