using JsonBin;

namespace NGameAsset.NCreature;

public class CreatureBaseData : ILoadJson
{
	public enum ERotationType
	{
		Normal,
		Blend,
		Ignore
	}

	public ERotationType RotationType;

	public float RotationBlendMaxSpeed = 180f;

	public float Scale = 1f;

	bool ILoadJson.Load(JsonRead json)
	{
		RotationType = json["RotationType"].GETENUM(RotationType);
		RotationBlendMaxSpeed = json["RotationBlendMaxSpeed"].GETFLOAT(RotationBlendMaxSpeed);
		Scale = json["Scale"].GETFLOAT(Scale);
		return true;
	}
}
