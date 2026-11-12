using System;
using System.IO;
using System.Threading;
using NGame2.NUI.NWindow;
using NGameAsset.NPatch;
using NVespa.NUtil;

namespace NVespa;

public class DownloadHandlerPack2 : DownloadHandlerBase
{
	public DownloadHandlerPack2()
	{
	}

	public DownloadHandlerPack2(Listener listener, byte[] buffer, DownloadContextData downloadContextData, Action<bool> onComplete)
		: base(listener, buffer, downloadContextData, onComplete)
	{
	}

	public override bool TryExtract()
	{
		byte[] buffer = _receiveStream.GetBuffer();
		long length = _receiveStream.Length;
		BytesReader bytesReader = new BytesReader(buffer);
		while (bytesReader.RemainedLength > 0)
		{
			string text = bytesReader.ReadUtf8String();
			if (String_Extension_Method.IsNullOrEmpty(text))
			{
				string obj = "Invalid EntryName : archive=" + _downloadContextData.ArchiveInfo.Name + " read=" + bytesReader.Position;
				VespaDebug.LogError(obj);
				ReleaseStream();
				return false;
			}
			string downloadedAssetPath = PatchDownloader.GetDownloadedAssetPath(UnityCache.persistentDataPath, text);
			_listener.OnCreateDicrectory(downloadedAssetPath);
			int num = bytesReader.ReadInt();
			if (!bytesReader.CanRead(num))
			{
				string obj2 = "Input read size mismatch : archive=" + _downloadContextData.ArchiveInfo.Name + " entry=" + text + " read=" + bytesReader.RemainedLength + " expected=" + num;
				VespaDebug.LogError(obj2);
				ReleaseStream();
				return false;
			}
			int num2 = 0;
			if (!string.IsNullOrEmpty(BackgroundDownloader.DownloadFileWriteSleepTime.Value))
			{
				num2 = Convert.ToInt32(BackgroundDownloader.DownloadFileWriteSleepTime.Value);
				if (num2 < 0 && num2 > 10)
				{
					num2 = 0;
				}
			}
			bool value = BackgroundDownloader.IsBackgroundDownload.Value;
			int num3 = 0;
			int num4 = num / 4096;
			int num5 = num % 4096;
			using (FileStream fileStream = new FileStream(downloadedAssetPath, FileMode.Create, FileAccess.Write, FileShare.Write))
			{
				while (true)
				{
					try
					{
						for (int i = 0; i < num4; i++)
						{
							fileStream.Write(bytesReader.Bytes, bytesReader.Position, 4096);
							bytesReader.Position += 4096;
							if (value)
							{
								Thread.Sleep(num2);
							}
						}
						if (num5 > 0)
						{
							fileStream.Write(bytesReader.Bytes, bytesReader.Position, num5);
							bytesReader.Position += num5;
						}
					}
					catch (Exception arg)
					{
						VespaDebug.LogError($"Write file exception: entryName={text} error={arg}");
						num3++;
						if (num3 >= 3)
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
			_listener.OnUpateExtractAsset(_downloadContextData, (double)bytesReader.Position / (double)length);
			_listener.OnCompleteExtractAsset(_downloadContextData, text);
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
