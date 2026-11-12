using System;
using System.IO;
using System.Text;
using NVespa;

namespace NShared;

public class TextAsyncLoader : ITextLoader
{
	private readonly string _prefix;

	public TextAsyncLoader(string prefix)
	{
		_prefix = prefix;
	}

	void ITextLoader.Load(string path, Action<string> onLoad)
	{
		if (onLoad == null)
		{
			return;
		}
		string text = Path.Combine(_prefix, path);
		FileInfo fileInfo = new FileInfo(text);
		if (!fileInfo.Exists)
		{
			VespaDebug.LogError("TextLoader : File not exist : " + text);
			onLoad(null);
			return;
		}
		long length = fileInfo.Length;
		FileStream fileStream = fileInfo.OpenRead();
		byte[] data = new byte[length];
		fileStream.BeginRead(data, 0, data.Length, delegate(IAsyncResult asyncResult)
		{
			fileStream.EndRead(asyncResult);
			fileStream.Close();
			string obj = Encoding.UTF8.GetString(data);
			onLoad(obj);
		}, null);
	}
}
