using NVespa;

namespace NGameAsset.NPlainAsset;

public class JsonContainer : SingletonAuto<JsonContainer>
{
	public IncreaseDic<string, PlainAssetJson> Dic = new IncreaseDic<string, PlainAssetJson>();

	public PlainAssetJson Lock(string path, bool reload = false)
	{
		if (reload)
		{
			Dic.RemoveSlow(path);
		}
		PlainAssetJson plainAssetJson = Dic.GETVALUE(path, null);
		if (plainAssetJson == null)
		{
			plainAssetJson = new PlainAssetJson();
			if (!plainAssetJson.Open(path))
			{
				return null;
			}
			Dic.Add(path, plainAssetJson);
		}
		plainAssetJson.Lock.Increase();
		return plainAssetJson;
	}

	public static void ReleaseAsset(PlainAssetJson assetJson)
	{
		assetJson?.Lock.Decrease(null);
	}

	public static void DestroyUnused()
	{
		if (SingletonAuto<JsonContainer>.isValidInstance)
		{
			SingletonAuto<JsonContainer>.instance.DestroyUnusedInternal();
		}
	}

	private void DestroyUnusedInternal()
	{
		for (int i = 0; i < Dic.Count; i++)
		{
			PlainAssetJson valueByIndex = Dic.GetValueByIndex(i);
			if (valueByIndex.IsDone && valueByIndex.Lock.Count <= 0)
			{
				valueByIndex.Close();
				Dic.RemoveByIndex(i);
				i--;
			}
		}
	}
}
