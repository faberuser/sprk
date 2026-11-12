using System;
using System.Collections;
using NGame2.NAccount;
using NNguiExtension;
using NShared;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NSoulWeapon;

public class SoulWeaponUpgradeWindow : CommonSoulWeaponWindow, SoulWeaponList.IListener
{
	public interface IListener
	{
		void UpgradeSoulWeapon(EquipItemInfo equipItemInfo, RequirePrice reqPrice, Action onClose);
	}

	[Serializable]
	public class NodeTreeStock
	{
		public int Grade;

		public string PackCode;

		public string TreeName;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/SoulWeapon/SoulWeaponUpgradeWindow";

	public NodeTreeUIPlayer BgNodeTreeUIPlayer;

	public NodeTreeStock[] NodeTreeStocks = new NodeTreeStock[3]
	{
		new NodeTreeStock
		{
			Grade = 0,
			PackCode = "Common/Cinema",
			TreeName = "SoulWeapon_RankUP_View_0"
		},
		new NodeTreeStock
		{
			Grade = 1,
			PackCode = "Common/Cinema",
			TreeName = "SoulWeapon_RankUP_View_1"
		},
		new NodeTreeStock
		{
			Grade = 2,
			PackCode = "Common/Cinema",
			TreeName = "SoulWeapon_RankUP_View_2"
		}
	};

	public SoulWeaponUpgradeInfo SoulWeaponUpgradeInfo;

	public SoulWeaponMaterialInfo SoulWeaponMaterialInfo;

	public RequirePrice RequirePrice;

	public Transform MaxGradeTransform;

	public Transform UpgradeInfoTransform;

	public Transform ReqPriceTransform;

	public UILabel UpgradeConditionLabel;

	public UIButton UpgradeButton;

	public UISpriteSelector BgSpriteSelector;

	private bool _isWaitUpgardeResult;

	private IListener _iListener;

	private int _prevGrade = -1;

	private DateTime _lastUpgradeButtonClickTime;

	private bool IsPossibleUpgrade
	{
		get
		{
			if (base.SoulWeaponInfo == null)
			{
				return false;
			}
			if (SoulWeaponMaterialInfo.IsMoreMyItemCount)
			{
				return base.SoulWeaponInfo.ReinforceLevel >= RequireReinforceLevel;
			}
			return false;
		}
	}

	private int RequireReinforceLevel
	{
		get
		{
			if (base.SoulWeaponInfo == null)
			{
				return 10;
			}
			int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("SoulWeaponUpgradeRatio", 10);
			if (base.SoulWeaponInfo.Grade > 0)
			{
				return valueInt * (base.SoulWeaponInfo.Grade + 1);
			}
			return valueInt;
		}
	}

	private SoulWeaponUpgradePriceData NextSoulWeaponUpgradePriceData
	{
		get
		{
			if (base.SoulWeaponInfo == null)
			{
				return null;
			}
			if (IsMaxGrade)
			{
				return null;
			}
			return GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponManager.GetNextUpgradeSoulWeaponInfo(base.SoulWeaponInfo)?.GetSoulWeaponUpgradePriceData();
		}
	}

	private bool IsMaxGrade
	{
		get
		{
			if (base.SoulWeaponInfo == null)
			{
				return true;
			}
			SoulWeaponInfo nextUpgradeSoulWeaponInfo = GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponManager.GetNextUpgradeSoulWeaponInfo(base.SoulWeaponInfo);
			if (nextUpgradeSoulWeaponInfo == null)
			{
				return true;
			}
			return base.SoulWeaponInfo.Grade == nextUpgradeSoulWeaponInfo.Grade;
		}
	}

	public void Open(EquipItemInfo equipItemInfo, IListener listener, bool isSelectFirst)
	{
		VespaCoroutine.Start(OpenAsyc(listener, equipItemInfo, isSelectFirst));
	}

	private IEnumerator OpenAsyc(IListener iListener, EquipItemInfo equipItemInfo, bool isSelectFirst)
	{
		Open(equipItemInfo);
		Setup(iListener, isSelectFirst);
		SetSoulWeaponNodeTree(equipItemInfo);
		_lastUpgradeButtonClickTime = DateTime.Now;
		_isWaitUpgardeResult = false;
		yield return null;
	}

	public override void Close()
	{
		if (!_isWaitUpgardeResult)
		{
			base.Close();
			_iListener = null;
			SoulWeaponUpgradeInfo.Close();
			SoulWeaponMaterialInfo.Close();
			if (BgNodeTreeUIPlayer != null)
			{
				BgNodeTreeUIPlayer.Close();
			}
			_prevGrade = -1;
		}
	}

	private void Setup(IListener listener, bool isSelectFirst)
	{
		_iListener = listener;
		InitSoulWeaponList(this, base.EquipItemInfo, SoulWeaponList.ListType.SoulWeapon, isSelectFirst);
		SetUpgradeInfo();
	}

