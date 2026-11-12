using NGame2;
using NVespa;
using UnityEngine;

namespace NNguiExtension;

public class OcclusionManager : SingletonAuto<OcclusionManager>
{
	private int _occlusionLayer = int.MinValue;

	private int _occlusionDepth = int.MinValue;

	public OcclusionManager()
	{
		int occlusionLayer = LayerMask.NameToLayer(Vespa_String_Extension_Method.GetEnumString(GameUIRoot.ELayerType.UI));
		SetOcclusionLayer(occlusionLayer);
	}

	public void SetOcclusionLayer(int layer)
	{
		_occlusionLayer = layer;
	}

	public bool IsVisible(UIWidget widget)
	{
		if (widget.panel.renderQueue != UIPanel.RenderQueue.Automatic)
		{
			return true;
		}
		if (widget.gameObject.layer != _occlusionLayer)
		{
			return true;
		}
		return _occlusionDepth <= widget.raycastDepth;
	}

	public bool IsVisible(int layer, int raycastDepth)
	{
		if (layer != _occlusionLayer)
		{
			return true;
		}
		return _occlusionDepth <= raycastDepth;
	}

	public void UpdateAll()
	{
		int occlusionDepth = _occlusionDepth;
		_occlusionDepth = int.MinValue;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(UIPanel.list); i++)
		{
			UIPanel uIPanel = UIPanel.list[i];
			if (uIPanel == null || uIPanel.gameObject.layer != _occlusionLayer || uIPanel.OcclusionWidget == null || !uIPanel.OcclusionFullScreen)
			{
				continue;
			}
			UIWidget occlusionWidget = uIPanel.OcclusionWidget;
			if (occlusionWidget.gameObject.activeInHierarchy && occlusionWidget.enabled && !(occlusionWidget.finalAlpha < 0.9999f))
			{
				int raycastDepth = uIPanel.raycastDepth;
				if (_occlusionDepth < raycastDepth)
				{
					_occlusionDepth = raycastDepth;
				}
			}
		}
		if (occlusionDepth == _occlusionDepth)
		{
			return;
		}
		int num = Mathf.Max(occlusionDepth, _occlusionDepth);
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(UIPanel.list); j++)
		{
			UIPanel uIPanel2 = UIPanel.list[j];
			if (!(uIPanel2 == null) && uIPanel2.gameObject.layer == _occlusionLayer && uIPanel2.raycastDepth < num)
			{
				uIPanel2.Invalidate(includeChildren: true);
			}
		}
	}
}
