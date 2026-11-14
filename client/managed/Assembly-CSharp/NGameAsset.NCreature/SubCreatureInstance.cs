using NVespa;
using UnityEngine;

namespace NGameAsset.NCreature;

public class SubCreatureInstance
{
	private CreatureInstance _parent;

	private ICreatureLoad _creatureAsync;

	private GameObject _extraGo;

	public ISubCreatureData Data { get; private set; }

	public SubCreatureInstance(CreatureInstance owner)
	{
		_parent = owner;
	}

	public bool Open(ICreatureOpenData parentOpenData, ISubCreatureData subCreatureData)
	{
		Data = subCreatureData;
		if (string.IsNullOrEmpty(Data?.CreatureCode))
		{
			// SPRK: unfinished model data (Maya's model lists an empty sub-creature);
			// skip it instead of failing a load and logging an error.
			return false;
		}
		CreatureStock stock = parentOpenData.GetStock();
		stock.Code = Data.CreatureCode;
		_creatureAsync = SingletonAuto<CreatureContainer>.instance.CreateAsync(stock);
		Transform pivotSafe = Vespa_Common_Class_Extension_Method.Interface((ICreature)_parent).GetPivotSafe(Data.PivotName);
		TransformData extraTransform = subCreatureData.GetExtraTransform();
		if (extraTransform == null)
		{
			_creatureAsync.Value.SetParent(pivotSafe);
		}
		else
		{
			_extraGo = new GameObject("ExtraTm");
			_extraGo.transform.parent = pivotSafe;
			_extraGo.transform.localPosition = extraTransform.Position;
			_extraGo.transform.localRotation = extraTransform.Rotation;
			_extraGo.transform.localScale = Vector3.one * extraTransform.Scale;
			_creatureAsync.Value.SetParent(_extraGo.transform);
		}
		return true;
	}

	public void Close()
	{
		if (_creatureAsync != null)
		{
			CreatureContainer.ReleaseCreature(_creatureAsync);
			_creatureAsync = null;
		}
		if (_extraGo != null)
		{
			VespaObject.DestroyGo(_extraGo);
			_extraGo = null;
		}
	}

	public void Visit(IVisitor visitor)
	{
		_creatureAsync.Value.Visit(visitor);
	}

	public void Apply(RenderGroupModifier2 modifier)
	{
		modifier.CopyTo(_creatureAsync.Value.GetRenderGroupModifier());
	}

	public void ChainUpdate()
	{
		_creatureAsync.Value.ChainUpdate();
	}

	public void SetTimer(ILocalTimer localTimer)
	{
		_creatureAsync.Value.SetTimer(localTimer);
	}

	public void SetRotation(Quaternion rotation, bool reset)
	{
		_creatureAsync.Value.SetRotation(rotation, reset);
	}

	public void ClearAnimation()
	{
		_creatureAsync.Value.ClearAnimation();
	}

	public void SetAnimation(AnimationStock parentStock)
	{
		AnimationStock animation = new AnimationStock
		{
			Key = Data.GetAnimationKey(parentStock.Key),
			Frame = parentStock.Frame,
			Weight = parentStock.Weight
		};
		_creatureAsync.Value.SetAnimation(animation);
	}
}
