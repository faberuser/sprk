using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class PunishmentRaidRewardInfoPopup : BaseWindow, CountInputField2.IListener
{
	public const string PrefabPath = "Resources/Prefab2/Window/Raid/PunishmentRaidRewardInfoPopup";

	public CountInputField2 CountInputField;

	public UILabel TItleLabel;

	public UILabel DescLabel;

	public UIBundleLoader BossBundleLoader;

	public UIBaseGrid BossRewardGrid;

	public UIBaseGrid SubRewardGrid;

	private List<Component> _listItemIconComp;

	private List<PunishmentRaidRewardSubComponent> _subBossCompList = new List<PunishmentRaidRewardSubComponent>();

	private int _raidGroupIndex;

	private int _level;

	public void Open(int raidGroupIndex, int level)
	{
		checked
		{
			Open(delegate
			{
				_raidGroupIndex = raidGroupIndex;
				_level = level;
				int num = 5;
				long maxCount = num;
				OpenPunishmentRaidInfo openPunishmentRaidInfo = GlobalAutoBehaviour<AccountManager>.instance.OpenPunishmentRaidManager.Get(raidGroupIndex, EPunishmentDungeonType.Boss);
				if (openPunishmentRaidInfo != null)
				{
					int clearLevel = openPunishmentRaidInfo.ClearLevel;
					int num2 = 5;
					int num3 = clearLevel;
					maxCount = unchecked((long)num3) + unchecked((long)num2);
				}
				CountInputField2 countInputField = CountInputField;
				PunishmentRaidRewardInfoPopup iListener = this;
				CountInputField2.CountInputFieldInfo obj = new CountInputField2.CountInputFieldInfo
				{
					MinCount = 1,
					MaxCount = maxCount
				};
				int level2 = _level;
				obj.InitCount = level2;
				countInputField.Open(iListener, obj);
				Init();
			}, null);
		}
	}

	public void Open(int raidGroupIndex)
	{
		Open(delegate
		{
			_raidGroupIndex = raidGroupIndex;
			Init();
		}, null);
	}

	public void Init()
	{
		ReleaseComponent();
		PunishmentGroupData data = GameRoot.Instance.PunishmentGroupDataContainer.GetData(_raidGroupIndex, EPunishmentDungeonType.Boss);
		if (data == null)
		{
			return;
		}
		UIHelper.SetLabelText(DescLabel, LocalizationManager.GetStringDirect(data.RewardDescKey));
		IList<PunishmentRaidData> punishmentRaidDatas = GameRoot.Instance.PunishmentRaidDataContainer.GetPunishmentRaidDatas(_raidGroupIndex);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(punishmentRaidDatas); i++)
		{
			PunishmentRaidData punishmentRaidData = punishmentRaidDatas[i];
			if (punishmentRaidData.IsBoss)
			{
				UIHelper.SetBundleTexture(BossBundleLoader, punishmentRaidData.ImagePath);
				SetBossReward(punishmentRaidData, _level);
			}
			else
			{
				SetSubBossReward(punishmentRaidData, _level);
			}
		}
		UIHelper.RepositionGrid(BossRewardGrid);
		UIHelper.RepositionGrid(SubRewardGrid);
	}

	private void SetBossReward(PunishmentRaidData raidData, int level)
	{
		if (raidData == null)
		{
			return;
		}
		PunishmentRaidRewardData punishmentRaidRewardData = GameRoot.Instance.PunishmentRaidRewardDataContainer.GetPunishmentRaidRewardData(PunishmentRaidRewardType.Main, raidData.RaidIndex);
		if (punishmentRaidRewardData != null)
		{
			int infinityRewardGroupIndex = punishmentRaidRewardData.InfinityRewardGroupIndex;
			if (GameRoot.Instance.InfinityRewardGroupDataContainer.GetData(infinityRewardGroupIndex) != null)
			{
				_listItemIconComp = UIHelper.CreateInfinityRewardItemIconComponentList(infinityRewardGroupIndex, level, BossRewardGrid);
			}
		}
	}

	private void SetSubBossReward(PunishmentRaidData raidData, int level)
	{
		if (raidData != null)
		{
			PunishmentRaidRewardData punishmentRaidRewardData = GameRoot.Instance.PunishmentRaidRewardDataContainer.GetPunishmentRaidRewardData(PunishmentRaidRewardType.Sub, raidData.RaidIndex);
			PunishmentRaidRewardSubComponent punishmentRaidRewardSubComponent = UIHelper.AddGridItem(SubRewardGrid, PunishmentRaidRewardSubComponent.PrefabAuto);
			punishmentRaidRewardSubComponent.Open(punishmentRaidRewardData, level);
			_subBossCompList.Add(punishmentRaidRewardSubComponent);
		}
	}

	public override void OnClosing()
	{
		base.OnClosing();
		ReleaseComponent();
	}

	private void ReleaseComponent()
	{
		FactoryBase.ReleaseAndClearList(_listItemIconComp);
		FactoryBase.ReleaseAndClearList(_subBossCompList);
	}

	public void OnClickClose()
	{
		Close();
	}

	void CountInputField2.IListener.ChangeCount()
	{
		long currentCount = CountInputField.CurrentCount;
		int level = _level;
		if (currentCount != level)
		{
			long currentCount2 = CountInputField.CurrentCount;
			_level = checked((int)currentCount2);
			Init();
		}
	}
}
