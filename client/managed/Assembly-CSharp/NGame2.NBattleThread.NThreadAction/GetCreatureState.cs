using NGame2.NBattleField;
using NShared;
using NShared.NCampaign;
using NVespa;
using UnityEngine;

namespace NGame2.NBattleThread.NThreadAction;

public class GetCreatureState : IThreadAction
{
	public static bool IsEnabled = true;

	public static bool Foldout = false;

	public int CreatureUid;

	public string states;

	public bool IsWaiting { get; set; }

	public GetCreatureState(int creatureUid)
	{
		CreatureUid = creatureUid;
	}

	public void ChainUpdate()
	{
		if (IsEnabled && !IsWaiting)
		{
			SingletonAuto<BattleFacade>.instance.AddThreadSync(this);
		}
	}

	void IThreadAction.Action(BattleInstance battleInstance, CampaignUpdater campaignUpdater)
	{
		BattleCreature creature = battleInstance.CreatureContainer.GetCreature(CreatureUid);
		if (creature != null)
		{
			states = creature.StateController.Dump();
		}
	}

	public void DrawToggleGUI()
	{
		IsEnabled = GUILayout.Toggle(IsEnabled, "Is Enabled");
		GUILayout.Label(states);
	}
}
