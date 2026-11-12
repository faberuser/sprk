using System;
using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class DownloadControlDataContainer : JitDataContainerDefault<int, DownloadControlData>, IData_from_PreloadInAppBytes
{
	public const string TABLENAME = "DefaultTableData/DownloadControlTable";

	public override TablePathDefine PathDefine => new TablePathDefine("DefaultTableData/DownloadControlTable");

	public EDownloadCategory[] GetDownloadCategories(int chapterIndex)
	{
		List<EDownloadCategory> list = new List<EDownloadCategory>();
		foreach (DownloadControlData allDatum in GetAllData())
		{
			if (chapterIndex != allDatum.ChapterIndex)
			{
				continue;
			}
			string[] categories = allDatum.Categories;
			foreach (string value in categories)
			{
				try
				{
					EDownloadCategory item = (EDownloadCategory)Enum.Parse(typeof(EDownloadCategory), value);
					list.Add(item);
				}
				catch
				{
				}
			}
			return list.ToArray();
		}
		return null;
	}

	public EDownloadCategory[] GetAllDownloadCategories()
	{
		List<EDownloadCategory> list = new List<EDownloadCategory>();
		foreach (DownloadControlData allDatum in GetAllData())
		{
			string[] categories = allDatum.Categories;
			foreach (string value in categories)
			{
				try
				{
					EDownloadCategory item = (EDownloadCategory)Enum.Parse(typeof(EDownloadCategory), value);
					list.Add(item);
				}
				catch
				{
				}
			}
		}
		return list.ToArray();
	}
}
