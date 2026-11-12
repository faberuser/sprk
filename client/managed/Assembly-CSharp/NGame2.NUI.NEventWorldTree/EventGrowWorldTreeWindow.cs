using System;
using System.Collections;
using System.Linq;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NHelper.NEventGrowWorldTree;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NEventWorldTree;

public class EventGrowWorldTreeWindow : BaseCinemaWindow, ItemIconComponent.Listener
{
	public interface IListener
	{
		void OnPutGrowPoint(CurrencyType type, Action<Action> onSuccess);

		void OnClickMissionReward(EventStepType type, Action onSuccess);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/EventGrowWorldTree/EventGrowWorldTreeWindow";

	public Transform EventPointIconRoot;

	public UIWidget WidgetJoinPanel;

	public UIWidget WidgetGroupPanel;

	public UIButton ButtonLeft;

	public UIButton ButtonRight;

	public UIButton ButtonPutGrowPoint;

	public UILabel LabelDailyStep;

	public UILabel LabelDailyPoint;

	public UILabel LabelTotalCount;

	public UILabel LabelJoinCount;

	public UILabel LabelGroupPoint;

	public UILabel LabelGroupTotalPercent;

	public UILabel LabelWorldTreeGrowStep;

	public UILabel LabelHasWorldTreeGrowPoint;

	public UISprite SpriteDailyPointProgressBar;

	public UISprite SpriteDailyPointProgressAnchor;

	public UISprite SpriteJoinPointProgressBar;

	public UISprite SpriteJoinPointProgressAnchor;

	public UITweener JoinHighlightTween;

	public UISprite SpriteGroupPointProgressBar;

	public UISprite SpriteGroupPointProgressAnchor;

	public UITweener GroupHighlightTween;

	public UIProgressBar ProgressBarGroupReward;

	public EventRewardComponent[] RewardComponents;

	public EventRewardComponent JoinMissionRewardComponent;

	public UIGame3DWidget EventPointIconEffect;

	private IListener _listener;

	private DateTime _lastDateTime = DateTime.Now;

	private int _groupRewardMenuState;

	private bool _isShownNotiMessage;

	private const int _groupRewardProgressBarAlignValue = 100;

	private const string WORLD_TREE_PACK = "Ui/WorldTree";

	private const CurrencyType _currencyType = CurrencyType.GrowWorldTreePoint;

	private IconProxy _eventPointIcon;

	public void Open(IListener listener)
	{
		_listener = listener;
		_lastDateTime = EventGrowWorldTreeHelper.GetTime();
		base.OpenWithSceneChanger(delegate
		{
			Init();
			SetWorldTreeIdle();
		}, delegate
		{
			InitWorldTreeNodeTree();
		});
	}

	public void Init()
	{
		JoinHighlightTween.ResetToBeginning();
		GroupHighlightTween.ResetToBeginning();
		Refresh();
	}

	public void Refresh()
	{
		EventStepInfo playerEventStepInfo = EventGrowWorldTreeHelper.GetPlayerEventStepInfo();
		SetDailyInfo();
		bool flag = playerEventStepInfo.MissionStep > 0;
		UIHelper.EnableGameObject(WidgetJoinPanel, !flag);
		UIHelper.EnableGameObject(WidgetGroupPanel, flag);
		if (flag)
		{
			SetGroupInfo(playerEventStepInfo);
		}
		else
		{
			SetJoinInfo();
		}
		SetEtcComponent();
	}

