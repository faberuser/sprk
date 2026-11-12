using System.Collections;
using NGame2.NUI.NComponent2.NSoulWeapon;
using NShared;
using UnityEngine;

namespace NGame2.NUI.NWindow.NSoulWeapon;

public class SoulWeaponLiberationWindow : CommonSoulWeaponWindow, SoulWeaponList.IListener
{
	public interface IListener
	{
		void LiberationSoulWeapon(EquipItemInfo equipItemInfo);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/SoulWeapon/SoulWeaponLiberationWindow";

	public NodeTreeUIPlayer BgNodeTreeUIPlayer;

	public SoulWeaponStatInfo SoulWeaponStatInfo;

	public SoulWeaponMaterialInfo SoulWeaponMaterialInfo;

	public SoulWeaponSkillInfo SoulWeaponSkillInfo;

	public UIButton LiberationButton;

	public Transform StatTransform;

	public Transform LiberationTransform;

	public Transform EmptyTransform;

	public Transform SpecialWeaponListTranform;

	private IListener _listener;

	private SkillUseConditionComponent _skillUseConditionComponent;

	public string BgNodeTreePackCode;

	public string BgNodeTreeName;

	private bool _initBgNodeTree;

	public void Open(EquipItemInfo equipItemInfo, IListener listener, bool isSelectFirst)
	{
		VespaCoroutine.Start(OpenAsyc(listener, equipItemInfo, isSelectFirst));
	}

	private IEnumerator OpenAsyc(IListener iListener, EquipItemInfo equipItemInfo, bool isSelectFirst)
	{
		Open(equipItemInfo);
		Setup(iListener, isSelectFirst);
		yield return null;
	}

	private void Setup(IListener listener, bool isSelectFirst)
	{
		_listener = listener;
		InitSoulWeaponList(this, base.EquipItemInfo, SoulWeaponList.ListType.Liberation, isSelectFirst);
		base.EquipItemInfo = UIHelper.GetFakeSoulWeaponInfo(base.EquipItemInfo);
		EnableInfo();
		SetSelectSoulWeaponInfo();
		EnableEmptyState();
		EnableSpecialWeaponListState();
	}

	public void Refresh(EquipItemInfo equipItemInfo)
	{
		ReleaseSelectSoulWeaponIcon();
		base.EquipItemInfo = equipItemInfo;
		if (equipItemInfo == null)
		{
			base.SoulWeaponInfo = null;
		}
		else
		{
			base.SoulWeaponInfo = base.EquipItemInfo.SoulWeaponInfo;
		}
		InitSoulWeaponList(this, base.EquipItemInfo, SoulWeaponList.ListType.Liberation);
		base.EquipItemInfo = UIHelper.GetFakeSoulWeaponInfo(base.EquipItemInfo);
		EnableInfo();
		SetSelectSoulWeaponInfo();
		EnableEmptyState();
		EnableSpecialWeaponListState();
	}

	private void SetSelectSoulWeaponInfo()
	{
		SoulWeaponStatInfo.Open(base.SoulWeaponInfo);
		SoulWeaponMaterialInfo.Open(null, (base.EquipItemInfo != null) ? base.EquipItemInfo.SlotIndex : 0, base.SoulWeaponInfo, enableItemCount: true);
		SoulWeaponSkillInfo.Open(base.SoulWeaponInfo);
		SetBgNodeTreePlayer();
		SetLiberationButtonState();
		SprkSoulLiberationCost.Show(this);
	}

	private void EnableInfo()
	{
		bool enable = base.SoulWeaponInfo != null;
		UIHelper.EnableGameObject(StatTransform, enable);
		UIHelper.EnableGameObject(LiberationTransform, enable);
	}

	private void EnableEmptyState()
	{
		UIHelper.EnableGameObject(EmptyTransform, !base.ExistList);
	}

	private void EnableSpecialWeaponListState()
	{
		UIHelper.EnableGameObject(SpecialWeaponListTranform, base.ExistList);
	}

	private void SetBgNodeTreePlayer()
	{
		if (!_initBgNodeTree)
		{
			if (base.EquipItemInfo != null)
			{
				ChangeSoulStone(base.EquipItemInfo.ItemData.Code, isAdd: true);
			}
			BgNodeTreeUIPlayer.PlayOnce(BgNodeTreePackCode, BgNodeTreeName);
			_initBgNodeTree = true;
		}
		else if (base.EquipItemInfo != null)
		{
			ChangeSoulStone(base.EquipItemInfo.ItemData.Code, isAdd: false);
		}
		else
		{
			BgNodeTreeUIPlayer.CreautureRelease();
		}
	}

	public void ChangeSoulStone(string itemCode, bool isAdd)
	{
		SoulWeaponNodeTreeData data = GameRoot.Instance.SoulWeaponNodeTreeDataContainer.GetData(itemCode);
		if (data != null)
		{
			float[] valueArray = GameRoot.Instance.ConstantDataContainer.GetValueArray<float>("SoulStoneUIPosition");
			float[] valueArray2 = GameRoot.Instance.ConstantDataContainer.GetValueArray<float>("SoulStoneUIRotation");
			string value = GameRoot.Instance.ConstantDataContainer.GetValue("SoulStoneUIScale");
			int soulStoneCreatureIndex = data.SoulStoneCreatureIndex;
			Vector3 arrayToVec = GetArrayToVec3(valueArray);
			Vector3 arrayToVec2 = GetArrayToVec3(valueArray2);
			float result = 0f;
			float.TryParse(value, out result);
			if (isAdd)
			{
				BgNodeTreeUIPlayer.AddCreatureStock(soulStoneCreatureIndex, 0, arrayToVec, arrayToVec2, result);
			}
			else
			{
				BgNodeTreeUIPlayer.SpawnCreature(soulStoneCreatureIndex, 0, arrayToVec, arrayToVec2, result);
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

	public override void Close()
	{
		base.Close();
		_listener = null;
		base.EquipItemInfo = null;
		SoulWeaponMaterialInfo.Close();
		if (BgNodeTreeUIPlayer != null)
		{
			BgNodeTreeUIPlayer.Close();
		}
		_initBgNodeTree = false;
	}

	private void SetLiberationButtonState()
	{
		EnableLiberationButtonState(enable: false);
		if (SoulWeaponMaterialInfo != null)
		{
			EnableLiberationButtonState(SoulWeaponMaterialInfo.IsMoreMyItemCount);
		}
	}

	public void EnableLiberationButtonState(bool enable)
	{
		UIHelper.EnableButton(LiberationButton, enable);
	}

	public void OnClickLiberationAltarButton()
	{
		if (_listener != null)
		{
			EnableLiberationButtonState(enable: false);
			_listener.LiberationSoulWeapon(base.EquipItemInfo);
		}
	}

	void SoulWeaponList.IListener.SelectSoulWeapon(EquipItemInfo equipItemInfo, SoulWeaponList.ListType listType)
	{
		if (equipItemInfo != null)
		{
			base.EquipItemInfo = UIHelper.GetFakeSoulWeaponInfo(equipItemInfo);
			SetSelectSoulWeaponInfo();
			EnableInfo();
			if (listType == AllSoulWeaponList.AllListType)
			{
				SoulWeaponList.SelectSoulWeaponItemIconComponent(equipItemInfo);
				EnableAllSoulWeaponList(enable: false);
			}
		}
	}
}
