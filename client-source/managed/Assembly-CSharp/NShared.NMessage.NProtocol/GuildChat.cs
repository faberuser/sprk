namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class GuildChat : BaseChat
{
	public bool IsGuildNotice { get; set; }
}
