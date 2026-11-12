using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.GZip;

public static class GZipCompressor
{
	public static byte[] Compress(string rawString)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(rawString);
		using MemoryStream memoryStream = new MemoryStream();
		Compress(memoryStream, bytes);
		return memoryStream.ToArray();
	}

	public static string Decompress(byte[] compressedData)
	{
		using MemoryStream memoryStream = new MemoryStream();
		Decompress(memoryStream, compressedData);
		byte[] bytes = memoryStream.ToArray();
		return Encoding.UTF8.GetString(bytes);
	}

	private static void Compress(Stream stream, byte[] rawData)
	{
		using GZipOutputStream gZipOutputStream = new GZipOutputStream(stream);
		gZipOutputStream.Write(rawData, 0, rawData.Length);
	}

	private static void Decompress(Stream stream, byte[] compressedData)
	{
		byte[] array = new byte[4096];
		using MemoryStream baseInputStream = new MemoryStream(compressedData);
		using GZipInputStream gZipInputStream = new GZipInputStream(baseInputStream);
		for (int num = -1; num != 0; num = gZipInputStream.Read(array, 0, array.Length))
		{
			if (num > 0)
			{
				stream.Write(array, 0, num);
			}
		}
	}
}
