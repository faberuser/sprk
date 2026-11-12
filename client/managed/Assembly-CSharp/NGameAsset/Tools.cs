using System;
using System.IO;
using NGameAsset.NBattleFieldMap2;
using NGameAsset.NCreature;
using NGameAsset.NEffect;
using NGameAsset.NSound;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGameAsset;

public static class Tools
{
	public static void ReadBytesAsync(string path, Action<byte[]> onRead)
	{
		FileStream fileStream;
		try
		{
			fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
		}
		catch (FileNotFoundException)
		{
			Vespa_System_Action_Extension_Method.fire(onRead, null);
			return;
		}
		long fileLength = fileStream.Length;
		byte[] bytes = new byte[fileLength];
		fileStream.BeginRead(bytes, 0, (int)fileLength, delegate(IAsyncResult asyncResult)
		{
			int num = fileStream.EndRead(asyncResult);
			fileStream.Dispose();
			fileStream = null;
			if (num != fileLength)
			{
				VespaDebug.LogError($"Read Invalid Size: {num} ({path})");
				byte[] array = new byte[num];
				Buffer.BlockCopy(bytes, 0, array, 0, Mathf.Min(num, (int)fileLength));
				bytes = array;
			}
			Vespa_System_Action_Extension_Method.fire(onRead, bytes);
		}, null);
	}

	public static void ReleaseAsyncLoad(IAsyncLoadBase asyncLoadBase)
	{
		if (asyncLoadBase != null)
		{
			if (asyncLoadBase is NGameAsset.NCreature.OpenData openData)
			{
				openData.Close();
			}
			else if (asyncLoadBase is NGameAsset.NEffect.OpenData openData2)
			{
				openData2.Close();
			}
			else if (asyncLoadBase is IAsyncLoad<AudioClip> asyncLoad)
			{
				AudioClipContainer.ReleaseAudioClip(asyncLoad);
			}
			else if (asyncLoadBase is NGameAsset.NBattleFieldMap2.OpenData openData3)
			{
				openData3.Close();
			}
			else if (!(asyncLoadBase is AsyncLoader.IPrefabLoad) && asyncLoadBase is AssetTexture assetTexture)
			{
				assetTexture.Close();
			}
		}
	}
}
