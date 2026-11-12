using System;
using System.Collections.Generic;
using System.IO;
using NPatchGenerator;
using NShared;
using NVespa;
using NVespa.NUtil;
using UnityEngine;

namespace NGameAsset.NPatch;

public class LocalPatchInfoCollector
{
	public bool requestStop;

	public void CollectLocalPatchInfoFromRealFile(Action<PatchInfo> OnEnd, Action<float> onChangeProgress = null)
	{
		PatchInfo i_argA = CreatePatchInfo();
		string[] array = null;
		string downloadedAssetPath = PatchDownloader.GetDownloadedAssetPath(UnityCache.persistentDataPath, string.Empty);
		if (!Directory.Exists(downloadedAssetPath))
		{
			Vespa_System_Action_Extension_Method.fire(OnEnd, i_argA);
			return;
		}
		try
		{
			array = Directory.GetFiles(downloadedAssetPath, "*", SearchOption.AllDirectories);
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.StackTrace);
			Vespa_System_Action_Extension_Method.fire(OnEnd, i_argA);
			return;
		}
		if (__Extension_Method_ICollection__.COUNT(array) == 0)
		{
			Vespa_System_Action_Extension_Method.fire(OnEnd, i_argA);
		}
		else
		{
			Collect(OnEnd, onChangeProgress, array, downloadedAssetPath);
		}
	}

	private void Collect(Action<PatchInfo> OnEnd, Action<float> onChangeProgress, string[] patchFiles, string downloadedPath)
	{
		Executer executer = SingletonUpdater<Executer>.instance;
		SingletonUpdater<ThreadManager>.instance.Enqueue((Action)delegate
		{
			PatchInfo patchInfo = CreatePatchInfo();
			int num = 0;
			float num2 = __Extension_Method_ICollection__.COUNT(patchFiles);
			float num3 = 0f;
			string[] array = patchFiles;
			foreach (string text in array)
			{
				if (requestStop)
				{
					return;
				}
				string text2 = text.Replace(downloadedPath, string.Empty);
				FileUtility.TryGetFileInfos(text, out var Md, out var WriteTime, out var Length);
				AssetInfo item = new AssetInfo
				{
					EntryName = text2.Replace("\\", "/"),
					Length = Length,
					Md5 = Md,
					Time = WriteTime
				};
				patchInfo.Archives[0].AssetInfos.Add(item);
				num++;
				float progressRatio = (float)num / num2;
				if (Mathf.Abs(progressRatio - num3) > 0.01f)
				{
					num3 = progressRatio;
					executer.NextRun(delegate
					{
						Vespa_System_Action_Extension_Method.fire(onChangeProgress, progressRatio);
					});
				}
			}
			executer.NextRun(delegate
			{
				Vespa_System_Action_Extension_Method.fire(OnEnd, patchInfo);
			});
		}, (Action)null);
	}

	private PatchInfo CreatePatchInfo()
	{
		PatchInfo patchInfo = new PatchInfo();
		patchInfo.Archives = new List<ArchiveInfo>();
		patchInfo.Archives.Add(new ArchiveInfo());
		patchInfo.Archives[0].AssetInfos = new List<AssetInfo>();
		return patchInfo;
	}
}
