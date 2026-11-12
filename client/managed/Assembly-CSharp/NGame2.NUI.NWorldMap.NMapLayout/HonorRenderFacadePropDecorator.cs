using System.Collections.Generic;
using NGame2.NHonoraryChamber;
using NGameAsset;
using NGameAsset.NPatch;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout;

internal class HonorRenderFacadePropDecorator : NGame2.NUI.NWorldMap.NMapLayout.RenderFacadePropDecorator
{
	private const string CreaturePrefix = "Trans";

	private const string LabelRoot = "LabelRoot";

	private const string TitleGO = "RankingTitle";

	private const string NameGO = "UserlName";

	private const string TextureGO = "Texture_CountryFlag";

	private const string BackGroundGO = "SpriteTag";

	private const string LocaleBanpick = "HONOR_RANK1_PVP2";

	private const string LocaleWorldBoss = "HONOR_RANK1_WORLDBOSS";

	private const string LocaleGuildSuppress = "HONOR_RANK1_GUILD_PUNITIVE";

	private Transform _labelRoot;

	private UIPanel _clipPanel;

	private UILabel _titleLabel;

	private UILabel _nameLabel;

	private UITexture _countryTexture;

	private UISprite _spriteLabelBg;

	private RecordsOfHonorTopInfo _topRankerInfo;

	public override void SetPropComponent(PropComponent prop, string[] propActionArgs, Mediator mapMediator)
	{
		base.SetPropComponent(prop, propActionArgs, mapMediator);
		SetUICache();
		SetTopRankerInfo(propActionArgs);
		SetRankerInfoLabels();
		CreateRenderFacadeStock(propActionArgs);
		PauseMotion();
	}

	private void SetUICache()
	{
		_clipPanel = _propComponent.transform.GetComponentInChildren<UIPanel>();
		_labelRoot = GetComponentByChildName<Transform>(_propComponent.transform, "LabelRoot");
		_titleLabel = GetComponentByChildName<UILabel>(_propComponent.transform, "RankingTitle");
		_nameLabel = GetComponentByChildName<UILabel>(_propComponent.transform, "UserlName");
		_countryTexture = GetComponentByChildName<UITexture>(_propComponent.transform, "Texture_CountryFlag");
		_spriteLabelBg = GetComponentByChildName<UISprite>(_propComponent.transform, "SpriteTag");
	}

	private T GetComponentByChildName<T>(Transform trs, string name)
	{
		Transform transform = Vespa_Unity_Transform_Extension_Method.FindChildRecursively(trs, name);
		if (transform == null)
		{
			return default(T);
		}
		return transform.GetComponent<T>();
	}

	private void SetTopRankerInfo(string[] propActionArgs)
	{
		RecordsOfHonorContentType valueEnum = ArgParser.GetValueEnum<string, RecordsOfHonorContentType>((IList<string>)propActionArgs, 1, RecordsOfHonorContentType.None);
		_topRankerInfo = SingletonAuto<HonoraryChamberManager>.instance.Get(valueEnum);
	}

	private void SetRankerInfoLabels()
	{
		UIHelper.EnableGameObject(_spriteLabelBg, _topRankerInfo != null);
		UIHelper.EnableGameObject(_clipPanel, _topRankerInfo != null);
	}

	protected void CreateRenderFacadeStock(string[] propActionArgs)
	{
		if (!GlobalAutoBehaviour<PatchDownloader>.instance.IsPatchDownloading() && _topRankerInfo != null)
		{
			int heroIndex = new UIAvatarHelper.AvatarInfo(_topRankerInfo.AvatarHeroIndex).HeroIndex;
			int valueInt = ArgParser.GetValueInt(propActionArgs, 2);
			int valueInt2 = ArgParser.GetValueInt(propActionArgs, 3);
			int valueInt3 = ArgParser.GetValueInt(propActionArgs, 4);
			SetLabelPosition(valueInt3);
			string name = _topRankerInfo.Name;
			CreatureData creatureDataForPrefix = GetCreatureDataForPrefix(heroIndex, "Trans");
			if (creatureDataForPrefix == null)
			{
				VespaDebug.LogError(string.Format("HonorRenderFacadeProp:: GetCreatureDataForPrefix Fail ( heroIndex : ({0}) , CreaturePrefix : ({1}) )", heroIndex, "Trans"));
				return;
			}
			UIHelper.SetLabelText(_titleLabel, GetContentsLocale(_topRankerInfo.ContentType));
			UIHelper.SetLabelText(_nameLabel, name);
			SetCountryTexture(_topRankerInfo.CountryCode);
			CreateRenderFacade(CreateStock(creatureDataForPrefix, valueInt, valueInt2));
		}
	}

	private void SetLabelPosition(int yPos)
	{
		if (!(_labelRoot == null) && yPos != 0)
		{
			_labelRoot.localPosition = new Vector3(0f, yPos, 0f);
		}
	}

	private void SetCountryTexture(string countryCode)
	{
		CountryCodeData dataByCode = GameRoot.Instance.CountryCodeDataContainer.GetDataByCode(countryCode);
		if (dataByCode != null)
		{
			AssetTexture plainTexture = UIHelper.GetPlainTexture(dataByCode.IconPath);
			UIHelper.ResetPlaneAssetTextureAsync(_propComponent, _countryTexture, plainTexture);
		}
	}

	private string GetContentsLocale(RecordsOfHonorContentType contentType)
	{
		string key = contentType.ToString();
		switch (contentType)
		{
		case RecordsOfHonorContentType.BanPick:
			key = "HONOR_RANK1_PVP2";
			break;
		case RecordsOfHonorContentType.WorldBoss:
			key = "HONOR_RANK1_WORLDBOSS";
			break;
		case RecordsOfHonorContentType.GuildSuppress:
			key = "HONOR_RANK1_GUILD_PUNITIVE";
			break;
		}
		return LocalizationManager.GetStringDirect(key);
	}

	private CreatureData GetCreatureDataForPrefix(int heroIndex, string prefix)
	{
		string codeName = GameRoot.Instance.CreatureDataContainer.GetCode(heroIndex);
		if (String_Extension_Method.IsNullOrEmpty(codeName))
		{
			VespaDebug.LogError($"heroIndex[{heroIndex}] CodeName is Null or Empty ");
			return null;
		}
		return GameRoot.Instance.CreatureDataContainer.GetDataOf((CreatureData data) => data.ModelCode == codeName && data.ModelPrefix == prefix);
	}

	public override void SetExtraAlpha(float alpha)
	{
		base.SetExtraAlpha(alpha);
		if (!(_clipPanel == null))
		{
			_clipPanel.alpha = alpha;
		}
	}

	public override void Release()
	{
		base.Release();
		_topRankerInfo = null;
		UIHelper.ClearPlaneAssetTexture(_countryTexture);
	}
}
