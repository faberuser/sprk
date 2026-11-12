using NVespa;
using UnityEngine;

namespace NGame2.NUI;

public class UIParticleWidget : UIWidgetExtraRenderQ
{
	private ParticleSystemRenderer _psRenderer;

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
		if (_psRenderer == null)
		{
			_psRenderer = GetComponent<ParticleSystemRenderer>();
		}
		if (_psRenderer == null)
		{
			return renderQueue;
		}
		if (_material == null)
		{
			_material = _psRenderer.material;
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
