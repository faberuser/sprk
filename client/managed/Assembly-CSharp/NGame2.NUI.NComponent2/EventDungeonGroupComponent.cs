using System;
using System.Collections;
using System.Linq;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class EventDungeonGroupComponent : UIScrollItemBehaviour, InitOnCreate
{
	public interface Listener
	{
		void OnClickEnter(EventDungeonGroupComponent caller);
	}

	public const string PrefabPath = "Prefab2/Component/EventDungeon/EventDungeonGroupComponent";

	public static PrefabAuto<EventDungeonGroupComponent> PrefabAuto = new PrefabAuto<EventDungeonGroupComponent>("Prefab2/Component/EventDungeon/EventDungeonGroupComponent");

	public UILabel NameLabel;

	public UILabel DescLabel;

	public UITexture ImageTexture;

	public UIButton ButtonEnter;

	public Transform OpenedRoot;

	public Transform ClosedRoot;

	public Transform LockSprite;

	public Animator OpenAnimator;

	public EventDugeonEnterCount EventDungeonEnterCount;

	private Listener _listener;

	private EventDungeonGroupData _eventDungeonGroupData;

	private bool _openConditionCheck;

	private bool _isPlayingEffect;

	private LocalAccountBool _isPlayOpenedEffect;

	private DateTime _lastUpdateTime;

	private const string EVENT_DUNGEON_OPEN_ANIM_NAME = "EventDungeonOpenAni";

	public EventDungeonGroupData EventDungeonGroupData => _eventDungeonGroupData;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}

	public void ChainUpdate()
	{
		if (_openConditionCheck && !_isPlayingEffect && (DateTime.Now - _lastUpdateTime).TotalSeconds > 1.0)
		{
			_lastUpdateTime = DateTime.Now;
			RefreshState();
		}
	}

	public void Open(Listener listener, EventDungeonGroupData eventDungeonGroupData)
	{
		_listener = listener;
		_eventDungeonGroupData = eventDungeonGroupData;
		_openConditionCheck = false;
		_isPlayingEffect = false;
		bool i_default = _eventDungeonGroupData.GroupType == EDungeonGroupType.Story;
		_isPlayOpenedEffect = new LocalAccountBool(i_default, "IsPlayOpenedEffect_" + _eventDungeonGroupData.ChapterIndex + "_" + _eventDungeonGroupData.GroupIndex);
		Init();
	}

	private void Init()
	{
		InitAnim();
		SetLabel();
		SetImage();
		if (_eventDungeonGroupData != null)
		{
			EventDungeonEnterCount.Init(_eventDungeonGroupData.ChapterIndex, _eventDungeonGroupData.DungeonIndices);
		}
	}

	private void InitAnim()
	{
		UIHelper.EnableGameObject(OpenAnimator, enable: false);
	}

	private void SetLabel()
	{
		string stringDirect = LocalizationManager.GetStringDirect(_eventDungeonGroupData.DungeonName);
		UIHelper.SetLabelText(NameLabel, stringDirect);
		string stringDirect2 = LocalizationManager.GetStringDirect(_eventDungeonGroupData.DungeonDesc);
		UIHelper.SetLabelText(DescLabel, stringDirect2);
	}

	private void SetImage()
	{
		AssetTexture plainTexture = UIHelper.GetPlainTexture(_eventDungeonGroupData.PortraitPath);
		UIHelper.ResetPlaneAssetTextureAsync(this, ImageTexture, plainTexture, UIHelper.EAssetTextureAssign.None, delegate
		{
			RefreshState();
			_openConditionCheck = true;
		});
	}

	public void RefreshState()
	{
		bool flag = CheckedGroupOpenCondition();
		if (_isPlayOpenedEffect != null && _isPlayOpenedEffect.Value)
		{
			SetState(flag);
		}
		else
		{
			PlayOpenedEffect(flag, SetState);
		}
	}

	private void SetState(bool isOpened)
	{
		UIHelper.EnableButton(ButtonEnter, isOpened, grayscaleIfDisable: true);
		UIHelper.EnableGameObject(OpenedRoot, isOpened);
		UIHelper.SetWidgetGrayScale(ImageTexture, !isOpened);
		UIHelper.EnableGameObject(LockSprite, !isOpened);
		UIHelper.EnableGameObject(ClosedRoot, !isOpened);
	}

	public void PlayOpenedEffect(bool isOpened, Action<bool> onComplete)
	{
		if (!isOpened)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete, isOpened);
			return;
		}
		if (_isPlayOpenedEffect != null)
		{
			_isPlayOpenedEffect.Value = true;
		}
		PlayOpenAnim(delegate
		{
			Vespa_System_Action_Extension_Method.fire(onComplete, isOpened);
		});
	}

	private void PlayOpenAnim(Action onComplete)
	{
		ExtensionMethod_VespaCoroutine.StartCoroutineThis(this, coPlayAnimation(OpenAnimator, "EventDungeonOpenAni", onComplete));
	}

	private IEnumerator coPlayAnimation(Animator animator, string animationName, Action onComplete = null)
	{
		if (animator == null)
		{
			VespaDebug.LogError("coPlayAnimation :: animator is null");
			yield break;
		}
		_isPlayingEffect = true;
		UIHelper.EnableGameObject(LockSprite, enable: false);
		UIHelper.EnableGameObject(animator, enable: true);
		animator.Play(animationName, 0);
		yield return new WaitForSeconds(0.1f);
		AnimatorClipInfo[] currentAnimatorClipInfo = animator.GetCurrentAnimatorClipInfo(0);
		if (currentAnimatorClipInfo != null)
		{
			float length = currentAnimatorClipInfo[0].clip.length;
			yield return new WaitForSeconds(length);
			_isPlayingEffect = false;
			UIHelper.EnableGameObject(animator, enable: false);
			Vespa_System_Action_Extension_Method.fire(onComplete);
		}
	}

	private EventDungeonData GetFirstDungeonGroupData()
	{
		if (_eventDungeonGroupData == null)
		{
			return null;
		}
		int chapterIndex = _eventDungeonGroupData.ChapterIndex;
		int dungeonIndex = _eventDungeonGroupData.DungeonIndices.FirstOrDefault();
		return GameRoot.Instance.EventDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
	}

	private bool CheckedGroupOpenCondition()
	{
		if (_eventDungeonGroupData == null)
		{
			return false;
		}
		if (_eventDungeonGroupData.GroupType == EDungeonGroupType.Story)
		{
			return true;
		}
		EventDungeonData firstDungeonGroupData = GetFirstDungeonGroupData();
		if (firstDungeonGroupData == null)
		{
			return false;
		}
		string notOpenText = "";
		return UIHelper.CheckEventDungeonOpenCondition(firstDungeonGroupData, out notOpenText);
	}

	public void OnClickEnter()
	{
		if (_listener != null)
		{
			_listener.OnClickEnter(this);
		}
	}
}
