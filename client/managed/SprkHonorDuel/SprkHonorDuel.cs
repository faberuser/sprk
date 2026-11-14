using System;
using System.Collections.Generic;
using System.Reflection;
using NGame2.NAccount;
using NGame2.NMatch;
using NGame2.NMatch.NDuelState;
using NGame2.NUI;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NShared.NMessage.NProtocol;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NSingleton;
using UnityEngine;

public static class SprkHonorDuel
{
	public const long GroupOffset = 1099511627776L;

	public const string ButtonName = "SprkHonorDuelButton";

	private static readonly FieldInfo InspectUserField = typeof(InspectUserMenuPopup).GetField("_inspectUser", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly MethodInfo BanPickFail = typeof(MatchManagement).GetMethod("OnFailCheckBanPickCondition", BindingFlags.Static | BindingFlags.NonPublic);

	private static readonly MethodInfo RefreshButton = typeof(InspectUserMenuPopup).GetMethod("RefreshButton", BindingFlags.Instance | BindingFlags.NonPublic);

	private static PvpResult layoutOwner;

	private static Vector3 rootPosition;

	private static Vector3 rootScale;

	private static Quaternion rootRotation;

	private static float winScale = -1f;

	private static float loseScale = -1f;

	private static readonly Dictionary<long, long> canceledInvites = new Dictionary<long, long>();

	private static object submitted;

	public static bool IsHonor(long groupId)
	{
		return groupId > 1099511627776L;
	}

	public static int PvpType(long groupId)
	{
		return IsHonor(groupId) ? 1 : 0;
	}

	public static void SetupAndRefresh(InspectUserMenuPopup menu)
	{
		Setup(menu);
		if (RefreshButton != null)
		{
			RefreshButton.Invoke(menu, null);
		}
	}

	public static void Setup(InspectUserMenuPopup menu)
	{
		if (menu == null || menu.DuelButton == null)
		{
			return;
		}
		UIButton duelButton = menu.DuelButton;
		Rename(duelButton.gameObject, SprkText.Get("Victory Duel"));
		Transform parent = duelButton.transform.parent;
		Transform transform = parent.Find("SprkHonorDuelButton");
		GameObject gameObject;
		if (transform != null)
		{
			gameObject = transform.gameObject;
		}
		else
		{
			gameObject = UnityEngine.Object.Instantiate(duelButton.gameObject, parent, worldPositionStays: false);
			gameObject.name = "SprkHonorDuelButton";
			UIButton component = gameObject.GetComponent<UIButton>();
			if (component != null)
			{
				component.onClick.Clear();
				EventDelegate.Add(component.onClick, delegate
				{
					Request(menu);
				});
			}
		}
		gameObject.transform.SetSiblingIndex(duelButton.transform.GetSiblingIndex() + 1);
		Rename(gameObject, SprkText.Get("Honor Duel"));
		if (gameObject.activeSelf != duelButton.gameObject.activeSelf)
		{
			gameObject.SetActive(duelButton.gameObject.activeSelf);
		}
		UIButton component2 = gameObject.GetComponent<UIButton>();
		if (component2 != null)
		{
			UIHelper.EnableButton(component2, duelButton.isEnabled, grayscaleIfDisable: true);
			SyncLook(duelButton, component2);
		}
		if (gameObject.activeSelf != duelButton.gameObject.activeSelf)
		{
			gameObject.SetActive(duelButton.gameObject.activeSelf);
		}
	}

	private static void SyncLook(UIButton victory, UIButton clone)
	{
		clone.normalSprite = victory.normalSprite;
		clone.hoverSprite = victory.hoverSprite;
		clone.pressedSprite = victory.pressedSprite;
		clone.disabledSprite = victory.disabledSprite;
		clone.defaultColor = victory.defaultColor;
		clone.hover = victory.hover;
		clone.pressed = victory.pressed;
		clone.disabledColor = victory.disabledColor;
		UISprite uISprite = ((victory.tweenTarget != null) ? victory.tweenTarget.GetComponent<UISprite>() : victory.GetComponent<UISprite>());
		UISprite uISprite2 = ((clone.tweenTarget != null) ? clone.tweenTarget.GetComponent<UISprite>() : clone.GetComponent<UISprite>());
		if (uISprite != null && uISprite2 != null)
		{
			uISprite2.atlas = uISprite.atlas;
			uISprite2.spriteName = uISprite.spriteName;
			uISprite2.color = uISprite.color;
			uISprite2.isGrayscale = uISprite.isGrayscale;
		}
	}

	private static bool IsFriendlyMatch()
	{
		if (SingletonUpdater<MatchManager>.isValidInstance)
		{
			return SingletonUpdater<MatchManager>.instance.DuelType == MatchDuelType.Friendly;
		}
		return false;
	}

	public static void OpenHonorWaiting(MatchManagement management)
	{
		if (!(management == null) && !IsFriendlyMatch())
		{
			management.OpenMatchWaiting_BanPickOnly();
		}
	}

	public static void FriendlyResult(PvpResult result)
	{
		if (!(result == null))
		{
			bool flag = IsFriendlyMatch();
			if (result.BottomRoot != null)
			{
				result.BottomRoot.gameObject.SetActive(!flag);
			}
			if (flag && result.RetryButton != null)
			{
				UIHelper.EnableButton(result.RetryButton, enable: false);
			}
		}
	}

	public static int ConfiguredWaitInterval(MatchManager match)
	{
		if (match == null || match.ArenaType == MatchArenaType.Normal || match.DuelType == MatchDuelType.Friendly)
		{
			return 0;
		}
		return 1;
	}

	public static float BanPickRevealDelay()
	{
		if (!IsFriendlyMatch())
		{
			return 5f;
		}
		return 0f;
	}

	private static TransformData EffectTransform(UIGame3DWidget widget)
	{
		if (!(widget == null) && widget.EntryList != null && widget.EntryList.Count != 0)
		{
			return widget.EntryList[0].ExtraTransform;
		}
		return null;
	}

	public static void PrepareResultEffect(PvpResult result)
	{
		if (result == null || result.EffectRoot == null)
		{
			return;
		}
		Transform effectRoot = result.EffectRoot;
		TransformData transformData = EffectTransform(result.EffectWin);
		TransformData transformData2 = EffectTransform(result.EffectLose);
		if ((object)layoutOwner != result)
		{
			layoutOwner = result;
			rootPosition = effectRoot.localPosition;
			rootRotation = effectRoot.localRotation;
			rootScale = effectRoot.localScale;
			winScale = transformData?.Scale ?? (-1f);
			loseScale = transformData2?.Scale ?? (-1f);
		}
		if (IsFriendlyMatch())
		{
			effectRoot.localPosition = Vector3.zero;
			effectRoot.localRotation = Quaternion.identity;
			effectRoot.localScale = Vector3.one;
			if (transformData != null)
			{
				transformData.Scale = 100f;
			}
			if (transformData2 != null)
			{
				transformData2.Scale = 100f;
			}
		}
		else
		{
			effectRoot.localPosition = rootPosition;
			effectRoot.localRotation = rootRotation;
			effectRoot.localScale = rootScale;
			if (transformData != null && winScale >= 0f)
			{
				transformData.Scale = winScale;
			}
			if (transformData2 != null && loseScale >= 0f)
			{
				transformData2.Scale = loseScale;
			}
		}
	}

	public static void NoteCancel(long sender, CancelDuel data)
	{
		if (data != null && SingletonUpdater<DuelManager>.isValidInstance && SingletonUpdater<DuelManager>.instance.DuelState is None)
		{
			canceledInvites[sender] = data.GroupId;
		}
	}

	public static void ForgetCancel(long sender)
	{
		canceledInvites.Remove(sender);
	}

	public static bool WasCanceled(long sender, long groupId)
	{
		if (canceledInvites.TryGetValue(sender, out var value))
		{
			return value == groupId;
		}
		return false;
	}

	public static bool SkipPartyScreen(StateBase state)
	{
		if (state != null)
		{
			if (!IsHonor(state.GroupId))
			{
				return WasCanceled(state.VsPlayerId, state.GroupId);
			}
			return true;
		}
		return false;
	}

	public static string InviteMessage(long groupId)
	{
		return SprkText.Get(IsHonor(groupId) ? "Honor Duel Request sent." : "Victory Duel Request sent.");
	}

	public static void SetInviteMessage(DuelRequestNotify notify, NShared.NMessage.NProtocol.RequestDuel data)
	{
		if (notify == null || data == null)
		{
			return;
		}
		UILabel[] componentsInChildren = notify.GetComponentsInChildren<UILabel>(includeInactive: true);
		foreach (UILabel uILabel in componentsInChildren)
		{
			if (uILabel.name == "Label_Message")
			{
				Rename(uILabel.gameObject, InviteMessage(data.GroupId));
				break;
			}
		}
	}

	private static void Rename(GameObject go, string text)
	{
		UILocalize[] componentsInChildren = go.GetComponentsInChildren<UILocalize>(includeInactive: true);
		foreach (UILocalize obj in componentsInChildren)
		{
			obj.enabled = false;
			UnityEngine.Object.Destroy(obj);
		}
		UILocalizeEx[] componentsInChildren2 = go.GetComponentsInChildren<UILocalizeEx>(includeInactive: true);
		foreach (UILocalizeEx obj2 in componentsInChildren2)
		{
			obj2.enabled = false;
			UnityEngine.Object.Destroy(obj2);
		}
		UILabel[] componentsInChildren3 = go.GetComponentsInChildren<UILabel>(includeInactive: true);
		for (int i = 0; i < componentsInChildren3.Length; i++)
		{
			componentsInChildren3[i].text = text;
		}
	}

	private static void Request(InspectUserMenuPopup menu)
	{
		if (menu == null || InspectUserField == null)
		{
			return;
		}
		object value = InspectUserField.GetValue(menu);
		PropertyInfo propertyInfo = value?.GetType().GetProperty("AccountId");
		if (!(propertyInfo == null))
		{
			long receiverId = (long)propertyInfo.GetValue(value, null);
			if (Eligible(out var _))
			{
				long id = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id;
				SingletonUpdater<DuelManager>.instance.RequestDuel(receiverId, id + 1099511627776L);
				menu.Close();
			}
		}
	}

	private static bool Eligible(out int[] heroes)
	{
		heroes = null;
		Action onFail = ((BanPickFail == null) ? null : ((Action)Delegate.CreateDelegate(typeof(Action), BanPickFail)));
		if (MatchManagement.CheckBanPickCondition(out heroes, onFail) && heroes != null)
		{
			return heroes.Length != 0;
		}
		return false;
	}

	public static bool IsInviter(long groupId)
	{
		if (IsHonor(groupId))
		{
			return groupId - 1099511627776L == GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id;
		}
		return false;
	}

	public static bool ShouldSubmit(string state, long groupId, bool inviter)
	{
		if (IsHonor(groupId))
		{
			if (!(state == "CompleteVsPlayerSetting"))
			{
				return state == "BattlePartySetting" && inviter;
			}
			return true;
		}
		return false;
	}

	public static void SubmitParty(StateBase state)
	{
		if (state == null || submitted == state)
		{
			return;
		}
		long groupId = state.GroupId;
		if (!ShouldSubmit(state.GetType().Name, groupId, IsInviter(groupId)))
		{
			return;
		}
		submitted = state;
		if (!Eligible(out var heroes))
		{
			if (SceneAutoBehaviour<DuelManagement>.isValidInstance)
			{
				SceneAutoBehaviour<DuelManagement>.instance.CancelDuel();
			}
			SingletonUpdater<DuelManager>.instance.CancelCurrentDuel();
		}
		else
		{
			SingletonUpdater<DuelManager>.instance.CompletePartySetting(groupId, heroes, 1);
		}
	}
}
