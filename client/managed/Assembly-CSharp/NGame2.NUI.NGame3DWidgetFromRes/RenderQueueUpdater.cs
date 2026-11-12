using System.Collections.Generic;
using NGame2.NUI.NGame3DWidget;
using NGameAsset;

namespace NGame2.NUI.NGame3DWidgetFromRes;

public class RenderQueueUpdater
{
	private List<IEffect> _assetEffectList = new List<IEffect>();

	public void UpdateExtraRenderQueue(UIPanel panel, RenderQueueVisitor visitor)
	{
		for (int i = 0; i < _assetEffectList.Count; i++)
		{
			_assetEffectList[i].GetRenderGroupModifier().SetExtraAlpha(panel.alpha);
			_assetEffectList[i].Visit(visitor);
		}
	}
}
