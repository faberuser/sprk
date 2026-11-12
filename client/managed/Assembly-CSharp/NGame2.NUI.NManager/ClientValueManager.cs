using System;
using System.Collections.Generic;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NManager;

public class ClientValueManager : GlobalAutoBehaviour<ClientValueManager>, InitOnCreate
{
	public interface IClientValue
	{
		void Draw();
	}

	public interface IClientValue<T> : IClientValue
	{
		T Value { get; }
	}

	public class ClientValue
	{
		protected readonly string _name;

		public ClientValue(string name)
		{
			_name = name;
		}
	}

	public class ClientValueBool<T> : ClientValue, IClientValue<bool>, IClientValue
	{
		private LocalStorageBool _localValue;

		public bool Value
		{
			get
			{
				return _localValue.Value;
			}
			set
			{
				_localValue.Value = value;
			}
		}

		public ClientValueBool(string name, bool defalutValue = false)
			: base(name)
		{
			_localValue = new LocalStorageBool(defalutValue, typeof(T).FullName + "_" + name);
		}

		public void Draw()
		{
			_ = Value;
			_localValue.Value = GUILayout.Toggle(_localValue.Value, _name);
		}
	}

	public class CustomValueBool<T> : ClientValue, IClientValue<bool>, IClientValue
	{
		private Func<bool> _getter;

		private Action<bool> _setter;

		public bool Value => Vespa_System_Action_Extension_Method.fire(_getter);

		public CustomValueBool(string name, Func<bool> getter, Action<bool> setter)
			: base(name)
		{
			_getter = getter;
			_setter = setter;
		}

		public void Draw()
		{
			_ = Value;
			bool flag = GUILayout.Toggle(Value, _name);
			if (flag != Value)
			{
				Vespa_System_Action_Extension_Method.fire(_setter, flag);
			}
		}
	}

	public class TitleValue : ClientValue, IClientValue
	{
		public TitleValue(string name)
			: base(name)
		{
		}

		public void Draw()
		{
			GUILayout.Label($"[{_name}]");
		}
	}

	private static List<IClientValue> ClientValueDrawList = new List<IClientValue>();

	private static ClientValueBool<ClientValueManager> ShowItemIndex = new ClientValueBool<ClientValueManager>("ShowItemIndex");

	private static ClientValueBool<ClientValueManager> ShowEquipItemSlotIndex = new ClientValueBool<ClientValueManager>("ShowEquipItemSlotIndex");

	private static ClientValueBool<ClientValueManager> ShowHeroIndex = new ClientValueBool<ClientValueManager>("ShowHeroIndex");

	private static ClientValueBool<ClientValueManager> ShowDungeonIndex = new ClientValueBool<ClientValueManager>("ShowDungeonIndex");

	private static ClientValueBool<ClientValueManager> IgnoreItemEffect = new ClientValueBool<ClientValueManager>("IgnoreItemEffect");

	private static ClientValueBool<ClientValueManager> ForceOpenContentsNoticePopup = new ClientValueBool<ClientValueManager>("ForceOpenContentsNoticePopup");

	private static ClientValueBool<ClientValueManager> ShowCostumeIndex = new ClientValueBool<ClientValueManager>("ShowCostumeIndex");

	private static ClientValueBool<ClientValueManager> CustomizingCostumeValidCheck = new ClientValueBool<ClientValueManager>("CustomizingCostumeValidCheck");

	private static ClientValueBool<ClientValueManager> ShowEquipItemExtraSkillIndex = new ClientValueBool<ClientValueManager>("ShowEquipItemExtraSkillIndex");

	private static ClientValueBool<ClientValueManager> SkipGetNewHero = new ClientValueBool<ClientValueManager>("SkipGetNewHero");

	private static ClientValueBool<ClientValueManager> SkipLobbyPopupQueue = new ClientValueBool<ClientValueManager>("SkipLobbyPopupQueue");

	private static ClientValueBool<ClientValueManager> ShowGachaSelectShopinfo = new ClientValueBool<ClientValueManager>("ShowGachaSelectShopinfo");

	private static CustomValueBool<ClientValueManager> ShowLog = new CustomValueBool<ClientValueManager>("ShowLog", () => Debug.unityLogger.logEnabled, delegate(bool value)
	{
		Debug.unityLogger.logEnabled = value;
	});

	private static CustomValueBool<ClientValueManager> ShowSkillEffect = new CustomValueBool<ClientValueManager>("ShowSkillEffect", () => SystemOptionData.Render_ShowSkillEffect, delegate(bool value)
	{
		SystemOptionData.Render_ShowSkillEffect = value;
	});

	private static CustomValueBool<ClientValueManager> ShowAllySkillEffect = new CustomValueBool<ClientValueManager>("ShowAllySkillEffect", () => SystemOptionData.Render_ShowAllySkillEffect, delegate(bool value)
	{
		SystemOptionData.Render_ShowAllySkillEffect = value;
	});

	private static CustomValueBool<ClientValueManager> ShowEnemySkillEffect = new CustomValueBool<ClientValueManager>("ShowEnemySkillEffect", () => SystemOptionData.Render_ShowEnemySkillEffect, delegate(bool value)
	{
		SystemOptionData.Render_ShowEnemySkillEffect = value;
	});

	private static ClientValueBool<ClientValueManager> EnableTraceLogToFile = new ClientValueBool<ClientValueManager>("EnableTraceLogToFile");

	private static bool _isInitialize = false;

