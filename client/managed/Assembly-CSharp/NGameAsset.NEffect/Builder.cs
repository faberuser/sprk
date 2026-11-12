using System.Collections.Generic;
using JsonBin;
using NGameAsset.NAsyncLoad;
using NGameAsset.NSound;
using NVespa;
using UnityEngine;

namespace NGameAsset.NEffect;

public class Builder
{
	public string Code;

	public BaseData Base = new BaseData();

	private PoolingList<ModelData> _modelList;

	private PoolingList<ParticleData> _particleList;

	private PoolingList<TrailData> _trailList;

	private PoolingList<SoundFxData> _soundFxList;

	public bool Open(string code, JsonRead json)
	{
		code.Contains("UI.HeroesMain.RoomOfOrdeal");
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

	public int GetSoundFxCount()
	{
		return _soundFxList.Count;
	}

	public SoundFxData GetSoundFxData(int index)
	{
		if (!_soundFxList.IsValidIndex(index))
		{
			return null;
		}
		return _soundFxList[index];
	}

	public OpenData CreateOpenData()
	{
		OpenData openData = new OpenData();
		openData.Open(this);
		return openData;
	}

	public void CreateModelAsyncList(ref PoolingList<AssetDataAsync<GameObject, ModelData>> asyncList)
	{
		asyncList.Clear();
		for (int i = 0; i < _modelList.Count; i++)
		{
			ModelData modelData = _modelList[i];
			if (modelData.Common.EnableSpec.CheckEnable())
			{
				asyncList.Add(modelData.CreateAssetAsync());
			}
		}
	}

	public void CreateParticleAsyncList(ref PoolingList<AssetDataAsync<GameObject, ParticleData>> asyncList)
	{
		asyncList.Clear();
		for (int i = 0; i < _particleList.Count; i++)
		{
			ParticleData particleData = _particleList[i];
			if (particleData.Common.EnableSpec.CheckEnable())
			{
				asyncList.Add(particleData.CreateAssetAsync());
			}
		}
	}

	public void CreateTrailAsyncList(ref PoolingList<AssetDataAsync<GameObject, TrailData>> asyncList)
	{
		asyncList.Clear();
		for (int i = 0; i < _trailList.Count; i++)
		{
			TrailData trailData = _trailList[i];
			if (trailData.Common.EnableSpec.CheckEnable())
			{
				asyncList.Add(trailData.CreateAssetAsync());
			}
		}
	}

	public void CreateAudioClipAsyncList(ref PoolingList<IAsyncLoad<AudioClip>> asyncList)
	{
		asyncList.Clear();
		List<IAsyncLoad<AudioClip>> list = asyncList.PopList();
		for (int i = 0; i < _soundFxList.Count; i++)
		{
			SoundFxData soundFxData = _soundFxList[i];
			if (soundFxData.EnableSpec.CheckEnable())
			{
				list = soundFxData.ReserveAudioClipAsync(list);
			}
		}
		asyncList.PushList(list);
	}

	private bool LoadFromJson(JsonRead json)
	{
		string sTRING = json["BundleTypeName"].GETSTRING("(empty)");
		string sTRING2 = json["VersionCode"].GETSTRING("(empty)");
		if (!Vespa_Common_Class_Extension_Method.EQUALS(sTRING, "[Effect]"))
		{
			return false;
		}
		if (!Vespa_Common_Class_Extension_Method.EQUALS(sTRING2, "20150623"))
		{
			return false;
		}
		Base.LOADFROMJSON(json["Base"]);
		ModelData.LoadList(json["ModelList"], ref _modelList);
		ParticleData.LoadList(json["ParticleList"], ref _particleList);
		TrailData.LoadList(json["TrailList"], ref _trailList);
		SoundFxData.LoadList(json["SoundFxList"], ref _soundFxList);
		return true;
	}
}
