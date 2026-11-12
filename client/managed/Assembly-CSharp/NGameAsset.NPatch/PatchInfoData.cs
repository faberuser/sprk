using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NShared;
using NVespa;
using UnityEngine;

namespace NGameAsset.NPatch;

public class PatchInfoData : PatchInfo, ICloneable
{
	public PatchInfoData()
	{
	}

	public PatchInfoData(PatchInfo patchInfo)
	{
		if (patchInfo != null && patchInfo.Archives != null)
		{
			base.Archives = patchInfo.Archives;
		}
	}

	public List<ArchiveInfo> GetDownloadArchiveList(PatchInfoData localPatchInfo)
	{
		List<ArchiveInfo> list = new List<ArchiveInfo>();
		Dictionary<string, AssetInfo> dictionary = ((localPatchInfo == null) ? new Dictionary<string, AssetInfo>() : localPatchInfo.GetAllAssetInfoDictionary());
		if (dictionary == null)
		{
			return base.Archives;
		}
		foreach (ArchiveInfo archive in base.Archives)
		{
			bool flag = false;
			foreach (AssetInfo assetInfo2 in archive.AssetInfos)
			{
				string entryName = assetInfo2.EntryName;
				if (!dictionary.ContainsKey(entryName))
				{
					Define.IsLiveBuild();
					flag = true;
					break;
				}
				AssetInfo assetInfo = dictionary[entryName];
				if (assetInfo == null)
				{
					Define.IsLiveBuild();
					flag = true;
					break;
				}
				if (assetInfo.Length != assetInfo2.Length)
				{
					Define.IsLiveBuild();
					flag = true;
					break;
				}
				if (!string.Equals(assetInfo.Md5, assetInfo2.Md5, StringComparison.InvariantCultureIgnoreCase))
				{
					Define.IsLiveBuild();
					flag = true;
					break;
				}
			}
			if (flag)
			{
				list.Add(archive);
			}
		}
		return list;
	}

	public List<ArchiveInfo> GetDownloadArchiveList(PatchInfo localPatchInfo)
	{
		List<ArchiveInfo> list = new List<ArchiveInfo>();
		PatchInfoData patchInfoData = new PatchInfoData(localPatchInfo);
		Dictionary<string, AssetInfo> dictionary = ((patchInfoData == null) ? new Dictionary<string, AssetInfo>() : patchInfoData.GetAllAssetInfoDictionary());
		if (dictionary == null)
		{
			return base.Archives;
		}
		foreach (ArchiveInfo archive in base.Archives)
		{
			bool flag = false;
			foreach (AssetInfo assetInfo2 in archive.AssetInfos)
			{
				string entryName = assetInfo2.EntryName;
				if (!dictionary.ContainsKey(entryName))
				{
					Define.IsLiveBuild();
					flag = true;
					break;
				}
				AssetInfo assetInfo = dictionary[entryName];
				if (assetInfo == null)
				{
					Define.IsLiveBuild();
					flag = true;
					break;
				}
				if (assetInfo.Length != assetInfo2.Length)
				{
					Define.IsLiveBuild();
					flag = true;
					break;
				}
				if (!string.Equals(assetInfo.Md5, assetInfo2.Md5, StringComparison.InvariantCultureIgnoreCase))
				{
					Define.IsLiveBuild();
					flag = true;
					break;
				}
			}
			if (flag)
			{
				list.Add(archive);
			}
		}
		return list;
	}

	public long GetDownloadSize(PatchInfoData localPatchInfo)
	{
		List<ArchiveInfo> downloadArchiveList = GetDownloadArchiveList(localPatchInfo);
		long num = 0L;
		for (int i = 0; i < downloadArchiveList.Count; i++)
		{
			ArchiveInfo archiveInfo = downloadArchiveList[i];
			if (archiveInfo != null)
			{
				num += archiveInfo.Length;
			}
		}
		return num;
	}

	public object Clone()
	{
		PatchInfoData patchInfoData = (PatchInfoData)MemberwiseClone();
		patchInfoData.Archives = new List<ArchiveInfo>();
		foreach (ArchiveInfo archive in base.Archives)
		{
			patchInfoData.Archives.Add((ArchiveInfo)archive.Clone());
		}
		return patchInfoData;
	}

