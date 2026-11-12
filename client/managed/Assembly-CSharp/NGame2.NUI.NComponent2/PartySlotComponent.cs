using NNguiExtension;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class PartySlotComponent : MonoBehaviour
{
	public enum MemberStateType
	{
		None,
		Joined,
		Ready
	}

	public UIWidget MainSlot;

	public UISimpleGrid MainClassTagGrid;

	public UIWidget SubSlot;

	public UISimpleGrid SubClassTagGrid;

	public UIWidget Activated;

	public UIWidget Deactivated;

	public UILabel NameLabel;

	public UISpriteSelector MemberIndexSelector;

	public UISpriteSelector Caterory;

	public UILabel Label_Class;

	private CreatureTagType creatureTagType;

	public long AccountId { get; set; }

	public int SlotIndex { get; set; }

	public MemberStateType MemberState { get; set; }

	public void Reset()
	{
		AccountId = 0L;
		SetName("");
		SetState(MemberStateType.None);
		SetMemberIndex(-1);
		SetClassSprite();
	}

	public void SetClassCategory(CreatureTagType type)
	{
		creatureTagType = type;
		SetClassSprite();
	}

	public void SetName(string memberName)
	{
		UIHelper.SetLabelText(NameLabel, memberName);
	}

	public void SetState(MemberStateType memberState)
	{
		MemberState = memberState;
		if (MemberState == MemberStateType.None)
		{
			UIHelper.SetLabelColor(NameLabel, Color.gray);
		}
		if (MemberState == MemberStateType.Joined)
		{
			UIHelper.SetLabelColor(NameLabel, Color.white);
		}
		if (MemberState == MemberStateType.Ready)
		{
			UIHelper.SetLabelColor(NameLabel, Color.yellow);
		}
	}

	public void SetMemberIndex(int memberIndex)
	{
		if (MemberIndexSelector != null)
		{
			if (memberIndex == -1)
			{
				UIHelper.EnableGameObject(MemberIndexSelector, enable: false);
				return;
			}
			UIHelper.EnableGameObject(MemberIndexSelector, enable: true);
			MemberIndexSelector.SelectByIndex(memberIndex);
		}
	}

	public void SetClassSprite()
	{
		if (Caterory == null)
		{
			return;
		}
		if (creatureTagType == CreatureTagType.None)
		{
			UIHelper.EnableGameObject(Caterory, enable: false);
			return;
		}
		Caterory.Select(creatureTagType.ToString());
		UIHelper.EnableGameObject(Caterory, enable: true);
		if (Label_Class != null)
		{
			string text = LocalizationManager.GetString(creatureTagType);
			UIHelper.SetLabelText(Label_Class, LocalizationManager.GetStringDirect("COMMON_REQ", text));
		}
	}
}
