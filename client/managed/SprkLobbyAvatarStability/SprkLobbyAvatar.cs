using Avatar = NGame2.NUI.NWorldMap.NMapLayout.Avatar;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using NGame2.N3D;
using NGame2.NAccount;
using NGame2.NRender;
using NGame2.NUI;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWorldMap;
using NGame2.NUI.NWorldMap.NMapLayout;
using NGameAsset;
using NShared;
using NVespa.NSingleton;
using UnityEngine;

public static class SprkLobbyAvatar
{
	private static readonly ConditionalWeakTable<PlayerAvatarHeroInfo, SprkLobbyRenderSnapshot> renderSnapshots = new ConditionalWeakTable<PlayerAvatarHeroInfo, SprkLobbyRenderSnapshot>();

	private static readonly ConditionalWeakTable<CachedHeroInfo, SprkLobbyRenderSnapshot> resolvedSnapshots = new ConditionalWeakTable<CachedHeroInfo, SprkLobbyRenderSnapshot>();

	private static readonly MethodInfo rebuild = typeof(MapLayout).GetMethod("ChangePlayerAvatarRenderFacadeOnly", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[2]
	{
		typeof(Avatar),
		typeof(int)
	}, null);

	public static object Multi(string name, params object[] args)
	{
		Type type = Type.GetType("SprkAccessories, SprkAccessories");
		if (type == null)
		{
			return null;
		}
		MethodInfo method = type.GetMethod(name);
		if (method == null)
		{
			return null;
		}
		ParameterInfo[] parameters = method.GetParameters();
		for (int i = 0; i < args.Length; i++)
		{
			if (args[i] != null && !parameters[i].ParameterType.IsInstanceOfType(args[i]))
			{
				return null;
			}
		}
		return method.Invoke(null, args);
	}

	public static object GetResolvedSnapshot(CachedHeroInfo hero)
	{
		if (hero == null || !resolvedSnapshots.TryGetValue(hero, out var value))
		{
			return null;
		}
		return value;
	}

	public static void ReadRenderSnapshot(PlayerAvatarHeroInfo avatar, IDictionary json)
	{
		if (avatar == null || json == null)
		{
			return;
		}
		renderSnapshots.Remove(avatar);
		if (!(json["SprkLobbyRenderInfo"] is IDictionary dictionary))
		{
			return;
		}
		SprkLobbyRenderSnapshot sprkLobbyRenderSnapshot = new SprkLobbyRenderSnapshot();
		sprkLobbyRenderSnapshot.Star = Convert.ToSByte(dictionary["Star"]);
		if (dictionary["Equipment"] is IEnumerable enumerable)
		{
			foreach (object item in enumerable)
			{
				if (item is IDictionary d)
				{
					sprkLobbyRenderSnapshot.Equipment.Add(JsonMarshaler.Parse<EquipItemInfo>(d));
				}
			}
		}
		renderSnapshots.Add(avatar, sprkLobbyRenderSnapshot);
	}

	public static IList<EquipItemInfo> GetRenderEquipment(PlayerAvatarHeroInfo avatar)
	{
		if (avatar == null || !renderSnapshots.TryGetValue(avatar, out var value))
		{
			return null;
		}
		return value.Equipment;
	}

	public static void ApplyRenderSnapshot(CachedHeroInfo hero, PlayerAvatarHeroInfo avatar)
	{
		if (hero != null && avatar != null && hero.HeroIndex == avatar.HeroIndex && renderSnapshots.TryGetValue(avatar, out var value))
		{
			hero.Star = value.Star;
			resolvedSnapshots.Remove(hero);
			resolvedSnapshots.Add(hero, value);
		}
	}