	public void SetSoulWeaponNodeTree(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo != null && equipItemInfo.IsSoulWeapon())
		{
			SoulWeaponInfo soulWeaponInfo = equipItemInfo.SoulWeaponInfo;
			int grade = soulWeaponInfo.Grade;
			if (_prevGrade == grade)
			{
				CreateSoulWeapon(equipItemInfo.ItemData.Code, grade, isAdd: false);
				return;
			}
			_prevGrade = grade;
			CreateSoulWeapon(equipItemInfo.ItemData.Code, grade, isAdd: true);
			NodeTreeStock nodeTreeStock = GetNodeTreeStock(soulWeaponInfo.Grade);
			BgNodeTreeUIPlayer.PlayOnce(nodeTreeStock.PackCode, nodeTreeStock.TreeName);
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
			data.GetModleViewValues(ESoulWeaponModelViewType.Upgrade, out position, out rotation, out scale);
			int weaponCreatureIndex = data.WeaponCreatureIndex;
			Vector3 arrayToVec = GetArrayToVec3(position);
			Vector3 arrayToVec2 = GetArrayToVec3(rotation);
			if (isAdd)
			{
				BgNodeTreeUIPlayer.AddCreatureStock(weaponCreatureIndex, grade, arrayToVec, arrayToVec2, scale);
			}
			else
			{
				BgNodeTreeUIPlayer.SpawnCreature(weaponCreatureIndex, grade, arrayToVec, arrayToVec2, scale);
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

	private void SetUpgradeInfo()
	{
		SetSelectSoulWeapon(null);
		SetBgSpriteSelector();
		SoulWeaponUpgradeInfo.Open(base.SoulWeaponInfo);
		SetUpgradeReqPrice();
		SetUpgradeCondtionLabel();
		SetMaterialInfo();
		SetUpgradeButtonState();
		SetSoulWeaponNodeTree(base.EquipItemInfo);
		SetUpgradeState();
	}

	private void SetBgSpriteSelector()
	{
		if (base.SoulWeaponInfo != null && BgSpriteSelector != null)
		{
			BgSpriteSelector.SelectByIndex(base.SoulWeaponInfo.Grade);
		}
	}

	private void SetMaterialInfo()
	{
		if (base.EquipItemInfo != null)
		{
			SoulWeaponInfo nextUpgradeSoulWeaponInfo = GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponManager.GetNextUpgradeSoulWeaponInfo(base.SoulWeaponInfo);
			SoulWeaponMaterialInfo.Open(null, base.EquipItemInfo.SlotIndex, nextUpgradeSoulWeaponInfo, enableItemCount: true);
		}
	}

	private void SetUpgradeReqPrice()
	{
		RequirePrice.Open(-1L, 0L);
		if (base.SoulWeaponInfo != null)
		{
			SoulWeaponUpgradePriceData nextSoulWeaponUpgradePriceData = NextSoulWeaponUpgradePriceData;
			if (nextSoulWeaponUpgradePriceData != null)
			{
				RequirePrice.Open(nextSoulWeaponUpgradePriceData.ReqGold, 0L);
			}
		}
	}

	private void SetUpgradeCondtionLabel()
	{
		UIHelper.SetLabelText(UpgradeConditionLabel, "-");
		if (base.SoulWeaponInfo != null)
		{
			string text = ((base.SoulWeaponInfo.ReinforceLevel >= RequireReinforceLevel) ? "[ffb629]" : "[F85E68]");
			UIHelper.SetLabelText(UpgradeConditionLabel, text + LocalizationManager.GetStringDirect("SOULWEAPON_INFO_ETHERREINFORCE") + " +" + RequireReinforceLevel + "[-]");
		}
	}

	private void SetUpgradeButtonState()
	{
		UIHelper.EnableButton(UpgradeButton, !IsMaxGrade && IsPossibleUpgrade);
	}

	private void SetUpgradeState()
	{
		bool isMaxGrade = IsMaxGrade;
		UIHelper.EnableGameObject(MaxGradeTransform, isMaxGrade);
		UIHelper.EnableGameObject(UpgradeInfoTransform, !isMaxGrade);
		UIHelper.EnableGameObject(ReqPriceTransform, !isMaxGrade);
	}

	public void Refresh(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo != null)
		{
			base.EquipItemInfo = equipItemInfo;
			SetUpgradeInfo();
			RefreshList();
			RefreshSoulWeaponListItemIconComponent(equipItemInfo);
		}
	}

	public void OnClickUpgradeButton()
	{
		DateTime now = DateTime.Now;
		if ((now - _lastUpgradeButtonClickTime).TotalSeconds < 0.20000000298023224)
		{
			return;
		}
		_lastUpgradeButtonClickTime = now;
		if (_iListener != null)
		{
			EnableAllSoulWeaponList(enable: false);
			_isWaitUpgardeResult = true;
			_iListener.UpgradeSoulWeapon(base.EquipItemInfo, RequirePrice, delegate
			{
				_isWaitUpgardeResult = false;
			});
		}
	}

	void SoulWeaponList.IListener.SelectSoulWeapon(EquipItemInfo equipItemInfo, SoulWeaponList.ListType listType)
	{
		if (equipItemInfo != null)
		{
			base.EquipItemInfo = equipItemInfo;
			SetUpgradeInfo();
			if (listType == AllSoulWeaponList.AllListType)
			{
				SoulWeaponList.SelectSoulWeaponItemIconComponent(equipItemInfo);
				EnableAllSoulWeaponList(enable: false);
			}
		}
	}
}
