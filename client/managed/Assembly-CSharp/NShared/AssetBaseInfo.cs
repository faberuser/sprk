using System;

namespace NShared;

[JsonSerializable]
public class AssetBaseInfo
{
	public string AssetName;

	public int Index;

	public int Cat { get; set; }

	public int Pri { get; set; }

	public int Length { get; set; }

	public string Md5 { get; set; }

	public uint Crc { get; set; }

	public long Time { get; set; }

	public AssetBaseInfo()
	{
		Init();
	}

	public static int GetDownloadCategoryValue(string category)
	{
		try
		{
			object obj = Enum.Parse(typeof(EDownloadCategory), category);
			if (obj == null)
			{
				return 1;
			}
			return (int)obj;
		}
		catch
		{
			return 1;
		}
	}

	public static string GetDownloadCategoryString(int value)
	{
		EDownloadCategory eDownloadCategory = (EDownloadCategory)value;
		if (Enum.IsDefined(typeof(EDownloadCategory), value))
		{
			return eDownloadCategory.ToString();
		}
		return EDownloadCategory.EnterLobbyScene.ToString();
	}

	public virtual void Init()
	{
		Index = 0;
		int downloadCategoryValue = GetDownloadCategoryValue(EDownloadCategory.EnterLobbyScene.ToString());
		SetBaseInfo(string.Empty, downloadCategoryValue, 0);
	}

	public void SetBaseInfo(string assetName, int category, int priorityValue)
	{
		AssetName = assetName;
		Cat = category;
		Pri = priorityValue;
	}

	public void SetFileInfos(int length, string md5, uint crc32, long writeTime)
	{
		Length = length;
		Md5 = md5;
		Crc = crc32;
		Time = writeTime;
	}
}