	public static bool IsEqualCostumeAndRender(CachedHeroInfo hero, PlayerAvatarHeroInfo avatar)
	{
		if (!hero.IsEqualCostumeInfo(avatar))
		{
			return false;
		}
		if (avatar == null || !renderSnapshots.TryGetValue(avatar, out var value))
		{
			return true;
		}
		if (hero.Star != value.Star)
		{
			return false;
		}
		List<EquipItemInfo> equipItemInfoList = hero.GetEquipItemInfoList();
		if (equipItemInfoList.Count != value.Equipment.Count)
		{
			return false;
		}
		for (int i = 0; i < equipItemInfoList.Count; i++)
		{
			if (JsonMarshaler.EncodeJsonString(equipItemInfoList[i]) != JsonMarshaler.EncodeJsonString(value.Equipment[i]))
			{
				return false;
			}
		}
		return true;
	}

	public static RenderFacadeBuilder Build(Transform parent, TeamType team, HeroInfo hero)
	{
		PlayerAvatarHeroInfo playerAvatarHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.UserManager.PlayerAvatarHeroInfo;
		IList<EquipItemInfo> list = ((playerAvatarHeroInfo != null && hero.HeroIndex == playerAvatarHeroInfo.HeroIndex) ? GetRenderEquipment(playerAvatarHeroInfo) : null);
		if (list != null)
		{
			return new RenderFacadeBuilder(parent, team, hero, list);
		}
		return new RenderFacadeBuilder(parent, team, hero);
	}

	public static int ProfileAvatarIndex(CachedHeroInfo hero, int fallback)
	{
		return fallback;
	}

	public static string GetProfileIconPath(int selected)
	{
		return UIAvatarHelper.GetIconPath(ProfileAvatarIndex(CurrentHero(), selected));
	}

	public static string GetProfilePortraitPath(int selected)
	{
		return UIAvatarHelper.GetPortraitPath(ProfileAvatarIndex(CurrentHero(), selected));
	}

	private static void WatchProfilePortrait(UIBundleLoader loader, bool full)
	{
		if (!(loader == null))
		{
			SprkProfilePortraitState sprkProfilePortraitState = loader.GetComponent<SprkProfilePortraitState>();
			if (sprkProfilePortraitState == null)
			{
				sprkProfilePortraitState = loader.gameObject.AddComponent<SprkProfilePortraitState>();
			}
			sprkProfilePortraitState.Target = loader;
			sprkProfilePortraitState.FullPortrait = full;
			sprkProfilePortraitState.Refresh();
		}
	}

	public static void WatchProfileIcon(LobbyTopmostMenu view)
	{
		WatchProfilePortrait(view.ProfileUi.BundleTexMyHero, full: false);
	}

	public static void WatchMyInfoPortrait(MyInfoWindow view)
	{
		WatchProfilePortrait(view.MyInfo.BundleTex_Avatar, full: true);
	}

	public static CachedHeroInfo CurrentHero()
	{
		if (CurrentOwner() == null)
		{
			return null;
		}
		AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
		if (instance.HeroManager == null)
		{
			return null;
		}
		CachedHeroInfo cachedHeroInfo = instance.HeroManager.Get(new UIAvatarHelper.AvatarInfo(instance.UserManager.AvatarHeroIndex).HeroIndex);
		if (cachedHeroInfo == null)
		{
			return null;
		}
		CachedHeroInfo cachedHeroInfo2 = cachedHeroInfo.CreatePlayerAvatarHeroInfo(instance.UserManager.PlayerAvatarHeroInfo);
		ApplyRenderSnapshot(cachedHeroInfo2, instance.UserManager.PlayerAvatarHeroInfo);
		return cachedHeroInfo2;
	}

	public static bool IsAppearanceChange(NGame2.NAccount.UserHeroManager.ChangeHeroInfo change, int hero)
	{
		if (change == null || change.HeroIndex != hero)
		{
			return false;
		}
		if (change.ChangeType != NGame2.NAccount.UserHeroManager.HeroChangeType.Costume && change.ChangeType != NGame2.NAccount.UserHeroManager.HeroChangeType.HairCostume && change.ChangeType != NGame2.NAccount.UserHeroManager.HeroChangeType.WeaponCostume && change.ChangeType != NGame2.NAccount.UserHeroManager.HeroChangeType.AccessoryCostume)
		{
			return change.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.HideUniqueWeapon;
		}
		return true;
	}

