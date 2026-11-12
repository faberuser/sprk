using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using LZ4;
using NVespa;

namespace NShared.NUtil;

public class Lz4
{
	public static string Compress(string sourceString)
	{
		try
		{
			using MemoryStream memoryStream = new MemoryStream();
			using LZ4Stream lZ4Stream = new LZ4Stream(memoryStream, CompressionMode.Compress);
			byte[] bytes = Encoding.UTF8.GetBytes(sourceString);
			lZ4Stream.Write(bytes, 0, bytes.Length);
			lZ4Stream.Flush();
			return Convert.ToBase64String(memoryStream.ToArray());
		}
		catch (Exception ex)
		{
			VespaDebug.LogError("Failed to compress replay : " + ex);
			return null;
		}
	}

	public static string Decompress(string compressedString)
	{
		try
		{
			using MemoryStream innerStream = new MemoryStream(Convert.FromBase64String(compressedString));
			using LZ4Stream lZ4Stream = new LZ4Stream(innerStream, CompressionMode.Decompress);
			using MemoryStream memoryStream = new MemoryStream();
			byte[] array = new byte[1024];
			while (true)
			{
				int num = lZ4Stream.Read(array, 0, array.Length);
				if (num <= 0)
				{
					break;
				}
				memoryStream.Write(array, 0, num);
			}
			memoryStream.Flush();
			byte[] bytes = memoryStream.ToArray();
			return Encoding.UTF8.GetString(bytes);
		}
		catch (Exception ex)
		{
			VespaDebug.LogError("Failed to decompress replay : " + ex);
			return null;
		}
	}

	public static byte[] Decompress2(byte[] compressedBytes)
	{
		try
		{
			using MemoryStream innerStream = new MemoryStream(compressedBytes);
			using LZ4Stream lZ4Stream = new LZ4Stream(innerStream, CompressionMode.Decompress);
			using MemoryStream memoryStream = new MemoryStream();
			byte[] array = new byte[1024];
			while (true)
			{
				int num = lZ4Stream.Read(array, 0, array.Length);
				if (num <= 0)
				{
					break;
				}
				memoryStream.Write(array, 0, num);
			}
			memoryStream.Flush();
			return memoryStream.ToArray();
		}
		catch (Exception ex)
		{
			VespaDebug.LogError("Failed to decompress replay : " + ex);
			return null;
		}
	}
}
