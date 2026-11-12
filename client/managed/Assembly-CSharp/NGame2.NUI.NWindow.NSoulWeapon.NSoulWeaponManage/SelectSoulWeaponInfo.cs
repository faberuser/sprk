using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NSoulWeapon;
using NGame2.NUtil.NStringProxy;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI.NWindow.NSoulWeapon.NSoulWeaponManage;

public class SelectSoulWeaponInfo : MonoBehaviour
{
	public interface IListener
	{
		void ChangeSoulWeaponInfo(SoulWeaponInfoType infoType);
	}

	public enum SoulWeaponInfoType
	{
		None,
		Reinforce,
		Upgrade,
		Transfer,
		Redistribution
	}

	[Serializable]
	public class NodeTreeStock
	{
		public int Grade;

		public string PackCode;

		public string TreeName;
	}

	[Serializable]
	public class ReinforceExpEffect
	{
		public Transform EffectTrasnform;

		public float EffectSpot;
	}

	[Serializable]
	public class SoulWeaponTransferUi
	{
		public UIButton Button_OnClickTransfer;

		public void InitUi(EquipItemInfo equipItemInfo)
		{
			UIHelper.EnableButton(Button_OnClickTransfer, !equipItemInfo.CLI_IsLimitBreaked());
		}
	}

	public NodeTreeUIPlayer SoulWeaponNodeTreeUIPlayer;

	public NodeTreeStock[] NodeTreeStocks = new NodeTreeStock[3]
	{
		new NodeTreeStock
		{
			Grade = 0,
			PackCode = "Common/Cinema",
			TreeName = "SoulWeapon_Info_View_0"
		},
		new NodeTreeStock
		{
			Grade = 1,
			PackCode = "Common/Cinema",
			TreeName = "SoulWeapon_Info_View_1"
		},
		new NodeTreeStock
		{
			Grade = 2,
			PackCode = "Common/Cinema",
			TreeName = "SoulWeapon_Info_View_2"
		}
	};

	public SelectSoulWeaponStoryInfo SelectSoulWeaponStoryInfo;

	public SoulWeaponSkillInfo SelectSoulWeaponSkillInfo;

	public SoulWeaponGradeInfo SoulWeaponGradeInfo;

	public SelectSoulWeaponReinforceExpInfo SelectSoulWeaponReinforceExpInfo;

	public UITexture CreatureTexture;

	public Transform StatComponentRootTransform;

	public UILabel SoulWeaponNameLabel;

	public UISprite SkillInfoBgSprite;

	public UIWidget SkillInfoRoot;

	public UITable StatTable;

	public UITable SkillInfoTable;

	public UITable SkillUpgradeInfoTable;

	public UIWidget SkillInfoUpgardeRoot;

	public UIScrollView InfoScroll;

	public UISpriteSelector BackGroundGradeSpriteSelector;

	public List<ReinforceExpEffect> ReinforceExpEffectTransformList = new List<ReinforceExpEffect>();

	public SoulWeaponTransferUi SoulWeaponTransfer;

	private IListener _iListener;

	private SoulWeaponStatComponent _soulWeaponStatComponent;

	private List<LabelFieldComponent> _statList = new List<LabelFieldComponent>();

	private List<SoulWeaponSkillUpgradeComponent> _skillUpgradeList = new List<SoulWeaponSkillUpgradeComponent>();

	private int _prevGrade = -1;

	public void Open(IListener iListener)
	{
		Init(iListener);
	}

	public void Close()
	{
		FactoryBase.ReleaseAndClearList(_statList);
		FactoryBase.ReleaseAndClearList(_skillUpgradeList);
		FactoryBase.SafeReleaseComponent(ref _soulWeaponStatComponent);
		if (SoulWeaponNodeTreeUIPlayer != null)
		{
			SoulWeaponNodeTreeUIPlayer.Close();
		}
		_prevGrade = -1;
		SelectSoulWeaponStoryInfo.InitStoryToggleState(state: false);
	}

	private void Init(IListener iListener)
	{
		_iListener = iListener;
		EnableSoulWeaponStoryInfo(enable: false);
	}

	public void SetSelectSoulWeaponInfo(EquipItemInfo equipItemInfo)
	{
		SoulWeaponInfo soulWeaponInfo = null;
		if (equipItemInfo != null)
		{
			soulWeaponInfo = equipItemInfo.SoulWeaponInfo;
		}
		SetHeroBundleLoader(equipItemInfo);
		SetSoulWeaponNodeTree(equipItemInfo);
		SetSoulWeaponReinforceExpInfo(soulWeaponInfo);
		SetSoulWeaponName(equipItemInfo);
		SetSoulWeaponGradeInfo(soulWeaponInfo);
		SelectSoulWeaponStoryInfo.Open(soulWeaponInfo);
		SetStatComponent(soulWeaponInfo);
		SetSkillInfo(equipItemInfo);
		SoulWeaponTransfer.InitUi(equipItemInfo);
		RepositionStatInfo();
		if (InfoScroll != null)
		{
			InfoScroll.ResetPosition();
		}
	}