	public static void Notify(MapLayout layout, NGame2.NAccount.UserHeroManager.ChangeHeroInfo change, bool forced)
	{
		if (!forced && !(layout == null) && layout.Mediator != null && CurrentOwner() != null)
		{
			CachedHeroInfo cachedHeroInfo = CurrentHero();
			if (cachedHeroInfo != null && IsAppearanceChange(change, cachedHeroInfo.HeroIndex))
			{
				Refresh(layout, layout.Mediator.Avatar, 0);
			}
		}
	}

	public static void Reload(MapLayout layout, Avatar target)
	{
		if (!(layout == null) && target != null && CurrentOwner() != null && layout.gameObject.activeInHierarchy)
		{
			rebuild.Invoke(layout, new object[2]
			{
				target,
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.AvatarHeroIndex
			});
		}
	}

	public static object CurrentOwner()
	{
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return null;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.UserManager;
	}

	public static void Bind(MapLayout layout)
	{
		SprkLobbyAccessoryState sprkLobbyAccessoryState = layout.GetComponent<SprkLobbyAccessoryState>();
		if (sprkLobbyAccessoryState == null)
		{
			sprkLobbyAccessoryState = layout.gameObject.AddComponent<SprkLobbyAccessoryState>();
		}
		sprkLobbyAccessoryState.Lease.Bind(CurrentOwner());
		sprkLobbyAccessoryState.Target = null;
		sprkLobbyAccessoryState.ObservedLoad = null;
		sprkLobbyAccessoryState.ObservedCreature = null;
	}

	public static void Track(MapLayout layout, bool forced)
	{
		if (layout == null)
		{
			return;
		}
		SprkLobbyAccessoryState component = layout.GetComponent<SprkLobbyAccessoryState>();
		if (forced)
		{
			if (component != null)
			{
				component.Lease.Cancel();
				component.Target = null;
			}
			return;
		}
		if (component != null)
		{
			component.Appearance.Observe(CurrentHero());
		}
		if (layout.Mediator != null)
		{
			Refresh(layout, layout.Mediator.Avatar, 0);
		}
	}

	public static CachedHeroInfo Create(CachedHeroInfo hero, PlayerAvatarHeroInfo avatar)
	{
		if (hero == null)
		{
			return null;
		}
		AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
		CachedHeroInfo cachedHeroInfo = hero.CreatePlayerAvatarHeroInfo(avatar);
		ApplyRenderSnapshot(cachedHeroInfo, avatar);
		foreach (AccessoryPartType item in AccessoryPartTypeHelper.PartTypes())
		{
			int num = cachedHeroInfo.GetAccessoryIndex(item);
			if (num != 0 && instance.AccessoryCostumeManager.Get(new PlayerAccessoryCostumeInfoKey(cachedHeroInfo.HeroIndex, num)) == null)
			{
				num = 0;
			}
			PositionInfo accessoryPositionInfo = cachedHeroInfo.GetAccessoryPositionInfo(item);
			cachedHeroInfo.SetAccessoryCostume(item, num, (accessoryPositionInfo == null) ? PositionInfo.Create() : accessoryPositionInfo.Clone());
		}
		return cachedHeroInfo;
	}

	public static void Refresh(MapLayout layout, Avatar avatar, int avatarIndex)
	{
		if (!(layout == null) && avatar != null && layout.gameObject.activeInHierarchy)
		{
			SprkLobbyAccessoryState component = layout.GetComponent<SprkLobbyAccessoryState>();
			if (!(component == null) && component.Lease.Queue(CurrentOwner()))
			{
				component.Target = avatar;
				component.Deadline = Time.realtimeSinceStartup + 10f;
				component.NextTry = 0f;
			}
		}
	}

