using System.Collections.Generic;
using NGame2.NNodeTree;
using NGameAsset;

namespace NGame2.NUI.NGame3DWidget;

public class RenderQueueUpdater : IObserver
{
	private List<IAsyncLoad<IEffect>> _assetEffectList = new List<IAsyncLoad<IEffect>>();

	private List<ObjectBase> _objectBaseList = new List<ObjectBase>();

	public void UpdateExtraRenderQueue(UIPanel panel, RenderQueueVisitor visitor)
	{
		for (int i = 0; i < _assetEffectList.Count; i++)
		{
			_assetEffectList[i].Value.GetRenderGroupModifier().SetExtraAlpha(panel.alpha);
			_assetEffectList[i].Value.Visit(visitor);
		}
		for (int j = 0; j < _objectBaseList.Count; j++)
		{
			_objectBaseList[j].Visit(visitor);
		}
	}

	void IObserver.OnCreate(IAsyncLoad<IEffect> effectAsync)
	{
		_assetEffectList.Add(effectAsync);
	}

	void IObserver.OnRelease(IAsyncLoad<IEffect> effectAsync)
	{
		_assetEffectList.Remove(effectAsync);
	}

	void IObserver.OnCreate(ObjectBase objectBase)
	{
		_objectBaseList.Add(objectBase);
	}

	void IObserver.OnRelease(ObjectBase objectBase)
	{
		_objectBaseList.Remove(objectBase);
	}
}
