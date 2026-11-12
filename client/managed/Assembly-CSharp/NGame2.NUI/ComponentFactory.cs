using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2.NHeroPortraitSub;
using NGame2.NUI.NComponent2.NItemIconSub;
using NGame2.NUI.NComponent2.NMonsterPortraitSub;
using NGame2.NUI.NComponent2.NPetPortraitSub;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI;

public class ComponentFactory : SingletonAuto<ComponentFactory>
{
	private Dictionary<Type, Func<Transform, Component>> _createDic = new Dictionary<Type, Func<Transform, Component>>();

	public static void OnSceneChanged()
	{
		if (SingletonAuto<ComponentFactory>.isValidInstance)
		{
			SingletonAuto<ComponentFactory>.instance._createDic.Clear();
			SingletonAuto<ComponentFactory>.ReleaseSingleton();
		}
	}

	public ComponentFactory()
	{
		Register(NGame2.NUI.NComponent2.NItemIconSub.CustomLabelSubComponent.PrefabAuto);
		Register(EmptySubComponent.PrefabAuto);
		Register(NGame2.NUI.NComponent2.NItemIconSub.NewMarkSubComponent.PrefabAuto);
		Register(GetMarkSubComponent.PrefabAuto);
		Register(PartsTypeIconSubComponent.PrefabAuto);
		Register(NGame2.NUI.NComponent2.NItemIconSub.SelectSubComponent.PrefabAuto);
		Register(SellSelectSubComponent.PrefabAuto);
		Register(BreakSelectSubComponent.PrefabAuto);
		Register(StatDiffSubComponent.PrefabAuto);
		Register(UpperLabelSubComponent.PrefabAuto);
		Register(StarBlinkSubComponent.PrefabAuto);
		Register(RewardSubComponent.PrefabAuto);
		Register(NGame2.NUI.NComponent2.NItemIconSub.LockedSubComponent.PrefabAuto);
		Register(NGame2.NUI.NComponent2.NHeroPortraitSub.SelectSubComponent.PrefabAuto);
		Register(BanSubComponent.PrefabAuto);
		Register(NGame2.NUI.NComponent2.NHeroPortraitSub.CustomLabelSubComponent.PrefabAuto);
		Register(RestrictedSubComponent.PrefabAuto);
		Register(SelectSmallSubComponent.PrefabAuto);
		Register(BookmarkSmallSubComponent.PrefabAuto);
		Register(NewMarkComponent.PrefabAuto);
		Register(ClassTagComponent.PrefabAuto);
		Register(PurifyComponent.PrefabAuto);
		Register(NGame2.NUI.NComponent2.NHeroPortraitSub.StarSubComponent.PrefabAuto);
		Register(SmallStarSubComponent.PrefabAuto);
		Register(TrialSelectSubComponent.PrefabAuto);
		Register(FragmentStateComponent.PrefabAuto);
		Register(HeroNameComponent.PrefabAuto);
		Register(EquipableStateComponent.PrefabAuto);
		Register(NGame2.NUI.NComponent2.NHeroPortraitSub.OwnedSubComponent.PrefabAuto);
		Register(NGame2.NUI.NComponent2.NHeroPortraitSub.LockedSubComponent.PrefabAuto);
		Register(DeckComponent.PrefabAuto);
		Register(NotOwnedSmallSubComponent.PrefabAuto);
		Register(CostumeNewTagSmallSubComponent.PrefabAuto);
		Register(ClassTagSubComponent.PrefabAuto);
		Register(AchievementClearedSubComponent.PrefabAuto);
		Register(DisableSubComponent.PrefabAuto);
		Register(DeckSlotSmallSubComponent.PrefabAuto);
		Register(BlindSubComponent.PrefabAuto);
		Register(DummySubComponent.PrefabAuto);
		Register(TechnoMagicClassSubComponent.PrefabAuto);
		Register(TechnoMagicIdentifiedSubComponent.PrefabAuto);
		Register(PetSoulInsertSubComponent.PrefabAuto);
		Register(ReviveSubComponent.PrefabAuto);
		Register(PetNameComponent.PrefabAuto);
		Register(PetGradeSubComponent.PrefabAuto);
		Register(PetFrameSubComponent.PrefabAuto);
		Register(MainPetMarkSubComponent.PrefabAuto);
		Register(NGame2.NUI.NComponent2.NPetPortraitSub.NewMarkSubComponent.PrefabAuto);
		Register(SleepMarkSubComponent.PrefabAuto);
		Register(NGame2.NUI.NComponent2.NPetPortraitSub.StarSubComponent.PrefabAuto);
		Register(NGame2.NUI.NComponent2.NPetPortraitSub.SelectSubComponent.PrefabAuto);
		Register(NGame2.NUI.NComponent2.NPetPortraitSub.OwnedSubComponent.PrefabAuto);
		Register(ExclamationMarkSubComponent.PrefabAuto);
		Register(DisableStatusComponent.PrefabAuto);
	}

	private void Register<T>(PrefabAuto<T> prefabAuto) where T : Component
	{
		if (!_createDic.ContainsKey(typeof(T)))
		{
			_createDic.Add(typeof(T), (Transform parentTm) => prefabAuto.Factory.Create(parentTm));
		}
	}

	public T Create<T>(Transform parentTm) where T : Component
	{
		Func<Transform, Component> vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_createDic, typeof(T), null);
		if (vALUE == null)
		{
			return null;
		}
		T val = Vespa_System_Action_Extension_Method.fire(vALUE, parentTm) as T;
		if (val == null)
		{
			return null;
		}
		return val;
	}
}
