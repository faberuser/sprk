using System.Collections.Generic;

namespace NGame2.NBattleView.NCamera;

public class FreezeCamera : CameraBase
{
	public override float EnterWeightPerSec => 0f;

	public override float LeaveWeightPerSec => 0f;

	public override void Reset()
	{
	}

	public void CopyFrom(CameraBase from)
	{
		Dic.Clear();
		for (int i = 0; i < from.Dic.Count; i++)
		{
			KeyValuePair<int, BoundsInfo> keyValuePairByIndex = from.Dic.GetKeyValuePairByIndex(i);
			int key = keyValuePairByIndex.Key;
			BoundsInfo value = keyValuePairByIndex.Value;
			BoundsInfo boundsInfo = new BoundsInfo(value.Uid);
			boundsInfo.Bounds = value.Bounds;
			boundsInfo.Weight = value.Weight;
			boundsInfo.LeaveDelay = value.LeaveDelay;
			boundsInfo.LastUpdateFrame = value.LastUpdateFrame;
			Dic.Add(key, boundsInfo);
		}
	}

	protected override bool IsAcceptable(Creature creature)
	{
		return false;
	}
}