	private void SetDailyInfo()
	{
		int currentStep = EventGrowWorldTreeHelper.GetCurrentStep(EventStepType.Daily);
		int eventStepTypeDataCount = EventGrowWorldTreeHelper.GetEventStepTypeDataCount(EventStepType.Daily);
		int num = currentStep + 1;
		num = ((num >= eventStepTypeDataCount) ? eventStepTypeDataCount : num);
		PlayerCurrencyInfo playerCurrencyInfo = EventGrowWorldTreeHelper.GetPlayerCurrencyInfo(CurrencyType.GrowWorldTreePoint);
		long num2;
		if (playerCurrencyInfo != null)
		{
			num2 = playerCurrencyInfo.Amount;
		}
		else
		{
			int num3 = 0;
			num2 = num3;
		}
		long num4 = num2;
		int targetScore = EventGrowWorldTreeHelper.GetTargetScore(EventStepType.Daily, num);
		long num5 = targetScore;
		int num6 = EventGrowWorldTreeHelper.GetCurrentStep(EventStepType.Group) + 1;
		int eventStepTypeDataCount2 = EventGrowWorldTreeHelper.GetEventStepTypeDataCount(EventStepType.Group);
		num6 = ((num6 >= eventStepTypeDataCount2) ? eventStepTypeDataCount2 : num6);
		string stringDirect = LocalizationManager.GetStringDirect("EVENT_WORLD_TREE_SINGLE_MISSION_POINT", num4, num5);
		string stringDirect2 = LocalizationManager.GetStringDirect("EVENT_WORLD_TREE_SINGLE_MISSION_UPPER_LIMIT", currentStep, eventStepTypeDataCount);
		string stringDirect3 = LocalizationManager.GetStringDirect("EVENT_WORLD_TREE_MULTI_MISSION_STEP", num6);
		UIHelper.SetProgressValue(SpriteDailyPointProgressBar, null, num4, num5);
		SetPositionForAnchor(SpriteDailyPointProgressBar, SpriteDailyPointProgressAnchor, GetGaugeAmount(num4, num5));
		UIHelper.SetLabelText(LabelDailyStep, stringDirect2);
		UIHelper.SetLabelText(LabelDailyPoint, stringDirect);
		UIHelper.SetLabelText(LabelWorldTreeGrowStep, stringDirect3);
		UIHelper.EnableButton(ButtonPutGrowPoint, num4 >= num5);
	}

	private void SetJoinInfo()
	{
		long totalPoint = EventGrowWorldTreeHelper.GetTotalPoint(EventStepType.General);
		int targetScore = EventGrowWorldTreeHelper.GetTargetScore(EventStepType.General, 1);
		long num = targetScore;
		totalPoint = ((totalPoint >= num) ? num : totalPoint);
		string stringDirect = LocalizationManager.GetStringDirect("EVENT_WORLD_TREE_JOIN_MISSION_POINT", totalPoint, num);
		UIHelper.SetLabelText(LabelJoinCount, stringDirect);
		UIHelper.SetProgressValue(SpriteJoinPointProgressBar, null, totalPoint, num);
		SetPositionForAnchor(SpriteJoinPointProgressBar, SpriteJoinPointProgressAnchor, GetGaugeAmount(totalPoint, num));
		SetJoinMissionReward(totalPoint >= num);
	}

	private void SetJoinMissionReward(bool isButtonEnabled)
	{
		EventStepData eventStepData = EventGrowWorldTreeHelper.GetEventStepData(EventStepType.General, 1);
		if (eventStepData != null)
		{
			JoinMissionRewardComponent.Open(EventStepType.General, eventStepData.RewardIndex, 150, _listener);
			JoinMissionRewardComponent.SetEnabled(isEnabled: true, isButtonEnabled);
		}
	}

	private void SetGroupInfo(EventStepInfo eventInfo)
	{
		int currentStep = EventGrowWorldTreeHelper.GetCurrentStep(EventStepType.Group);
		int eventStepTypeDataCount = EventGrowWorldTreeHelper.GetEventStepTypeDataCount(EventStepType.Group);
		int groupRemainScore = eventInfo.GroupRemainScore;
		long num = groupRemainScore;
		int groupTargetScore = eventInfo.GroupTargetScore;
		long num2 = groupTargetScore;
		bool flag = currentStep >= eventStepTypeDataCount;
		float num3 = (flag ? 1f : GetGaugeAmount(num, num2));
		float num4 = num3 * 100f;
		string text = (flag ? $"({num}/MAX)" : $"({num}/{num2})");
		string stringDirect = LocalizationManager.GetStringDirect("EVENT_WORLD_TREE_MULTI_MISSION_POINT", eventInfo.TotalCount);
		string stringDirect2 = LocalizationManager.GetStringDirect("EVENT_WORLD_TREE_MULTI_MISSION_ACHIEVEMENT_RATE", $"{num4:0.##}");
		UIHelper.SetLabelText(LabelGroupPoint, text);
		UIHelper.SetLabelText(LabelTotalCount, stringDirect);
		UIHelper.SetLabelText(LabelGroupTotalPercent, stringDirect2);
		UISprite spriteGroupPointProgressBar = SpriteGroupPointProgressBar;
		long num5;
		if (!flag)
		{
			num5 = num;
		}
		else
		{
			int num6 = 1;
			num5 = num6;
		}
		long value = num5;
		long num7;
		if (!flag)
		{
			num7 = num2;
		}
		else
		{
			int num8 = 1;
			num7 = num8;
		}
		UIHelper.SetProgressValue(spriteGroupPointProgressBar, null, value, num7);
		SetPositionForAnchor(SpriteGroupPointProgressBar, SpriteGroupPointProgressAnchor, num3);
		SetGroupRewardItemList(isForcedListUpdate: true);
	}

