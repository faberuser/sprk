using NGameAsset.NCommon;
using NVespa;
using UnityEngine;

namespace NGameAsset.NCreature;

public class WeaponEffectInstance
{
	private WeaponInstance _owner;

	private IWeaponEffectData _data;

	private bool _enable;

	private IAsyncLoad<IEffect> _effectAsync;

	public bool Open(WeaponInstance owner, IWeaponEffectData data)
	{
		_owner = owner;
		_data = data;
		return _enable = _owner.ModelGo != null;
	}

	public void Close()
	{
		_owner = null;
		_data = null;
		_enable = false;
		ReleaseEffect();
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

	public void RefreshExtraRenderFx(ExtraRenderFxController extraRenderFxController)
	{
		if (_effectAsync != null && extraRenderFxController != null)
		{
			if (extraRenderFxController.ShaderTransform == null)
			{
				_effectAsync.Value.SetShaderTransform(null, null);
			}
			else
			{
				_effectAsync.Value.SetShaderTransform(extraRenderFxController.ShaderTransform, extraRenderFxController.ShaderTransformPivot);
			}
			_effectAsync.Value.SetGrayscale(extraRenderFxController.GrayscaleEnabled);
		}
	}

	public void SetTimer(ILocalTimer localTimer)
	{
		if (_effectAsync != null)
		{
			_effectAsync.Value.SetTimer(localTimer);
		}
	}

	public void SetVisible(bool isVisible)
	{
		if (_effectAsync != null && _enable && isVisible)
		{
			_effectAsync.Value.SetVisible(isVisible);
		}
	}

	public void SetVisible(IAnimationData animationData, AnimationStock stock)
	{
		if (_effectAsync != null)
		{
			bool isNullNode = true;
			bool nodeVisible = animationData.GetNodeVisible(_owner.Data.Name, stock.Frame, out isNullNode);
			_effectAsync.Value.SetVisible(nodeVisible);
		}
	}

	public void ChainUpdate()
	{
		if (_effectAsync != null)
		{
			_effectAsync.Value.Update();
		}
	}

	public void CheckCreateEffect(ExtraRenderFxController extraRenderFxController, ILocalTimer localTimer)
	{
		if (_effectAsync == null && _owner != null && _effectAsync == null)
		{
			CreateEffect(extraRenderFxController, localTimer);
		}
	}

	private void CreateEffect(ExtraRenderFxController extraRenderFxController, ILocalTimer localTimer)
	{
		_effectAsync = SingletonAuto<EffectContainer>.instance.CreateAsync(_data.Code, _owner.ModelGo.layer, _data.ExtraTransform);
		RefreshExtraRenderFx(extraRenderFxController);
		if (localTimer != null)
		{
			_effectAsync.Value.SetTimer(localTimer);
		}
		GameObject modelGo = _owner.ModelGo;
		Transform transform = Vespa_Unity_Transform_Extension_Method.FindChildRecursively(modelGo.transform, _data.TargetPivot);
		if (transform == null)
		{
			transform = modelGo.transform;
		}
		_effectAsync.Value.SetParent(transform);
		_enable = true;
	}

	private void ReleaseEffect()
	{
		EffectContainer.ReleaseEffect(_effectAsync, detachParent: true);
		_effectAsync = null;
	}
}
