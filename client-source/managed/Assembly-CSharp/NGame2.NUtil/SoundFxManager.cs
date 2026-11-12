using System.Collections;
using System.Collections.Generic;
using LitJson;
using NGame2.NUtil.NSoundFx;
using NGameAsset;
using NGameAsset.NCreature;
using NShared;
using NVespa;

namespace NGame2.NUtil;

public class SoundFxManager : SingletonAuto<SoundFxManager>, IBaseDataContainer
{
	public NGame2.NUtil.NSoundFx.CreatureContainer CreatureContainer = new NGame2.NUtil.NSoundFx.CreatureContainer();

	public string TableDataPath => "TableData/SoundFx.json";

	public bool IsLoaded { get; protected set; }

	public string LoadError { get; private set; }

	IEnumerable IBaseDataContainer.GetAllData()
	{
		return CreatureContainer.Dic.Values;
	}

	object IBaseDataContainer.GetData(object key)
	{
		return null;
	}

	public List<FrameSoundData> GetAnimationSoundFx(string creatureCode, AnimationKeyData animationKey)
	{
		return CreatureContainer.Get(creatureCode, animationKey);
	}

	public IBaseDataContainer Load(LitJsonAssetAsyncLoader jsonLoader)
	{
		if (jsonLoader == null)
		{
			VespaDebug.LogError("JsonLoader is null : TableDataPath=" + TableDataPath);
			IsLoaded = true;
			return this;
		}
		jsonLoader.Load(PlainAssetHelper.EPathType.Table, TableDataPath, OnCompleteJsonLoad);
		return this;
	}

	private void OnCompleteJsonLoad(JsonData json)
	{
		if (json.isNone())
		{
			IsLoaded = true;
			LoadError = "Failed to load SoundFx.";
		}
		else
		{
			OnCompleteJson(json);
		}
	}

	private void OnCompleteJson(JsonData json)
	{
		if (!json.IsObject())
		{
			IsLoaded = true;
			return;
		}
		CreatureContainer.LOADFROMJSON(json["CreatureContainer"]);
		IsLoaded = true;
	}
}
