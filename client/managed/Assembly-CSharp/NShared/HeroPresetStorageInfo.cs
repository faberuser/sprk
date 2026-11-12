namespace NShared;

[JsonSerializable(Javascript = true)]
public class HeroPresetStorageInfo : IAccountAttrKey<string>
{
	string IAccountAttrKey<string>.Key => StorageKey;

	public string StorageKey { get; set; }

	public string Name { get; set; }

	public HeroPresetInfo[] HeroPresetInfos { get; set; }
}