	private void SetEtcComponent()
	{
		long growWorldTreeEventCoin = EventGrowWorldTreeHelper.GetGrowWorldTreeEventCoin();
		OpenCurrencyInfoComp();
		UIHelper.SetLabelText(LabelHasWorldTreeGrowPoint, growWorldTreeEventCoin);
	}

	private void OpenCurrencyInfoComp()
	{
		if (_eventPointIcon == null)
		{
			_eventPointIcon = IconProxy.PrefabAuto.Factory.Create(EventPointIconRoot);
		}
		_eventPointIcon.SetupCurrencyNoBg(CurrencyType.GrowWorldTreePoint);
		_eventPointIcon.SetDepth(150);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_eventPointIcon.transform);
		if (EventPointIconEffect != null && !EventPointIconEffect.IsPlaying)
		{
			EventPointIconEffect.PlayInstantEffect("UI.MainLobby.GoldGlare", new TransformData(100f), null);
		}
	}

	private void SetPositionForAnchor(UISprite sprParent, UISprite sprAnchor, float amount)
	{
		bool flag = amount == 1f;
		UIHelper.EnableGameObject(sprAnchor, !flag);
		if (!flag)
		{
			Vector3[] worldCorners = sprParent.worldCorners;
			float x = worldCorners[0].x;
			float x2 = worldCorners[2].x;
			Vector3 position = sprAnchor.transform.position;
			sprAnchor.transform.position = Vespa_Unity_Vector3_Extension_Method.ReplaceX(position, Mathf.Lerp(x, x2, amount));
		}
	}

	private void SetGroupRewardItemList(bool isForcedListUpdate)
	{
		EventStepInfo playerEventStepInfo = EventGrowWorldTreeHelper.GetPlayerEventStepInfo();
		if (isForcedListUpdate)
		{
			_groupRewardMenuState = ((playerEventStepInfo.GroupTotalStep >= RewardComponents.Length) ? 1 : 0);
		}
		int num = RewardComponents.Length;
		int num2 = ((_groupRewardMenuState == 1) ? num : 0);
		int currentStep = EventGrowWorldTreeHelper.GetCurrentStep(EventStepType.Group);
		int eventStepTypeDataCount = EventGrowWorldTreeHelper.GetEventStepTypeDataCount(EventStepType.Group);
		int num3 = ((playerEventStepInfo.GroupTotalStep >= eventStepTypeDataCount) ? eventStepTypeDataCount : playerEventStepInfo.GroupTotalStep);
		for (int i = 0; i < num; i++)
		{
			EventStepData eventStepData = EventGrowWorldTreeHelper.GetEventStepData(EventStepType.Group, num2 + 1);
			EventRewardComponent obj = RewardComponents[i];
			obj.Open(EventStepType.Group, eventStepData.RewardIndex, 150, _listener);
			obj.SetEnabled(num2 >= num3, currentStep > num3 && num2 == num3);
			num2++;
		}
		int num4 = num3 - _groupRewardMenuState * num;
		num4 = ((num4 >= 0) ? num4 : 0);
		SetGroupRewardGaugeUI(num4);
		UIHelper.EnableButton(ButtonLeft, _groupRewardMenuState == 1);
		UIHelper.EnableButton(ButtonRight, _groupRewardMenuState == 0);
	}