	public void Filtering(ESpecType spec, SystemLanguage language)
	{
		FilterBySpec(spec);
		FilterByLanguage(language);
	}

	public void FilterBySpec(ESpecType spec)
	{
		if (base.Archives == null || base.Archives.Count == 0)
		{
			return;
		}
		HashSet<ArchiveInfo> hashSet = new HashSet<ArchiveInfo>();
		foreach (ArchiveInfo archive in base.Archives)
		{
			int num = archive.AssetInfos.Count;
			foreach (AssetInfo assetInfo in archive.AssetInfos)
			{
				switch (spec)
				{
				case ESpecType.Undefine:
					goto end_IL_0144;
				case ESpecType.Original:
					if (assetInfo.EntryName.IndexOf("/MidSpec/") >= 0 || assetInfo.EntryName.IndexOf("/LowSpec/") >= 0 || assetInfo.EntryName.IndexOf("/MidAndLowSpec/") >= 0 || assetInfo.EntryName.IndexOf("/LowLargeSpec/") >= 0)
					{
						num--;
					}
					break;
				}
				if (spec == ESpecType.Mid && (assetInfo.EntryName.IndexOf("/OriginalSpec/") >= 0 || assetInfo.EntryName.IndexOf("/LowSpec/") >= 0 || assetInfo.EntryName.IndexOf("/LowLargeSpec/") >= 0))
				{
					num--;
				}
				if (spec == ESpecType.Low && (assetInfo.EntryName.IndexOf("/OriginalSpec/") >= 0 || assetInfo.EntryName.IndexOf("/MidSpec/") >= 0 || assetInfo.EntryName.IndexOf("/HighAndMidSpec/") >= 0))
				{
					num--;
				}
				continue;
				end_IL_0144:
				break;
			}
			if (num <= 0)
			{
				hashSet.Add(archive);
			}
		}
		base.Archives.RemoveAll(hashSet.Contains);
	}

