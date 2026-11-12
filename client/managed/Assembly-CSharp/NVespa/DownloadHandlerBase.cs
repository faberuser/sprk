using System;
using System.IO;
using System.Threading;
using NGame2.NUI.NWindow;
using NVespa.NUtil;
using UnityEngine;
using UnityEngine.Networking;

namespace NVespa;

public abstract class DownloadHandlerBase : DownloadHandlerScript
{
	public interface Listener
	{
		void OnCreateDicrectory(string assetFilePath);

		void OnUpateExtractAsset(DownloadContextData downloadContextData, double ratio);

		void OnCompleteExtractAsset(DownloadContextData downloadContextData, string entryName);

		void OnCompleteExtractArchive(DownloadContextData downloadContextData);
	}

	protected const int ChunkSize = 4096;

	protected CustomStreamBase _inputStream;

	protected Listener _listener;

	protected Action<bool> _onComplete;

	protected DownloadContextData _downloadContextData;

	protected MemoryStream _receiveStream;

	protected int _dataTotalLength;

	public DownloadHandlerBase()
	{
	}

	public DownloadHandlerBase(Listener listener, byte[] buffer, DownloadContextData downloadContextData, Action<bool> onComplete)
		: base(buffer)
	{
		_listener = listener;
		_downloadContextData = downloadContextData;
		_onComplete = onComplete;
		_dataTotalLength = _downloadContextData.ArchiveInfo.Length;
	}

	public virtual void CreateCustomStream(long length)
	{
	}

	public abstract bool TryExtract();

	protected override bool ReceiveData(byte[] data, int dataLength)
	{
		if (data == null || data.Length == 0)
		{
			Debug.Log("[DownloadExtractPackHandler] ReceiveData - received a null/empty buffer");
			return false;
		}
		if (_receiveStream == null)
		{
			try
			{
				_receiveStream = new MemoryStream(_dataTotalLength);
			}
			catch (OutOfMemoryException)
			{
				Debug.Log("[DownloadExtractPackHandler] ReceiveData - out of memory");
				return false;
			}
		}
		int num = 0;
		if (!string.IsNullOrEmpty(BackgroundDownloader.DownloadFileWriteSleepTime.Value))
		{
			num = Convert.ToInt32(BackgroundDownloader.DownloadFileWriteSleepTime.Value);
			if (num < 0 && num > 10)
			{
				num = 0;
			}
		}
		bool value = BackgroundDownloader.IsBackgroundDownload.Value;
		int num2 = dataLength / 4096;
		int num3 = dataLength % 4096;
		int num4 = 0;
		for (int i = 0; i < num2; i++)
		{
			_receiveStream.Write(data, num4, 4096);
			num4 += 4096;
			if (value)
			{
				Thread.Sleep(num);
			}
		}
		if (num3 > 0)
		{
			_receiveStream.Write(data, num4, num3);
			num4 += num3;
		}
		if (num4 != dataLength)
		{
			Debug.LogError($"[DownloadExtractPackHandler] ReceiveData offset:{num4} != dataLength:{dataLength}");
		}
		return true;
	}

	protected override void CompleteContent()
	{
		SingletonUpdater<ThreadManager>.instance.Enqueue((Action)delegate
		{
			bool result = TryExtract();
			SingletonUpdater<Executer>.instance.NextRun(delegate
			{
				Vespa_System_Action_Extension_Method.fire(_onComplete, result);
			});
		}, (Action)null);
	}
}
