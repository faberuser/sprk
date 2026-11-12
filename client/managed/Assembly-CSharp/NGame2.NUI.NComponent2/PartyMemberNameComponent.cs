using System;
using NNguiExtension;
using NShared.NParty;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class PartyMemberNameComponent : LinkFactoryComponent
{
	public interface Listener
	{
		void OnClickName(PartyMemberNameComponent component);
	}

	public enum StateType
	{
		None,
		Master,
		Ready,
		Battle
	}

	[Serializable]
	public class StateObject
	{
		public StateType State;

		public UIWidget GameObject;

		public Color NameColor = Color.white;
	}

	public const string PrefabPath = "Prefab2/Component/Party/PartyMemberNameComponent";

	public static PrefabAuto<PartyMemberNameComponent> PrefabAuto = new PrefabAuto<PartyMemberNameComponent>("Prefab2/Component/Party/PartyMemberNameComponent");

	public UILabel NickLabel;

	public UISpriteSelector MemberIndexSelector;

	public StateObject[] StateObjects;

	private Listener _listener;

	public MemberInfo PartyMemberInfo { get; private set; }

	public void Open(Listener listener, MemberInfo partyMemberInfo)
	{
		_listener = listener;
		PartyMemberInfo = partyMemberInfo;
		UIHelper.SetLabelText(NickLabel, partyMemberInfo.Nick);
		int memberIndex = partyMemberInfo.MemberIndex;
		MemberIndexSelector.SelectByIndex(memberIndex);
		SetStatus(PartyMemberInfo.Status);
	}

	public void SetState(StateType state)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(StateObjects); i++)
		{
			UIHelper.EnableGameObject(StateObjects[i].GameObject, state == StateObjects[i].State);
			if (state == StateObjects[i].State)
			{
				UIHelper.SetWidgetColor(NickLabel, StateObjects[i].NameColor);
			}
		}
	}

	public void SetStatus(MemberStatus status)
	{
		switch (status)
		{
		case MemberStatus.Battle:
			SetState(StateType.Battle);
			break;
		case MemberStatus.EndCampaign:
			SetState(StateType.Battle);
			break;
		case MemberStatus.PartyRoom:
			UIHelper.SetLabelText(NickLabel, PartyMemberInfo.Nick);
			if (PartyMemberInfo.IsMaster)
			{
				SetState(StateType.Master);
			}
			else if (PartyMemberInfo.IsBattleReady)
			{
				SetState(StateType.Ready);
			}
			else
			{
				SetState(StateType.None);
			}
			break;
		}
	}

	public void OnClickName()
	{
		if (_listener != null)
		{
			_listener.OnClickName(this);
		}
	}
}
