using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class CreatureAwakeChallengeItemData : IDataKeySelector<CreatureTagType>
{
	[Key(0)]
	public CreatureTagType TagType { get; set; }

	[Key(1)]
	public string ItemCode1 { get; set; }

	[Key(2)]
	public string ItemCode2 { get; set; }

	[Key(3)]
	public string ItemCode3 { get; set; }

	[Key(4)]
	public string TranscendItemCode1 { get; set; }

	[Key(5)]
	public string TranscendItemCode2 { get; set; }

	[Key(6)]
	public string TranscendItemCode3 { get; set; }

	CreatureTagType IDataKeySelector<CreatureTagType>.Key => TagType;

	public string GetItemCode(int slotIndex)
	{
		return slotIndex switch
		{
			1 => ItemCode1, 
			2 => ItemCode2, 
			3 => ItemCode3, 
			_ => string.Empty, 
		};
	}

	public string GetTranscendItemCode(int slotIndex)
	{
		return slotIndex switch
		{
			1 => TranscendItemCode1, 
			2 => TranscendItemCode2, 
			3 => TranscendItemCode3, 
			_ => string.Empty, 
		};
	}

	public override string ToString()
	{
		return "TagType=" + TagType;
	}
}
