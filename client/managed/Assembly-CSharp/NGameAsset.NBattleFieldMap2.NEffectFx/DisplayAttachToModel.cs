using NVespa;
using UnityEngine;

namespace NGameAsset.NBattleFieldMap2.NEffectFx;

public class DisplayAttachToModel : EffectFxInstance.IDisplayEffect
{
	private EffectFxInstance _owner;

	private IAsyncLoad<IEffect> _effectAsync;

	public DisplayAttachToModel(EffectFxInstance owner)
	{
		_owner = owner;
	}

	void EffectFxInstance.IDisplayEffect.Close()
	{
		if (_effectAsync != null)
		{
			EffectContainer.ReleaseEffect(_effectAsync, detachParent: true, immediatelyRelease: true);
			_effectAsync = null;
		}
	}

	void EffectFxInstance.IDisplayEffect.Play()
	{
		string attachPath = _owner.Data.AttachPath;
		Transform nodeByPath = _owner.Owner.GetNodeByPath(attachPath);
		if (!(nodeByPath == null))
		{
			_effectAsync = SingletonAuto<EffectContainer>.instance.CreateAsync(_owner.Data.Code, nodeByPath.gameObject.layer, _owner.Data.FxTransform);
			_effectAsync.Value.SetParent(nodeByPath);
		}
	}

	void EffectFxInstance.IDisplayEffect.Stop()
	{
		if (_effectAsync != null)
		{
			EffectContainer.ReleaseEffect(_effectAsync, detachParent: true);
			_effectAsync = null;
		}
	}

	void EffectFxInstance.IDisplayEffect.Update(Vector3 cameraPos)
	{
		if (_effectAsync != null)
		{
			_effectAsync.Value.Update();
		}
	}

	void EffectFxInstance.IDisplayEffect.Apply(RenderGroupModifier2 modifier)
	{
		if (_effectAsync != null)
		{
			modifier.CopyTo(_effectAsync.Value.GetRenderGroupModifier(), ignoreReplaceTex: true);
		}
	}

	IAsyncLoad<IEffect> EffectFxInstance.IDisplayEffect.GetEffectAsync()
	{
		return _effectAsync;
	}
}
