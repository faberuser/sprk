using NGameAsset.NEffect;
using NVespa;
using UnityEngine;

namespace NGameAsset.NCreature;

public class CustomizeItemInstance : ITransformInstnace
{
	public class OpenData : IAsyncLoadBase
	{
		private NGameAsset.NEffect.OpenData _effectOpenData;

		public CustomizeStock Stock { get; private set; }

		string IAsyncLoadBase.KeyDesc => _effectOpenData.KEYDESC();

		bool IAsyncLoadBase.IsDone => _effectOpenData.ISDONE();

		bool IAsyncLoadBase.IsValid => _effectOpenData.ISVALID();

		string IAsyncLoadBase.LoadMessage => _effectOpenData.LOADMESSAGE();

		public void Open(NGameAsset.NEffect.OpenData effectOpenData, CustomizeStock stock)
		{
			_effectOpenData = effectOpenData;
			Stock = stock;
		}

		public void Close()
		{
			_effectOpenData.Close();
		}
	}

	private enum TransformDirectionPivot
	{
		RenderFacade,
		Parent
	}

	private Vector3 InitPositon = Vector3.zero;

	private Vector3 InitRoationEuler = Vector3.zero;

	private float InitScale = 1f;

	private Vector3 ExtraPosition = Vector3.zero;

	private Vector3 ExtraRotaionEuler = Vector3.zero;

	private float ExtraScale = 1f;

	private CreatureInstance _owner;

	private OpenData _data;

	private bool _enabled;

	private IAsyncLoad<IEffect> _effectAsync;

	private Transform _targetPivot;

	private Transform _transformAxis;

	private Transform _transformRoot;

	public CustomizeStock Stock => _data.Stock;

	public void Open(CreatureInstance owner, OpenData data)
	{
		_owner = owner;
		_data = data;
		_enabled = true;
	}

	public void SetActive(bool active)
	{
		if (!(_transformAxis == null))
		{
			_transformAxis.gameObject.SetActive(active);
		}
	}

	public void SetEnable(bool enable)
	{
		_enabled = enable;
	}

	public void Close()
	{
		Release();
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

	public void CheckEnable()
	{
		if (_enabled && _effectAsync == null)
		{
			CreateEffect();
		}
		else if (!_enabled && _effectAsync != null)
		{
			Release();
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
		int layer = _owner.InstanceCache.Go.layer;
		_effectAsync = SingletonAuto<EffectContainer>.instance.CreateAsync(Stock.Code, layer);
		RefreshExtraRenderFx();
		if (_owner.LocalTimer != null)
		{
			_effectAsync.Value.SetTimer(_owner.LocalTimer);
		}
		AttachEffect();
	}

	private void AttachEffect()
	{
		int layer = _owner.InstanceCache.Go.layer;
		Transform transform = null;
		ICreature creature = Vespa_Common_Class_Extension_Method.Interface((ICreature)_owner);
		if (!String_Extension_Method.IsNullOrEmpty(Stock.Pivot))
		{
			transform = creature.GetPivotSafe(Stock.Pivot);
		}
		else if (!String_Extension_Method.IsNullOrEmpty(Stock.TransformName1))
		{
			transform = creature.FindTransform(Stock.TransformName1);
		}
		if (transform == null && !String_Extension_Method.IsNullOrEmpty(Stock.TransformName2))
		{
			transform = creature.FindTransform(Stock.TransformName2);
		}
		if (transform == null)
		{
			VespaDebug.LogError($"find trasnform parent failt  Trs1({Stock.TransformName1}) , Trs2({Stock.TransformName2})");
			Release();
			return;
		}
		GameObject gameObject = new GameObject("CustomizeItem_Axis");
		gameObject.layer = layer;
		_transformAxis = gameObject.transform;
		Vespa_Unity_Transform_Extension_Method.ParentResetTM(_transformAxis, transform);
		GameObject gameObject2 = new GameObject("CustomizeItem_Transform");
		gameObject2.layer = layer;
		_transformRoot = gameObject2.transform;
		Vespa_Unity_Transform_Extension_Method.ParentResetTM(_transformRoot, _transformAxis);
		_targetPivot = TransformPivot(TransformDirectionPivot.Parent);
		_transformAxis.LookAt(_targetPivot.transform.position + _targetPivot.up, -_targetPivot.right);
		InitPositon = Stock.InitPosition;
		InitRoationEuler = Stock.InitRotation;
		InitScale = Stock.InitScale;
		ExtraPosition = Stock.Position;
		ExtraRotaionEuler = Stock.Rotation;
		ExtraScale = Stock.Scale;
		SetPosition();
		SetRotation();
		SetScale();
		_effectAsync.Value.SetParent(_transformRoot);
	}

	private void SetScale()
	{
		_transformRoot.localScale = Vector3.one * InitScale * ExtraScale;
	}

	private void SetRotation()
	{
		_transformRoot.localRotation = Quaternion.Euler(InitRoationEuler) * Quaternion.Euler(ExtraRotaionEuler);
	}

	private void SetPosition()
	{
		_transformRoot.localPosition = InitPositon + ExtraPosition;
	}

	private Transform TransformPivot(TransformDirectionPivot transformDirectionPivot)
	{
		if (transformDirectionPivot == TransformDirectionPivot.RenderFacade)
		{
			return _owner.InstanceCache.Tm;
		}
		return _transformAxis;
	}

	private void SetLocalPosition(Vector3 position)
	{
		ExtraPosition = position;
		SetPosition();
	}

	private Vector3 GetLocalPostion()
	{
		return ExtraPosition;
	}

	private void SetLocalRotation(Vector3 rotation)
	{
		ExtraRotaionEuler = rotation;
		SetRotation();
	}

	private Vector3 GetLocalRotation()
	{
		return ExtraRotaionEuler;
	}

	private void SetLocalScale(float scale)
	{
		ExtraScale = scale;
		SetScale();
	}

	private float GetLocalScale()
	{
		return ExtraScale;
	}

	private void Release()
	{
		EffectContainer.ReleaseEffect(_effectAsync, detachParent: true);
		_effectAsync = null;
		VespaObject.DestroyGo(_transformAxis);
	}

	void ITransformInstnace.SetPosition(Vector3 position)
	{
		SetLocalPosition(position);
	}

	void ITransformInstnace.SetRotation(Vector3 rotation)
	{
		SetLocalRotation(rotation);
	}

	void ITransformInstnace.SetScale(float scale)
	{
		SetLocalScale(scale);
	}

	Vector3 ITransformInstnace.GetPosition()
	{
		return GetLocalPostion();
	}

	Vector3 ITransformInstnace.GetRotation()
	{
		return GetLocalRotation();
	}

	float ITransformInstnace.GetScale()
	{
		return GetLocalScale();
	}
}
