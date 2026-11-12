using NVespa;
using UnityEngine;

namespace NGame2.NUI;

public class UIMeshRendererWidget : UIWidgetExtraRenderQ
{
	private MeshRenderer meshRenderer;

	private Material _material;

	private void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			ReleaseAllContents();
		}
	}

	private void ReleaseAllContents()
	{
		if (_material != null)
		{
			VespaObject.DestroyObject(_material);
			_material = null;
		}
	}

	public override int UpdateExtraRenderQueue(UIPanel panel, int renderQueue, bool isExplicit)
	{
		if (meshRenderer == null)
		{
			meshRenderer = GetComponent<MeshRenderer>();
		}
		if (meshRenderer == null)
		{
			return renderQueue;
		}
		if (_material == null)
		{
			_material = meshRenderer.material;
		}
		if (_material == null)
		{
			return renderQueue;
		}
		if (!isExplicit)
		{
			renderQueue++;
		}
		_material.renderQueue = renderQueue;
		return renderQueue;
	}
}
