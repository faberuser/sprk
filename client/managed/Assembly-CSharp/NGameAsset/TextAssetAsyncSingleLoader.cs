using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NGame2.NUI.NWindow;
using NGameAsset.NPatch;
using NPatchGenerator;
using NShared;
using NShared.NUtil;
using NVespa;
using NVespa.NGlobal;
using UnityEngine;

namespace NGameAsset;

public class TextAssetAsyncSingleLoader : ITextLoader
{
	private class LoadTextData
	{
		public string Path;

		public bool Encrypted;

		public Action<string> OnRead;
	}

	private static Coroutine _loadCo;

	private static Queue<LoadTextData> _loadQueue = new Queue<LoadTextData>();

	void ITextLoader.Load(string path, Action<string> onLoad)
	{
		bool encrypted = false;
		string text = PatchDownloader.GetDownloadedAssetPath(UnityCache.persistentDataPath, path);
		if (DefinePlatform.IsEditor && Define.LocalAssetLoading.Value)
		{
			text = DefinePlatform.GetLocalDataPath(path);
			encrypted = false;
		}
		else
		{
			int originalTextExtensionIndex = PatchGenerator.GetOriginalTextExtensionIndex(text);
			if (originalTextExtensionIndex >= 0)
			{
				string[] originalTextExtensions = PatchGenerator.GetOriginalTextExtensions();
				string[] convertedTextExtensions = PatchGenerator.GetConvertedTextExtensions();
				string oldValue = originalTextExtensions[originalTextExtensionIndex];
				string newValue = convertedTextExtensions[originalTextExtensionIndex];
				text = text.Replace(oldValue, newValue);
				encrypted = true;
			}
		}
		if (_loadCo == null)
		{
			_loadCo = VespaCoroutine.Start(coLoadLoop());
		}
		_loadQueue.Enqueue(new LoadTextData
		{
			Path = text,
			Encrypted = encrypted,
			OnRead = onLoad
		});
	}

	private static IEnumerator coLoadLoop()
	{
		while (true)
		{
			yield return null;
			if (_loadQueue.Count == 0)
			{
				continue;
			}
			LoadTextData loadText = _loadQueue.Dequeue();
			yield return SingletonUpdater<GCManager>.instance.Collect();
			bool isClosePopup = false;
			bool isDetail = false;
			GlobalMessagePopup.OpenGlobal("load table view detail", Path.GetFileName(loadText.Path), delegate
			{
				isClosePopup = true;
				isDetail = true;
			}, delegate
			{
				isClosePopup = true;
				isDetail = false;
			});
			while (!isClosePopup)
			{
				yield return null;
			}
			yield return null;
			bool isComplete = false;
			string text = string.Empty;
			ReadByWww2(loadText.Path, loadText.Encrypted, delegate(string iText)
			{
				isComplete = true;
				text = iText;
			});
			while (!isComplete)
			{
				yield return null;
			}
			if (loadText.Encrypted)
			{
				if (isDetail)
				{
					isClosePopup = false;
					GlobalMessagePopup.OpenGlobal("decrypt", Path.GetFileName(loadText.Path), delegate
					{
						isClosePopup = true;
					}, retryButton: false);
					while (!isClosePopup)
					{
						yield return null;
					}
				}
				yield return null;
				text = DecryptText(loadText.Path, text);
				yield return null;
				if (isDetail)
				{
					isClosePopup = false;
					int length = text.Length;
					GlobalMessagePopup.OpenGlobal("FromBase64String: textlen=" + Vespa_Int_Extension_Method.ToCommaString(length), Path.GetFileName(loadText.Path), delegate
					{
						isClosePopup = true;
					}, retryButton: false);
					while (!isClosePopup)
					{
						yield return null;
					}
				}
				yield return null;
				byte[] compressedBytes = Convert.FromBase64String(text);
				yield return null;
				if (isDetail)
				{
					isClosePopup = false;
					int num = __Extension_Method_ICollection__.COUNT(compressedBytes);
					GlobalMessagePopup.OpenGlobal("decompress: compressed size=" + Vespa_Int_Extension_Method.ToCommaString(num), Path.GetFileName(loadText.Path), delegate
					{
						isClosePopup = true;
					}, retryButton: false);
					while (!isClosePopup)
					{
						yield return null;
					}
				}
				yield return null;
				byte[] decompressedBytes = Lz4.Decompress2(compressedBytes);
				yield return null;
				if (isDetail)
				{
					isClosePopup = false;
					int num2 = __Extension_Method_ICollection__.COUNT(decompressedBytes);
					GlobalMessagePopup.OpenGlobal("Encoding.UTF8: decompressed size=" + Vespa_Int_Extension_Method.ToCommaString(num2), Path.GetFileName(loadText.Path), delegate
					{
						isClosePopup = true;
					}, retryButton: false);
					while (!isClosePopup)
					{
						yield return null;
					}
				}
				yield return null;
				text = Encoding.UTF8.GetString(decompressedBytes);
				yield return null;
			}
			if (isDetail)
			{
				isClosePopup = false;
				int length2 = text.Length;
				GlobalMessagePopup.OpenGlobal("read table: text size=" + Vespa_Int_Extension_Method.ToCommaString(length2), Path.GetFileName(loadText.Path), delegate
				{
					isClosePopup = true;
				}, retryButton: false);
				while (!isClosePopup)
				{
					yield return null;
				}
			}
			yield return null;
			Vespa_System_Action_Extension_Method.fire(loadText.OnRead, text);
			yield return null;
		}
	}

