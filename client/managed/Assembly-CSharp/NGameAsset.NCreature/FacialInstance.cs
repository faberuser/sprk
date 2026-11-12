using System.Collections.Generic;
using NVespa;
using UnityEngine;

namespace NGameAsset.NCreature;

public class FacialInstance
{
	public class BlendShapeData : SmallPooling<BlendShapeData>, ISmallPoolingItem
	{
		public int BlendShapeIndex;

		public IncreaseDic<object, float> WeightDic = new IncreaseDic<object, float>();

		public float CalcWeight()
		{
			float num = 0f;
			for (int i = 0; i < WeightDic.Count; i++)
			{
				float valueByIndex = WeightDic.GetValueByIndex(i);
				if (valueByIndex < Mathf.Epsilon)
				{
					WeightDic.RemoveByIndex(i);
					i--;
				}
				else
				{
					num += valueByIndex;
				}
			}
			return num;
		}

		void ISmallPoolingItem.OnCreateNew()
		{
		}

		void ISmallPoolingItem.OnDestroy()
		{
		}

		void ISmallPoolingItem.OnCreate()
		{
		}

		void ISmallPoolingItem.OnRelease()
		{
			BlendShapeIndex = 0;
			WeightDic.Clear();
		}
	}

	public class PartsData : SmallPooling<PartsData>, ISmallPoolingItem
	{
		public SkinnedMeshRenderer Render;

		public IncreaseDic<string, BlendShapeData> BlendDic = new IncreaseDic<string, BlendShapeData>();

		void ISmallPoolingItem.OnCreateNew()
		{
		}

		void ISmallPoolingItem.OnDestroy()
		{
		}

		void ISmallPoolingItem.OnCreate()
		{
		}

		void ISmallPoolingItem.OnRelease()
		{
			Render = null;
			for (int i = 0; i < BlendDic.Count; i++)
			{
				SmallPooling<BlendShapeData>.ReleaseInstance(BlendDic.GetValueByIndex(i));
			}
			BlendDic.Clear();
		}
	}

	public CreatureInstance Owner;

	public IncreaseDic<string, PartsData> PartsDic = new IncreaseDic<string, PartsData>();

	public bool Open(CreatureInstance creatureInstance, ICreatureOpenData data, RenderCache renderCache)
	{
		Owner = creatureInstance;
		for (int i = 0; i < data.GetFacialModelCount(); i++)
		{
			IFacialModelData facialModelData = data.GetFacialModelData(i);
			string facialNodeName = data.GetModelData().GetFacialNodeName(i);
			if (facialModelData == null || String_Extension_Method.IsNullOrEmpty(facialNodeName))
			{
				return false;
			}
			RenderProxy renderProxyByName = renderCache.GetRenderProxyByName(facialNodeName);
			if (renderProxyByName == null)
			{
				return false;
			}
			PartsData partsData = SmallPooling<PartsData>.CreateInstance();
			partsData.Render = renderProxyByName.renderer as SkinnedMeshRenderer;
			if (partsData.Render == null)
			{
				return false;
			}
			Mesh facialModelMesh = data.GetFacialModelMesh(i);
			if (facialModelMesh == null)
			{
				return false;
			}
			partsData.Render.sharedMesh = facialModelMesh;
			for (int j = 0; j < facialModelMesh.blendShapeCount; j++)
			{
				string blendShapeName = facialModelMesh.GetBlendShapeName(j);
				if (!String_Extension_Method.IsNullOrEmpty(blendShapeName))
				{
					BlendShapeData blendShapeData = SmallPooling<BlendShapeData>.CreateInstance();
					blendShapeData.BlendShapeIndex = j;
					partsData.BlendDic.AddAndOverride(blendShapeName, blendShapeData);
				}
			}
			PartsDic.Add(facialNodeName, partsData);
		}
		return true;
	}

	public void SetFacialWeight(FacialAnimationData facialAnimation, float frame, float weight)
	{
		if (facialAnimation.Type == FacialAnimationData.EType.Static)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(facialAnimation.StaticShapeList); i++)
			{
				string blendShapeName = facialAnimation.StaticShapeList[i];
				SetFacialWeight(i, blendShapeName, facialAnimation, weight);
			}
		}
		else
		{
			if (facialAnimation.Type != FacialAnimationData.EType.KeyAnimation)
			{
				return;
			}
			foreach (KeyValuePair<FacialAnimationData.FacialKey, AnimationCurve> item in facialAnimation.KeyAnimationDic)
			{
				FacialAnimationData.FacialKey key = item.Key;
				AnimationCurve value = item.Value;
				PartsData vALUE = PartsDic.GETVALUE(key.FacialName, null);
				if (vALUE != null)
				{
					float num = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(value, frame, 0f);
					SetFacialWeight(vALUE, key.BlendShapeName, facialAnimation, weight * num);
				}
			}
		}
	}

	public void SetFacialWeight(int index, string blendShapeName, object from, float weight)
	{
		if (PartsDic.IsValidIndex(index))
		{
			PartsData valueByIndex = PartsDic.GetValueByIndex(index);
			SetFacialWeight(valueByIndex, blendShapeName, from, weight);
		}
	}

	private void SetFacialWeight(PartsData parts, string blendShapeName, object from, float weight)
	{
		if (!String_Extension_Method.IsNullOrEmpty(blendShapeName) && from != null && !(parts.Render == null) && parts.BlendDic.TryGetValue(blendShapeName, out var o_value))
		{
			o_value.WeightDic.AddAndOverride(from, weight);
			parts.Render.SetBlendShapeWeight(o_value.BlendShapeIndex, o_value.CalcWeight() * 100f);
		}
	}

	public void Close()
	{
		Owner = null;
		for (int i = 0; i < PartsDic.Count; i++)
		{
			SmallPooling<PartsData>.ReleaseInstance(PartsDic.GetValueByIndex(i));
		}
		PartsDic.Clear();
	}
}
