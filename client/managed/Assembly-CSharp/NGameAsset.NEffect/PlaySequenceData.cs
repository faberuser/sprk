using System.Collections.Generic;
using JsonBin;

namespace NGameAsset.NEffect;

public class PlaySequenceData : ILoadJson
{
	public enum EPlayType
	{
		Clamp,
		Loop,
		End,
		None
	}

	public EPlayType PlayType;

	public string PhaseName = string.Empty;

	public float TimeLength;

	public float ClipStartFrame;

	public float ClipEndFrame;

	public int LoopCount;

	public bool CompleteLoop;

	bool ILoadJson.Load(JsonRead json)
	{
		PlayType = json["PlayType"].GETENUM(PlayType);
		PhaseName = json["PhaseName"].GETSTRING(PhaseName);
		TimeLength = json["TimeLength"].GETFLOAT(TimeLength);
		ClipStartFrame = json["ClipStartFrame"].GETFLOAT(ClipStartFrame);
		ClipEndFrame = json["ClipEndFrame"].GETFLOAT(ClipEndFrame);
		LoopCount = json["LoopCount"].GETINT(LoopCount);
		CompleteLoop = json["CompleteLoop"].GETBOOLEAN(CompleteLoop);
		return true;
	}

	public static void LoadList(JsonRead json, List<PlaySequenceData> list)
	{
		list.Clear();
		if (!json.IsArray())
		{
			return;
		}
		for (int i = 0; i < json.Count; i++)
		{
			JsonRead fromArray = json.GetFromArray(i);
			PlaySequenceData playSequenceData = new PlaySequenceData();
			if (playSequenceData.LOADFROMJSON(fromArray))
			{
				list.Add(playSequenceData);
			}
		}
	}
}
