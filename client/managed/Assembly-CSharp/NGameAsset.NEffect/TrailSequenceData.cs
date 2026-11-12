using JsonBin;

namespace NGameAsset.NEffect;

public class TrailSequenceData : ILoadJson
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
}
