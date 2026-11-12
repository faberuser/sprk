namespace NShared.NEvent;

[JsonSerializable]
public class GlobalTimeResume : BaseGlobalTime
{
	public float GlobalEndTime { get; set; }
}
