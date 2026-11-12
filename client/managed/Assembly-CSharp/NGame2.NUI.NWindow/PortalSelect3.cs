using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NUI.NComponent2;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class PortalSelect3 : BaseWindow, PortalComponent2.Listener, LeftMenuMissionProgress.Listner
{
	public interface Listener
	{
		void MoveMap(int ChapterIndex, int DungeonIndex);

		void ClickMoveLastBattlePos();

		void OpenContentsShortCut(int chapterIndex, int dungeonIndex);

		void OpenPopup(int chapterIndex, int dungeonIndex);

		void OnClickQuestButton();

		void OnClickMissionButton(MissionComponentData data);
	}

	public interface IDataProvider
	{
		bool CanGoToLatelyBattleNode();
	}

	private struct DragInfo
	{
		public int TouchId;

		public Vector2 CurrentPos;

		public Vector2 Delta;
	}

	private class ChapterInformation
	{
		public int x { get; set; }

		public int y { get; set; }

		public int ChapterIdx { get; private set; }

		public string PopupImagePath { get; set; }

		public ChapterInformation(PortalSelect3 _parent, PortalData portalData)
		{
			x = -portalData.PositionX + _parent.SpriteWidget.width / 2;
			y = portalData.PositionY - _parent.SpriteWidget.height / 2;
			ChapterIdx = portalData.ChapterIndex;
			PopupImagePath = portalData.PopupImagePath;
		}
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Portal/PortalSelect3";

	public UIWidget SpriteWidget;

	public UIWidget RootPanel;

	public Transform MapRoot;

	public UIButton Button_Back;

	public UIGame3DWidget EffectWidget;

	public float ZoomInLimit;

	public float ZoomOutLimit;

	public UISimpleGrid ItemGrid;

	public GameObject EclipseKeyGaugeRoot;

	public LeftMenuMissionProgress MissionProgress;

	private EclipseKeyGaugeComponent _eclipseKeyGauge;

	private Listener _listener;

	private IDataProvider _dataProvider;

	private List<PortalComponent2> _ListPortalComponent = new List<PortalComponent2>();

	private List<PortalComponent2> _ListWoldMapPortalComponent = new List<PortalComponent2>();

	private List<ChapterInformation> _ListChapterInfo = new List<ChapterInformation>();

	private bool _dragging;

	private IncreaseDic<int, DragInfo> _dragDic = new IncreaseDic<int, DragInfo>();

	private static LocalAccountBoolArray _openPortalEffectIndecies = new LocalAccountBoolArray(i_default: false, "OpenPortalEffectIndecies");

	private bool IsCameraMoving;

	private bool IsEffectPlaying;

	public static bool GetOpenPortalEffectEnable(int index)
	{
		return _openPortalEffectIndecies[index];
	}

	public static void SetOpenPortalEffect(int index, bool enable)
	{
		_openPortalEffectIndecies[index] = enable;
	}

	public void Open(Listener listener, IDataProvider dataProvider)
	{
		Open(delegate
		{
			Init(listener, dataProvider);
		}, null);
	}

	public void Init(Listener listener, IDataProvider dataProvider)
	{
		_listener = listener;
		_dataProvider = dataProvider;
		RootPanel.ResetAndUpdateAnchors();
		SetupPortalList();
		SetupPortalProp();
		if (MissionProgress != null)
		{
			MissionProgress.Init(this);
		}
		if (ZoomInLimit < 1f)
		{
			ZoomInLimit = 1f;
		}
		if (ZoomOutLimit > 1f)
		{
			ZoomOutLimit = 1f;
		}
		Vector2 localSize = RootPanel.localSize;
		if ((float)SpriteWidget.width * ZoomOutLimit < localSize.x)
		{
			ZoomOutLimit = localSize.x / (float)SpriteWidget.width;
		}
		if ((float)SpriteWidget.height * ZoomOutLimit < localSize.y)
		{
			ZoomOutLimit = localSize.y / (float)SpriteWidget.height;
		}
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		SetZoomRate(ZoomInLimit);
		UpdateCenter(chapterIndex);
		ItemGrid.Grid.sorting = UIGrid.Sorting.Custom;
		ItemGrid.Grid.onCustomSort = SortButton;
		ItemGrid.RepositionGrid();
		CheckPortalOpenEffect();
		InitEclipseKeyGauge();
	}

	public void InitEclipseKeyGauge()
	{
		if (EclipseKeyGaugeComponent.ShowEclipseKeyGauge())
		{
			if (_eclipseKeyGauge != null)
			{
				FactoryBase.SafeReleaseComponent(ref _eclipseKeyGauge);
			}
			_eclipseKeyGauge = EclipseKeyGaugeComponent.PrefabAuto.Factory.Create(EclipseKeyGaugeRoot.transform);
			long stamina = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetStamina(StaminaType.EclipseKey);
			long currencyValue = GlobalAutoBehaviour<AccountManager>.instance.PlayerCurrencyManager.GetCurrencyValue(UserManager.UserStatType.EclipsePoint);
			CurrencyResultInfo3 currencyResultInfo = new CurrencyResultInfo3();
			currencyResultInfo.AddValue = 0L;
			currencyResultInfo.NewValue = currencyValue;
			currencyResultInfo.CurrencyType = CurrencyType.EclipsePoint;
			_eclipseKeyGauge.Init(stamina, currencyResultInfo, playFillAnimation: false);
		}
	}

	public void CheckPortalOpenEffect()
	{
		for (int i = 0; i < _ListWoldMapPortalComponent.Count; i++)
		{
			if (_openPortalEffectIndecies[i])
			{
				_openPortalEffectIndecies[i] = false;
				VespaCoroutine.Start(coMoveToPortalComponentCenter(i, 0.5f, playEffect: true, 0, 0, CheckPortalOpenEffect));
				break;
			}
		}
	}

	private void SetupPortalList()
	{
		foreach (PortalData allDatum in GameRoot.Instance.PortalDataContainer.GetAllData())
		{
			if (allDatum.Hidden && !Helper.CheckDropBonusEventNode(allDatum.ChapterIndex, allDatum.DungeonIndex))
			{
				continue;
			}
			if (allDatum.DungeonIndex == -1)
			{
				ChapterInformation item = new ChapterInformation(this, allDatum);
				_ListChapterInfo.Add(item);
			}
			else if (!allDatum.hasParent())
			{
				int chapterIndex = allDatum.ChapterIndex;
				int dungeonIndex = allDatum.DungeonIndex;
				if (allDatum.ShortCutButton)
				{
					PortalComponent2 portalComponent = UIHelper.AddGridItem(ItemGrid, PortalComponent2.PrefabAutoButton);
					portalComponent.OpenButton(this, chapterIndex, dungeonIndex, allDatum.Name, allDatum.ImagePath);
					_ListPortalComponent.Add(portalComponent);
				}
				else
				{
					PortalComponent2 portalComponent = PortalComponent2.PrefabAutoLarge.Factory.Create(MapRoot);
					portalComponent.Open(this, chapterIndex, dungeonIndex, allDatum.Name, allDatum.ImagePath, SpriteWidget.width, SpriteWidget.height, _ListWoldMapPortalComponent.Count, allDatum.PortalGaugeCurrencyType, allDatum.PortalGaugePath);
					_ListWoldMapPortalComponent.Add(portalComponent);
				}
			}
		}
	}

	public bool CheckPropDataCondition(PortalPropData propData)
	{
		if (propData.ConditionType == PortalPropConditionType.BonusEvent)
		{
			EventBonusType valueEnum = ArgParser.GetValueEnum(propData.ConditionArgs, 0, EventBonusType.None);
			string valueString = ArgParser.GetValueString(propData.ConditionArgs, 1);
			if (GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(valueEnum, valueString) != null)
			{
				return true;
			}
		}
		return false;
	}

	private void SetupPortalProp()
	{
		List<PortalPropData> propDataOfPortal = GameRoot.Instance.PortalPropDataContainer.GetPropDataOfPortal();
		for (int i = 0; i < propDataOfPortal.Count; i++)
		{
			PortalPropData portalPropData = propDataOfPortal[i];
			if (portalPropData == null || !CheckPropDataCondition(portalPropData))
			{
				continue;
			}
			if (portalPropData.PropType == PortalPropType.Prefab)
			{
				string propName = portalPropData.PropName;
				GameObject gameObject = VespaResource.Load<GameObject>(propName);
				if (gameObject == null)
				{
					VespaDebug.LogError("Prop prefab not found : " + propName);
				}
				else if (UnityEngine.Object.Instantiate(gameObject) == null)
				{
					VespaDebug.LogError("Instantiated propObject is null : " + propName);
				}
			}
			else if (portalPropData.PropType == PortalPropType.Effect)
			{
				TransformData transformData = new TransformData();
				transformData.Position = new Vector3(portalPropData.PositionX, portalPropData.PositionY, 0f);
				transformData.Scale = portalPropData.Scale;
				EffectWidget.PlayInstantEffect(portalPropData.PropName, transformData, null);
			}
		}
	}

	public override void Reset()
	{
		FactoryBase.ReleaseAndClearList(_ListPortalComponent);
		_ListChapterInfo.Clear();
		FactoryBase.ReleaseAndClearList(_ListWoldMapPortalComponent);
		_ListWoldMapPortalComponent.Clear();
		_listener = null;
		_dataProvider = null;
		if (MissionProgress != null)
		{
			MissionProgress.Clear();
		}
	}

	public void OnClickBack()
	{
		Close();
	}

	public void OnClickBackground()
	{
	}

	private float CalcZoomInValue(Vector2 delta, Vector2 position, Vector2 pivot)
	{
		if (delta.magnitude < Mathf.Epsilon)
		{
			return 0f;
		}
		if (Vector2.Dot(pivot - position, delta) < 0f)
		{
			return 0f - delta.magnitude;
		}
		return delta.magnitude;
	}

	private void GetTranslateLimit(out int transX, out int transY)
	{
		Vector2 localSize = RootPanel.localSize;
		transX = (int)Mathf.Abs((MapRoot.localScale.x * (float)SpriteWidget.width - localSize.x) / 2f);
		transY = (int)Mathf.Abs((MapRoot.localScale.y * (float)SpriteWidget.height - localSize.y) / 2f);
	}

	private void SetMapRootPosition(Vector3 position)
	{
		GetTranslateLimit(out var transX, out var transY);
		position.x = Mathf.Clamp(position.x, -transX, transX);
		position.y = Mathf.Clamp(position.y, -transY, transY);
		MapRoot.localPosition = position;
	}

	private void DragPortalMap(Vector2 delta)
	{
		float num = RootPanel.localSize.x / (float)Screen.width;
		Vector3 localPosition = MapRoot.localPosition;
		localPosition.x += delta.x * num;
		localPosition.y += delta.y * num;
		SetMapRootPosition(localPosition);
	}

	private void SetZoomRate(float zoomRate)
	{
		Vector3 localScale = MapRoot.localScale;
		zoomRate = Mathf.Clamp(zoomRate, ZoomOutLimit, ZoomInLimit);
		localScale.x = (localScale.y = zoomRate);
		MapRoot.localScale = localScale;
		Vector3 localPosition = MapRoot.localPosition;
		SetMapRootPosition(localPosition);
	}

	private void ZoomPortalMap(float zoomInValue)
	{
		Vector3 localScale = MapRoot.localScale;
		float p = (0f - zoomInValue) / (float)Screen.height;
		p = localScale.x * Mathf.Pow(2f, p);
		SetZoomRate(p);
	}

	public void StartDrag()
	{
		_dragging = true;
		DragInfo value = new DragInfo
		{
			TouchId = UICamera.currentTouchID,
			CurrentPos = UICamera.currentTouch.pos,
			Delta = UICamera.currentTouch.delta
		};
		_dragDic.AddAndOverride(value.TouchId, value);
	}

	public void OnDrag()
	{
		if (_dragging && _dragDic.ContainsKey(UICamera.currentTouchID))
		{
			DragInfo value = new DragInfo
			{
				TouchId = UICamera.currentTouchID,
				CurrentPos = UICamera.currentTouch.pos,
				Delta = UICamera.currentTouch.delta
			};
			_dragDic.AddAndOverride(value.TouchId, value);
		}
	}

	public void EndDrag()
	{
		_dragging = false;
		_dragDic.Clear();
	}

	public PortalComponent2 GetWorldMapPortalComponent(int componentIndex)
	{
		if (componentIndex >= 0 && componentIndex < __Extension_Method_ICollection__.COUNT(_ListWoldMapPortalComponent))
		{
			return _ListWoldMapPortalComponent[componentIndex];
		}
		return null;
	}

	public void MoveToPortalComponent(int componentIndex, float totalMovingTime, bool playEffect = false, int offsetX = 0, int offsetY = 0, Action onComplete = null)
	{
		VespaCoroutine.StopAll(this);
		VespaCoroutine.Start(coMoveToPortalComponentCenter(componentIndex, totalMovingTime, playEffect, offsetX, offsetY, onComplete));
	}

	private IEnumerator coMoveToPortalComponentCenter(int componentIndex, float totalMovingTime, bool playEffect = false, int offsetX = 0, int offsetY = 0, Action onComplete = null)
	{
		PortalComponent2 component = GetWorldMapPortalComponent(componentIndex);
		if (component == null)
		{
			yield break;
		}
		IsCameraMoving = true;
		float elapsedTime = 0f;
		Vector3 localScale = MapRoot.localScale;
		Vector2 originPos = new Vector2(MapRoot.localPosition.x / localScale.x, MapRoot.localPosition.y / localScale.y);
		int num = -component.PortalData.PositionX + offsetX + SpriteWidget.width / 2;
		int num2 = component.PortalData.PositionY + offsetY - SpriteWidget.height / 2;
		Vector2 destPos = new Vector2(num, num2);
		if (totalMovingTime > 0f)
		{
			while (elapsedTime < totalMovingTime)
			{
				elapsedTime += Time.deltaTime;
				Vector2 vector = Vector2.Lerp(originPos, destPos, elapsedTime / totalMovingTime);
				UpdateCenter((int)vector.x, (int)vector.y);
				yield return null;
			}
		}
		else
		{
			UpdateCenter((int)destPos.x, (int)destPos.y);
		}
		IsCameraMoving = false;
		if (playEffect)
		{
			yield return VespaCoroutine.Start(coPlayToPortalComponentEffect(component, onComplete));
		}
		else
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
		}
	}

	private IEnumerator coPlayToPortalComponentEffect(PortalComponent2 component, Action onComplete = null)
	{
		Animator animator = component.GetComponentInChildren<Animator>();
		if (animator != null)
		{
			animator.enabled = true;
			animator.Play("Anim_Portal", 0);
			IsEffectPlaying = true;
			yield return new WaitForSeconds(0.1f);
			AnimatorClipInfo[] currentAnimatorClipInfo = animator.GetCurrentAnimatorClipInfo(0);
			if (__Extension_Method_ICollection__.COUNT(currentAnimatorClipInfo) == 0)
			{
				yield break;
			}
			float length = currentAnimatorClipInfo[0].clip.length;
			yield return new WaitForSeconds(length);
			animator.enabled = false;
		}
		Vespa_System_Action_Extension_Method.fire(onComplete);
		IsEffectPlaying = false;
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (!IsCameraMoving && !IsEffectPlaying && _dragging && _dragDic.Count != 0)
		{
			if (_dragDic.Count == 1)
			{
				DragPortalMap(_dragDic.GetValueByIndex(0).Delta);
			}
			else if (_dragDic.Count == 2)
			{
				DragInfo valueByIndex = _dragDic.GetValueByIndex(0);
				DragInfo valueByIndex2 = _dragDic.GetValueByIndex(1);
				float num = CalcZoomInValue(valueByIndex.Delta, valueByIndex.CurrentPos, valueByIndex2.CurrentPos);
				float num2 = CalcZoomInValue(valueByIndex2.Delta, valueByIndex2.CurrentPos, valueByIndex.CurrentPos);
				ZoomPortalMap(num + num2);
			}
			for (int i = 0; i < _dragDic.Count; i++)
			{
				DragInfo valueByIndex3 = _dragDic.GetValueByIndex(i);
				valueByIndex3.Delta = Vector2.zero;
				_dragDic.AddAndOverride(valueByIndex3.TouchId, valueByIndex3);
			}
		}
	}

	void PortalComponent2.Listener.StartDrag()
	{
		StartDrag();
	}

	void PortalComponent2.Listener.OnDrag()
	{
		OnDrag();
	}

	void PortalComponent2.Listener.EndDrag()
	{
		EndDrag();
	}

	public string GetChapterPopupImagePath(int ChapterIndex)
	{
		foreach (ChapterInformation item in _ListChapterInfo)
		{
			if (item.ChapterIdx == ChapterIndex)
			{
				return item.PopupImagePath;
			}
		}
		return null;
	}

	private void UpdateCenter(int x, int y)
	{
		Vector3 localPosition = MapRoot.localPosition;
		Vector3 localScale = MapRoot.localScale;
		localPosition.x = (float)x * localScale.x;
		localPosition.y = (float)y * localScale.y;
		SetMapRootPosition(localPosition);
	}

	private void UpdateCenter(int ChapterIndex)
	{
		foreach (ChapterInformation item in _ListChapterInfo)
		{
			if (item.ChapterIdx == ChapterIndex)
			{
				UpdateCenter(item.x, item.y);
				return;
			}
		}
		UpdateCenter(_ListChapterInfo[0].x, _ListChapterInfo[0].y);
	}

	private int SortButton(Transform a, Transform b)
	{
		PortalComponent2 component = a.GetComponent<PortalComponent2>();
		PortalComponent2 component2 = b.GetComponent<PortalComponent2>();
		PortalData portalData = ((component == null) ? null : component.PortalData);
		PortalData portalData2 = ((component2 == null) ? null : component2.PortalData);
		if (portalData == null || portalData2 == null)
		{
			if (portalData != null)
			{
				return -1;
			}
			if (portalData2 != null)
			{
				return 1;
			}
			return 0;
		}
		return -portalData.DungeonIndex.CompareTo(portalData2.DungeonIndex);
	}

	void PortalComponent2.Listener.SelectPortal(PortalComponent2 component)
	{
		if (component.hasChild() && component.Selectable())
		{
			PortalChildListPopup.Open(component.ChapterIndex, component.DungeonIndex, delegate(int x, int y)
			{
				_listener.MoveMap(x, y);
			}, delegate
			{
				CenterMessage.Open(LocalizationManager.GetString(WorldMap.WORLDMAP_CANNOT_VISIT_NODE));
			});
		}
		else if (component.Selectable())
		{
			if (_listener != null)
			{
				int num = component.DungeonIndex;
				int latelyPlayDungeonIndex_Chapter = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.GetLatelyPlayDungeonIndex_Chapter(component.ChapterIndex);
				bool flag = latelyPlayDungeonIndex_Chapter > num;
				if (latelyPlayDungeonIndex_Chapter > 0 && flag)
				{
					num = latelyPlayDungeonIndex_Chapter;
				}
				_listener.MoveMap(component.ChapterIndex, num);
			}
		}
		else
		{
			CenterMessage.Open(LocalizationManager.GetString(WorldMap.WORLDMAP_CANNOT_VISIT_NODE));
		}
	}

	void PortalComponent2.Listener.SelectButton(PortalComponent2 component)
	{
		if (component.hasChild())
		{
			if (_listener != null)
			{
				if (component.PortalData.ContentsShortCutButton)
				{
					_listener.OpenContentsShortCut(component.ChapterIndex, component.DungeonIndex);
				}
				else
				{
					_listener.OpenPopup(component.ChapterIndex, component.DungeonIndex);
				}
			}
		}
		else if (_listener != null)
		{
			_listener.ClickMoveLastBattlePos();
		}
	}

	void PortalComponent2.Listener.SelectEnableContentsShortCut(PortalComponent2 component)
	{
	}

	void PortalComponent2.Listener.SelectDisableContentsShortCut(string disableText)
	{
	}

	bool PortalComponent2.Listener.CanOpenShorCutBtn()
	{
		if (_dataProvider == null)
		{
			return false;
		}
		return _dataProvider.CanGoToLatelyBattleNode();
	}

	void LeftMenuMissionProgress.Listner.OnClickMission(MissionComponentData data)
	{
		Close();
		if (_listener != null)
		{
			_listener.OnClickMissionButton(data);
		}
	}

	void LeftMenuMissionProgress.Listner.OnClick()
	{
		Close();
		if (_listener != null)
		{
			_listener.OnClickQuestButton();
		}
	}
}
