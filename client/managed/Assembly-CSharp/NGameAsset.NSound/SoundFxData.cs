using System.Collections.Generic;
using JsonBin;
using NVespa;
using UnityEngine;

namespace NGameAsset.NSound;

public class SoundFxData : ILoadJson
{
	public const int ReserveCount = 1;

	public EnableSpecData EnableSpec = new EnableSpecData();

	public string Name { get; private set; }

	public string SoundCode { get; private set; }

	public float BeginTime { get; private set; }

	public bool EnableEndTime { get; private set; }

	public float EndTime { get; private set; }

	public List<IAsyncLoad<AudioClip>> ReserveAudioClipAsync(List<IAsyncLoad<AudioClip>> asyncList)
	{
		return SingletonAuto<SoundContainer>.instance.ReserveSound(SoundCode, asyncList);
	}

	bool ILoadJson.Load(JsonRead json)
	{
		Name = json["Name"].GETSTRING(Name);
		SoundCode = json["SoundCode"].GETSTRING(SoundCode);
		BeginTime = json["BeginTime"].GETFLOAT(BeginTime);
		EnableEndTime = json["EnableEndTime"].GETBOOLEAN(EnableEndTime);
		EndTime = json["EndTime"].GETFLOAT(EndTime);
		EnableSpec.LOADFROMJSON(json["EnableSpec"]);
		return true;
	}

	public static void LoadList(JsonRead json, ref PoolingList<SoundFxData> poolingList)
	{
		poolingList.Clear();
		if (json.IsArray() && json.Count != 0)
		{
			List<SoundFxData> list = poolingList.PopList(forceCreate: true);
			LoadList(json, list);
			poolingList.PushList(list);
		}
	}

	public static void LoadList(JsonRead json, List<SoundFxData> soundFxList)
	{
		soundFxList.Clear();
		if (!json.IsArray())
		{
			return;
		}
		for (int i = 0; i < json.Count; i++)
		{
			JsonRead fromArray = json.GetFromArray(i);
			SoundFxData soundFxData = new SoundFxData();
			if (soundFxData.LOADFROMJSON(fromArray))
			{
				soundFxList.Add(soundFxData);
			}
		}
	}
}
