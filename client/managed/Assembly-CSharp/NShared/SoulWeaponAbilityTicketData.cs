using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class SoulWeaponAbilityTicketData : IDataKeySelector<string>
{
	[Key(0)]
	public string ItemCode { get; set; }

	[Key(1)]
	public ESoulWeaponAbilityTicketType ItemType { get; set; }

	[Key(2)]
	public int[] FixedEquipItemIndex { get; set; }

	[Key(3)]
	public int SoulWeaponGrade { get; set; }

	[Key(4)]
	public int EtherReinforceLevel { get; set; }

	string IDataKeySelector<string>.Key => ItemCode;
}
