using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class MultipleHeroSelectItemData : IItemSubData, IDataKeySelector<string>
{
	[Key(0)]
	public string ItemCode { get; set; }

	[Key(1)]
	public int HeroStar { get; set; }

	[Key(2)]
	public int HeroLevel { get; set; }

	[Key(3)]
	public int HeroTranscend { get; set; }

	[Key(4)]
	public int AllOwnedRewardIndex1 { get; set; }

	[Key(5)]
	public string AllOwendRewardDesc1 { get; set; }

	[Key(6)]
	public string SelectTitle1 { get; set; }

	[Key(7)]
	public string SelectDesc1 { get; set; }

	[Key(8)]
	public int[] HeroIndices1 { get; set; }

	[Key(9)]
	public int AllOwnedRewardIndex2 { get; set; }

	[Key(10)]
	public string AllOwendRewardDesc2 { get; set; }

	[Key(11)]
	public string SelectTitle2 { get; set; }

	[Key(12)]
	public string SelectDesc2 { get; set; }

	[Key(13)]
	public int[] HeroIndices2 { get; set; }

	string IDataKeySelector<string>.Key => ItemCode;

	public override string ToString()
	{
		return "ItemCode=" + ItemCode;
	}
}
