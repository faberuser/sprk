using System;
using JsonBin;
using UnityEngine;

namespace NGameAsset.NCreature.NAnimationSample;

public class SampleFrameList : ILoadJson
{
	public AnimationSampleData.SampleFrame[] Array;

	public int Count => __Extension_Method_ICollection__.COUNT(Array);

	public AnimationSampleData.SampleFrame Get(int index)
	{
		return Array[index];
	}

	public bool IsValidIndex(int index)
	{
		return Vespa_Generic_IList_Extension_Method.IsValidIndex(Array, index);
	}

	public bool GetNeighbor(out AnimationSampleData.SampleFrame o_minT, out AnimationSampleData.SampleFrame o_maxT, Func<AnimationSampleData.SampleFrame, int> decisionFunc)
	{
		return Vespa_Generic_IList_Extension_Method.GetNeighbor(Array, out o_minT, out o_maxT, decisionFunc);
	}

	bool ILoadJson.Load(JsonRead json)
	{
		if (!json.IsArray() || json.Count == 0)
		{
			return false;
		}
		Array = new AnimationSampleData.SampleFrame[json.Count];
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(Array); i++)
		{
			JsonRead fromArray = json.GetFromArray(i);
			int iNT = fromArray["Frame"].GETINT(0);
			Vector3 vECTOR = fromArray["Position"].GETVECTOR3(Vector3.zero);
			Quaternion qUATERNION = fromArray["Rotation"].GETQUATERNION(Quaternion.identity);
			AnimationSampleData.SampleFrame sampleFrame = AnimationSampleData.SampleFrame.CreateSample(iNT, vECTOR, qUATERNION, isDynamic: false);
			Array[i] = sampleFrame;
		}
		return true;
	}
}
