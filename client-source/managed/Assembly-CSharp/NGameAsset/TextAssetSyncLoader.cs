using System;
using System.IO;
using NGameAsset.NPatch;
using NShared;
using NVespa;

namespace NGameAsset;

public class TextAssetSyncLoader : ITextLoader
{
	void ITextLoader.Load(string path, Action<string> onLoad)
	{
		string path2 = PatchDownloader.GetDownloadedAssetPath(UnityCache.persistentDataPath, path);
		if (DefinePlatform.IsEditor && Define.LocalAssetLoading.Value)
		{
			path2 = DefinePlatform.GetLocalDataPath(path);
		}
		if (File.Exists(path2))
		{
			try
			{
				string i_argA = File.ReadAllText(path2);
				Vespa_System_Action_Extension_Method.fire(onLoad, i_argA);
			}
			catch (Exception)
			{
				Vespa_System_Action_Extension_Method.fire(onLoad, null);
			}
		}
	}
}
