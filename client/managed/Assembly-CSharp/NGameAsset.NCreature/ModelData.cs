using System.Collections.Generic;
using JsonBin;
using NGameAsset.NAsyncLoad;
using UnityEngine;

namespace NGameAsset.NCreature;

public class ModelData : ILoadJson, IModelData
{
	public int Star;

	public string Prefix = string.Empty;

	public string BundleFileName = string.Empty;

	public string AssetName = string.Empty;

	public string HairMaterialAssetName = string.Empty;

	public string DefaultHairName = string.Empty;

	public List<string> FacialNodeList = new List<string>();

	public bool ResetRotation;

	public bool AttachBoxCollider;

	public Vector3 RotationEuler = Vector3.zero;

	private LazyLoadList<AnimationData> _animationList;

	private LazyLoadList<FacialModelData> _facialModelList;

	private LazyLoadList<SelfEffectData> _selfEffectList;

	private LazyLoadList<WeaponEffectData> _weaponEffectList;

	public List<AnimationData> AnimationList => _animationList.Value;

	public List<FacialModelData> FacialModelList => _facialModelList.Value;

	public List<SelfEffectData> SelfEffectList => _selfEffectList.Value;

	public List<WeaponEffectData> WeaponEffectList => _weaponEffectList.Value;

	int IModelData.Star => Star;

	string IModelData.Prefix => Prefix;

	public AssetDataAsync<GameObject, ModelData> CreateAssetAsync()
	{
		AssetDataAsync<GameObject, ModelData> assetDataAsync = new AssetDataAsync<GameObject, ModelData>();
		assetDataAsync.Open(this, BundleFileName, AssetName);
		return assetDataAsync;
	}

	public AssetDataAsync<Material, ModelData> CreateHairReplacementAssetAsync()
	{
		if (String_Extension_Method.IsNullOrEmpty(HairMaterialAssetName))
		{
			return null;
		}
		AssetDataAsync<Material, ModelData> assetDataAsync = new AssetDataAsync<Material, ModelData>();
		assetDataAsync.Open(this, BundleFileName, HairMaterialAssetName);
		return assetDataAsync;
	}

	string IModelData.GetFacialNodeName(int index)
	{
		if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(FacialNodeList, index))
		{
			return string.Empty;
		}
		return FacialNodeList[index];
	}

	GameObject IModelData.InstantiateModel(GameObject assetGo)
	{
		if (!assetGo)
		{
			return null;
		}
		GameObject gameObject = Object.Instantiate(assetGo);
		if (gameObject == null)
		{
			return null;
		}
		if (ResetRotation)
		{
			gameObject.transform.localRotation = Quaternion.Euler(RotationEuler);
		}
		if (AttachBoxCollider)
		{
			gameObject.AddComponent<BoxCollider>();
		}
		return gameObject;
	}

	bool ILoadJson.Load(JsonRead json)
	{
		Star = json["Star"].GETINT(Star);
		Prefix = json["Prefix"].GETSTRING(Prefix);
		BundleFileName = json["BundleFileName"].GETSTRING(BundleFileName);
		AssetName = json["AssetName"].GETSTRING(AssetName);
		HairMaterialAssetName = json["HairMaterialAssetName"].GETSTRING(HairMaterialAssetName);
		DefaultHairName = json["DefaultHairName"].GETSTRING(DefaultHairName);
		FacialNodeList.LoadListFromJson(json["FacialNodeList"]);
		ResetRotation = json["ResetRotation"].GETBOOLEAN(ResetRotation);
		RotationEuler = json["RotationEuler"].GETVECTOR3(RotationEuler);
		AttachBoxCollider = json["AttachBoxCollider"].GETBOOLEAN(AttachBoxCollider);
		_animationList.SetJsonRead(json["AnimationList"]);
		_facialModelList.SetJsonRead(json["FacialModelList"]);
		_selfEffectList.SetJsonRead(json["SelfEffectList"]);
		_weaponEffectList.SetJsonRead(json["WeaponEffectList"]);
		return true;
	}

	public static void LoadList(JsonRead json, List<ModelData> modelList)
	{
		modelList.Clear();
		if (!json.IsArray())
		{
			return;
		}
		for (int i = 0; i < json.Count; i++)
		{
			JsonRead fromArray = json.GetFromArray(i);
			ModelData modelData = new ModelData();
			if (modelData.LOADFROMJSON(fromArray))
			{
				modelList.Add(modelData);
			}
		}
	}

	public static T SelectDataFromList<T>(int star, string prefix, List<T> modelList) where T : class, IModelData
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(modelList))
		{
			return null;
		}
		if (prefix == null)
		{
			prefix = string.Empty;
		}
		T result = modelList[0];
		int num = -999;
		for (int i = 0; i < modelList.Count; i++)
		{
			T val = modelList[i];
			if (val.Star == star && Vespa_Common_Class_Extension_Method.EQUALS(val.Prefix, prefix))
			{
				return val;
			}
			int num2 = -Mathf.Abs(star - val.Star);
			if (val.Star == star)
			{
				num2 += 20;
			}
			else if (val.Star < star)
			{
				num2 += 10;
			}
			if (Vespa_Common_Class_Extension_Method.EQUALS(val.Prefix, prefix))
			{
				num2 += 200;
			}
			else if (String_Extension_Method.IsNullOrEmpty(val.Prefix))
			{
				num2 += 100;
			}
			if (num2 > num)
			{
				result = val;
				num = num2;
			}
		}
		return result;
	}

	List<SelfEffectData> IModelData.GetSelfEffectList()
	{
		return SelfEffectList;
	}
}
