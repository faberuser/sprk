using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NScene;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NHelper;
using NGame2.NUI.NManager;
using NGameAsset;
using NShared;
using NShared.NLocalization;
using NShared.UsePackageItem;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class BaseRewardPopup : BaseWindow, IInspectorGuiDrawer, ItemIconComponent.Listener, CurrencyIconComponent.Listener
{
	public class PopupOpenData
	{
		private Dictionary<CurrencyType, List<CurrencyResultInfo3>> CurrencyResultInfos = new Dictionary<CurrencyType, List<CurrencyResultInfo3>>();

		public string title { get; private set; }

		public bool IsSortForMail { get; set; }

		public bool ShowOnlyReward { get; set; }

		public ExpResultInfo expResult { get; private set; }

		public ItemResultInfo[] itemResultInfos { get; private set; }

		public EquipItemInfo[] equipResultInfos { get; private set; }

		public EquipItemBoostInfo[] boostEquipResultInfos { get; private set; }

		public MultiplayMemberResultInfo[] multiplayMemberResults { get; private set; }

		public string[] selectedRewardItemCodes { get; private set; }

		public GuildPointResultInfo guildPointResult { get; private set; }

		public FriendshipPointResultInfo friendshipPointResult { get; private set; }

		public FriendshipPointResultInfo[] friendshipPointResults { get; private set; }

		public StaminaResultInfo[] staminaResultInfos { get; private set; }

		public IndexedRewardItemInfo[] guildRaidBootyItemRewardInfos { get; private set; }

		public Action closeCallback { get; private set; }

		public PopupOpenData SetTitle(string value)
		{
			title = value;
			return this;
		}

		public PopupOpenData SetExpResult(ExpResultInfo value)
		{
			expResult = value;
			return this;
		}

		public PopupOpenData SetItemResultInfos(ItemResultInfo[] value)
		{
			itemResultInfos = value;
			return this;
		}

		public PopupOpenData SetEquipResultInfos(EquipItemInfo[] value)
		{
			equipResultInfos = value;
			return this;
		}

		public PopupOpenData SetBoostEquipResultInfos(EquipItemBoostInfo[] value)
		{
			boostEquipResultInfos = value;
			return this;
		}

		public PopupOpenData SetMultiplayMemberResultInfos(MultiplayMemberResultInfo[] value)
		{
			multiplayMemberResults = value;
			return this;
		}

		public PopupOpenData SetSelectedRewardItemCodes(string[] value)
		{
			selectedRewardItemCodes = value;
			return this;
		}

		private PopupOpenData SetCurrenyResultInfo3(CurrencyResultInfo3 value)
		{
			if (value == null)
			{
				return this;
			}
			List<CurrencyResultInfo3> list = new List<CurrencyResultInfo3>();
			if (list != null)
			{
				list.Add(value);
				if (CurrencyResultInfos.ContainsKey(value.CurrencyType))
				{
					CurrencyResultInfos.Remove(value.CurrencyType);
				}
				CurrencyResultInfos.Add(value.CurrencyType, list);
			}
			return this;
		}

		private void ClearCurrentResultInfos()
		{
			CurrencyResultInfos.Clear();
		}

		public PopupOpenData SetCurrencyResultInfos3(CurrencyResultInfo3[] value)
		{
			ClearCurrentResultInfos();
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(value); i++)
			{
				SetCurrenyResultInfo3(value[i]);
			}
			return this;
		}

		public PopupOpenData SetCurrencyResultInfo3Single(CurrencyResultInfo3 value)
		{
			ClearCurrentResultInfos();
			SetCurrenyResultInfo3(value);
			return this;
		}

		public PopupOpenData AddPetAddResultInfos(PetAddResultInfo[] values)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(values); i++)
			{
				PetAddResultInfo value = values[i];
				AddPetAddResultInfo(value);
			}
			return this;
		}

		public PopupOpenData AddPetAddResultInfo(PetAddResultInfo value)
		{
			if (value == null)
			{
				return this;
			}
			if (__Extension_Method_ICollection__.COUNT(value.PetSoulResults) > 0)
			{
				AddItemResultInfos(value.PetSoulResults);
			}
			AddPetInfo(value.PetResult);
			return this;
		}

		public PopupOpenData AddPetInfos(PetInfo[] values)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(values); i++)
			{
				PetInfo value = values[i];
				AddPetInfo(value);
			}
			return this;
		}

		public PopupOpenData AddPetInfo(PetInfo value)
		{
			if (value == null)
			{
				return this;
			}
			ItemResultInfo itemResultInfo = ConvertPetInfoToItemResultInfo(value);
			ItemResultInfo[] value2 = new ItemResultInfo[1] { itemResultInfo };
			AddItemResultInfos(value2);
			return this;
		}

		private ItemResultInfo ConvertPetInfoToItemResultInfo(PetInfo petInfo)
		{
			if (petInfo == null)
			{
				return null;
			}
			int petIndex = petInfo.PetIndex;
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(petIndex);
			if (data == null)
			{
				return null;
			}
			return new ItemResultInfo
			{
				ItemIndex = data.Index,
				AddCount = 1
			};
		}

		public PopupOpenData SetGuildPointResult(GuildPointResultInfo value)
		{
			guildPointResult = value;
			return this;
		}

		public PopupOpenData SetFriendshipPointResult(FriendshipPointResultInfo value)
		{
			friendshipPointResult = value;
			return this;
		}

		public PopupOpenData SetFriendshipPointResults(FriendshipPointResultInfo[] value)
		{
			friendshipPointResults = value;
			return this;
		}

		public PopupOpenData SetStaminaResultInfos(StaminaResultInfo[] value)
		{
			staminaResultInfos = value;
			return this;
		}

		public PopupOpenData SetGuildRaidBootyItemRewardInfos(IndexedRewardItemInfo[] value)
		{
			guildRaidBootyItemRewardInfos = value;
			return this;
		}

		public PopupOpenData SetShowOnlyReward(bool value)
		{
			ShowOnlyReward = value;
			return this;
		}

		public PopupOpenData SetCloseCallback(Action callback)
		{
			closeCallback = callback;
			return this;
		}

		public T[] Combine<T>(T[] values1, T[] values2)
		{
			List<T> list = new List<T>();
			list.AddRange(values1);
			list.AddRange(values2);
			return list.ToArray();
		}

		public T[] Combine<T>(T[] values1, T value2)
		{
			List<T> list = new List<T>();
			list.AddRange(values1);
			list.Add(value2);
			return list.ToArray();
		}

		public T[] Combine<T>(T[] values1, List<T> values2)
		{
			List<T> list = new List<T>();
			list.AddRange(values1);
			list.AddRange(values2);
			return list.ToArray();
		}

		public PopupOpenData AddItemResultInfos(ItemResultInfo[] value)
		{
			if (value == null)
			{
				return this;
			}
			itemResultInfos = ((itemResultInfos == null) ? value : Combine(itemResultInfos, value));
			Array.Sort(itemResultInfos, (ItemResultInfo l, ItemResultInfo r) => l.ItemIndex.CompareTo(r.ItemIndex));
			return this;
		}

		public PopupOpenData AddEquipResultInfos(EquipItemInfo[] value)
		{
			if (value == null)
			{
				return this;
			}
			equipResultInfos = ((equipResultInfos == null) ? value : Combine(equipResultInfos, value));
			Array.Sort(equipResultInfos, (EquipItemInfo l, EquipItemInfo r) => l.ItemIndex.CompareTo(r.ItemIndex));
			return this;
		}

		public PopupOpenData AddCurrencyResultInfos3(CurrencyResultInfo3[] infos)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
			{
				CurrencyResultInfo3 currencyResultInfo = infos[i];
				List<CurrencyResultInfo3> value = null;
				if (CurrencyResultInfos.TryGetValue(currencyResultInfo.CurrencyType, out value))
				{
					value.Add(currencyResultInfo);
					continue;
				}
				value = new List<CurrencyResultInfo3>();
				if (value != null)
				{
					value.Add(currencyResultInfo);
					CurrencyResultInfos.Add(currencyResultInfo.CurrencyType, value);
				}
			}
			return this;
		}

		public PopupOpenData AddGuildPointResult(GuildPointResultInfo value)
		{
			if (value == null)
			{
				return this;
			}
			checked
			{
				if (guildPointResult == null)
				{
					guildPointResult = value;
				}
				else
				{
					guildPointResult.AddValue += value.AddValue;
				}
				return this;
			}
		}

		public PopupOpenData AddStaminaResultInfos(StaminaResultInfo[] value)
		{
			if (__Extension_Method_ICollection__.COUNT(value) == 0)
			{
				return this;
			}
			List<StaminaResultInfo> list = null;
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(value); i++)
			{
				StaminaResultInfo staminaResultInfo = value[i];
				if (staminaResultInfo != null && !staminaResultInfo.IsHide)
				{
					if (list == null)
					{
						list = new List<StaminaResultInfo>();
					}
					list.Add(staminaResultInfo);
				}
			}
			if (__Extension_Method_ICollection__.COUNT(list) == 0)
			{
				return this;
			}
			if (staminaResultInfos == null)
			{
				staminaResultInfos = list.ToArray();
			}
			else
			{
				staminaResultInfos = Combine(staminaResultInfos, list);
			}
			return this;
		}

		public PopupOpenData()
		{
			IsSortForMail = false;
		}

		public PopupOpenData(NShared.UsePackageItem.Response response)
		{
			Dictionary<CurrencyType, List<CurrencyResultInfo3>> dictionary = new Dictionary<CurrencyType, List<CurrencyResultInfo3>>();
			List<ItemResultInfo> list = new List<ItemResultInfo>();
			List<EquipItemInfo> list2 = new List<EquipItemInfo>();
			List<StaminaResultInfo> list3 = new List<StaminaResultInfo>();
			ExpResultInfo expResultInfo = new ExpResultInfo();
			RewardResultInfo[] rewardResults = response.RewardResults;
			foreach (RewardResultInfo rewardResultInfo in rewardResults)
			{
				Insert(list, rewardResultInfo.ItemResults);
				Insert(list2, rewardResultInfo.EquipItemResults);
				for (int j = 0; j < __Extension_Method_ICollection__.COUNT(rewardResultInfo.CurrencyResults); j++)
				{
					CurrencyResultInfo3 currencyResultInfo = rewardResultInfo.CurrencyResults[j];
					if (currencyResultInfo != null)
					{
						CurrencyType currencyType = currencyResultInfo.CurrencyType;
						List<CurrencyResultInfo3> list4 = null;
						if (!dictionary.ContainsKey(currencyType))
						{
							list4 = new List<CurrencyResultInfo3>();
							dictionary.Add(currencyType, list4);
						}
						else
						{
							list4 = dictionary[currencyType];
						}
						list4.Add(currencyResultInfo);
					}
				}
				Insert(list3, rewardResultInfo.StaminaResultInfos);
				if (rewardResultInfo.ExpResultInfos != null && __Extension_Method_ICollection__.COUNT(rewardResultInfo.ExpResultInfos) > 0)
				{
					ExpResultInfo[] expResultInfos = rewardResultInfo.ExpResultInfos;
					foreach (ExpResultInfo expResultInfo2 in expResultInfos)
					{
						expResultInfo.OldLevel = expResultInfo2.OldLevel;
						expResultInfo.NewLevel = expResultInfo2.NewLevel;
						expResultInfo.AddValue += expResultInfo2.AddValue;
					}
				}
				for (int l = 0; l < __Extension_Method_ICollection__.COUNT(rewardResultInfo.PetInfos); l++)
				{
					PetInfo petInfo = rewardResultInfo.PetInfos[l];
					if (petInfo != null)
					{
						ItemResultInfo itemResultInfo = ConvertPetInfoToItemResultInfo(petInfo);
						if (itemResultInfo != null)
						{
							list.Add(itemResultInfo);
						}
					}
				}
			}
			itemResultInfos = list.ToArray();
			equipResultInfos = list2.ToArray();
			foreach (KeyValuePair<CurrencyType, List<CurrencyResultInfo3>> item in dictionary)
			{
				CurrencyType key = item.Key;
				List<CurrencyResultInfo3> value = item.Value;
				CurrencyResultInfos.Add(key, value);
			}
			staminaResultInfos = list3.ToArray();
			expResult = expResultInfo;
			IsSortForMail = false;
		}

		private static void Insert<T>(List<T> sum, T[] array)
		{
			if (array != null)
			{
				foreach (T item in array)
				{
					sum.Add(item);
				}
			}
		}

		public virtual int CalcRewardCount()
		{
			int num = 0 + ((expResult != null && (expResult.AddValue != 0 || expResult.NewLevel != expResult.OldLevel)) ? 1 : 0) + GetRewardItemResultInfoCount(itemResultInfos) + __Extension_Method_ICollection__.COUNT(equipResultInfos);
			int num3;
			if (friendshipPointResult != null)
			{
				long addValue = friendshipPointResult.AddValue;
				int num2 = 0;
				if (addValue > num2)
				{
					num3 = 1;
					goto IL_0085;
				}
			}
			num3 = 0;
			goto IL_0085;
			IL_0085:
			int num4 = num + num3;
			int num6;
			if (guildPointResult != null)
			{
				long addValue2 = guildPointResult.AddValue;
				int num5 = 0;
				if (addValue2 > num5)
				{
					num6 = 1;
					goto IL_00b3;
				}
			}
			num6 = 0;
			goto IL_00b3;
			IL_00b3:
			return num4 + num6 + __Extension_Method_ICollection__.COUNT(guildRaidBootyItemRewardInfos) + GetRewardStaminaInfoCount(staminaResultInfos) + GetFriendshipPointResultsCount(friendshipPointResults) + GetRewardCurrencyResultInfosCount();
		}

		private static int GetFriendshipPointResultsCount(FriendshipPointResultInfo[] resultInfos)
		{
			int num = 0;
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(resultInfos); i++)
			{
				if (resultInfos[i] != null)
				{
					long addValue = resultInfos[i].AddValue;
					int num2 = 0;
					if (addValue > num2)
					{
						num++;
					}
				}
			}
			return num;
		}

		public CurrencyResultInfo3[] GetCurrencyResultListByType(CurrencyType type)
		{
			List<CurrencyResultInfo3> value = null;
			if (CurrencyResultInfos.TryGetValue(type, out value))
			{
				return value.ToArray();
			}
			return null;
		}

		private int GetRewardCurrencyResultInfosCount()
		{
			if (__Extension_Method_ICollection__.IsNullOrEmpty(CurrencyResultInfos))
			{
				return 0;
			}
			int count = 0;
			foreach (KeyValuePair<CurrencyType, List<CurrencyResultInfo3>> currencyResultInfo in CurrencyResultInfos)
			{
				currencyResultInfo.Value.ForEach(delegate(CurrencyResultInfo3 item)
				{
					if (item.AddValue > 0)
					{
						int num = count + 1;
						count = num;
					}
				});
			}
			return count;
		}

		public Dictionary<CurrencyType, List<CurrencyResultInfo3>> GetCurrencyResultInfos()
		{
			return CurrencyResultInfos;
		}

		protected int GetRewardItemResultInfoCount(ItemResultInfo[] itemResultInfos)
		{
			if (__Extension_Method_ICollection__.IsNullOrEmpty(itemResultInfos))
			{
				return 0;
			}
			int num = itemResultInfos.Length;
			for (int i = 0; i < itemResultInfos.Length; i++)
			{
				if (!DoesApplyRewardItem(itemResultInfos[i]))
				{
					num--;
				}
			}
			return num;
		}

		private int GetRewardStaminaInfoCount(StaminaResultInfo[] staminaResults)
		{
			if (__Extension_Method_ICollection__.IsNullOrEmpty(staminaResults))
			{
				return 0;
			}
			int num = 0;
			foreach (StaminaResultInfo staminaResultInfo in staminaResults)
			{
				if (staminaResultInfo != null && GetCurrencyTypeByStaminaType(staminaResultInfo.Type) != CurrencyType.None)
				{
					long addValue = staminaResultInfo.AddValue;
					int num2 = 0;
					if (addValue > num2)
					{
						num++;
					}
				}
			}
			return num;
		}
	}

	public GameObject RewardPopupRoot;

	public const string PrefabPath = "Resources/Prefab2/Window/RewardPopup/BaseRewardPopup";

	public static PrefabAuto<BaseRewardPopup> PrefabAuto = new PrefabAuto<BaseRewardPopup>("Resources/Prefab2/Window/RewardPopup/BaseRewardPopup");

	public float RewardPopInterval = 0.2f;

	public float InputDisabledTime = 0.5f;

	public float SkippableTime = 4f;

	public UIScrollGrid RewardItemGrid;

	public UILabel TitleLabel;

	public Transform TeamLevelGaugeRoot;

	public int MaxShowLine = 4;

	public UIWidget DummyFront;

	public UIWidget DummyRear;

	protected PopupOpenData _popupOpen;

	protected ItemAnimationContainer _itemAnimationContainer = new ItemAnimationContainer(open3dModelPopup: true);

	protected List<Component> _rewardList = new List<Component>();

	private float _openedTime;

	private bool _releaseOnClose;

	private Action _onClose;

	private TeamLevelGauge _teamLevelGauge;

	public Transform EclipseKeyGaugeRoot;

	private EclipseKeyGaugeComponent _eclipseKeyGauge;

	public Transform ShakmehGaugeRoot;

	private ShakmehGaugeComponent _shakmehGaugeComponent;

	private bool _showEclipseCenterMessage;

	private bool _showShakmehCneterMessage;

	private bool _addedRewardPopup;

	private string _callstack;

	public UISprite Spr_Bg;

	public Transform Trs_Title;

	public Transform Trs_Line;

	public UILabel Guide_Label;

	public static BaseRewardPopup OpenPopup(PopupOpenData popupOpen, bool isGlobal, Action onClose = null)
	{
		if (popupOpen.CalcRewardCount() == 0)
		{
			Vespa_System_Action_Extension_Method.fire(onClose);
			return null;
		}
		if (String_Extension_Method.IsNullOrEmpty(popupOpen.title))
		{
			popupOpen.SetTitle(LocalizationManager.GetString(NShared.NLocalization.Common.INFORM));
		}
		Transform parentTm = ((!isGlobal) ? GameUIRoot.instance.ContentsGroup.transform : GlobalUIRoot.instance.ContentsGroup.transform);
		BaseRewardPopup baseRewardPopup = PrefabAuto.Factory.Create(parentTm);
		if (!baseRewardPopup.OpenWithData(popupOpen, isGlobal, onClose))
		{
			Vespa_System_Action_Extension_Method.fire(onClose);
			return null;
		}
		baseRewardPopup.SetTitle(popupOpen.title);
		baseRewardPopup.ShowOnlyReward(popupOpen.ShowOnlyReward);
		return baseRewardPopup;
	}

	public static BaseRewardPopup Create(bool isGlobal)
	{
		Transform parentTm = ((!isGlobal) ? GameUIRoot.instance.ContentsGroup.transform : GlobalUIRoot.instance.ContentsGroup.transform);
		return PrefabAuto.Factory.Create(parentTm, setActive: false);
	}

	public bool OpenWithData(PopupOpenData popupOpen, bool isGlobal, Action onClose = null, bool isOpenWithNoReward = false)
	{
		_showEclipseCenterMessage = false;
		_showShakmehCneterMessage = false;
		_addedRewardPopup = false;
		_callstack = VespaDebug.GetCallStackDump("Open BaseRewardPopup: ");
		_popupOpen = popupOpen;
		UIHelper.EnableGameObject(Guide_Label, enable: false);
		int num = 0;
		if (popupOpen != null)
		{
			num = popupOpen.CalcRewardCount();
		}
		if (num == 0 && !isOpenWithNoReward)
		{
			Vespa_System_Action_Extension_Method.fire(onClose);
			Close();
			return false;
		}
		checked
		{
			Open(delegate
			{
				_openedTime = Time.time;
				_onClose = onClose;
				OnOpenAction();
			}, delegate
			{
				if (_popupOpen == null)
				{
					FinishSetting(isGlobal);
				}
				else
				{
					if (_popupOpen.expResult != null && _popupOpen.expResult.AddValue > 0 && TeamLevelGaugeRoot != null)
					{
						_teamLevelGauge = TeamLevelGauge.PrefabAuto.Factory.Create(TeamLevelGaugeRoot);
						SingletonAuto<SoundContainer>.instance.PlayUnique("UI.Campaign.GetExp", this);
						_teamLevelGauge.Open(_popupOpen.expResult, delegate
						{
							SoundContainer.StopUnique("UI.Campaign.GetExp", this);
						});
					}
					if (_popupOpen.GetCurrencyResultListByType(CurrencyType.EclipsePoint) != null && (bool)EclipseKeyGaugeRoot && EclipseKeyGaugeComponent.ShowEclipseKeyGauge())
					{
						_eclipseKeyGauge = EclipseKeyGaugeComponent.PrefabAuto.Factory.Create(EclipseKeyGaugeRoot);
						long num2 = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetStamina(StaminaType.EclipseKey);
						CurrencyResultInfo3[] currencyResultListByType = _popupOpen.GetCurrencyResultListByType(CurrencyType.EclipsePoint);
						CurrencyResultInfo3 currencyResultInfo = new CurrencyResultInfo3
						{
							CurrencyType = CurrencyType.EclipsePoint
						};
						CurrencyResultInfo3[] array = currencyResultListByType;
						foreach (CurrencyResultInfo3 currencyResultInfo2 in array)
						{
							currencyResultInfo.AddValue += currencyResultInfo2.AddValue;
						}
						int exchangeCostValue = GameRoot.Instance.CurrencyTypeDataContainer.GetCurrencyData(CurrencyType.EclipsePoint).ExchangeCostValue;
						long num4 = currencyResultListByType[^1].NewValue;
						if (num4 >= exchangeCostValue)
						{
							num4 = unchecked(num4 % exchangeCostValue);
						}
						currencyResultInfo.NewValue = num4;
						long num5 = currencyResultListByType[0].NewValue - currencyResultListByType[0].AddValue;
						if (num5 + currencyResultInfo.AddValue >= exchangeCostValue)
						{
							long num6 = num2;
							long num7 = num5 + currencyResultInfo.AddValue;
							int num8 = exchangeCostValue;
							num2 = num6 - unchecked(num7 / num8);
							currencyResultInfo.NewValue = num5 + currencyResultInfo.AddValue;
						}
						_eclipseKeyGauge.Init(num2, currencyResultInfo, playFillAnimation: true, delegate
						{
							if (__Extension_Method_ICollection__.COUNT(_rewardList) == 0)
							{
								Close();
							}
						});
						_showEclipseCenterMessage = true;
					}
					if (_popupOpen.GetCurrencyResultListByType(CurrencyType.ShakmehMiddleBossPoint) != null && ShakmehGaugeRoot != null)
					{
						_shakmehGaugeComponent = ShakmehGaugeComponent.PrefabAuto.Factory.Create(ShakmehGaugeRoot);
						CurrencyResultInfo3[] currencyResultListByType2 = _popupOpen.GetCurrencyResultListByType(CurrencyType.ShakmehMiddleBossPoint);
						long userStat = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetUserStat(UserManager.UserStatType.ShakmehMiddleBossPoint);
						long num9 = userStat;
						CurrencyResultInfo3[] array = currencyResultListByType2;
						foreach (CurrencyResultInfo3 currencyResultInfo3 in array)
						{
							num9 -= currencyResultInfo3.AddValue;
						}
						_shakmehGaugeComponent.Open(num9, userStat, 1f, autoStart: true);
						_showShakmehCneterMessage = true;
					}
					SetupRewardItems();
				}
			});
			FinishSetting(isGlobal);
			return true;
		}
	}

	private void FinishSetting(bool isGlobal)
	{
		if (isGlobal)
		{
			GlobalAutoBehaviour<PopupManegement>.instance.AddGlobal(null, this);
		}
		else
		{
			GlobalAutoBehaviour<PopupManegement>.instance.Add(null, this);
		}
		_addedRewardPopup = true;
	}

	public void ActiveLabel(string label)
	{
		UIHelper.SetLabelText(Guide_Label, label);
		UIHelper.EnableGameObject(Guide_Label, enable: true);
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		_itemAnimationContainer.ChainUpdate();
		if (_showEclipseCenterMessage && _addedRewardPopup && __Extension_Method_ICollection__.COUNT(_rewardList) == 0)
		{
			MultiCenterMessages multiCenterMessages = MultiCenterMessages.Open();
			multiCenterMessages.AddIconMessage(LocalizationManager.GetStringDirect("ECLIPSE_POINT_GET_CENTER_MESSAGE"), "EclipsePoint");
			multiCenterMessages.RepositionMessage();
			_showEclipseCenterMessage = false;
			_addedRewardPopup = false;
		}
		if (_showShakmehCneterMessage && _addedRewardPopup && GlobalAutoBehaviour<LoadingManager>.instance.CurrentSceneType == LoadingManager.ESceneType.Lobby)
		{
			if (__Extension_Method_ICollection__.COUNT(_rewardList) == 0)
			{
				MultiCenterMessages multiCenterMessages2 = MultiCenterMessages.Open();
				multiCenterMessages2.AddIconMessage(LocalizationManager.GetStringDirect("LOCAL_SHAKMEH_MIDDLE_BOSS_POINT_GAIN_TEXT"), "EclipsePoint");
				multiCenterMessages2.RepositionMessage();
			}
			_showShakmehCneterMessage = false;
			_addedRewardPopup = false;
		}
	}

	public float GetSecondAfterOpened()
	{
		return Time.time - _openedTime;
	}

	private static bool DoesApplyRewardItem(ItemResultInfo itemResultInfo)
	{
		if (itemResultInfo == null)
		{
			return false;
		}
		if (itemResultInfo.AddCount <= 0)
		{
			return false;
		}
		return true;
	}

	public override void Reset()
	{
		base.Reset();
		ClearItem();
		ShowOnlyReward(rewardOnly: false);
		if (_teamLevelGauge != null)
		{
			FactoryBase.SafeReleaseComponent(ref _teamLevelGauge);
		}
	}

	public override void OnClosing()
	{
		base.OnClosing();
		Vespa_System_Action_Extension_Method.fire(_onClose);
		_onClose = null;
	}

	protected virtual void OnOpenAction()
	{
	}

	public void SetTitle(string title)
	{
		UIHelper.SetLabelText(TitleLabel, title);
	}

	protected void ClearItem()
	{
		_itemAnimationContainer.Reset();
		FactoryBase.ReleaseAndClearList(_rewardList);
		FactoryBase.SafeReleaseComponent(ref _eclipseKeyGauge);
		FactoryBase.SafeReleaseComponent(ref _shakmehGaugeComponent);
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent component)
	{
		OnClickBackground();
	}

	void CurrencyIconComponent.Listener.OnClick(CurrencyIconComponent component)
	{
		OnClickBackground();
	}

	public override void OnSystemBackButton()
	{
		OnClickBackground();
	}

	public virtual void OnClickBackground()
	{
		if (Time.time - _openedTime < InputDisabledTime)
		{
			return;
		}
		if (!_itemAnimationContainer.IsEmpty)
		{
			if (!(Time.time - _openedTime < SkippableTime))
			{
				_itemAnimationContainer.Reset();
			}
		}
		else
		{
			CloseCallback();
			Close();
		}
	}

	private void CloseCallback()
	{
		Vespa_System_Action_Extension_Method.fire(_popupOpen.closeCallback);
	}

	protected void SetupRewardItems(ItemResultInfo[] items, bool displayInvenCount = true, bool displayBoosterInfo = true)
	{
		if (items == null)
		{
			return;
		}
		List<ItemResultInfo> list = MergeItemInfo(items);
		SetBattleType(out var battleType);
		bool isBestTeam = false;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(list); i++)
		{
			ItemResultInfo itemResultInfo = list[i];
			if (itemResultInfo == null || itemResultInfo.AddCount <= 0)
			{
				continue;
			}
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemResultInfo.ItemIndex);
			if (data == null)
			{
				continue;
			}
			CampaignRewardComponent campaignRewardComponent = UIHelper.AddGridItem(RewardItemGrid, CampaignRewardComponent.PrefabAuto);
			if (!(campaignRewardComponent == null))
			{
				if (battleType == BattleType.Eclipse)
				{
					isBestTeam = itemResultInfo.IsFirstClearReward;
					itemResultInfo.IsFirstClearReward = false;
				}
				GetItemAnimationData getItemAnimation = campaignRewardComponent.Open(data, itemResultInfo.AddCount, itemResultInfo.AddBoosterCount, itemResultInfo.AddNPCBoosterCount, itemResultInfo.AddBonusAssignedItemPercent, this, displayInvenCount, displayBoosterInfo, itemResultInfo.IsFirstClearReward, isBestTeam);
				_itemAnimationContainer.Add(getItemAnimation);
				_rewardList.Add(campaignRewardComponent);
			}
		}
	}

	protected virtual List<ItemResultInfo> MergeItemInfo(ItemResultInfo[] items)
	{
		List<ItemResultInfo> list = new List<ItemResultInfo>();
		foreach (ItemResultInfo itemInfo in items)
		{
			ItemResultInfo itemResultInfo = (itemInfo.IsFirstClearReward ? null : list.Find((ItemResultInfo find) => find.ItemIndex == itemInfo.ItemIndex && !find.IsFirstClearReward));
			if (itemResultInfo == null)
			{
				itemResultInfo = new ItemResultInfo
				{
					ItemIndex = itemInfo.ItemIndex,
					NewCount = itemInfo.NewCount,
					AddCount = itemInfo.AddCount,
					AddBoosterCount = itemInfo.AddBoosterCount,
					AddNPCBoosterCount = itemInfo.AddNPCBoosterCount,
					AddBonusAssignedItemPercent = itemInfo.AddBonusAssignedItemPercent,
					IsFirstClearReward = itemInfo.IsFirstClearReward
				};
				list.Add(itemResultInfo);
			}
			else
			{
				itemResultInfo.NewCount = itemInfo.NewCount;
				itemResultInfo.AddCount += itemInfo.AddCount;
				itemResultInfo.AddBoosterCount += itemInfo.AddBoosterCount;
				itemResultInfo.AddNPCBoosterCount += itemInfo.AddNPCBoosterCount;
				itemResultInfo.AddBonusAssignedItemPercent += itemInfo.AddBonusAssignedItemPercent;
			}
		}
		return list;
	}

	protected virtual void SetBattleType(out BattleType battleType)
	{
		battleType = BattleType.None;
	}

	protected void SetupRewardItems(EquipItemInfo[] equipItems, bool displayBoostInfo = true)
	{
		if (equipItems == null)
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(equipItems); i++)
		{
			EquipItemInfo equipItem = equipItems[i];
			CampaignRewardComponent campaignRewardComponent = UIHelper.AddGridItem(RewardItemGrid, CampaignRewardComponent.PrefabAuto);
			if (!(campaignRewardComponent == null))
			{
				bool flag = __Extension_Method_ICollection__.COUNT(_popupOpen.boostEquipResultInfos) > 0 && Array.Find(_popupOpen.boostEquipResultInfos, (EquipItemBoostInfo e) => equipItem.SlotIndex == e.SlotIndex) != null;
				flag = flag && displayBoostInfo;
				GetItemAnimationData getItemAnimation = campaignRewardComponent.Open(equipItem, flag, this);
				_itemAnimationContainer.Add(getItemAnimation);
				_rewardList.Add(campaignRewardComponent);
			}
		}
	}

	private void SetupRewardItems(StaminaResultInfo[] staminas)
	{
		if (staminas == null)
		{
			return;
		}
		Dictionary<CurrencyType, StaminaResultInfo> dictionary = new Dictionary<CurrencyType, StaminaResultInfo>();
		checked
		{
			foreach (StaminaResultInfo staminaResultInfo in staminas)
			{
				if (staminaResultInfo == null)
				{
					continue;
				}
				long addValue = staminaResultInfo.AddValue;
				int num = 0;
				if (addValue <= num)
				{
					continue;
				}
				CurrencyType currencyTypeByStaminaType = GetCurrencyTypeByStaminaType(staminaResultInfo.Type);
				if (currencyTypeByStaminaType != CurrencyType.None)
				{
					if (!dictionary.ContainsKey(currencyTypeByStaminaType))
					{
						StaminaResultInfo value = new StaminaResultInfo();
						dictionary.Add(currencyTypeByStaminaType, value);
					}
					dictionary[currencyTypeByStaminaType].AddValue += staminaResultInfo.AddValue;
				}
			}
			foreach (KeyValuePair<CurrencyType, StaminaResultInfo> item in dictionary)
			{
				CurrencyType key = item.Key;
				long addValue2 = item.Value.AddValue;
				int num2 = 0;
				SetupRewardItemComponent(addValue2, key, "", num2);
			}
		}
	}

	protected void SetupRewardItems<T>(T[] results, bool isSum) where T : ICurrencyResultInfo
	{
		if (results == null || __Extension_Method_ICollection__.COUNT(results) == 0)
		{
			return;
		}
		int num = 0;
		long num2 = num;
		CurrencyType currencyType = CurrencyType.None;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(results); i++)
		{
			T val = results[i];
			if (val == null)
			{
				continue;
			}
			long addValue = val.AddValue;
			int num3 = 0;
			if (addValue > num3)
			{
				if (!isSum)
				{
					long addValue2 = val.AddValue;
					CurrencyType currencyType2 = val.CurrencyType;
					int num4 = 0;
					SetupRewardItemComponent(addValue2, currencyType2, "", num4);
				}
				else
				{
					num2 = checked(num2 + val.AddValue);
					currencyType = val.CurrencyType;
				}
			}
		}
		if (isSum)
		{
			long value = num2;
			CurrencyType currencyType3 = currencyType;
			int num5 = 0;
			SetupRewardItemComponent(value, currencyType3, "", num5);
		}
	}

	protected virtual void SetupRewardItems(CurrencyResultInfo3[] results, bool isSum)
	{
		if (results == null || __Extension_Method_ICollection__.COUNT(results) == 0)
		{
			return;
		}
		int num = 0;
		long num2 = num;
		int num3 = 0;
		long fieldValue = num3;
		string fieldName = "";
		CurrencyType currencyType = CurrencyType.None;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(results); i++)
		{
			CurrencyResultInfo3 currencyResultInfo = results[i];
			if (currencyResultInfo != null && currencyResultInfo.AddValue > 0)
			{
				int num4 = 0;
				fieldValue = num4;
				fieldName = "";
				GetFieldValueByCurrencyType(currencyResultInfo.CurrencyType, currencyResultInfo, out fieldName, out fieldValue);
				if (!isSum)
				{
					SetupRewardItemComponent(currencyResultInfo.AddValue, currencyResultInfo.CurrencyType, fieldName, fieldValue);
					continue;
				}
				num2 = checked(num2 + currencyResultInfo.AddValue);
				currencyType = currencyResultInfo.CurrencyType;
			}
		}
		if (isSum)
		{
			SetupRewardItemComponent(num2, currencyType, fieldName, fieldValue);
		}
	}

	protected void GetFieldValueByCurrencyType(CurrencyType type, CurrencyResultInfo3 info, out string fieldName, out long fieldValue)
	{
		int num = 0;
		fieldValue = num;
		fieldName = "";
		SetFieldProperty("FirstDiffClear", info, ref fieldName, ref fieldValue);
		SetFieldProperty("AddBoosterValue", info, ref fieldName, ref fieldValue);
		SetFieldProperty("AddBoosterCount", info, ref fieldName, ref fieldValue);
		SetFieldProperty("Type", info, ref fieldName, ref fieldValue);
	}

	private void SetFieldProperty(string findFieldName, CurrencyResultInfo3 info, ref string fieldName, ref long fieldValue)
	{
		if (fieldValue == 0L)
		{
			fieldName = findFieldName;
			fieldValue = info.GetFieldValue(fieldName);
		}
	}

	private void SetupRewardItem<T>(T[] result) where T : ICurrencyResultInfo
	{
		if (result != null)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(result); i++)
			{
				SetupReward(result[i]);
			}
		}
	}

	public void SetupReward<T>(T result) where T : ICurrencyResultInfo
	{
		if (result != null)
		{
			long addValue = result.AddValue;
			int num = 0;
			if (addValue > num)
			{
				long addValue2 = result.AddValue;
				CurrencyType currencyType = result.CurrencyType;
				int num2 = 0;
				SetupRewardItemComponent(addValue2, currencyType, "", num2);
			}
		}
	}

	public void SetupRewardItemComponent(long value, CurrencyType currencyType, string fieldName = "", long fieldValue = 0)
	{
		int num = 0;
		if (value <= num || currencyType == CurrencyType.EclipsePoint || currencyType == CurrencyType.EclipseKey || currencyType == CurrencyType.ShakmehMiddleBossPoint)
		{
			return;
		}
		CampaignRewardComponent campaignRewardComponent = UIHelper.AddGridItem(RewardItemGrid, CampaignRewardComponent.PrefabAuto);
		if (campaignRewardComponent != null)
		{
			GetItemAnimationData rewardItemAnimationData = GetRewardItemAnimationData(campaignRewardComponent, currencyType, value, fieldName, fieldValue);
			if (rewardItemAnimationData != null)
			{
				_itemAnimationContainer.Add(rewardItemAnimationData);
			}
			_rewardList.Add(campaignRewardComponent);
		}
	}

	private GetItemAnimationData GetRewardItemAnimationData(CampaignRewardComponent comp, CurrencyType currencyType, long value, string fieldName, long fieldValue)
	{
		switch (fieldName)
		{
		case "FirstDiffClear":
			return comp.Open(currencyType, value, this, (byte)fieldValue);
		case "AddBoosterValue":
		case "AddBoosterCount":
			return comp.Open(currencyType, value, fieldName, fieldValue, this);
		case "Type":
			return comp.Open(currencyType, value, GetGloryPointTitle(checked((int)fieldValue)), this);
		default:
			return comp.Open(currencyType, value, string.Empty, this);
		}
	}

	private string GetGloryPointTitle(int enumValue)
	{
		string result = string.Empty;
		GloryPointType gloryPointType = GloryPointType.None;
		if (Enum.IsDefined(typeof(GloryPointType), enumValue))
		{
			gloryPointType = (GloryPointType)enumValue;
		}
		switch (gloryPointType)
		{
		case GloryPointType.SuccessiveWin:
			result = LocalizationManager.GetStringDirect("MATCH_PICKBAN_CONTINUE_WIN");
			break;
		case GloryPointType.Attendance:
			result = LocalizationManager.GetStringDirect("MATCH_PICKBAN_JOIN_REWARD");
			break;
		case GloryPointType.Win:
			result = LocalizationManager.GetStringDirect("MATCH_PICKBAN_WIN_REWARD");
			break;
		case GloryPointType.Recommand:
			result = LocalizationManager.GetStringDirect("MATCH_WEEKEND_BONUS_HERO_REWARD_1");
			break;
		}
		return result;
	}

	private void SetupRewardItems(IndexedRewardItemInfo[] rewardInfos)
	{
		if (__Extension_Method_ICollection__.COUNT(rewardInfos) == 0)
		{
			return;
		}
		List<ItemResultInfo> list = null;
		List<EquipItemInfo> list2 = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(rewardInfos); i++)
		{
			IndexedRewardItemInfo indexedRewardItemInfo = rewardInfos[i];
			int itemIndex = indexedRewardItemInfo.ItemIndex;
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemIndex);
			if (data != null)
			{
				if (data.Type == ItemType.Equip)
				{
					list2 = list2 ?? new List<EquipItemInfo>();
					list2.Add(new EquipItemInfo
					{
						ItemIndex = itemIndex,
						Star = indexedRewardItemInfo.Star
					});
				}
				else
				{
					list = list ?? new List<ItemResultInfo>();
					list.Add(new ItemResultInfo
					{
						ItemIndex = itemIndex,
						AddCount = indexedRewardItemInfo.Count
					});
				}
			}
		}
		if (__Extension_Method_ICollection__.COUNT(list2) > 0)
		{
			SetupRewardItems(list2.ToArray());
		}
		if (__Extension_Method_ICollection__.COUNT(list) > 0)
		{
			SetupRewardItems(list.ToArray());
		}
	}

	protected virtual void SetupRewardItems()
	{
		RewardItemIconOrderData[] array = GameRoot.Instance.RewardItemIconOrderDataContainer.GetAllData().ToArray();
		Array.Sort(array, (RewardItemIconOrderData x, RewardItemIconOrderData y) => x.Priority.CompareTo(y.Priority));
		ItemResultInfo[] items = CheckSelected_Item();
		RewardItemIconOrderData[] array2 = array;
		foreach (RewardItemIconOrderData rewardItemIconOrderData in array2)
		{
			switch (rewardItemIconOrderData.RewardItemIconType)
			{
			case RewardItemIconType.Item:
				SetupRewardItems(_popupOpen.itemResultInfos);
				break;
			case RewardItemIconType.Equip:
				SetupRewardItems(_popupOpen.equipResultInfos);
				break;
			case RewardItemIconType.GuildRaidBootyItem:
				SetupRewardItems(_popupOpen.guildRaidBootyItemRewardInfos);
				break;
			case RewardItemIconType.Currency:
			{
				CurrencyResultInfo3[] currencyResultListByType = _popupOpen.GetCurrencyResultListByType(rewardItemIconOrderData.CurrencyType);
				if (currencyResultListByType == null)
				{
					switch (rewardItemIconOrderData.CurrencyType)
					{
					case CurrencyType.Stamina:
						SetupRewardItems(_popupOpen.staminaResultInfos);
						break;
					case CurrencyType.GuildPoint:
						SetupReward(_popupOpen.guildPointResult);
						break;
					case CurrencyType.FriendshipPoint:
						SetupReward(_popupOpen.friendshipPointResult);
						SetupRewardItem(_popupOpen.friendshipPointResults);
						break;
					}
				}
				else
				{
					SetupRewardItems(currencyResultListByType, rewardItemIconOrderData.Sum);
				}
				break;
			}
			}
		}
		foreach (KeyValuePair<CurrencyType, List<CurrencyResultInfo3>> info in _popupOpen.GetCurrencyResultInfos())
		{
			if (!Array.Exists(array, (RewardItemIconOrderData orderData) => orderData.CurrencyType == info.Key))
			{
				SetupRewardItems(info.Value.ToArray(), isSum: true);
			}
		}
		SetupRewardItems(items);
		Reposition();
	}

	private ItemResultInfo[] CheckSelected_Item()
	{
		string[] selectedRewardItemCodes = _popupOpen.selectedRewardItemCodes;
		List<ItemResultInfo> list = new List<ItemResultInfo>();
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(selectedRewardItemCodes))
		{
			List<ItemResultInfo> list2 = _popupOpen.itemResultInfos.ToList();
			int num = 0;
			for (int num2 = list2.Count() - 1; num2 >= 0; num2--)
			{
				ItemResultInfo itemResultInfo = list2[num2];
				ItemData itemData = GameRoot.Instance.ItemDataContainer.GetData(itemResultInfo.ItemIndex);
				if (itemData != null && !String_Extension_Method.IsNullOrEmpty(Array.Find(selectedRewardItemCodes, (string x) => x == itemData.Code)))
				{
					num++;
					list.Add(itemResultInfo);
					list2.Remove(itemResultInfo);
				}
			}
			if (num > 0)
			{
				_popupOpen.SetItemResultInfos(list2.ToArray());
			}
		}
		return list.ToArray();
	}

	public virtual void Reposition()
	{
		UIHelper.EnableGameObject(RewardPopupRoot, __Extension_Method_ICollection__.COUNT(_rewardList) > 0);
		if (__Extension_Method_ICollection__.COUNT(_rewardList) == 0)
		{
			return;
		}
		if (DummyFront == null || DummyRear == null)
		{
			UIHelper.RepositionGrid(RewardItemGrid);
			return;
		}
		int num = RewardItemGrid.Grid.maxPerLine * MaxShowLine;
		Component component = null;
		Component component2 = null;
		if (_rewardList.Count <= num)
		{
			RewardItemGrid.ScrollView.contentPivot = UIWidget.Pivot.Center;
			UIHelper.RepositionGrid(RewardItemGrid);
			component = _rewardList[0];
			component2 = _rewardList[System.Math.Max(_rewardList.Count - 1, 0)];
		}
		else
		{
			RewardItemGrid.ScrollView.contentPivot = UIWidget.Pivot.Top;
			UIHelper.RepositionGrid(RewardItemGrid);
			component = _rewardList[0];
			component2 = _rewardList[System.Math.Max(num - 1, 0)];
		}
		if (DummyFront != null && component != null)
		{
			DummyFront.transform.position = component.transform.position;
		}
		if (DummyRear != null && component2 != null)
		{
			DummyRear.transform.position = component2.transform.position;
		}
	}

	public static CurrencyType GetCurrencyTypeByStaminaType(StaminaType staminaType)
	{
		return EnumAttributeContainer<StaminaType, StaminaTypeAttribute>.Get(staminaType)?.CurrencyType ?? CurrencyType.None;
	}

	public void ShowOnlyReward(bool rewardOnly)
	{
		UIHelper.EnableGameObject(Trs_Title, !rewardOnly);
		if (Spr_Bg != null)
		{
			Spr_Bg.enabled = !rewardOnly;
		}
	}

	public void AddRewardList(Component component)
	{
		_rewardList.Add(component);
	}
}
