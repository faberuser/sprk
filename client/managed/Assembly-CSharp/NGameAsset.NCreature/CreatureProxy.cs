using System.Collections.Generic;
using NVespa;
using UnityEngine;

namespace NGameAsset.NCreature;

public class CreatureProxy : ICreature
{
	private bool _isClosed;

	private Transform _parentTm;

	private Tuple<ShaderTransformData, Transform> _tupleShaderTransform;

	private bool _grayscaleEnabled;

	private RenderGroupModifier2 _renderGroupModifier;

	private ILocalTimer _localTimer;

	private Quaternion _rotation;

	private List<ICreatureMaterialModifier> _mtrlModifierList;

	private IncreaseDic<AnimationKeyData, AnimationStock> _animationDic = new IncreaseDic<AnimationKeyData, AnimationStock>(AnimationKeyData.Comparer.Default);

	public void ApplyToReal(CreatureInstance creature)
	{
		if (!_isClosed)
		{
			if (_parentTm != null)
			{
				Vespa_Common_Class_Extension_Method.Interface((ICreature)creature).SetParent(_parentTm);
			}
			if (_localTimer != null)
			{
				Vespa_Common_Class_Extension_Method.Interface((ICreature)creature).SetTimer(_localTimer);
			}
			if (_tupleShaderTransform != null)
			{
				Vespa_Common_Class_Extension_Method.Interface((ICreature)creature).SetShaderTransform(_tupleShaderTransform.Item1, _tupleShaderTransform.Item2);
			}
			if (_grayscaleEnabled)
			{
				Vespa_Common_Class_Extension_Method.Interface((ICreature)creature).SetGrayscale(_grayscaleEnabled);
			}
			if (_renderGroupModifier != null)
			{
				RenderGroupModifier2 renderGroupModifier = Vespa_Common_Class_Extension_Method.Interface((ICreature)creature).GetRenderGroupModifier();
				_renderGroupModifier.CopyTo(renderGroupModifier);
				creature.FlushRenderGroupModifier();
			}
			Vespa_Common_Class_Extension_Method.Interface((ICreature)creature).SetRotation(_rotation, reset: true);
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_mtrlModifierList); i++)
			{
				ICreatureMaterialModifier modifier = _mtrlModifierList[i];
				Vespa_Common_Class_Extension_Method.Interface((ICreature)creature).AddMaterialModifier(modifier);
			}
			for (int j = 0; j < _animationDic.Count; j++)
			{
				AnimationStock valueByIndex = _animationDic.GetValueByIndex(j);
				Vespa_Common_Class_Extension_Method.Interface((ICreature)creature).SetAnimation(valueByIndex);
			}
		}
	}

	void ICreature.Close()
	{
		_isClosed = true;
		if (_renderGroupModifier != null)
		{
			SmallPooling<RenderGroupModifier2>.ReleaseInstance(_renderGroupModifier);
			_renderGroupModifier = null;
		}
	}

	void ICreature.Visit(IVisitor visitor)
	{
	}

	void ICreature.SetParent(Transform parentTm)
	{
		if (!_isClosed)
		{
			_parentTm = parentTm;
		}
	}

	void ICreature.SetShaderTransform(ShaderTransformData shaderTransform, Transform pivotTm)
	{
		_tupleShaderTransform = Tuple.Create(shaderTransform, pivotTm);
	}

	void ICreature.SetGrayscale(bool isEnable)
	{
		_grayscaleEnabled = isEnable;
	}

	RenderGroupModifier2 ICreature.GetRenderGroupModifier()
	{
		if (_renderGroupModifier == null)
		{
			_renderGroupModifier = SmallPooling<RenderGroupModifier2>.CreateInstance();
			_renderGroupModifier.Open(useSoloTimeColor: true);
		}
		return _renderGroupModifier;
	}

	void ICreature.ChainUpdate()
	{
	}

	void ICreature.SetTimer(ILocalTimer localTimer)
	{
		_localTimer = localTimer;
	}

	void ICreature.SetRotation(Quaternion rotation, bool reset)
	{
		_rotation = rotation;
	}

	void ICreature.AddMaterialModifier(ICreatureMaterialModifier modifier)
	{
		if (_mtrlModifierList == null)
		{
			_mtrlModifierList = new List<ICreatureMaterialModifier>();
		}
		_mtrlModifierList.Add(modifier);
	}

	void ICreature.ClearAnimation()
	{
		_animationDic.Clear();
	}

	void ICreature.SetAnimation(AnimationStock stock)
	{
		if (!_isClosed)
		{
			if (stock.Weight < Mathf.Epsilon)
			{
				_animationDic.RemoveSlow(stock.Key);
			}
			else
			{
				_animationDic.AddAndOverride(stock.Key, stock);
			}
		}
	}

	void ICreature.ClearFacialAnimation(FacialAnimationData facialAnimation)
	{
	}

	IAnimationData ICreature.GetAnimationData(AnimationKeyData key)
	{
		return null;
	}

	float ICreature.GetAnimationBlend(AnimationKeyData motionFrom, AnimationKeyData motionTo)
	{
		return DefineCreature.DefaultBlendTime(motionTo.MotionType);
	}

	ICreatureOpenData ICreature.GetCreatureOpenData()
	{
		return null;
	}

	ITransformInstnace ICreature.GetCutomizeItemInstance(CustomizeStock stock)
	{
		return null;
	}

	Transform ICreature.GetPivot(EPivotType pivot)
	{
		return null;
	}

	Transform ICreature.GetPivot(string name)
	{
		return null;
	}

	Transform ICreature.GetPivotSafe(string name)
	{
		return null;
	}

	Transform ICreature.FindTransform(string name, bool skipNullCaching)
	{
		return null;
	}
}
