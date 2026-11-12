using System;
using System.Collections.Generic;

namespace NShared;

[JsonSerializable]
public class ArchiveInfo : AssetBaseInfo, ICloneable
{
	public static Comparison<ArchiveInfo> PriorityAscendingSort = delegate(ArchiveInfo a, ArchiveInfo b)
	{
		if (a.Pri == 0)
		{
			if (b.Pri > 0)
			{
				return 1;
			}
		}
		else if (b.Pri == 0)
		{
			if (a.Pri > 0)
			{
				return -1;
			}
		}
		else
		{
			if (a.Pri < b.Pri)
			{
				return -1;
			}
			if (a.Pri > b.Pri)
			{
				return 1;
			}
		}
		return 0;
	};

	public string Name { get; set; }

	public int SrcVersion { get; set; }

	public bool Zip { get; set; }

	public List<AssetInfo> AssetInfos { get; set; }

	public PatchArchiveType ArchiveType { get; set; }

	public ArchiveInfo()
	{
		Init();
	}

	public override void Init()
	{
		base.Init();
	}

	public string GetArchiveFilePath(string outputPrefix)
	{
		return outputPrefix + "/" + SrcVersion + "/" + Name;
	}

	public void SetBaseInfo(AssetInfo assetInfo)
	{
		if (assetInfo.Pri != 0 && (base.Pri == 0 || base.Pri > assetInfo.Pri))
		{
			base.Pri = assetInfo.Pri;
		}
		base.Cat = assetInfo.Cat;
	}

	public object Clone()
	{
		ArchiveInfo archiveInfo = (ArchiveInfo)MemberwiseClone();
		archiveInfo.Name = ((Name == null) ? string.Empty : string.Copy(Name));
		archiveInfo.SrcVersion = SrcVersion;
		archiveInfo.Length = base.Length;
		archiveInfo.Md5 = ((base.Md5 == null) ? string.Empty : string.Copy(base.Md5));
		archiveInfo.Crc = base.Crc;
		archiveInfo.Time = base.Time;
		archiveInfo.Zip = Zip;
		archiveInfo.Pri = base.Pri;
		archiveInfo.AssetInfos = new List<AssetInfo>();
		foreach (AssetInfo assetInfo in AssetInfos)
		{
			archiveInfo.AssetInfos.Add((AssetInfo)assetInfo.Clone());
		}
		return archiveInfo;
	}
}
