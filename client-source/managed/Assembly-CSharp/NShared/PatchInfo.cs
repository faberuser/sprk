using System.Collections.Generic;

namespace NShared;

[JsonSerializable]
public class PatchInfo
{
	public List<ArchiveInfo> Archives { get; set; }

	public virtual Dictionary<string, AssetInfo> GetAllAssetInfoDictionary()
	{
		Dictionary<string, AssetInfo> dictionary = new Dictionary<string, AssetInfo>();
		if (Archives == null)
		{
			return null;
		}
		foreach (ArchiveInfo archive in Archives)
		{
			foreach (AssetInfo assetInfo in archive.AssetInfos)
			{
				if (assetInfo != null && !string.IsNullOrEmpty(assetInfo.EntryName))
				{
					dictionary.Remove(assetInfo.EntryName);
					dictionary.Add(assetInfo.EntryName, assetInfo);
				}
			}
		}
		return dictionary;
	}
}
