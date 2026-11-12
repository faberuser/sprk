using System;
using NShared;
using UnityEngine;

namespace NGame2.NUI.NWindow.NSoulWeapon;

[Serializable]
public class AllSoulWeaponList
{
	public Transform AllSoulWeaponListTransform;

	public SoulWeaponList SoulWeaponList;

	public SoulWeaponList.ListType AllListType { get; private set; }

	public void Open(SoulWeaponList.IListener soulWeaponListIListener, SoulWeaponList.ListType listType)
	{
		AllListType = listType;
		SoulWeaponList.OpenAllList(soulWeaponListIListener, listType);
	}

	public void Close()
	{
		SoulWeaponList.Close();
	}

	public void Refresh(EquipItemInfo equipItemInfo)
	{
		SoulWeaponList.Refresh(equipItemInfo);
	}

	public void EnableAllSoulWeaponList(bool enable)
	{
		bool flag = false;
		if (AllSoulWeaponListTransform != null)
		{
			flag = AllSoulWeaponListTransform.gameObject.activeSelf;
			UIHelper.EnableGameObject(AllSoulWeaponListTransform, enable);
		}
		if (enable && !flag)
		{
			SoulWeaponList.SetList(isSelectFirst: false);
		}
	}
}
