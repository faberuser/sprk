using System;
using System.IO;
using LitJson;
using NGameAsset;
using NVespa;

namespace NGame2.NNodeTree;

public class PackLoad : IAsyncLoad<PackData>, IAsyncLoadBase
{
	public string PathName;

	public bool IsDone;

	public PackData Value;

	public string Error;

	string IAsyncLoadBase.KeyDesc => PathName;

	bool IAsyncLoadBase.IsDone => IsDone;

	bool IAsyncLoadBase.IsValid => Value != null;

	string IAsyncLoadBase.LoadMessage => Error;

	PackData IAsyncLoad<PackData>.Value => Value;

	public static bool IsExists(string pathName)
	{
		return PlainAssetJson.IsExist(String_Extension_Method.CombinePath("NodeTree/", pathName));
	}

	public void Open(string pathName)
	{
		PathName = pathName;
		IsDone = false;
		Error = string.Empty;
		string text = String_Extension_Method.CombinePath("NodeTree/", pathName);
		if (!text.EndsWith(".json"))
		{
			text += ".json";
		}
		text = String_Extension_Method.CombinePath("Text", text);
		PlainAssetHelper.GetAssetFileInfo(PlainAssetHelper.EPathType.PlainAsset, text, out var assetPath, out var isEncrypted, out var isJsonBin);
		SingletonUpdater<ThreadManager>.instance.Enqueue((Action)delegate
		{
			LoadPack(assetPath, isEncrypted, isJsonBin);
		}, (Action)null, ThreadManager.EEntryType.LitJson);
	}

	private void LoadPack(string assetPath, bool isEncrypt, bool isJsonBin)
	{
		JsonData json;
		try
		{
			json = ((!isJsonBin) ? PlainAssetHelper.LoadLitJsonFromJsonText(File.ReadAllText(assetPath), assetPath, isEncrypt) : PlainAssetHelper.LoadLitJsonFromJsonBin(File.ReadAllBytes(assetPath), assetPath, isEncrypt));
		}
		catch (Exception ex)
		{
			Error = ex.Message;
			json = null;
		}
		if (!json.IsObject())
		{
			VespaDebug.LogError("Load Fail1: " + PathName);
			Error = "Load Fail1: " + PathName;
			Value = null;
			IsDone = true;
			return;
		}
		PackData packData = new PackData();
		if (!packData.LOADFROMJSON(json))
		{
			VespaDebug.LogError("Load Fail2: " + PathName);
			Error = "Load Fail2: " + PathName;
			Value = null;
			IsDone = true;
			return;
		}
		packData.PathName = PathName;
		Value = packData;
		if (json != null)
		{
			SingletonAuto<JsonPool>.instance.SafeRelease(ref json);
		}
		IsDone = true;
	}
}
