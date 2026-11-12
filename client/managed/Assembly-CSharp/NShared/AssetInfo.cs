namespace NShared;

[JsonSerializable]
public class AssetInfo : AssetBaseInfo
{
	public string FullPath;

	public PatchAssetRootData RootData;

	public string EntryName { get; set; }

	public AssetInfo()
	{
		Init();
	}

	public override void Init()
	{
		base.Init();
	}

	public void SetEmpty()
	{
		EntryName = string.Empty;
		FullPath = string.Empty;
		AssetName = string.Empty;
		base.Cat = AssetBaseInfo.GetDownloadCategoryValue(EDownloadCategory.EnterLobbyScene.ToString());
		base.Pri = 0;
		SetFileInfos(0, string.Empty, 0u, 0L);
		RootData = null;
	}

	public bool IsEmpty()
	{
		if (EntryName == null || EntryName.Length <= 0)
		{
			return true;
		}
		if (FullPath == null || FullPath.Length <= 0)
		{
			return true;
		}
		if (RootData == null)
		{
			return true;
		}
		return false;
	}

	public object Clone()
	{
		AssetInfo obj = (AssetInfo)MemberwiseClone();
		obj.RootData = RootData;
		obj.FullPath = ((FullPath == null) ? string.Empty : string.Copy(FullPath));
		obj.EntryName = ((EntryName == null) ? string.Empty : string.Copy(EntryName));
		obj.Md5 = ((base.Md5 == null) ? string.Empty : string.Copy(base.Md5));
		obj.Crc = base.Crc;
		obj.Length = base.Length;
		obj.Time = base.Time;
		obj.Pri = base.Pri;
		return obj;
	}
}
