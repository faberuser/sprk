using JsonBin;

namespace NGameAsset.NCreature;

public class EventSoundFx : ILoadJson
{
	public float Frame;

	public string SoundCode = string.Empty;

	bool ILoadJson.Load(JsonRead json)
	{
		Frame = json["Frame"].GETFLOAT(Frame);
		SoundCode = json["SoundCode"].GETSTRING(SoundCode);
		return true;
	}
}
