using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using MiniJSON;

namespace NShared.NJit;

public class JsonSyncLoader2
{
	public byte[] GetMD5(TablePathDefine pathDefine)
	{
		if (!pathDefine.IsValid())
		{
			return new byte[0];
		}
		return GetMD5FromList(GetPartialAssetPathList(pathDefine.JsonTablePath_NoExtension));
	}

	public static byte[] GetMD5FromList(List<string> pathList)
	{
		byte[] array = new byte[100000];
		using MD5 mD = MD5.Create();
		foreach (string path in pathList)
		{
			using FileStream fileStream = File.OpenRead(path);
			while (fileStream.Position < fileStream.Length)
			{
				int inputCount = fileStream.Read(array, 0, array.Length);
				mD.TransformBlock(array, 0, inputCount, null, 0);
			}
		}
		mD.TransformFinalBlock(array, 0, 0);
		return mD.Hash;
	}

	public static bool EqualsMd5Hash(IList<byte> lhs, IList<byte> rhs)
	{
		if (__Extension_Method_ICollection__.COUNT(lhs) != __Extension_Method_ICollection__.COUNT(rhs))
		{
			return false;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(lhs); i++)
		{
			if (lhs[i] != rhs[i])
			{
				return false;
			}
		}
		return true;
	}

	public object Load(TablePathDefine pathDefine)
	{
		if (!pathDefine.IsValid())
		{
			return null;
		}
		string jsonTablePath_NoExtension = pathDefine.JsonTablePath_NoExtension;
		try
		{
			List<object> list = JsonPool.instance.CreateList();
			foreach (string partialAssetPath in GetPartialAssetPathList(jsonTablePath_NoExtension))
			{
				string text = File.ReadAllText(partialAssetPath);
				JitOption.JitLog("Json file loaded (sync) : " + partialAssetPath + "  length=" + text.Length);
				if (Json.Deserialize(text) is List<object> collection)
				{
					list.AddRange(collection);
				}
			}
			return list;
		}
		catch (Exception ex)
		{
			JitOption.JitLogError("Failed to load json file sync : " + ex);
			return null;
		}
	}

	private static List<string> GetPartialAssetPathList(string path_NoExtension)
	{
		List<string> list = new List<string>();
		if (string.IsNullOrEmpty(path_NoExtension))
		{
			return list;
		}
		string text = path_NoExtension + ".json";
		if (File.Exists(text))
		{
			list.Add(text);
		}
		int num = 100;
		for (int i = 0; i <= num; i++)
		{
			string text2 = path_NoExtension + i + ".json";
			if (File.Exists(text2))
			{
				list.Add(text2);
			}
		}
		string directoryName = Path.GetDirectoryName(path_NoExtension);
		string fileName = Path.GetFileName(path_NoExtension);
		string[] files = Directory.GetFiles(directoryName, fileName + "#*.json");
		if (files != null)
		{
			foreach (string item in files)
			{
				list.Add(item);
			}
		}
		return list;
	}
}
