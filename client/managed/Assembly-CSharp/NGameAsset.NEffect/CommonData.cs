using JsonBin;
using NVespa;
using UnityEngine;

namespace NGameAsset.NEffect;

public class CommonData : ILoadJson
{
	public enum EAttachMethod
	{
		Pivot,
		NodePath,
		NodeFind,
		EffectModel
	}

	public enum EBillboardType
	{
		None,
		Billboard,
		VerticalBillboard
	}

	public EnableSpecData EnableSpec = new EnableSpecData();

	public EAttachMethod AttachMethod;

	public string AttachTarget = string.Empty;

	public float BeginTime;

	public float EndTime = 1f;

	public TransformSetData TransformSet = new TransformSetData();

	public EBillboardType BillboardType;

	public float TimeLength => EndTime - BeginTime;

	public GlobalBillboardManager.EBillboardType GetGlobalBillboardType()
	{
		if (BillboardType == EBillboardType.None)
		{
			return GlobalBillboardManager.EBillboardType.None;
		}
		if (BillboardType == EBillboardType.Billboard)
		{
			return GlobalBillboardManager.EBillboardType.Billboard;
		}
		if (BillboardType == EBillboardType.VerticalBillboard)
		{
			return GlobalBillboardManager.EBillboardType.VerticalBillboard;
		}
		return GlobalBillboardManager.EBillboardType.None;
	}

	public Transform GetTransformForEffectModelAttach(EffectInstance owner)
	{
		return GetTransformForEffectModelAttach(owner, null);
	}

	public Transform GetTransformForEffectModelAttach(EffectInstance owner, ModelInstance exception)
	{
		if (AttachMethod != EAttachMethod.EffectModel || String_Extension_Method.IsNullOrEmpty(AttachTarget))
		{
			return null;
		}
		for (int i = 0; i < owner.GetModelInstanceCount(); i++)
		{
			ModelInstance modelInstance = owner.GetModelInstance(i);
			if (!(modelInstance.ModelGo == null) && (exception == null || modelInstance != exception))
			{
				Transform transform = Vespa_Unity_Transform_Extension_Method.FindChildRecursively(modelInstance.ModelGo.transform, AttachTarget);
				if (transform != null)
				{
					return transform;
				}
			}
		}
		return null;
	}

	public Transform GetAttachTransform(ICreature parent)
	{
		if (parent == null)
		{
			return null;
		}
		if (AttachMethod == EAttachMethod.Pivot && !String_Extension_Method.IsNullOrEmpty(AttachTarget))
		{
			Transform pivot = parent.GetPivot(AttachTarget);
			if (pivot != null)
			{
				return pivot;
			}
		}
		Transform pivot2 = parent.GetPivot(EPivotType.AssetRoot);
		bool findSuccess;
		Transform attachTransformInternal = GetAttachTransformInternal(pivot2, out findSuccess);
		if (findSuccess)
		{
			return attachTransformInternal;
		}
		return GetAttachTransformInternal(parent.GetPivot(EPivotType.Root), out findSuccess);
	}

	private Transform GetAttachTransformInternal(Transform parentTm, out bool findSuccess)
	{
		if (parentTm == null)
		{
			findSuccess = false;
			return null;
		}
		Transform transform = null;
		if (!String_Extension_Method.IsNullOrEmpty(AttachTarget))
		{
			if (AttachMethod == EAttachMethod.NodeFind)
			{
				transform = Vespa_Unity_Transform_Extension_Method.FindChildRecursively(parentTm, AttachTarget);
			}
			else if (AttachMethod == EAttachMethod.NodePath)
			{
				transform = parentTm.Find(AttachTarget);
			}
		}
		if (transform != null)
		{
			findSuccess = true;
			return transform;
		}
		findSuccess = String_Extension_Method.IsNullOrEmpty(AttachTarget);
		return parentTm;
	}

	public Transform GetAttachTransform(Transform parentTm)
	{
		bool findSuccess;
		return GetAttachTransformInternal(parentTm, out findSuccess);
	}

	bool ILoadJson.Load(JsonRead json)
	{
		EnableSpec.LOADFROMJSON(json["EnableSpec"]);
		BeginTime = json["BeginTime"].GETFLOAT(BeginTime);
		EndTime = json["EndTime"].GETFLOAT(EndTime);
		AttachMethod = json["AttachMethod"].GETENUM(AttachMethod);
		AttachTarget = json["AttachTarget"].GETSTRING(AttachTarget);
		TransformSet.LOADFROMJSON(json["TransformSet"]);
		BillboardType = json["BillboardType"].GETENUM(BillboardType);
		JsonRead jsonRead = json["DisableThis"];
		if (jsonRead.IsBoolean())
		{
			bool bOOLEAN = jsonRead.GETBOOLEAN(iDefault: false);
			EnableSpec.EnableHigh = !bOOLEAN;
			EnableSpec.EnableMid = !bOOLEAN;
			EnableSpec.EnableLow = !bOOLEAN;
		}
		return true;
	}
}
