namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class DelegateGuildMaster
{
	public long PrevMasterId { get; set; }

	public long CurMasterId { get; set; }

	public string PrevMasterNick { get; set; }

	public string CurMasterNick { get; set; }

	public bool Forced { get; set; }
}
