using NVespa;
using UnityEngine;

namespace NGameAsset.NBattleFieldMap2.NEffectFx;

public class DisplayNormal : EffectFxInstance.IDisplayEffect
{
	private EffectFxInstance _owner;

	private GameObject _effectGo;

	private IAsyncLoad<IEffect> _effectAsync;

	public DisplayNormal(EffectFxInstance owner)
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
		if (_effectGo != null)
		{
			VespaObject.DestroyGo(_effectGo);
		}
	}

	void EffectFxInstance.IDisplayEffect.Play()
	{
		GameObject instanceGo = _owner.Owner.InstanceGo;
		if (!(instanceGo == null))
		{
			if (_effectGo == null)
			{
				_effectGo = new GameObject();
				Vespa_Unity_Transform_Extension_Method.ParentResetTM(_effectGo.transform, instanceGo.transform);
			}
			_effectAsync = SingletonAuto<EffectContainer>.instance.CreateAsync(_owner.Data.Code, instanceGo.layer, _owner.Data.FxTransform);
			_effectAsync.Value.SetParent(_effectGo.transform);
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
		if (_owner.Data.IsLoop && _owner.Data.LoopLength > 0.1f)
		{
			float x = _owner.Data.FxTransform.Position.x;
			float loopLength = _owner.Data.LoopLength;
			int num = Mathf.RoundToInt((cameraPos.x - x) / loopLength);
			_effectGo.transform.localPosition = Vector3.right * (loopLength * (float)num);
		}
		if (_owner.Data.IsBillboard)
		{
			Vector3 forward = -cameraPos;
			forward.z = Mathf.Abs(forward.z) + 1f;
			forward.x = 0f;
			forward.Normalize();
			_effectGo.transform.localRotation = Quaternion.LookRotation(forward);
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
