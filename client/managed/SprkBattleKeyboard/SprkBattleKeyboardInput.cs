using System.Collections.Generic;
using NGame2.NBattleField;
using NGame2.NUI.NComponent2.NBattle;
using NGame2.NUI.NWindow.NBattle;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

public sealed class SprkBattleKeyboardInput : MonoBehaviour
{
	private HeroParty party;
	private readonly List<PartySlotComponent2> slots = new List<PartySlotComponent2>(4);

	public static void Attach(HeroParty owner)
	{
		SprkBattleKeyboardInput input = owner.GetComponent<SprkBattleKeyboardInput>();
		if (input == null)
		{
			input = owner.gameObject.AddComponent<SprkBattleKeyboardInput>();
		}
		input.party = owner;
	}

	// HeroParty's normal UI update is throttled to 0.1s; key-down edges need every frame.
	private void LateUpdate()
	{
		if (!SprkBattleKeyboard.ControlsEnabled || party == null || !party.IsShow
			|| !party.IsSelectedMain || GlobalTimer.pause || !Input.anyKeyDown
			|| !Visible(party.Panel_Party))
		{
			return;
		}
		BattleFacade facade = SingletonAuto<BattleFacade>.instance;
		if (!facade.IsValidBattle())
		{
			return;
		}
		BattleFacade.BattleCacheData battle = facade.GetBattleCache();
		if (!battle.IsBattleStarted || battle.IsBattleEnded || !battle.IsBattleEnabled)
		{
			return;
		}
		slots.Clear();
		for (int i = 0; i < party.PartySlotDic.Count; i++)
		{
			PartySlotComponent2 slot = party.PartySlotDic.GetValueByIndex(i);
			if (slot != null && slot.gameObject.activeInHierarchy && Visible(slot.RootWidget))
			{
				slots.Add(slot);
			}
		}
		// Dictionary/wave order is not necessarily the left-to-right rendered order.
		slots.Sort(ComparePosition);
		for (int hero = 0; hero < slots.Count && hero < 4; hero++)
		{
			PartySlotComponent2 slot = slots[hero];
			if (slot.CreatureCache.Uid == 0 || slot.CreatureCache.IsDeadOrResting())
			{
				continue;
			}
			for (int action = 0; action < 4; action++)
			{
				if (!SprkBattleKeyboard.IsPressed(hero, action))
				{
					continue;
				}
				BaseSkillComponent skill = FindAction(slot, action);
				if (skill != null && skill.gameObject.activeInHierarchy && Visible(skill.WidgetThis))
				{
					// Preserve native locking, disabled-slot, next-skill and reservation behavior.
					skill.OnClickSkill();
				}
			}
		}
	}

	private static BaseSkillComponent FindAction(PartySlotComponent2 slot, int action)
	{
		if (action == 3)
		{
			return slot.FindSkillComponent(skill => skill is SoulWeaponSkillComponent);
		}
		int index = 0;
		return slot.FindSkillComponent(skill => skill is NormalSkillComponent && index++ == action);
	}

	private static int ComparePosition(PartySlotComponent2 left, PartySlotComponent2 right)
	{
		return ScreenX(left).CompareTo(ScreenX(right));
	}

	private static float ScreenX(PartySlotComponent2 slot)
	{
		Camera camera = NGUITools.FindCameraForLayer(slot.gameObject.layer);
		return camera != null ? camera.WorldToScreenPoint(slot.transform.position).x : slot.transform.position.x;
	}

	private static bool Visible(UIPanel panel)
	{
		return panel != null && panel.enabled && panel.gameObject.activeInHierarchy
			&& panel.CalculateFinalAlpha(Time.frameCount) > 0.01f;
	}

	private static bool Visible(UIWidget widget)
	{
		return widget != null && widget.enabled && widget.gameObject.activeInHierarchy
			&& widget.isVisible && widget.CalculateFinalAlpha(Time.frameCount) > 0.01f;
	}
}