	private void SetGroupRewardGaugeUI(int step)
	{
		if (!(RewardComponents.FirstOrDefault() == null))
		{
			int width = ProgressBarGroupReward.backgroundWidget.width;
			EventRewardComponent eventRewardComponent = RewardComponents.FirstOrDefault();
			float x = eventRewardComponent.IconRoot.localScale.x;
			UISprite componentInChildren = eventRewardComponent.GetComponentInChildren<UISprite>();
			if (!(componentInChildren == null))
			{
				float num = (float)componentInChildren.width * x / 2f;
				float num2 = 100f;
				float num3 = num + num2 * (float)step;
				ProgressBarGroupReward.value = num3 / (float)width;
			}
		}
	}

	private void InitWorldTreeNodeTree()
	{
		if (CompareWorldTreeGrowStep())
		{
			PlayWorldTreeGrow(isInit: true);
		}
	}

	private bool CompareWorldTreeGrowStep()
	{
		int prevWorldTreeStep = SceneAutoBehaviour<EventGrowWorldTreeManagement>.instance.PrevWorldTreeStep;
		int currentStep = EventGrowWorldTreeHelper.GetCurrentStep(EventStepType.Group);
		if (prevWorldTreeStep == currentStep)
		{
			return false;
		}
		SceneAutoBehaviour<EventGrowWorldTreeManagement>.instance.PrevWorldTreeStep = currentStep;
		EventStepData eventStepData = GameRoot.Instance.EventStepDataContainer.GetEventStepData(CurrencyType.GrowWorldTreePoint, EventStepType.Group, currentStep + 1);
		string text = ((eventStepData == null) ? string.Empty : eventStepData.WorldTreeIdleName);
		if (String_Extension_Method.IsNullOrEmpty(text))
		{
			return false;
		}
		EventStepData eventStepData2 = GameRoot.Instance.EventStepDataContainer.GetEventStepData(CurrencyType.GrowWorldTreePoint, EventStepType.Group, prevWorldTreeStep + 1);
		if (((eventStepData2 == null) ? string.Empty : eventStepData2.WorldTreeIdleName) == text)
		{
			return false;
		}
		return true;
	}

	private void SetWorldTreeIdle(Action onEnd = null)
	{
		string nodeTreePack = "Ui/WorldTree";
		string worldTreeIdleName = EventGrowWorldTreeHelper.GetCurrentStepData(EventStepType.Group).WorldTreeIdleName;
		SetNextCienmaNodeTree(isOncePlay: false, nodeTreePack, worldTreeIdleName, delegate
		{
		}, delegate
		{
		});
	}

	private void PlayWorldTreeGrow(bool isInit = false, Action onEnd = null)
	{
		VespaCoroutine.Start(this, coPlayGrow(isInit, onEnd));
	}

	private IEnumerator coPlayGrow(bool isInit, Action onEnd)
	{
		if (isInit)
		{
			yield return new WaitForSeconds(1f);
		}
		bool executeEnd = false;
		string nodeTreePack = "Ui/WorldTree";
		string worldTreeGrowName = EventGrowWorldTreeHelper.GetCurrentStepData(EventStepType.Group).WorldTreeGrowName;
		SetNextCienmaNodeTree(isOncePlay: false, nodeTreePack, worldTreeGrowName, delegate
		{
		}, delegate
		{
			executeEnd = true;
		});
		while (!executeEnd)
		{
			yield return null;
		}
		Vespa_System_Action_Extension_Method.fire(onEnd);
	}

	private void PlayGetRewardAnim(Action onEnd)
	{
		VespaCoroutine.Start(this, coGetRewardAnim(onEnd));
	}

	private IEnumerator coGetRewardAnim(Action onEnd)
	{
		GlobalTopmostWindow.instance.AddTouchDisabler(this);
		bool executeEnd = false;
		string nodeTreePack = "Ui/WorldTree";
		string getRewardName = EventGrowWorldTreeHelper.GetCurrentStepData(EventStepType.Group).GetRewardName;
		SetNextCienmaNodeTree(isOncePlay: false, nodeTreePack, getRewardName, delegate
		{
		}, delegate
		{
			executeEnd = true;
		});
		while (!executeEnd)
		{
			yield return null;
		}
		yield return new WaitForSeconds(0.1f);
		JoinHighlightTween.PlayForwardReset();
		GroupHighlightTween.PlayForwardReset();
		Vespa_System_Action_Extension_Method.fire(onEnd);
		GlobalTopmostWindow.instance.RemoveTouchDisabler(this);
	}

