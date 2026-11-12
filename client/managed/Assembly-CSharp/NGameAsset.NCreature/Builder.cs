using System.Collections.Generic;
using JsonBin;
using NGameAsset.NAsyncLoad;
using NVespa;
using UnityEngine;

namespace NGameAsset.NCreature;

public class Builder
{
	public string Code;

	public CreatureBaseData CreatureBase = new CreatureBaseData();

	public List<ModelData> ModelList = new List<ModelData>();

	public List<HairData> HairList = new List<HairData>();

	public List<FacialModelData> FacialModelList = new List<FacialModelData>();

	public AnimationBlendData AnimationBlend = new AnimationBlendData();

	public List<AnimationData> AnimationList = new List<AnimationData>();

	public List<PivotData> PivotList = new List<PivotData>();

	public List<AnimationSampleData> AnimationSampleList = new List<AnimationSampleData>();

	public List<MotionSequenceData> MotionSequenceList = new List<MotionSequenceData>();

	public List<SelfEffectData> SelfEffectList = new List<SelfEffectData>();

	public List<WeaponData> WeaponList = new List<WeaponData>();

	public PoolingList<SubCreatureData> SubCreatureList;

	public bool Open(string code, JsonRead json)
	{
		Code = code;
		if (!json.IsObject())
		{
			return false;
		}
		if (!LoadFromJson(json))
		{
			return false;
		}
		return true;
	}

	public void DestroyOnSceneChanged()
	{
		for (int i = 0; i < AnimationSampleList.Count; i++)
		{
			AnimationSampleList[i].DestroyOnSceneChanged();
		}
	}

	private bool LoadFromJson(JsonRead json)
	{
		string sTRING = json["BundleTypeName"].GETSTRING("(empty)");
		string sTRING2 = json["VersionCode"].GETSTRING("(empty)");
		if (!Vespa_Common_Class_Extension_Method.EQUALS(sTRING, "[Creature]"))
		{
			return false;
		}
		if (!Vespa_Common_Class_Extension_Method.EQUALS(sTRING2, "20150524"))
		{
			return false;
		}
		CreatureBase.LOADFROMJSON(json["CreatureBase"]);
		ModelData.LoadList(json["ModelList"], ModelList);
		HairData.LoadList(json["HairList"], HairList);
		FacialModelData.LoadList(json["FacialModelList"], FacialModelList);
		AnimationBlend.LOADFROMJSON(json["AnimationBlend"]);
		AnimationData.LoadList(json["AnimationList"], AnimationList);
		PivotData.LoadList(json["PivotList"], PivotList);
		AnimationSampleList.LoadListFromJson(json["AnimationSampleList"]);
		MotionSequenceList.LoadListFromJson(json["MotionSequenceList"]);
		SelfEffectList.LoadListFromJson(json["SelfEffectList"]);
		WeaponData.LoadList(json["WeaponList"], WeaponList);
		SubCreatureData.LoadList(json["SubCreatureList"], ref SubCreatureList);
		return true;
	}

	public OpenData CreateOpenData(CreatureStock stock)
	{
		if (ModelData.SelectDataFromList(stock.Star, stock.BodyPrefix, ModelList) == null)
		{
			return null;
		}
		OpenData openData = new OpenData();
		if (!openData.Open(this, stock))
		{
			openData.Close();
			return null;
		}
		return openData;
	}

	public AnimationSampleData GetAnimationSampleData(string sampleName)
	{
		for (int i = 0; i < AnimationSampleList.Count; i++)
		{
			if (AnimationSampleList[i].Name == sampleName)
			{
				return AnimationSampleList[i];
			}
		}
		return null;
	}

	public List<AnimationData> CreateAnimationList(ModelData modelData, CreatureStock.ELoadLevel loadLevel)
	{
		return AnimationData.GetAnimationDataList(AnimationList, modelData, loadLevel);
	}

	public AnimationData GetAnimationData(ModelData modelData, AnimationKeyData key)
	{
		List<AnimationData> animationList = AnimationList;
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(modelData.AnimationList))
		{
			animationList = modelData.AnimationList;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(animationList); i++)
		{
			AnimationData animationData = animationList[i];
			if (AnimationKeyData.Comparer.Default.Equals(animationData.Key, key))
			{
				return animationData;
			}
		}
		return null;
	}

	public List<AssetDataAsync<Mesh, FacialModelData>> CreateFacialModelAsyncList(ModelData modelData)
	{
		List<FacialModelData> facialModelList = FacialModelList;
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(modelData.FacialModelList))
		{
			facialModelList = modelData.FacialModelList;
		}
		List<AssetDataAsync<Mesh, FacialModelData>> list = new List<AssetDataAsync<Mesh, FacialModelData>>();
		for (int i = 0; i < facialModelList.Count; i++)
		{
			AssetDataAsync<Mesh, FacialModelData> assetDataAsync = facialModelList[i].CreateAssetAsync();
			if (assetDataAsync == null)
			{
				return null;
			}
			list.Add(assetDataAsync);
		}
		return list;
	}

	public List<SelfEffectData> CreateSelfEffectList(ModelData modelData)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(modelData.SelfEffectList))
		{
			return SelfEffectList;
		}
		List<SelfEffectData> list = new List<SelfEffectData>(SelfEffectList);
		list.AddRange(modelData.SelfEffectList);
		return list;
	}
}
