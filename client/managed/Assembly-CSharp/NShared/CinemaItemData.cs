using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class CinemaItemData : IItemSubData, IDataKeySelector<string>
{
	[Key(0)]
	public string ItemCode { get; set; }

	[Key(1)]
	public string PackCode { get; set; }

	[Key(2)]
	public string AppearName { get; set; }

	[Key(3)]
	public string ReactionName { get; set; }

	[Key(4)]
	public string ReturnName { get; set; }

	string IDataKeySelector<string>.Key => ItemCode;

	public override string ToString()
	{
		return "ItemCode=" + ItemCode;
	}
}
