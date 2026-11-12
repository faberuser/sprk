namespace NShared;

[JsonSerializable(Javascript = true)]
public class PlayerSelectShopInfo : IAccountAttrKey<string>
{
	string IAccountAttrKey<string>.Key => CategoryGroup;

	public string CategoryGroup { get; set; }

	public int GachaSelectIndex { get; set; }

	public int ResetCount { get; set; }

	public string CreatedTime { get; set; }

	public string RewardInfo { get; set; }
}