	private static ClientValueBool<ClientValueManager> BlockingDuringBackgoundPatchDownload = new ClientValueBool<ClientValueManager>("BlockingDuringBackgoundPatchDownload");

	private static ClientValueBool<ClientValueManager> ForceCheckBlockUIDuringDownloading = new ClientValueBool<ClientValueManager>("ForceCheckBlockUIDuringDownloading");

	private static ClientValueBool<ClientValueManager> ShowSkillValue = new ClientValueBool<ClientValueManager>("ShowSkillValue");

	private static ClientValueBool<ClientValueManager> ShowSoulWeaponReinforceSuccessRate = new ClientValueBool<ClientValueManager>("ShowSoulWeaponReinforceSuccessRate");

	private static ClientValueBool<ClientValueManager> ShowEclipseJobState = new ClientValueBool<ClientValueManager>("ShowEclipseJobState");

	private static ClientValueBool<ClientValueManager> ShowNewPayshopProductIndex = new ClientValueBool<ClientValueManager>("ShowNewPayshopProductIndex");

	public static bool IsShowItemIndex
	{
		get
		{
			return ShowItemIndex.Value;
		}
		set
		{
			ShowItemIndex.Value = value;
		}
	}

	public static bool IsShowEquipItemSlotIndex
	{
		get
		{
			return ShowEquipItemSlotIndex.Value;
		}
		set
		{
			ShowEquipItemSlotIndex.Value = value;
		}
	}

	public static bool IsShowHeroIndex => ShowHeroIndex.Value;

	public static bool IsShowDungeonIndex => ShowDungeonIndex.Value;

	public static bool IsIgnoreItemEffect => IgnoreItemEffect.Value;

	public static bool IsForceOpenContentsNoticePopup => ForceOpenContentsNoticePopup.Value;

	public static bool IsShowCostumeIndex => ShowCostumeIndex.Value;

	public static bool CheckCustomizingCostumeValid => CustomizingCostumeValidCheck.Value;

	public static bool IsShowEquipItemExtraSkillIndex => ShowEquipItemExtraSkillIndex.Value;

	public static bool IsSkipGetNewHero => SkipGetNewHero.Value;

	public static bool IsSkipLobbyPopupQueue => SkipLobbyPopupQueue.Value;

	public static bool IsShowGachaSelectShopInfo => ShowGachaSelectShopinfo.Value;

	public static bool IsEnableTraceLogToFile => EnableTraceLogToFile.Value;

	public static bool ActiveBlockingDuringBackgoundPatchDownload => BlockingDuringBackgoundPatchDownload.Value;

	public static bool IsBlockingDuringBackgoundPatchDownload => ForceCheckBlockUIDuringDownloading.Value;

	public static bool IsShowSkillValue => ShowSkillValue.Value;

	public static bool IsShowSoulWeaponReinforceSuccessRate => ShowSoulWeaponReinforceSuccessRate.Value;

	public static bool IsShowEclipseJobState => ShowEclipseJobState.Value;

	public static bool IsShowNewPayshopProductIndex => ShowNewPayshopProductIndex.Value;

	void InitOnCreate.InitializeSingleton()
	{
		InitClientValueList();
		GlobalAutoBehaviour<GuiManager>.instance.Add("ClientValueManager", this, delegate
		{
			Draw();
		});
	}

	private static void InitClientValueList()
	{
		ClientValueDrawList.Clear();
		ClientValueDrawList.Add(new TitleValue("UI"));
		ClientValueDrawList.Add(ShowItemIndex);
		ClientValueDrawList.Add(ShowEquipItemSlotIndex);
		ClientValueDrawList.Add(ShowHeroIndex);
		ClientValueDrawList.Add(ShowDungeonIndex);
		ClientValueDrawList.Add(IgnoreItemEffect);
		ClientValueDrawList.Add(ForceOpenContentsNoticePopup);
		ClientValueDrawList.Add(ShowCostumeIndex);
		ClientValueDrawList.Add(CustomizingCostumeValidCheck);
		ClientValueDrawList.Add(SkipGetNewHero);
		ClientValueDrawList.Add(ShowSkillValue);
		ClientValueDrawList.Add(ShowSoulWeaponReinforceSuccessRate);
		ClientValueDrawList.Add(ShowEquipItemExtraSkillIndex);
		ClientValueDrawList.Add(ShowGachaSelectShopinfo);
		ClientValueDrawList.Add(SkipLobbyPopupQueue);
		ClientValueDrawList.Add(new TitleValue("System"));
		ClientValueDrawList.Add(ShowLog);
		ClientValueDrawList.Add(new TitleValue("Effect"));
		ClientValueDrawList.Add(ShowSkillEffect);
		ClientValueDrawList.Add(ShowAllySkillEffect);
		ClientValueDrawList.Add(ShowEnemySkillEffect);
		ClientValueDrawList.Add(new TitleValue("Battle"));
		ClientValueDrawList.Add(EnableTraceLogToFile);
		ClientValueDrawList.Add(BlockingDuringBackgoundPatchDownload);
		ClientValueDrawList.Add(ForceCheckBlockUIDuringDownloading);
		ClientValueDrawList.Add(ShowEclipseJobState);
		ClientValueDrawList.Add(ShowNewPayshopProductIndex);
		_isInitialize = true;
	}

	public static void Draw()
	{
		if (!_isInitialize)
		{
			InitClientValueList();
		}
		foreach (IClientValue clientValueDraw in ClientValueDrawList)
		{
			clientValueDraw.Draw();
		}
	}
}
