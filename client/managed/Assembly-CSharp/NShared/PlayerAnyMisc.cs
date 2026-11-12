namespace NShared;

[JsonSerializable(Javascript = true)]
public class PlayerAnyMisc : IAccountAttrKey<string>
{
	string IAccountAttrKey<string>.Key => MiscKey;

	public string MiscKey { get; set; }

	public string MiscValue { get; set; }

	public string EndTime { get; set; }
}
