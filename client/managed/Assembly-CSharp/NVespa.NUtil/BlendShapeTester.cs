using UnityEngine;

namespace NVespa.NUtil;

public class BlendShapeTester : MonoBehaviour, IInspectorGuiDrawer
{
	private SkinnedMeshRenderer _renderer;

	private Mesh _blendShapeMesh;

	private void ReserveData()
	{
		if (!(_blendShapeMesh != null))
		{
			SkinnedMeshRenderer component = GetComponent<SkinnedMeshRenderer>();
			if (!(component == null) && component.sharedMesh.blendShapeCount != 0)
			{
				_renderer = component;
				_blendShapeMesh = component.sharedMesh;
			}
		}
	}
}
