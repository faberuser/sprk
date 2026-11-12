namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class ChangePartyRoomMaster
{
	public long NewMasterAccountId { get; set; }
}
