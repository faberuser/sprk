using NVespa;

namespace NGameAsset.NCreature;

public class SelfEffectInstance
{
	public enum EReleaseType
	{
		Close,
		Disabled
	}

	private CreatureInstance _owner;

	private SelfEffectData _data;

	private bool _enabled;

	private IAsyncLoad<IEffect> _effectAsync;

	public void Open(CreatureInstance owner, SelfEffectData data)
	{
		_owner = owner;
		_data = data;
		_enabled = _data.PlayType == SelfEffectData.EPlayType.Always;
	}

	public void SetEnable(bool enable)
	{
		_enabled = enable;
	}

	public void Close()
	{
		ReleaseEffect(EReleaseType.Close);
	}

	public void Visit(IVisitor visitor)
	{
		visitor.TryVisit(this);
		if (_effectAsync != null)
		{
			_effectAsync.Value.Visit(visitor);
		}
	}

	public void Apply(RenderGroupModifier2 modifier)
	{
		if (_effectAsync != null)
		{
			modifier.CopyTo(_effectAsync.Value.GetRenderGroupModifier(), ignoreReplaceTex: true);
		}
	}

	public RenderGroupModifier2 GetRenderGroupModifier()
	{
		if (_effectAsync == null)
		{
			return null;
		}
		return _effectAsync.Value.GetRenderGroupModifier();
	}

	public void RefreshExtraRenderFx()
	{
		if (_effectAsync != null)
		{
			if (_owner.ExtraRenderFxController.ShaderTransform == null)
			{
				_effectAsync.Value.SetShaderTransform(null, null);
			}
			else
			{
				_effectAsync.Value.SetShaderTransform(_owner.ExtraRenderFxController.ShaderTransform, _owner.ExtraRenderFxController.ShaderTransformPivot);
			}
			_effectAsync.Value.SetGrayscale(_owner.ExtraRenderFxController.GrayscaleEnabled);
		}
	}

	public void SetTimer(ILocalTimer localTimer)
	{
		if (_effectAsync != null)
		{
			_effectAsync.Value.SetTimer(localTimer);
		}
	}

	public void OnAnimation(AnimationStock stock)
	{
		if (_data.PlayType != SelfEffectData.EPlayType.Always)
		{
			if (_data.PlayType == SelfEffectData.EPlayType.EnableList)
			{
				_enabled = _data.ContainsKey(stock.Key);
			}
			else if (_data.PlayType == SelfEffectData.EPlayType.DisableList)
			{
				_enabled = !_data.ContainsKey(stock.Key);
			}
		}
	}

	public bool IsPlayTypeAlways()
	{
		return _data.PlayType == SelfEffectData.EPlayType.Always;
	}

	public void CheckEnable()
	{
		if (_enabled && _effectAsync == null)
		{
			CreateEffect();
		}
		else if (!_enabled && _effectAsync != null)
		{
			ReleaseEffect(EReleaseType.Disabled);
		}
	}

	public void ChainUpdate()
	{
		if (_effectAsync != null)
		{
			_effectAsync.Value.Update();
		}
	}

	private void CreateEffect()
	{
		_effectAsync = SingletonAuto<EffectContainer>.instance.CreateAsync(_data.Code, _owner.InstanceCache.Go.layer, _data.ExtraTransform);
		RefreshExtraRenderFx();
		if (_owner.LocalTimer != null)
		{
			_effectAsync.Value.SetTimer(_owner.LocalTimer);
		}
		_effectAsync.Value.SetParent(_owner);
	}

	private void ReleaseEffect(EReleaseType releaseType)
	{
		bool immediatelyRelease = releaseType == EReleaseType.Close && _data.ReleaseImmediately;
		EffectContainer.ReleaseEffect(_effectAsync, detachParent: true, immediatelyRelease);
		_effectAsync = null;
	}
}
