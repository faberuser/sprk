using System;
using System.IO;
using System.Security.Cryptography;
using Force.Crc32;
using NVespa;

namespace NPatchGenerator;

public class FileUtility
{
	private static DateTime Time_t = new DateTime(2015, 1, 1);

	public static void DeleteDirectory(string target_dir)
	{
		string[] files = Directory.GetFiles(target_dir);
		string[] directories = Directory.GetDirectories(target_dir);
		string[] array = files;
		foreach (string path in array)
		{
			File.SetAttributes(path, FileAttributes.Normal);
			File.Delete(path);
		}
		array = directories;
		for (int i = 0; i < array.Length; i++)
		{
			DeleteDirectory(array[i]);
		}
		try
		{
			Directory.Delete(target_dir, recursive: false);
		}
		catch
		{
		}
	}

	public unsafe static uint StringToHashCode(string str)
	{
		fixed (char* ptr = str)
		{
			int num = 5381;
			int num2 = num;
			char* ptr2 = ptr;
			int num3;
			while ((num3 = *ptr2) != 0)
			{
				num = ((num << 5) + num) ^ num3;
				num3 = ptr2[1];
				if (num3 == 0)
				{
					break;
				}
				num2 = ((num2 << 5) + num2) ^ num3;
				ptr2 += 2;
			}
			return (uint)(num + num2 * 1566083941);
		}
	}

	public unsafe static uint CharsToHashCode(char[] chars, int len)
	{
		fixed (char* ptr = chars)
		{
			int num = 5381;
			int num2 = num;
			char* ptr2 = ptr;
			int num3;
			while ((num3 = *ptr2) != 0)
			{
				num = ((num << 5) + num) ^ num3;
				num3 = ptr2[1];
				if (num3 == 0)
				{
					break;
				}
				num2 = ((num2 << 5) + num2) ^ num3;
				ptr2 += 2;
			}
			return (uint)(num + num2 * 1566083941);
		}
	}

	public static void TryGetFileInfos(string filePath, out string Md5, out long WriteTime, out uint Crc32, out int Length)
	{
		string fileMd5Hash = GetFileMd5Hash(filePath);
		long fileWriteTime = GetFileWriteTime(filePath);
		uint fileCrc = GetFileCrc32(filePath);
		int fileLength = GetFileLength(filePath);
		Md5 = fileMd5Hash;
		WriteTime = fileWriteTime;
		Crc32 = fileCrc;
		Length = fileLength;
	}

	public static void TryGetFileInfos(string filePath, out string Md5, out long WriteTime, out int Length)
	{
		string fileMd5Hash = GetFileMd5Hash(filePath);
		long fileWriteTime = GetFileWriteTime(filePath);
		int fileLength = GetFileLength(filePath);
		Md5 = fileMd5Hash;
		WriteTime = fileWriteTime;
		Length = fileLength;
	}

	public static uint GetFileCrc32(string path)
	{
		if (!File.Exists(path))
		{
			return 0u;
		}
		return Crc32Algorithm.Compute(File.ReadAllBytes(path));
	}

	public static long GetFileWriteTime(string path)
	{
		long result = 0L;
		if (!File.Exists(path))
		{
			return result;
		}
		try
		{
			int num = (int)(File.GetLastWriteTime(path) - Time_t).TotalSeconds;
			if (num < 0)
			{
				VespaDebug.LogError(path + " : created before time_t.");
				return result;
			}
			return num;
		}
		catch (Exception obj)
		{
			VespaDebug.LogError(obj);
			return result;
		}
	}

	public static bool SetFileWriteTime(string path, long diffSeconds)
	{
		if (!File.Exists(path))
		{
			return false;
		}
		try
		{
			DateTime time_t = Time_t;
			time_t = time_t.AddSeconds(diffSeconds);
			File.SetCreationTime(path, time_t);
		}
		catch (Exception obj)
		{
			VespaDebug.LogError(obj);
			return false;
		}
		return true;
	}

	public static string GetFileMd5Hash(string path)
	{
		if (!File.Exists(path))
		{
			return string.Empty;
		}
		using MD5 mD = MD5.Create();
		using FileStream inputStream = File.OpenRead(path);
		return BitConverter.ToString(mD.ComputeHash(inputStream)).Replace("-", "").ToLower();
	}

	public static int GetFileLength(string path)
	{
		if (!File.Exists(path))
		{
			return 0;
		}
		return (int)new FileInfo(path).Length;
	}

	public static int StreamRead(Stream stream, byte[] buffer, int offset, int count)
	{
		return stream.Read(buffer, offset, count);
	}

	private static void MD5TransformBlock(MD5 md5Hasher, byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
	{
		md5Hasher.TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
	}

	private static void TransformFinalBlock(MD5 md5Hasher, byte[] inputBuffer, int inputOffset, int inputCount)
	{
		md5Hasher.TransformFinalBlock(inputBuffer, inputOffset, inputCount);
	}

	private static byte[] GetMd5Hash(MD5 md5Hasher)
	{
		return md5Hasher.Hash;
	}
}
