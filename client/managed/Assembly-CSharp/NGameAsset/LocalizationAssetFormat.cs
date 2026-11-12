using System;
using System.IO;
using NGameAsset.NPatch;
using NVespa;

namespace NGameAsset;

public class LocalizationAssetFormat
{
	public void GetFileFormat(string path, ref string fileFormat, ref uint version)
	{
		if (!string.IsNullOrEmpty(fileFormat))
		{
			return;
		}
		string downloadedAssetPath = PatchDownloader.GetDownloadedAssetPath(UnityCache.persistentDataPath, path);
		if (!File.Exists(downloadedAssetPath))
		{
			fileFormat = "csv";
			version = 0u;
			return;
		}
		byte[] array = new byte[4];
		FileStream fileStream = new FileStream(downloadedAssetPath, FileMode.Open);
		try
		{
			fileStream.Read(array, 0, 4);
			uint num = BitConverter.ToUInt32(array, 0);
			fileStream.Read(array, 0, 4);
			uint num2 = BitConverter.ToUInt32(array, 0);
			if (num != 1128875084)
			{
				fileFormat = "csv";
				version = 0u;
			}
			else
			{
				fileFormat = "ldic";
				version = num2;
			}
		}
		catch (Exception)
		{
		}
		finally
		{
			fileStream.Close();
		}
	}
}
