namespace NShared;

[JsonSerializable(Javascript = true)]
public class HeroStorageSlotInfo : IAccountAttrKey<string>
{
	string IAccountAttrKey<string>.Key => StorageKey;

	public string StorageKey { get; set; }

	public string Name { get; set; }

	public string LinkInfo { get; set; }

	public string UpdatedTime { get; set; }
}
