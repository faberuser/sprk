using System.Collections.Generic;
using NGame2.NAccount;
using NGameAsset;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class PunishmentRaidRewardComponent : LinkFactoryComponent, NVespa.NPrefab2.InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/Raid/PunishmentRaidRewardComponent";

	public static PrefabAuto<PunishmentRaidRewardComponent> PrefabAuto = new PrefabAuto<PunishmentRaidRewardComponent>("Prefab2/Component/Raid/PunishmentRaidRewardComponent");

	public const string PrefabPath2 = "Prefab2/Component/Raid/PunishmentRaidRewardComponent2";

	public static PrefabAuto<PunishmentRaidRewardComponent> PrefabAuto2 = new PrefabAuto<PunishmentRaidRewardComponent>("Prefab2/Component/Raid/PunishmentRaidRewardComponent2");

	public UIWidget ThisWidget;

	public UITexture MonsterTexture;

	public UILabel MonsterName;

	public UISprite ClearedSprite;

	public UIBaseGrid Grid_MainReward;

	public UIBaseGrid Grid_SubReward;

	private PunishmentRaidData _punishmentRaidData;

	private PunishmentRaidRewardData _bossRewardData;

	private List<Component> _mainRewardCompList = new List<Component>();

	private List<Component> _subRewardCompList = new List<Component>();

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		_punishmentRaidData = null;
		_bossRewardData = null;
		ClearItemIconList();
	}

	public void Open(PunishmentRaidData punishmentRaidData, PunishmentRaidRewardData punishmentRaidRewardData)
	{
		_punishmentRaidData = punishmentRaidData;
		_bossRewardData = punishmentRaidRewardData;
		InitRaidInfo();
		InitReward();
	}

	public void InitRaidInfo()
	{
		if (_punishmentRaidData != null)
		{
			string stringDirect = LocalizationManager.GetStringDirect(_punishmentRaidData.EntryLocalKey);
			string text = stringDirect;
			if (!String_Extension_Method.IsNullOrEmpty(_punishmentRaidData.LabelColor))
			{
				text = $"[{_punishmentRaidData.LabelColor}]{stringDirect}[-]";
			}
			UIHelper.SetLabelText(MonsterName, text);
			string imagePath = _punishmentRaidData.ImagePath;
			AssetTexture assetTexture = new AssetTexture();
			assetTexture.Open(imagePath);
			UIHelper.ResetPlaneAssetTextureAsync(this, MonsterTexture, assetTexture);
			bool flag = GlobalAutoBehaviour<AccountManager>.instance.OpenPunishmentRaidManager.IsOpenPunishmentRaid(_punishmentRaidData.GroupIndex, EPunishmentDungeonType.Boss);
			PunishmentRaidInfo punishmentRaidInfo = GlobalAutoBehaviour<AccountManager>.instance.OpenPunishmentRaidManager.GetPunishmentRaidInfo(_punishmentRaidData.GroupIndex, _punishmentRaidData.RaidIndex, _punishmentRaidData.RaidLevel);
			bool flag2 = punishmentRaidInfo != null && punishmentRaidInfo.ClearCheckIndex != 0 && !String_Extension_Method.IsNullOrEmpty(punishmentRaidInfo.ClearedTime);
			UIHelper.EnableGameObject(ClearedSprite, flag && flag2);
		}
	}

	public void InitReward()
	{
		if (_bossRewardData != null)
		{
			int infinityRewardGroupIndex = _bossRewardData.InfinityRewardGroupIndex;
			_mainRewardCompList = UIHelper.CreateInfinityRewardItemIconComponentList(infinityRewardGroupIndex, 1, Grid_MainReward);
			UIHelper.RepositionGrid(Grid_MainReward);
		}
	}

	public void AddSubReward(PunishmentRaidRewardData punishmentRaidRewardData)
	{
	}

	private void ClearItemIconList()
	{
		FactoryBase.ReleaseAndClearList(_mainRewardCompList);
		FactoryBase.ReleaseAndClearList(_subRewardCompList);
	}
}
