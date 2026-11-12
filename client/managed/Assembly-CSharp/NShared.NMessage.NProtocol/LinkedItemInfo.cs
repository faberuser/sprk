namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class LinkedItemInfo
{
	public int MaxEquipLevel { get; set; }

	public EquipItemInfo EquipItemInfo { get; set; }
}
