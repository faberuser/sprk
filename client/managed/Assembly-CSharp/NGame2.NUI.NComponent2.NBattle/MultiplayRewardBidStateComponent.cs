using System;
using NNguiExtension;
using NShared.NParty;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NBattle;

public class MultiplayRewardBidStateComponent : LinkFactoryComponent
{
	public enum BidState
	{
		Progressing,
		Selected,
		Abandoned
	}

	[Serializable]
	public class BidStateInfo
	{
		public BidState State;

		public GameObject GameObject;
	}

	public const string PrefabPath = "Prefab2/Component/Battle/MultiplayRewardBidStateComponent";

	public static PrefabAuto<MultiplayRewardBidStateComponent> PrefabAuto = new PrefabAuto<MultiplayRewardBidStateComponent>("Prefab2/Component/Battle/MultiplayRewardBidStateComponent");

	public UISpriteSelector MemberIndexSelector;

	public UILabel NameLabel;

	public BidStateInfo[] BidStateObjects;

	public GameObject DiceRoot;

	public UILabel DiceNumberLabel;

	public MemberInfo MemberInfo { get; private set; }

	public void Open(MemberInfo memberInfo)
	{
		MemberInfo = memberInfo;
		UIHelper.SetLabelText(NameLabel, MemberInfo.Nick);
		int memberIndex = memberInfo.MemberIndex;
		MemberIndexSelector.SelectByIndex(memberIndex);
		SetSelectedRewardItemNo(memberInfo.SelectedRewardItemNo);
		UIHelper.EnableGameObject(DiceRoot, enable: false);
	}

	public void SetSelectedRewardItemNo(int rewardItemNo)
	{
		BidState bidState = BidState.Progressing;
		bidState = rewardItemNo switch
		{
			-1 => BidState.Progressing, 
			-2 => BidState.Abandoned, 
			_ => BidState.Selected, 
		};
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(BidStateObjects); i++)
		{
			UIHelper.EnableGameObject(BidStateObjects[i].GameObject, BidStateObjects[i].State == bidState);
		}
	}

	public void ClearState()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(BidStateObjects); i++)
		{
			UIHelper.EnableGameObject(BidStateObjects[i].GameObject, enable: false);
		}
		UIHelper.EnableGameObject(DiceRoot, enable: false);
	}

	public void ShowDiceNumber(int diceNumber)
	{
		UIHelper.EnableGameObject(DiceRoot, enable: true);
		UIHelper.SetLabelText(DiceNumberLabel, diceNumber);
	}
}
