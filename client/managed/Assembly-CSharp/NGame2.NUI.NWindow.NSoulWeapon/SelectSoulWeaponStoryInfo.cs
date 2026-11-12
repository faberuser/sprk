using System;
using NGame2.NAccount;
using NShared;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NSoulWeapon;

[Serializable]
public class SelectSoulWeaponStoryInfo
{
	public Transform StoryInfoTransform;

	public UIScrollView StoryScrollView;

	public UILabel StroyLabel;

	public UILabel SoulWeaponCreatedDateLabel;

	public UIToggle StoryToggle;

	private SoulWeaponInfo _soulWeaponInfo;

	public void Open(SoulWeaponInfo soulWeaponInfo)
	{
		Init(soulWeaponInfo);
	}

	private void Init(SoulWeaponInfo soulWeaponInfo)
	{
		_soulWeaponInfo = soulWeaponInfo;
		InitStoryDesc();
		InitSoulWeaponCreatedDate();
	}

	private void InitStoryDesc()
	{
		UIHelper.SetLabelText(StroyLabel, "-");
		if (_soulWeaponInfo != null)
		{
			EquipItemData data = GameRoot.Instance.EquipItemDataContainer.GetData(_soulWeaponInfo.GetItemCode());
			if (data != null && !__Extension_Method_ICollection__.IsNullOrEmpty(data.CreatureIndex) && data.CreatureIndex.Length >= 0)
			{
				int num = data.CreatureIndex[0];
				UIHelper.SetLabelText(StroyLabel, LocalizationManager.GetStringDirect("SOULWEAPON_STORY_" + num));
				StoryScrollView.ResetPosition();
			}
		}
	}

	private void InitSoulWeaponCreatedDate()
	{
		UIHelper.SetLabelText(SoulWeaponCreatedDateLabel, "-");
		if (_soulWeaponInfo != null)
		{
			if (Vespa_String_Extension_Method2.TryParseNetworkTime2(_soulWeaponInfo.CreatedTime, out var o_dateTime))
			{
				o_dateTime = GlobalAutoBehaviour<AccountManager>.instance.ServerUTCToLocal(o_dateTime);
				string text = $"{o_dateTime:yyyy-MM-dd}";
				UIHelper.SetLabelText(SoulWeaponCreatedDateLabel, LocalizationManager.GetStringDirect("SOULWEAPON_INFO_DATE", text));
			}
			else
			{
				UIHelper.SetLabelText(SoulWeaponCreatedDateLabel, LocalizationManager.GetStringDirect("SOULWEAPON_INFO_DATE", _soulWeaponInfo.CreatedTime));
			}
		}
	}

	public void InitStoryToggleState(bool state)
	{
		if (StoryToggle != null)
		{
			StoryToggle.Set(state);
		}
	}

	public void EnableStory(bool enable)
	{
		if (StoryInfoTransform != null)
		{
			UIHelper.EnableGameObject(StoryInfoTransform, enable);
		}
	}
}
