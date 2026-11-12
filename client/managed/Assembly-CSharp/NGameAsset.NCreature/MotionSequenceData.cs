using System.Collections.Generic;
using JsonBin;

namespace NGameAsset.NCreature;

public class MotionSequenceData : ILoadJson
{
	public class MotionData : ILoadJson
	{
		public AnimationKeyData Key;

		public DefineCreature.EMotionEnd MotionEnd;

		public float EndDelay;

		public bool BlendMotion;

		bool ILoadJson.Load(JsonRead json)
		{
			Key = AnimationKeyData.CreateFromJson(json["Key"]);
			MotionEnd = json["MotionEnd"].GETENUM(MotionEnd);
			EndDelay = json["EndDelay"].GETFLOAT(EndDelay);
			BlendMotion = json["BlendMotion"].GETBOOLEAN(BlendMotion);
			return true;
		}
	}

	public List<MotionData> MotionList = new List<MotionData>();

	public MotionSequenceData()
	{
	}

	public MotionSequenceData(DefineCreature.EMotion motionType, DefineCreature.EMotionEnd motionEnd)
	{
		MotionData item = new MotionData
		{
			Key = new AnimationKeyData(motionType, string.Empty),
			MotionEnd = motionEnd
		};
		MotionList.Add(item);
	}

	public MotionSequenceData(AnimationKeyData animationKey)
	{
		MotionData item = new MotionData
		{
			Key = animationKey
		};
		MotionList.Add(item);
	}

	bool ILoadJson.Load(JsonRead json)
	{
		MotionList.LoadListFromJson(json["MotionList"]);
		return true;
	}
}