	private void SetSoulWeaponReinforceExpInfo(SoulWeaponInfo soulWeaponInfo)
	{
		SelectSoulWeaponReinforceExpInfo.Open(soulWeaponInfo);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(ReinforceExpEffectTransformList))
		{
			return;
		}
		ReinforceExpEffect reinforceExpEffect = null;
		for (int i = 0; i < ReinforceExpEffectTransformList.Count; i++)
		{
			reinforceExpEffect = ReinforceExpEffectTransformList[i];
			if (reinforceExpEffect != null)
			{
				UIHelper.EnableGameObject(reinforceExpEffect.EffectTrasnform, SelectSoulWeaponReinforceExpInfo.Progress >= reinforceExpEffect.EffectSpot);
			}
		}
	}

	private void SetSoulWeaponGradeInfo(SoulWeaponInfo soulWeaponInfo)
	{
		if (soulWeaponInfo != null)
		{
			SoulWeaponGradeInfo.Open(soulWeaponInfo.Grade, soulWeaponInfo.ReinforceLevel, isLocalize: false);
			if (BackGroundGradeSpriteSelector != null)
			{
				BackGroundGradeSpriteSelector.SelectByIndex(soulWeaponInfo.Grade);
			}
		}
	}

	private void SetHeroBundleLoader(EquipItemInfo equipItemInfo)
	{
		if (CreatureTexture == null || equipItemInfo == null || equipItemInfo.EquipItemData == null)
		{
			return;
		}
		int[] creatureIndex = equipItemInfo.EquipItemData.CreatureIndex;
		if (__Extension_Method_ICollection__.IsNullOrEmpty(creatureIndex) || creatureIndex.Length == 0 || GameRoot.Instance.CreatureDataContainer.GetData(creatureIndex[0]) == null)
		{
			return;
		}
		CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(creatureIndex[0]);
		if (cachedHeroInfo == null)
		{
			return;
		}
		CostumeData dataByHeroStar = GameRoot.Instance.CostumeDataContainer.GetDataByHeroStar(cachedHeroInfo.HeroIndex, cachedHeroInfo.Star);
		if (dataByHeroStar != null)
		{
			string iconPath = UIAvatarHelper.GetIconPath(new UIAvatarHelper.AvatarInfo(dataByHeroStar).AvatarIndex);
			if (!String_Extension_Method.IsNullOrEmpty(iconPath))
			{
				UIHelper.ResetPlaneAssetTextureAsync(this, CreatureTexture, UIHelper.GetPlainTexture(iconPath));
			}
		}
	}

	private void SetSoulWeaponName(EquipItemInfo equipItemInfo)
	{
		UIHelper.SetLabelText(SoulWeaponNameLabel, "-");
		if (equipItemInfo != null && equipItemInfo.SoulWeaponInfo != null)
		{
			UIHelper.SetLabelText(SoulWeaponNameLabel, ItemNameBuilder.GetItemName(equipItemInfo.ItemData, equipItemInfo));
		}
	}

	private void SetStatComponent(SoulWeaponInfo soulWeaponInfo)
	{
		if (StatComponentRootTransform == null)
		{
			return;
		}
		if (_soulWeaponStatComponent == null)
		{
			_soulWeaponStatComponent = SoulWeaponStatComponent.PrefabAuto.Factory.Create(StatComponentRootTransform);
			if (_soulWeaponStatComponent == null)
			{
				return;
			}
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_soulWeaponStatComponent.transform);
		}
		_soulWeaponStatComponent.Open(null, soulWeaponInfo);
	}

	private void SetSkillInfo(EquipItemInfo equipItemInfo)
	{
		SelectSoulWeaponSkillInfo.Open(equipItemInfo?.SoulWeaponInfo);
		SetSkillUpgradeInfo(equipItemInfo);
		SkillInfoTable.Reposition();
		SkillInfoTable.repositionNow = true;
	}

	private void SetSkillUpgradeInfo(EquipItemInfo equipItemInfo)
	{
		FactoryBase.ReleaseAndClearList(_skillUpgradeList);
		if (SkillUpgradeInfoTable == null || equipItemInfo == null || !equipItemInfo.IsSoulWeapon())
		{
			return;
		}
		IList<SoulWeaponUpgradeData> allData = GameRoot.Instance.SoulWeaponUpgradeDataContainer.GetAllData();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(allData))
		{
			return;
		}
		for (int i = 0; i < allData.Count; i++)
		{
			SoulWeaponUpgradeData soulWeaponUpgradeData = allData[i];
			if (soulWeaponUpgradeData != null && soulWeaponUpgradeData.Grade > 0)
			{
				SoulWeaponSkillUpgradeComponent soulWeaponSkillUpgradeComponent = SoulWeaponSkillUpgradeComponent.PrefabAuto.Factory.Create(SkillUpgradeInfoTable.transform);
				if (!(soulWeaponSkillUpgradeComponent == null))
				{
					soulWeaponSkillUpgradeComponent.Open(equipItemInfo.SoulWeaponInfo, soulWeaponUpgradeData.Grade);
					_skillUpgradeList.Add(soulWeaponSkillUpgradeComponent);
				}
			}
		}
		SkillUpgradeInfoTable.Reposition();
		SkillUpgradeInfoTable.repositionNow = true;
		UIHelper.TableCalcBounds(SkillUpgradeInfoTable, SkillInfoUpgardeRoot, isFixedWidth: true);
	}

	private void RepositionStatInfo()
	{
		SingletonUpdater<Executer>.instance.NextRun(delegate
		{
			SkillInfoTable.Reposition();
			SkillInfoTable.repositionNow = true;
			UIHelper.TableCalcBounds(SkillInfoTable, SkillInfoBgSprite, isFixedWidth: true);
			UIHelper.TableCalcBounds(SkillInfoTable, SkillInfoRoot, isFixedWidth: true);
			StatTable.Reposition();
			StatTable.repositionNow = true;
		});
	}

	public void SetSoulWeaponNodeTree(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo == null || !equipItemInfo.IsSoulWeapon())
		{
			return;
		}
		SoulWeaponInfo soulWeaponInfo = equipItemInfo.SoulWeaponInfo;
		if (soulWeaponInfo != null)
		{
			int grade = soulWeaponInfo.Grade;
			if (_prevGrade == grade)
			{
				CreateSoulWeapon(equipItemInfo.ItemData.Code, grade, isAdd: false);
				return;
			}
			_prevGrade = grade;
			CreateSoulWeapon(equipItemInfo.ItemData.Code, grade, isAdd: true);
			NodeTreeStock nodeTreeStock = GetNodeTreeStock(soulWeaponInfo.Grade);
			SoulWeaponNodeTreeUIPlayer.PlayOnce(nodeTreeStock.PackCode, nodeTreeStock.TreeName);
		}
	}

	public void CreateSoulWeapon(string itemCode, int grade, bool isAdd)
	{
		SoulWeaponNodeTreeData data = GameRoot.Instance.SoulWeaponNodeTreeDataContainer.GetData(itemCode);
		if (data != null)
		{
			float[] position = null;
			float[] rotation = null;
			float scale = 0f;
			data.GetModleViewValues(ESoulWeaponModelViewType.Manage, out position, out rotation, out scale);
			int weaponCreatureIndex = data.WeaponCreatureIndex;
			Vector3 arrayToVec = GetArrayToVec3(position);
			Vector3 arrayToVec2 = GetArrayToVec3(rotation);
			if (isAdd)
			{
				SoulWeaponNodeTreeUIPlayer.AddCreatureStock(weaponCreatureIndex, grade, arrayToVec, arrayToVec2, scale);
			}
			else
			{
				SoulWeaponNodeTreeUIPlayer.SpawnCreature(weaponCreatureIndex, grade, arrayToVec, arrayToVec2, scale);
			}
		}
	}

	private Vector3 GetArrayToVec3(float[] vectorArray)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(vectorArray) || __Extension_Method_ICollection__.COUNT(vectorArray) < 3)
		{
			return Vector3.zero;
		}
		float x = vectorArray[0];
		float y = vectorArray[1];
		float z = vectorArray[2];
		return new Vector3(x, y, z);
	}

	public NodeTreeStock GetNodeTreeStock(int grade)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(NodeTreeStocks); i++)
		{
			NodeTreeStock nodeTreeStock = NodeTreeStocks[i];
			if (nodeTreeStock != null && nodeTreeStock.Grade == grade)
			{
				return nodeTreeStock;
			}
		}
		return new NodeTreeStock();
	}

	private void EnableSoulWeaponStoryInfo(bool enable)
	{
		if (SelectSoulWeaponStoryInfo != null)
		{
			SelectSoulWeaponStoryInfo.EnableStory(enable);
		}
	}

	public void OnClickStory(UIToggle toggle)
	{
		if (toggle != null)
		{
			EnableSoulWeaponStoryInfo(!toggle.value);
		}
	}

	public void OnClickReinforce()
	{
		if (_iListener != null)
		{
			_iListener.ChangeSoulWeaponInfo(SoulWeaponInfoType.Reinforce);
		}
	}

	public void OnClickUpgrade()
	{
		if (_iListener != null)
		{
			_iListener.ChangeSoulWeaponInfo(SoulWeaponInfoType.Upgrade);
		}
	}

	public void OnClickTransfer()
	{
		if (_iListener != null)
		{
			_iListener.ChangeSoulWeaponInfo(SoulWeaponInfoType.Transfer);
		}
	}

	public void OnClickReditribution()
	{
		if (_iListener != null)
		{
			_iListener.ChangeSoulWeaponInfo(SoulWeaponInfoType.Redistribution);
		}
	}

	public void OnClickCloseSoulWeaponStory(UIToggle toggle)
	{
		if (toggle != null)
		{
			toggle.Set(state: false);
			EnableSoulWeaponStoryInfo(enable: false);
		}
	}
}