	private float GetGaugeAmount(long curPoint, long maxPoint)
	{
		if (!((float)curPoint / (float)maxPoint >= 1f))
		{
			return (float)curPoint / (float)maxPoint;
		}
		return 1f;
	}

	public void OnPutGrowPoint()
	{
		if (!EventGrowWorldTreeHelper.IsOpenContents())
		{
			EventGrowWorldTreeHelper.ForceCloseEventWindow("", delegate
			{
				OnSystemBackButton();
			});
		}
		else
		{
			if (_listener == null)
			{
				return;
			}
			_listener.OnPutGrowPoint(CurrencyType.GrowWorldTreePoint, delegate(Action openRewardPopup)
			{
				PlayGetRewardAnim(delegate
				{
					if (!CompareWorldTreeGrowStep())
					{
						Refresh();
						Vespa_System_Action_Extension_Method.fire(openRewardPopup);
					}
					else
					{
						_ = GlobalAutoBehaviour<AccountManager>.instance.EventGrowWorldTreeManager.PlayerEventStepInfo;
						PlayWorldTreeGrow(isInit: false, delegate
						{
							Refresh();
							Vespa_System_Action_Extension_Method.fire(openRewardPopup);
						});
					}
				});
			});
		}
	}

	public void OnClickGroupMissionReward(int index)
	{
		if (!EventGrowWorldTreeHelper.IsOpenContents())
		{
			EventGrowWorldTreeHelper.ForceCloseEventWindow("", delegate
			{
				OnSystemBackButton();
			});
		}
		else if (_listener != null)
		{
			_listener.OnClickMissionReward(EventStepType.Group, delegate
			{
				Refresh();
			});
		}
	}

	public void OnClickLeft()
	{
		if (_groupRewardMenuState != 0)
		{
			_groupRewardMenuState = 0;
			SetGroupRewardItemList(isForcedListUpdate: false);
		}
	}

	public void OnClickRight()
	{
		if (_groupRewardMenuState != 1)
		{
			_groupRewardMenuState = 1;
			SetGroupRewardItemList(isForcedListUpdate: false);
		}
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent comp)
	{
		if (!EventGrowWorldTreeHelper.IsOpenContents())
		{
			EventGrowWorldTreeHelper.ForceCloseEventWindow("", delegate
			{
				OnSystemBackButton();
			});
		}
		else if (_listener != null)
		{
			_listener.OnClickMissionReward(EventStepType.General, delegate
			{
				Refresh();
			});
		}
	}

	public override void OnClosing()
	{
		base.OnClosing();
		SceneAutoBehaviour<WorldMapManagement>.instance.UpdateEventWorldTreeIndicator();
	}

	public override void Reset()
	{
		base.Reset();
		ReleaseRewards();
		FactoryBase.SafeReleaseComponent(ref _eventPointIcon);
	}

	private void ReleaseRewards()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(RewardComponents); i++)
		{
			RewardComponents[i].Release();
		}
		JoinMissionRewardComponent.Release();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		DateTime time = EventGrowWorldTreeHelper.GetTime();
		if (!_isShownNotiMessage && time.Day > _lastDateTime.Day)
		{
			_isShownNotiMessage = true;
			string stringDirect = LocalizationManager.GetStringDirect("EVENT_GROW_WORLD_TREE_TITLE");
			string stringDirect2 = LocalizationManager.GetStringDirect("EVENT_WORLD_TREE_DAILY_POINT_RESET_NOTI_MESSAGE");
			GlobalMessagePopup.OpenGlobal(stringDirect, stringDirect2, delegate
			{
				OnSystemBackButton();
				_isShownNotiMessage = false;
			}, retryButton: false);
		}
		_lastDateTime = time;
	}
}