	public static bool Apply(Avatar avatar)
	{
		if (avatar == null || CurrentOwner() == null)
		{
			return false;
		}
		RenderFacade renderFacade = avatar.RenderFacade;
		if (renderFacade == null || renderFacade.AssetCreatureAsync == null || !renderFacade.AssetCreatureAsync.IsDone || !renderFacade.AssetCreatureAsync.IsValid)
		{
			return false;
		}
		AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
		int heroIndex = new UIAvatarHelper.AvatarInfo(instance.UserManager.AvatarHeroIndex).HeroIndex;
		CachedHeroInfo cachedHeroInfo = CurrentHero();
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroIndex);
		if (cachedHeroInfo == null || data == null || renderFacade.AssetCreatureAsync.Stock.Code != (string.IsNullOrEmpty(data.ModelCode) ? data.CodeName : data.ModelCode))
		{
			return false;
		}
		ICreatureLoad assetCreatureAsync = renderFacade.AssetCreatureAsync;
		List<CustomizeStock> list = new List<CustomizeStock>();
		foreach (AccessoryPartType item in AccessoryPartTypeHelper.PartTypes())
		{
			int accessoryIndex = cachedHeroInfo.GetAccessoryIndex(item);
			CustomizeStock stock = CustomizeStock.CreateEmptyPart(item);
			if (accessoryIndex != 0)
			{
				if (instance.AccessoryCostumeManager.Get(new PlayerAccessoryCostumeInfoKey(heroIndex, accessoryIndex)) == null)
				{
					return false;
				}
				AccessoryCostumeData data2 = GameRoot.Instance.AccessoryCostumeDataContainer.GetData(accessoryIndex);
				PositionInfo accessoryPositionInfo = cachedHeroInfo.GetAccessoryPositionInfo(item);
				if (data2 == null || !RenderFacadeBuilder.TryAssignCustomizeStock(ref stock, heroIndex, data2, (accessoryPositionInfo == null) ? PositionInfo.Create() : accessoryPositionInfo.Clone()))
				{
					return false;
				}
			}
			list.Add(stock);
		}
		if (Multi("Stocks", cachedHeroInfo) is CustomizeStock[] array)
		{
			CustomizeStock[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				CustomizeStock customizeStock = array2[i];
				if (instance.AccessoryCostumeManager.Get(new PlayerAccessoryCostumeInfoKey(heroIndex, customizeStock.AccessoryIndex)) == null)
				{
					return false;
				}
			}
			list = new List<CustomizeStock>(array);
		}
		return Reconcile(assetCreatureAsync, list.ToArray());
	}

	public static bool Reconcile(ICreatureLoad load, CustomizeStock[] desired)
	{
		object obj = Multi("Reconcile", load, desired);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (load == null || !load.IsDone || !load.IsValid || load.Value == null || desired == null)
		{
			return false;
		}
		ICreature value = load.Value;
		ICreatureOpenData creatureOpenData = value.GetCreatureOpenData();
		if (creatureOpenData == null)
		{
			return false;
		}
		CustomizeStock[] array = desired;
		for (int i = 0; i < array.Length; i++)
		{
			CustomizeStock customizeStock = array[i];
			bool flag = false;
			Dictionary<AccessoryPartType, CustomizeStock>.ValueCollection valueCollection = creatureOpenData.GetStock().CustomizeStockValues();
			if (valueCollection != null)
			{
				foreach (CustomizeStock item in valueCollection)
				{
					if (item.Part == customizeStock.Part)
					{
						flag = true;
						if (!item.IsEqualItem(customizeStock) || !item.IsEqualTransform(customizeStock) || (!customizeStock.IsEmptyPart() && value.GetCutomizeItemInstance(customizeStock) == null))
						{
							load.ChangeCustomizeStock(customizeStock);
						}
						break;
					}
				}
			}
			if (!flag && !customizeStock.IsEmptyPart())
			{
				load.ChangeCustomizeStock(customizeStock);
			}
		}
		array = desired;
		for (int i = 0; i < array.Length; i++)
		{
			CustomizeStock stock = array[i];
			if (!stock.IsEmptyPart() && value.GetCutomizeItemInstance(stock) == null)
			{
				return false;
			}
		}
		return true;
	}
}
