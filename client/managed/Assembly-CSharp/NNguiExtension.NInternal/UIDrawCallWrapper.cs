using UnityEngine;

namespace NNguiExtension.NInternal;

public class UIDrawCallWrapper : IDrawCallWrapper
{
	public UIDrawCall dc;

	bool IDrawCallWrapper.isDirty
	{
		get
		{
			return dc.isDirty;
		}
		set
		{
			dc.isDirty = true;
		}
	}

	UIDrawCall IDrawCallWrapper.Native => dc;

	int IDrawCallWrapper.depthStart => dc.depthStart;

	int IDrawCallWrapper.depthEnd => dc.depthEnd;

	public UIDrawCallWrapper(UIDrawCall drawCall)
	{
		dc = drawCall;
	}

	void IDrawCallWrapper.UpdateRenderQueue(UIPanel panel, Vector3 pos, Quaternion rot, Vector3 scale, ref int curRenderQueue)
	{
		Transform cachedTransform = dc.cachedTransform;
		cachedTransform.position = pos;
		cachedTransform.rotation = rot;
		cachedTransform.localScale = scale;
		dc.renderQueue = curRenderQueue;
		dc.alwaysOnScreen = panel.alwaysOnScreen && (panel.clipping == UIDrawCall.Clipping.None || panel.clipping == UIDrawCall.Clipping.ConstrainButDontClip);
		dc.sortingOrder = panel.sortingOrder;
		dc.clipTexture = panel.clipTexture;
		if (panel.renderQueue != UIPanel.RenderQueue.Explicit)
		{
			curRenderQueue++;
		}
	}

	void IDrawCallWrapper.SetLayer(int layer)
	{
		dc.gameObject.layer = layer;
	}
}
