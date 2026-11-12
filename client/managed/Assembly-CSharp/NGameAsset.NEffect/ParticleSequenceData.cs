using System.Collections.Generic;
using JsonBin;
using NVespa;

namespace NGameAsset.NEffect;

public class ParticleSequenceData : ILoadJson
{
	public float SequenceTime;

	public string PhaseName = string.Empty;

	public bool IsPlay;

	bool ILoadJson.Load(JsonRead json)
	{
		SequenceTime = json["SequenceTime"].GETFLOAT(SequenceTime);
		PhaseName = json["PhaseName"].GETSTRING(PhaseName);
		IsPlay = json["IsPlay"].GETBOOLEAN(IsPlay);
		return true;
	}

	public static void LoadFromJson(ref PoolingList<ParticleSequenceData> poolingList, JsonRead json)
	{
		if (json.IsArray() && json.Count != 0)
		{
			List<ParticleSequenceData> list = poolingList.PopList(forceCreate: true);
			list.LoadListFromJson(json);
			poolingList.PushList(list);
		}
	}
}
