using LitJson;
using NVespa;

namespace NGameAsset;

public class PlainAssetJson : IAsyncLoad<JsonData>, IAsyncLoadBase
{
	public LockCounter Lock = new LockCounter();

	public JsonData Json;

	public string Path { get; private set; }

	public bool IsDone { get; set; }

	public bool IsValid { get; set; }

	JsonData IAsyncLoad<JsonData>.Value => Json;

	string IAsyncLoadBase.KeyDesc => Path;

	string IAsyncLoadBase.LoadMessage => string.Empty;

	public static bool IsExist(string path)
	{
		string filePath = GetFilePath(path);
		string assetPath;
		bool isEncrypt;
		bool isJsonBin;
		return PlainAssetHelper.GetAssetFileInfo(PlainAssetHelper.EPathType.PlainAsset, filePath, out assetPath, out isEncrypt, out isJsonBin);
	}

	public bool Open(string path)
	{
		Path = path;
		SingletonAuto<LitJsonAssetAsyncLoader>.instance.Load(PlainAssetHelper.EPathType.PlainAsset, GetFilePath(path), OnComplete);
		return true;
	}

	public void Close()
	{
		SingletonAuto<JsonPool>.instance.SafeRelease(ref Json);
		Path = string.Empty;
		IsDone = false;
		IsValid = false;
		Lock.Reset();
	}

	private static string GetFilePath(string path)
	{
		if (!path.EndsWith(".json"))
		{
			path += ".json";
		}
		return String_Extension_Method.CombinePath("Text", path);
	}

	private void OnComplete(JsonData json)
	{
		Json = json;
		IsDone = true;
		IsValid = !json.isNone();
	}
}
