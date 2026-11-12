using NGame2.NAccount;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NSoulWeapon;

public class SoulWeaponLiberationResultWindow : BaseWindow
{
	public enum NodeTreeSatate
	{
		LiberationWait,
		LiberationAction,
		Liberation,
		Close
	}

	public const string PrefabPath = "Resources/Prefab2/Window/SoulWeapon/SoulWeaponLiberationResultWIndow";

	public SoulWeaponStatInfo SoulWeaponStatInfo;

	public SoulWeaponSkillInfo SoulWeaponSkillInfo;

	public SoulWeaponStatInfo SoulWeaponStatInfoLeft;

	public SoulWeaponSkillInfo SoulweaponSkillInfoLeft;

	public GameObject GoDescRoot;

	public GameObject GoDescLeftRoot;

	public GameObject GoDescRightRoot;

	public GameObject GoTouchRoot;

	public NodeTreeUIPlayer BgNodeTreePlayer;

	public UIGame3DWidget FadeOutGame3DWidget;

	private SoulWeaponInfo _soulWeaponInfo;

	private NodeTreeSatate _nodeTreeState;

	private EquipItemInfo _equipItemInfo;

	public string NodeTreeWaitPackCode = "Common/Cinema";

	public string NodeTreeWaitTreeName = "SoulWeapon_Liberation_Wait";

	public string NodeTreeActionPackCode = "Common/Cinema";

	public string NodeTreeActionTreeName = "SoulWeapon_Liberation_Action";

	public bool IsWaitingResult;

	public void Open(SoulWeaponInfo soulWeaponInfo, EquipItemInfo equipItemInfo = null)
	{
		Open(delegate
		{
			Setup(soulWeaponInfo, equipItemInfo);
		}, null);
	}

	public void Setup(SoulWeaponInfo soulWeaponInfo, EquipItemInfo equipItemInfo = null)
	{
		_soulWeaponInfo = soulWeaponInfo;
		_equipItemInfo = equipItemInfo;
		IsWaitingResult = true;
		SetNodeTreePlay(soulWeaponInfo);
		SetupDescSide();
	}

	public override void Close()
	{
		if (!IsWaitingResult)
		{
			CloseWindow();
		}
	}

	public void CloseWindow()
	{
		base.Close();
		SoulWeaponStatInfo.Close();
		SoulWeaponSkillInfo.Close();
		SoulWeaponStatInfoLeft.Close();
		SoulWeaponStatInfoLeft.Close();
		if (BgNodeTreePlayer != null)
		{
			BgNodeTreePlayer.Close();
		}
		_soulWeaponInfo = null;
		_nodeTreeState = NodeTreeSatate.LiberationWait;
		SingletonAuto<BgmManager2>.instance.Request("EntryMenu.KainAscensionBGM", isEvent: true);
	}

	public void SetNodeTreePlay(SoulWeaponInfo soulWeaponInfo)
	{
		int equipItemSlotIndex = soulWeaponInfo.EquipItemSlotIndex;
		EquipItemInfo equipItemInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.GetEquipItemBySlotIndex(equipItemSlotIndex);
		if (equipItemInfo == null && _equipItemInfo != null)
		{
			equipItemInfo = _equipItemInfo;
		}
		switch (_nodeTreeState)
		{
		case NodeTreeSatate.LiberationWait:
			OnOffTouchRoot(isOn: true);
			OnOffDescRoot(isOn: false);
			AddSoulStone(equipItemInfo.ItemData.Code);
			BgNodeTreePlayer.PlayOnce(NodeTreeWaitPackCode, NodeTreeWaitTreeName);
			_nodeTreeState = NodeTreeSatate.LiberationAction;
			break;
		case NodeTreeSatate.LiberationAction:
			OnOffTouchRoot(isOn: false);
			BgNodeTreePlayer.PlaySequence(NodeTreeActionPackCode, NodeTreeActionTreeName, null, delegate
			{
				SetNodeTreePlay(soulWeaponInfo);
			});
			_nodeTreeState = NodeTreeSatate.Liberation;
			break;
		case NodeTreeSatate.Liberation:
			UIHelper.PlaySoulWeaponNodeTree(BgNodeTreePlayer, equipItemInfo, SoulWeaponNodeTreeType.Liberation, delegate
			{
				SingletonAuto<BgmManager2>.instance.SetAssetBgmVolume(0f);
				PlayFadeOutEffect();
			}, delegate
			{
				OnOffTouchRoot(isOn: true);
				OnOffDescRoot(isOn: true);
				IsWaitingResult = false;
			});
			_nodeTreeState = NodeTreeSatate.Close;
			break;
		default:
			Close();
			break;
		}
	}

