namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class WebEventEvent
{
	public int Index { get; set; }

	public bool HasAReward { get; set; }

	public bool IsComplete { get; set; }
}
