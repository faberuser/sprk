using NGame2.NUI.NGame3DWidget;
using NGame2.NUI.NWindow;
using NVespa;
using UnityEngine;

namespace NGameAsset.NBattleFieldMap2.NEffectFx;

public class DisplayScreen : EffectFxInstance.IDisplayEffect
{
	private EffectFxInstance _owner;

	private IContents _effectContents;

	public DisplayScreen(EffectFxInstance owner)
	{
		_owner = owner;
	}

	void EffectFxInstance.IDisplayEffect.Close()
	{
		if (_effectContents != null)
		{
			_effectContents.ReleaseAll();
			_effectContents = null;
		}
	}

	void EffectFxInstance.IDisplayEffect.Play()
	{
		_effectContents = ScreenEffect.PlayEffect(_owner.Data.Code, _owner.Data.FxTransform, null);
	}

	void EffectFxInstance.IDisplayEffect.Stop()
	{
		if (_effectContents != null)
		{
			_effectContents.Stop(immediatelyClear: false);
			_effectContents = null;
		}
	}

	void EffectFxInstance.IDisplayEffect.Update(Vector3 cameraPos)
	{
	}

	void EffectFxInstance.IDisplayEffect.Apply(RenderGroupModifier2 modifier)
	{
	}

	IAsyncLoad<IEffect> EffectFxInstance.IDisplayEffect.GetEffectAsync()
	{
		return null;
	}
}