	private void PlayFadeOutEffect()
	{
		if (FadeOutGame3DWidget != null)
		{
			FadeOutGame3DWidget.Play(delegate
			{
				SingletonAuto<BgmManager2>.instance.SetAssetBgmVolume(1f);
			});
		}
		else
		{
			SingletonAuto<BgmManager2>.instance.SetAssetBgmVolume(1f);
		}
	}

	private void OnOffDescRoot(bool isOn)
	{
		UIHelper.EnableGameObject(GoDescRoot, isOn);
	}

	private void SetupDescSide()
	{
		ESoulweaponLiberationResultSide eSoulweaponLiberationResultSide = ESoulweaponLiberationResultSide.Left;
		if (_soulWeaponInfo != null)
		{
			string itemCode = _soulWeaponInfo.GetItemCode();
			SoulWeaponNodeTreeData data = GameRoot.Instance.SoulWeaponNodeTreeDataContainer.GetData(itemCode);
			if (data != null)
			{
				eSoulweaponLiberationResultSide = data.LiberationResultSide;
			}
		}
		UIHelper.EnableGameObject(GoDescLeftRoot, eSoulweaponLiberationResultSide == ESoulweaponLiberationResultSide.Left);
		UIHelper.EnableGameObject(GoDescRightRoot, eSoulweaponLiberationResultSide == ESoulweaponLiberationResultSide.Right);
		if (eSoulweaponLiberationResultSide == ESoulweaponLiberationResultSide.Left)
		{
			SoulWeaponStatInfoLeft.Open(_soulWeaponInfo);
			SoulweaponSkillInfoLeft.Open(_soulWeaponInfo);
		}
		else
		{
			SoulWeaponStatInfo.Open(_soulWeaponInfo);
			SoulWeaponSkillInfo.Open(_soulWeaponInfo);
		}
	}

	private void OnOffTouchRoot(bool isOn)
	{
		UIHelper.EnableGameObject(GoTouchRoot, isOn);
	}

	public void OnClickNextNodeTree()
	{
		SetNodeTreePlay(_soulWeaponInfo);
	}

	public void AddSoulStone(string itemCode)
	{
		SoulWeaponNodeTreeData data = GameRoot.Instance.SoulWeaponNodeTreeDataContainer.GetData(itemCode);
		if (data != null)
		{
			float[] valueArray = GameRoot.Instance.ConstantDataContainer.GetValueArray<float>("SoulStoneLiberationPosition");
			float[] valueArray2 = GameRoot.Instance.ConstantDataContainer.GetValueArray<float>("SoulStoneLiberationRotation");
			string value = GameRoot.Instance.ConstantDataContainer.GetValue("SoulStoneLiberationScale");
			int soulStoneCreatureIndex = data.SoulStoneCreatureIndex;
			Vector3 arrayToVec = GetArrayToVec3(valueArray);
			Vector3 arrayToVec2 = GetArrayToVec3(valueArray2);
			float result = 0f;
			float.TryParse(value, out result);
			BgNodeTreePlayer.AddCreatureStock(soulStoneCreatureIndex, 0, arrayToVec, arrayToVec2, result);
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
}
