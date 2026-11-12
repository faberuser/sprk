using System;
using System.Collections.Generic;
using System.IO;
using LitJson;
using MiniJSON;
using NGameAsset.NPatch;
using NPatchGenerator;
using NShared.NUtil;
using NVespa;

namespace NGameAsset;

public static class PlainAssetHelper
{
	public enum EPathType
	{
		Table,
		TableMessagePack,
		PlainAsset
	}

	public static string DecryptText(string path, string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		try
		{
			_ = DateTime.UtcNow;
			return Lz4.Decompress(PatchGenerator.Aes.DecryptTextLegacy(text, PatchGenerator.GetTest(first: true) + PatchGenerator.GetTest(first: false)));
		}
		catch (Exception ex)
		{
			VespaDebug.LogError("Faild to decrypt " + path + ". error=" + ex);
			return null;
		}
	}

	public static byte[] DecryptBytes(string path, byte[] bytes)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(bytes))
		{
			return null;
		}
		try
		{
			_ = DateTime.UtcNow;
			return PatchGenerator.Aes.DecryptBinary(bytes, PatchGenerator.GetTest(first: true) + PatchGenerator.GetTest(first: false));
		}
		catch (Exception ex)
		{
			VespaDebug.LogError("Faild to decrypt " + path + ". error=" + ex);
			return null;
		}
	}

	public static JsonData LoadLitJsonFromJsonBin(byte[] bytes, string path, bool encrypted)
	{
		if (bytes == null || bytes.Length == 0)
		{
			return null;
		}
		if (encrypted)
		{
			bytes = DecryptBytes(path, bytes);
		}
		return JsonBinMapper.ToObject(bytes);
	}

	public static JsonData LoadLitJsonFromJsonText(string text, string path, bool encrypted)
	{
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		if (encrypted)
		{
			text = DecryptText(path, text);
		}
		return JsonMapper.ToObject(text);
	}

	public static List<object> LoadJsonObjectFromJsonBin(byte[] bytes, string path, bool encrypted)
	{
		if (bytes == null || bytes.Length == 0)
		{
			return null;
		}
		if (encrypted)
		{
			bytes = DecryptBytes(path, bytes);
		}
		return MiniJSON.JsonBin.Deserialize(bytes) as List<object>;
	}

	public static List<object> LoadJsonObjectFromJsonText(string text, string path, bool encrypted)
	{
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		if (encrypted)
		{
			text = DecryptText(path, text);
		}
		List<object> list = Json.Deserialize(text) as List<object>;
		if (list == null && text[0] != '[')
		{
			list = Json.Deserialize(text.Substring(1)) as List<object>;
		}
		return list;
	}

	public static bool GetAssetFileInfo(EPathType pathType, string path, out string assetPath, out bool isEncrypt, out bool isJsonBin)
	{
		if (DefinePlatform.IsEditor && Define.LocalAssetLoading.Value)
		{
			return GetAssetFileInfoDirect(pathType switch
			{
				EPathType.Table => DefinePlatform.GetLocalDataPath(path), 
				EPathType.TableMessagePack => DefinePlatform.GetTableRootPath(path), 
				EPathType.PlainAsset => DefinePlatform.GetLocalPlainAssetPath(path), 
				_ => path, 
			}, out assetPath, out isEncrypt, out isJsonBin);
		}
		bool assetFileInfoDirect = GetAssetFileInfoDirect(pathType switch
		{
			EPathType.Table => path, 
			EPathType.TableMessagePack => path, 
			EPathType.PlainAsset => String_Extension_Method.CombinePath("PlainAsset", path), 
			_ => path, 
		}, out assetPath, out isEncrypt, out isJsonBin);
		assetPath = PatchDownloader.GetDownloadedAssetPath(UnityCache.persistentDataPath, assetPath);
		return assetFileInfoDirect;
	}

	public static bool GetAssetFileInfoDirect(string origAssetPath, out string assetPath, out bool isEncrypt, out bool isJsonBin)
	{
		if (origAssetPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
		{
			string messagePackExtension = PatchGenerator.GetMessagePackExtension();
			string text = Path.ChangeExtension(origAssetPath, messagePackExtension);
			if (SingletonAuto<AssetFileManager>.instance.TableExist(text))
			{
				assetPath = text;
				isEncrypt = false;
				isJsonBin = false;
				return true;
			}
			string binaryJsonExtension = PatchGenerator.GetBinaryJsonExtension(encrypt: true);
			string text2 = Path.ChangeExtension(origAssetPath, binaryJsonExtension);
			if (SingletonAuto<AssetFileManager>.instance.TableExist(text2))
			{
				assetPath = text2;
				isEncrypt = true;
				isJsonBin = true;
				return true;
			}
			string binaryJsonExtension2 = PatchGenerator.GetBinaryJsonExtension(encrypt: false);
			string text3 = Path.ChangeExtension(origAssetPath, binaryJsonExtension2);
			if (SingletonAuto<AssetFileManager>.instance.TableExist(text3))
			{
				assetPath = text3;
				isEncrypt = false;
				isJsonBin = true;
				return true;
			}
		}
		int originalTextExtensionIndex = PatchGenerator.GetOriginalTextExtensionIndex(origAssetPath);
		if (originalTextExtensionIndex < 0)
		{
			assetPath = origAssetPath;
			isEncrypt = false;
			isJsonBin = false;
			return true;
		}
		string[] originalTextExtensions = PatchGenerator.GetOriginalTextExtensions();
		string[] convertedTextExtensions = PatchGenerator.GetConvertedTextExtensions();
		_ = originalTextExtensions[originalTextExtensionIndex];
		string extension = convertedTextExtensions[originalTextExtensionIndex];
		string text4 = Path.ChangeExtension(origAssetPath, extension);
		if (SingletonAuto<AssetFileManager>.instance.ExistInAssetInfos(text4))
		{
			assetPath = text4;
			isEncrypt = true;
			isJsonBin = false;
			return true;
		}
		assetPath = origAssetPath;
		isEncrypt = false;
		isJsonBin = false;
		return SingletonAuto<AssetFileManager>.instance.ExistInAssetInfos(assetPath);
	}
}
