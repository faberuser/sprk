using System;
using System.Collections.Generic;
using JsonBin;
using LitJson;
using NGameAsset.NSound;
using NVespa;
using UnityEngine;

namespace NGameAsset.NEffect;

public class OpenDataFromResource : MonoBehaviour, IEffectOpenData, IInspectorGuiDrawer
{
	[Serializable]
	public class NamedAsset
	{
		public string Name = string.Empty;

		public GameObject Asset;
	}

	public class AssetFactory<TData> : IAssetFactory<GameObject> where TData : class
	{
		public TData Data;

		public GameObject Asset;

		public AssetFactory(TData data, GameObject asset)
		{
			Data = data;
			Asset = asset;
		}

		GameObject IAssetFactory<GameObject>.Create()
		{
			if (Asset == null)
			{
				return null;
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(Asset);
			if (gameObject == null)
			{
				return null;
			}
			return gameObject;
		}

		void IAssetFactory<GameObject>.Release(GameObject assetInstance)
		{
			assetInstance.transform.parent = null;
			VespaObject.PendingDestroyGo(assetInstance);
		}

		void IAssetFactory<GameObject>.Close()
		{
		}

		void IAssetFactory<GameObject>.DestroyUnused()
		{
		}
	}

	public string Code = string.Empty;

	public TextAsset JsonText;

	[HideInInspector]
	public List<NamedAsset> NamedAssetList = new List<NamedAsset>();

	private bool _initialized;

	private BaseData _base = new BaseData();

	private List<AssetFactory<ModelData>> _modelList = new List<AssetFactory<ModelData>>();

	private List<AssetFactory<ParticleData>> _particleList = new List<AssetFactory<ParticleData>>();

	private List<AssetFactory<TrailData>> _trailList = new List<AssetFactory<TrailData>>();

	public EffectInstance CreateEffect(int layer, TransformData extraTransform = null)
	{
		Initialize();
		EffectInstance effectInstance = new EffectInstance();
		if (!effectInstance.Open(this, layer, extraTransform))
		{
			effectInstance.Release();
			return null;
		}
		return effectInstance;
	}

	private static void ReleaseAssetFactoryList<TData>(List<AssetFactory<TData>> list) where TData : class
	{
		for (int i = 0; i < list.Count; i++)
		{
			AssetFactory<TData> assetFactory = list[i];
			assetFactory.Data = null;
			assetFactory.Asset = null;
		}
		list.Clear();
	}

	public void Release()
	{
		ReleaseAssetFactoryList(_modelList);
		ReleaseAssetFactoryList(_particleList);
		ReleaseAssetFactoryList(_trailList);
		_initialized = false;
	}

	public void Initialize()
	{
		if (_initialized)
		{
			return;
		}
		_initialized = true;
		if (JsonText == null)
		{
			return;
		}
		JsonData jsonData = JsonMapper.ToObject(JsonText.text);
		if (!jsonData.IsObject())
		{
			return;
		}
		_base.LOADFROMJSON(new JsonRead(jsonData["Base"]));
		List<ModelData> list = new List<ModelData>();
		List<ParticleData> list2 = new List<ParticleData>();
		List<TrailData> list3 = new List<TrailData>();
		ModelData.LoadList(new JsonRead(jsonData["ModelList"]), list);
		ParticleData.LoadList(new JsonRead(jsonData["ParticleList"]), list2);
		TrailData.LoadList(new JsonRead(jsonData["TrailList"]), list3);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(list); i++)
		{
			ModelData modelData = list[i];
			GameObject assetByName = GetAssetByName(modelData.Name);
			if (!(assetByName == null))
			{
				_modelList.Add(new AssetFactory<ModelData>(modelData, assetByName));
			}
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(list2); j++)
		{
			ParticleData particleData = list2[j];
			GameObject assetByName2 = GetAssetByName(particleData.Name);
			if (!(assetByName2 == null))
			{
				_particleList.Add(new AssetFactory<ParticleData>(particleData, assetByName2));
			}
		}
		for (int k = 0; k < list3.Count; k++)
		{
			TrailData trailData = list3[k];
			GameObject assetByName3 = GetAssetByName(trailData.Name);
			if (!(assetByName3 == null))
			{
				_trailList.Add(new AssetFactory<TrailData>(trailData, assetByName3));
			}
		}
	}

	private GameObject GetAssetByName(string name)
	{
		for (int i = 0; i < NamedAssetList.Count; i++)
		{
			if (NamedAssetList[i].Name == name)
			{
				return NamedAssetList[i].Asset;
			}
		}
		return null;
	}

	string IEffectOpenData.GetCode()
	{
		return Code;
	}

	BaseData IEffectOpenData.GetBaseData()
	{
		return _base;
	}

	int IEffectOpenData.GetModelCount()
	{
		return __Extension_Method_ICollection__.COUNT(_modelList);
	}

	IModelData IEffectOpenData.GetModelData(int index)
	{
		if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(_modelList, index))
		{
			return null;
		}
		return _modelList[index].Data;
	}

	IAssetFactory<GameObject> IEffectOpenData.GetModelFactory(int index)
	{
		if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(_modelList, index))
		{
			return null;
		}
		return _modelList[index];
	}

	int IEffectOpenData.GetParticleCount()
	{
		return __Extension_Method_ICollection__.COUNT(_particleList);
	}

	IParticleData IEffectOpenData.GetParticleData(int index)
	{
		if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(_particleList, index))
		{
			return null;
		}
		return _particleList[index].Data;
	}

	IAssetFactory<GameObject> IEffectOpenData.GetParticleFactory(int index)
	{
		if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(_particleList, index))
		{
			return null;
		}
		return _particleList[index];
	}

	int IEffectOpenData.GetTrailCount()
	{
		return __Extension_Method_ICollection__.COUNT(_trailList);
	}

	ITrailData IEffectOpenData.GetTrailData(int index)
	{
		if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(_trailList, index))
		{
			return null;
		}
		return _trailList[index].Data;
	}

	IAssetFactory<GameObject> IEffectOpenData.GetTrailFactory(int index)
	{
		if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(_trailList, index))
		{
			return null;
		}
		return _trailList[index];
	}

	int IEffectOpenData.GetSoundFxCount()
	{
		return 0;
	}

	SoundFxData IEffectOpenData.GetSoundFxData(int index)
	{
		return null;
	}
}
