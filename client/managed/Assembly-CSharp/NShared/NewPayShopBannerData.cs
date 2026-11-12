using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class NewPayShopBannerData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public PayShopUserGroup Group { get; set; }

	[Key(2)]
	public BannerPlayTypes Type { get; set; }

	[Key(3)]
	public string Url { get; set; }

	[Key(4)]
	public string MoveCategory { get; set; }

	[Key(5)]
	public int OpenProductIndex { get; set; }

	[Key(6)]
	public int Sort { get; set; }

	[Key(7)]
	public bool IsEnable { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return "PayShop Banner Index=" + Index + " Group=" + Group.ToString() + " URL=" + Url;
	}
}
