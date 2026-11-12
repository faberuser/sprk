using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace NShared.NUtil;

public class Sha512
{
	public static string GetChecksum(byte[] data)
	{
		using SHA512 sHA = SHA512.Create();
		sHA.ComputeHash(data);
		return BitConverter.ToString(sHA.Hash).Replace("-", "");
	}

	public static void GetDirectoryInfo(string path, out int count, out int length, out string checksum)
	{
		count = 0;
		length = 0;
		checksum = string.Empty;
		string[] files = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);
		if (files == null || files.Length == 0)
		{
			return;
		}
		List<byte> list = new List<byte>(1048576);
		List<string> list2 = new List<string>();
		list2.AddRange(files);
		list2.Sort((string l, string r) => l.CompareTo(r));
		foreach (string item in list2)
		{
			byte[] array = File.ReadAllBytes(item);
			if (array != null && array.Length != 0)
			{
				list.AddRange(array);
				count++;
			}
		}
		byte[] array2 = list.ToArray();
		checksum = GetChecksum(array2);
		length = array2.Length;
	}
}
