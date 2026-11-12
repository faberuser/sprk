using System;
using System.IO;
using NGameAsset.NPatch;
using NPatchGenerator;
using NShared;
using NShared.NUtil;
using NVespa;

namespace NGameAsset;

public class TextAssetAsyncLoader : ITextLoader
{
	public enum FileReadMethodType
	{
		Thread
	}

	public static LocalStorageEnum<FileReadMethodType> FileReadMethod = new LocalStorageEnum<FileReadMethodType>(FileReadMethodType.Thread, typeof(TextAssetAsyncLoader).FullName + "_FileReadMode2");

	void ITextLoader.Load(string path, Action<string> onLoad)
	{
		GetAssetFileInfo(path, out var assetPath, out var isEncrypt, out var _);
		ReadDirect(assetPath, isEncrypt, onLoad);
	}

	public void GetAssetFileInfo(string path, out string assetPath, out bool isEncrypt, out bool isJsonBin)
	{
		if (DefinePlatform.IsEditor && Define.LocalAssetLoading.Value)
		{
			string localDataPath = DefinePlatform.GetLocalDataPath(path);
			GetAssetFileInfoInternal(localDataPath, out assetPath, out isEncrypt, out isJsonBin);
		}
		else
		{
			GetAssetFileInfoInternal(path, out assetPath, out isEncrypt, out isJsonBin);
			assetPath = PatchDownloader.GetDownloadedAssetPath(UnityCache.persistentDataPath, assetPath);
		}
	}

	private void GetAssetFileInfoInternal(string origAssetPath, out string assetPath, out bool isEncrypt, out bool isJsonBin)
	{
		if (origAssetPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
		{
			string binaryJsonExtension = PatchGenerator.GetBinaryJsonExtension(encrypt: true);
			string text = Path.ChangeExtension(origAssetPath, binaryJsonExtension);
			if (SingletonAuto<AssetFileManager>.instance.ExistInAssetInfos(text))
			{
				assetPath = text;
				isEncrypt = true;
				isJsonBin = true;
				return;
			}
			string binaryJsonExtension2 = PatchGenerator.GetBinaryJsonExtension(encrypt: false);
			string text2 = Path.ChangeExtension(origAssetPath, binaryJsonExtension2);
			if (SingletonAuto<AssetFileManager>.instance.ExistInAssetInfos(text2))
			{
				assetPath = text2;
				isEncrypt = false;
				isJsonBin = true;
				return;
			}
		}
		int originalTextExtensionIndex = PatchGenerator.GetOriginalTextExtensionIndex(origAssetPath);
		if (originalTextExtensionIndex < 0)
		{
			assetPath = origAssetPath;
			isEncrypt = false;
			isJsonBin = false;
			return;
		}
		PatchGenerator.GetOriginalTextExtensions();
		string extension = PatchGenerator.GetConvertedTextExtensions()[originalTextExtensionIndex];
		string text3 = Path.ChangeExtension(origAssetPath, extension);
		if (SingletonAuto<AssetFileManager>.instance.ExistInAssetInfos(text3))
		{
			assetPath = text3;
			isEncrypt = true;
			isJsonBin = false;
		}
		else
		{
			assetPath = origAssetPath;
			isEncrypt = false;
			isJsonBin = false;
		}
	}

	private static string DecryptText(string path, string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		try
		{
			return Lz4.Decompress(PatchGenerator.Aes.DecryptTextLegacy(text, PatchGenerator.GetTest(first: true) + PatchGenerator.GetTest(first: false)));
		}
		catch (Exception ex)
		{
			VespaDebug.LogError("Faild to decrypt " + path + ". error=" + ex);
			return null;
		}
	}

	private static void ReadDirect(string path, bool encrypted, Action<string> onRead)
	{
		string text = null;
		try
		{
			if (File.Exists(path))
			{
				text = File.ReadAllText(path);
				if (encrypted)
				{
					text = DecryptText(path, text);
				}
			}
		}
		catch (Exception ex)
		{
			VespaDebug.LogError("Faild to load " + path + ". error=" + ex);
			text = null;
		}
		Vespa_System_Action_Extension_Method.fire(onRead, text);
	}

	private static void ReadByThread(string path, bool encrypted, Action<string> onRead)
	{
		string text = null;
		SingletonUpdater<ThreadManager>.instance.Enqueue((Action)delegate
		{
			try
			{
				if (File.Exists(path))
				{
					text = File.ReadAllText(path);
					if (encrypted)
					{
						text = DecryptText(path, text);
					}
				}
			}
			catch (Exception ex)
			{
				VespaDebug.LogError("Faild to load " + path + ". error=" + ex);
				text = null;
			}
		}, (Action)delegate
		{
			Vespa_System_Action_Extension_Method.fire(onRead, text);
		});
	}

	private static void ReadByWww(string path, bool encrypted, Action<string> onRead)
	{
		WwwText www = new WwwText("file://" + path);
		SingletonUpdater<AsyncManager>.instance.Add(www, delegate
		{
			string text = null;
			try
			{
				if (string.IsNullOrEmpty(www.error))
				{
					text = www.text;
					if (encrypted)
					{
						SingletonUpdater<ThreadManager>.instance.Enqueue((Action)delegate
						{
							text = DecryptText(path, text);
						}, (Action)delegate
						{
							Vespa_System_Action_Extension_Method.fire(onRead, text);
						});
						return;
					}
				}
				else
				{
					VespaDebug.LogError("Faild to load " + path + " : " + www.error);
				}
			}
			catch (Exception ex)
			{
				VespaDebug.LogError("Faild to load " + path + ". error=" + ex);
				text = null;
			}
			Vespa_System_Action_Extension_Method.fire(onRead, text);
		});
	}
}