	public void FilterByLanguage(SystemLanguage language)
	{
		if (base.Archives == null || base.Archives.Count == 0)
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(base.Archives); i++)
		{
			ArchiveInfo archiveInfo = base.Archives[i];
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(archiveInfo.AssetInfos); j++)
			{
				AssetInfo assetInfo = archiveInfo.AssetInfos[j];
				SystemLanguage systemLanguage;
				if (IsLdicAsset(assetInfo.EntryName, out var languageEnd))
				{
					systemLanguage = CheckLanguage(assetInfo.EntryName, "Localization_", languageEnd);
					if (systemLanguage == SystemLanguage.Unknown)
					{
						continue;
					}
				}
				else
				{
					systemLanguage = CheckAssetLanguage(assetInfo.EntryName, "/[", "]/");
					if (systemLanguage == SystemLanguage.Unknown)
					{
						continue;
					}
				}
				if (systemLanguage != language)
				{
					archiveInfo.AssetInfos.RemoveAt(j);
					j--;
				}
			}
			if (__Extension_Method_ICollection__.COUNT(archiveInfo.AssetInfos) == 0)
			{
				base.Archives.RemoveAt(i);
				i--;
			}
		}
	}

	private bool IsLdicAsset(string entryName, out int languageEnd)
	{
		languageEnd = entryName.IndexOf(".ldic");
		return languageEnd >= 0;
	}

	public SystemLanguage CheckAssetLanguage(string entryName, string beginStr, string endStr)
	{
		int num = entryName.IndexOf(endStr);
		if (num < 0)
		{
			return SystemLanguage.Unknown;
		}
		return CheckLanguage(entryName, beginStr, num);
	}

	public SystemLanguage CheckLanguage(string entryName, string beginStr, int languageEnd)
	{
		int num = entryName.IndexOf(beginStr);
		if (num < 0)
		{
			return SystemLanguage.Unknown;
		}
		int length = beginStr.Length;
		if (languageEnd - num <= length)
		{
			return SystemLanguage.Unknown;
		}
		return EnumLookup<SystemLanguage>.GetByName(entryName.Substring(num + length, languageEnd - num - length), null)?.EnumValue ?? SystemLanguage.Unknown;
	}

	public void FilterCategory(EDownloadCategory[] categories)
	{
		if (base.Archives == null || base.Archives.Count == 0 || categories.Length == 0)
		{
			return;
		}
		List<int> list = new List<int>();
		foreach (EDownloadCategory item in categories)
		{
			list.Add((int)item);
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(base.Archives); j++)
		{
			ArchiveInfo archiveInfo = base.Archives[j];
			if (!list.Contains(archiveInfo.Cat))
			{
				base.Archives.RemoveAt(j);
				j--;
			}
		}
	}

	public void FilterByVideoSpec()
	{
		Dictionary<string, HashSet<EVideoSpecType>> dicTarget = new Dictionary<string, HashSet<EVideoSpecType>>();
		HashSet<ArchiveInfo> hashSet = new HashSet<ArchiveInfo>();
		foreach (ArchiveInfo archive in base.Archives)
		{
			if (archive.Name.Contains("Video/"))
			{
				string fileName = Path.GetFileName(archive.Name);
				if (dicTarget.ContainsKey(fileName))
				{
					dicTarget[fileName].Add(GetSpecType(archive.Name));
					continue;
				}
				dicTarget.Add(fileName, new HashSet<EVideoSpecType> { GetSpecType(archive.Name) });
			}
		}
		CleanUpUselessSpec(ref dicTarget);
		foreach (ArchiveInfo archive2 in base.Archives)
		{
			if (!archive2.Name.Contains("Video/"))
			{
				continue;
			}
			string fileName2 = Path.GetFileName(archive2.Name);
			if (dicTarget.ContainsKey(fileName2))
			{
				EVideoSpecType eVideoSpecType = dicTarget[fileName2].FirstOrDefault();
				if (GetSpecType(archive2.Name) != eVideoSpecType)
				{
					hashSet.Add(archive2);
				}
			}
		}
		base.Archives.RemoveAll(hashSet.Contains);
	}

	private EVideoSpecType GetSpecType(string strPath)
	{
		if (strPath.Contains("Video/Origin/"))
		{
			return EVideoSpecType.Original;
		}
		if (strPath.Contains("Video/High/"))
		{
			return EVideoSpecType.High;
		}
		if (strPath.Contains("Video/Mid/"))
		{
			return EVideoSpecType.Mid;
		}
		if (strPath.Contains("Video/Low/"))
		{
			return EVideoSpecType.Low;
		}
		return EVideoSpecType.Undefine;
	}

	private void CleanUpUselessSpec(ref Dictionary<string, HashSet<EVideoSpecType>> dicTarget)
	{
		foreach (KeyValuePair<string, HashSet<EVideoSpecType>> item in dicTarget)
		{
			if (item.Value.Contains(SystemOptionData.VideoSpec))
			{
				item.Value.Clear();
				item.Value.Add(SystemOptionData.VideoSpec);
			}
			else if (!FindSpecAndCleanUp(item))
			{
				VespaDebug.LogError($"{item.Key} Not Found..");
			}
		}
	}

	private bool FindSpecAndCleanUp(KeyValuePair<string, HashSet<EVideoSpecType>> pair)
	{
		int num = Enum.GetValues(typeof(EVideoSpecType)).Cast<int>().Min();
		int num2 = Enum.GetValues(typeof(EVideoSpecType)).Cast<int>().Max();
		bool num3 = SystemOptionData.VideoSpec == (EVideoSpecType)num2;
		int num4 = (num3 ? num : (num2 + 1));
		int num5 = (num3 ? num2 : (num + 1));
		int num6 = ((!num3) ? 1 : (-1));
		for (int i = num5; i != num4; i += num6)
		{
			EVideoSpecType eVideoSpecType = (EVideoSpecType)i;
			if (eVideoSpecType != SystemOptionData.VideoSpec && pair.Value.Contains(eVideoSpecType))
			{
				pair.Value.Clear();
				pair.Value.Add(eVideoSpecType);
				return true;
			}
		}
		return false;
	}
}
