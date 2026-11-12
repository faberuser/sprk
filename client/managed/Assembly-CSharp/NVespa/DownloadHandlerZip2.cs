using System;
using System.IO;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;
using NGameAsset.NPatch;

namespace NVespa;

public class DownloadHandlerZip2 : DownloadHandlerBase
{
	public DownloadHandlerZip2()
	{
	}

	public DownloadHandlerZip2(Listener listener, byte[] buffer, DownloadContextData downloadContextData, Action<bool> onComplete)
		: base(listener, buffer, downloadContextData, onComplete)
	{
	}

	public override bool TryExtract()
	{
		byte[] array = new byte[4096];
		_receiveStream.Position = 0L;
		using (ZipInputStream zipInputStream = new ZipInputStream(_receiveStream))
		{
			while (true)
			{
				ZipEntry nextEntry = zipInputStream.GetNextEntry();
				if (nextEntry == null)
				{
					break;
				}
				string name = nextEntry.Name;
				string downloadedAssetPath = PatchDownloader.GetDownloadedAssetPath(UnityCache.persistentDataPath, name);
				_listener.OnCreateDicrectory(downloadedAssetPath);
				int num = 0;
				int num2 = 0;
				int num3 = 0;
				using (FileStream fileStream = new FileStream(downloadedAssetPath, FileMode.Create, FileAccess.Write, FileShare.Write))
				{
					while (true)
					{
						try
						{
							while (true)
							{
								if (num2 <= 0)
								{
									if (zipInputStream.Available == 0)
									{
										break;
									}
									num2 = zipInputStream.Read(array, 0, array.Length);
								}
								if (num2 > 0)
								{
									fileStream.Write(array, 0, num2);
									num3 += num2;
									num2 = 0;
									_listener.OnUpateExtractAsset(_downloadContextData, (double)_receiveStream.Position / (double)_receiveStream.Length);
								}
							}
						}
						catch (Exception ex)
						{
							VespaDebug.LogError("Write file exception: entryName=" + name + " error=" + ex);
							num3 = 0;
							num++;
							if (num >= 3)
							{
								ReleaseStream();
								return false;
							}
							Thread.Sleep(200);
							continue;
						}
						break;
					}
				}
				if (nextEntry.Size != num3)
				{
					VespaDebug.LogError(string.Format("{2} Size {0} != Total {1}", nextEntry.Size, num3, downloadedAssetPath));
				}
				_listener.OnCompleteExtractAsset(_downloadContextData, nextEntry.Name);
			}
		}
		_listener.OnCompleteExtractArchive(_downloadContextData);
		ReleaseStream();
		return true;
	}

	private void ReleaseStream()
	{
		_receiveStream.Flush();
		_receiveStream.Close();
		_receiveStream = null;
	}
}