	private static string DecryptText(string path, string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		try
		{
			return PatchGenerator.Aes.DecryptTextLegacy(text, PatchGenerator.GetTest(first: true) + PatchGenerator.GetTest(first: false));
		}
		catch (Exception ex)
		{
			VespaDebug.LogError("Faild to decrypt " + path + ". error=" + ex);
			return null;
		}
	}

	private static string DecompressText(string path, string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		try
		{
			_ = DateTime.UtcNow;
			return Lz4.Decompress(text);
		}
		catch (Exception ex)
		{
			VespaDebug.LogError("Faild to decompress " + path + ". error=" + ex);
			return null;
		}
	}

	private static void ReadByWww(string path, bool encrypted, Action<string> onRead)
	{
		WwwText www = new WwwText("file://" + path);
		SingletonUpdater<AsyncManager>.instance.Add(www, delegate
		{
			string text = null;
			try
			{
				if (string.IsNullOrEmpty(www.error))
				{
					text = www.text;
					if (encrypted)
					{
						SingletonUpdater<ThreadManager>.instance.Enqueue((Action)delegate
						{
							text = DecryptText(path, text);
						}, (Action)delegate
						{
							Vespa_System_Action_Extension_Method.fire(onRead, text);
						});
						return;
					}
				}
				else
				{
					VespaDebug.LogError("Faild to load " + path + " : " + www.error);
				}
			}
			catch (Exception ex)
			{
				VespaDebug.LogError("Faild to load " + path + ". error=" + ex);
				text = null;
			}
			Vespa_System_Action_Extension_Method.fire(onRead, text);
		});
	}

	private static void ReadByWww2(string path, bool encrypted, Action<string> onRead)
	{
		WwwText www = new WwwText("file://" + path);
		SingletonUpdater<AsyncManager>.instance.Add(www, delegate
		{
			string i_argA = null;
			try
			{
				if (string.IsNullOrEmpty(www.error))
				{
					i_argA = www.text;
				}
				else
				{
					VespaDebug.LogError("Faild to load " + path + " : " + www.error);
				}
			}
			catch (Exception ex)
			{
				VespaDebug.LogError("Faild to load " + path + ". error=" + ex);
				i_argA = null;
			}
			Vespa_System_Action_Extension_Method.fire(onRead, i_argA);
		});
	}
}
