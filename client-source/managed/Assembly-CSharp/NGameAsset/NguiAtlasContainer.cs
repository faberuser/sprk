using NGameAsset.NNguiAtlas;
using NVespa;

namespace NGameAsset;

public class NguiAtlasContainer : SingletonAuto<NguiAtlasContainer>
{
	private IncreaseDic<string, AtlasLoad> _dic = new IncreaseDic<string, AtlasLoad>();

	public INguiAtlas Lock(string code)
	{
		AtlasLoad atlasLoad = _dic.GETVALUE(code, null);
		if (atlasLoad == null)
		{
			atlasLoad = new AtlasLoad();
			if (!atlasLoad.Open(code))
			{
				return null;
			}
			_dic.Add(code, atlasLoad);
		}
		atlasLoad.Lock.Increase();
		return atlasLoad;
	}

	public static void ReleaseAtlas(INguiAtlas atlas, bool destroyImmediately)
	{
		if (atlas == null)
		{
			return;
		}
		if (!(atlas is AtlasLoad atlasLoad))
		{
			atlas.Close(destroyImmediately: false);
			return;
		}
		atlasLoad.Lock.Decrease(null);
		if (destroyImmediately && atlasLoad.Lock.Count <= 0 && SingletonAuto<NguiAtlasContainer>.isValidInstance)
		{
			SingletonAuto<NguiAtlasContainer>.instance.DestroyUnusedThisNow(atlasLoad);
		}
	}

	public static void DestroyUnusedAll()
	{
		if (SingletonAuto<NguiAtlasContainer>.isValidInstance)
		{
			SingletonAuto<NguiAtlasContainer>.instance.DestroyUnusedAllInternal();
		}
	}

	private void DestroyUnusedThisNow(AtlasLoad This)
	{
		if (!This.ISDONE() || This.Lock.Count > 0)
		{
			return;
		}
		for (int i = 0; i < _dic.Count; i++)
		{
			AtlasLoad valueByIndex = _dic.GetValueByIndex(i);
			if (valueByIndex == This)
			{
				Vespa_Common_Class_Extension_Method.Interface((INguiAtlas)valueByIndex).Close(destroyImmediately: true);
				_dic.RemoveByIndex(i);
				break;
			}
		}
	}

	private void DestroyUnusedAllInternal()
	{
		for (int i = 0; i < _dic.Count; i++)
		{
			AtlasLoad valueByIndex = _dic.GetValueByIndex(i);
			if (valueByIndex.ISDONE() && valueByIndex.Lock.Count <= 0)
			{
				Vespa_Common_Class_Extension_Method.Interface((INguiAtlas)valueByIndex).Close(destroyImmediately: false);
				_dic.RemoveByIndex(i);
				i--;
			}
		}
	}
}
