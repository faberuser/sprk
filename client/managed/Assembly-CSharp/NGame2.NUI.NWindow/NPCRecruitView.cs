using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NRender;
using NGame2.NRender.NRenderFacade;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NRender3D.NCinema;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using NVespa.NTask.NRuntime;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class NPCRecruitView : HeroRecruitView, LobbyTopmostMenu.IUserStatObserver, NPCFriendshipItemComponent.Listener, IUserObserver<NPCFriendlyInfo>, IUserObserver, IUserObserver<ItemInfo>
{
	public interface Listener
	{
		void OnOpenRewardList(NPCFriendlyInfo info, NpcFriendlyType type);

		void OnGiveGift(int heroIndex, int itemIndex, int itemCount);

		void OnTakeRewardItem(int heroIndex, int step, NpcFriendlyType type);

		void OnTakeRewardItem(int heroIndex, int step, string title, Action<System.Action> effectAction);

		void OnShowDetail(int heroIndex);
	}

	public enum ParentWindowType
	{
		Default,
		Cinema
	}

	public const string PrefabPath = "Resources/Prefab2/Window/NPCRecruit/NPCRecruitView";

	public const string PrefabPath2 = "Resources/Prefab2/Window/Valance/ValanceRecruitWindow";

	[Header(" - RewardItemInfo [Left]")]
	public UIWidget LeftRoot;

	public UILabel RewardTitleLabel;

	public Transform ItemIconPosition;

	public UILabel ItemName;

	public UIButton TakeRewardButton;

	[Header(" - GiveGiftList [Right]")]
	public UIWidget RightRoot;

	public UILabel FriendshipGradeLabel;

	public UIProgressBar CurrentFriendshipPointProgress;

	public UIProgressBar AddedFriendshipPointProgress;

	public UILabel FriendshipPointLabel;

	public UILabel AddedFriendshipPointLabel;

	public UIInput ItemCountInput;

	public UISimpleGrid ItemGrid;

	public UILabel NoneGiftItemLabel;

	[Space(20f)]
	public UIWidget SpeakBubble;

	public UILabel SpeakLabel;

	public TweenAlpha BubbleTweenAlpha;

	public UIButton NPCDetailInfoButton;

	public UITexture NodeTreeTexture;

	protected Listener _listener;

	protected ItemIconComponent RewardItemIcon;

	private List<NPCFriendshipItemComponent> PointItems = new List<NPCFriendshipItemComponent>();

	private NPCFriendshipItemComponent SelectedPointItem;

	public NpcRecruitType RecruitType;

	public ParentWindowType ParentType;

	private Coroutine CoroutineIncreaseAnimation;

	private int _cachedWindowGuideIndex;

	private RenderFacade _cinemaRenderFacade;

	private AnimationData.StockData _prevAnimationStockData;

	private bool _isFirstReceiveGift = true;

	private bool _isReserveIdleAnimation;

	public UserManager.UserStatType StatType
	{
		get
		{
			if (_friendlyType == NpcFriendlyType.Event)
			{
				return UserManager.UserStatType.EventGiftPoint;
			}
			return UserManager.UserStatType.None;
		}
	}

	protected int CurrentFriedshipPoint { get; set; }

	protected int AddedFriedshipPoint { get; set; }

	protected int MaxFriedshipPoint { get; set; }

	protected int TotalFriedshipPoint => CurrentFriedshipPoint + AddedFriedshipPoint;

	private int ItemCount { get; set; }

	public NPCData NPCData { get; set; }

	public NPCFriendlyInfo NPCFriendlyInfo { get; set; }

	protected NpcFriendlyType _friendlyType { get; set; }

	public void Open(NPCData npcData, NpcFriendlyType friendlyType, NpcRecruitType recruitType, Listener listener)
	{
		_listener = listener;
		NPCData = npcData;
		_friendlyType = friendlyType;
		RecruitType = recruitType;
		ParentType = GetParentWindowType(RecruitType);
		if (friendlyType == NpcFriendlyType.Event || friendlyType == NpcFriendlyType.EventGiftItem || friendlyType == NpcFriendlyType.EventChristmas || friendlyType == NpcFriendlyType.EventGodKing)
		{
			_cachedWindowGuideIndex = WindowGuideIndex;
			WindowGuideIndex = 0;
		}
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(npcData.CreatureIndex);
		if (data == null)
		{
			return;
		}
		UIHelper.EnableGameObject(NPCDetailInfoButton, friendlyType == NpcFriendlyType.Recruit);
		CachedHeroInfo referenceHero = SceneAutoBehaviour<StatDataCache>.instance.GetReferenceHero(data.Index, 1);
		NPCFriendlyInfo = GlobalAutoBehaviour<AccountManager>.instance.NPCFriendlyManager.Get(NPCData.HeroIndex);
		if (NPCFriendlyInfo == null)
		{
			NPCFriendlyInfo = new NPCFriendlyInfo
			{
				FriendlyPoint = 0,
				Step = 1,
				HeroIndex = NPCData.HeroIndex
			};
		}
		if (ParentType == ParentWindowType.Cinema)
		{
			if (NodeTreeTexture != null)
			{
				CinemaOwner cinemaOwner = SingletonAuto<CinemaOwnerContainer>.instance.GetCinemaOwner(ECinemaOwnerType.NodeTreeEntry);
				string code = string.Format("{0}.{1}", "NPC", NPCData.Code);
				_cinemaRenderFacade = cinemaOwner.Cinema.GetRenderFacade(code);
				cinemaOwner.DataProvider.SetTexture(NodeTreeTexture);
				cinemaOwner.SetRenderTexture();
				_isFirstReceiveGift = true;
				_isReserveIdleAnimation = false;
			}
			new DelegateTask(OpenAsync(referenceHero, isSelectHero: true, "", isInitModel: false)).Commit();
		}
		else
		{
			new DelegateTask(OpenAsync(referenceHero, isSelectHero: true)).Commit();
		}
		HideSpeak();
	}

	public static ParentWindowType GetParentWindowType(NpcRecruitType recruitType)
	{
		return recruitType switch
		{
			NpcRecruitType.Default => ParentWindowType.Default, 
			NpcRecruitType.Valance => ParentWindowType.Cinema, 
			_ => ParentWindowType.Default, 
		};
	}

	public static string GetPrefabPath(NpcRecruitType recruitType)
	{
		return recruitType switch
		{
			NpcRecruitType.Default => "Resources/Prefab2/Window/NPCRecruit/NPCRecruitView", 
			NpcRecruitType.Valance => "Resources/Prefab2/Window/Valance/ValanceRecruitWindow", 
			_ => "Resources/Prefab2/Window/NPCRecruit/NPCRecruitView", 
		};
	}

	public void Reload(HeroInfo heroInfo)
	{
		new DelegateTask(ReloadAsync(heroInfo)).Commit();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		SetIdleAnimation();
	}

	private void SetIdleAnimation(bool isForce = false)
	{
		if (ParentType == ParentWindowType.Cinema && _isReserveIdleAnimation && (isForce || _cinemaRenderFacade.AnimationData.Runtime.IsEnd))
		{
			AnimationData.StockData stock = new AnimationData.StockData
			{
				AnimationKey = _prevAnimationStockData.AnimationKey,
				MotionEnd = _prevAnimationStockData.MotionEnd,
				State = _prevAnimationStockData.State
			};
			_cinemaRenderFacade.Action.Motion(stock, blendMotion: true, null);
			_isReserveIdleAnimation = false;
		}
	}

	public override void Close()
	{
		if (_cachedWindowGuideIndex != 0)
		{
			WindowGuideIndex = _cachedWindowGuideIndex;
			_cachedWindowGuideIndex = 0;
		}
		SetIdleAnimation(isForce: true);
		base.Close();
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.SafeReleaseComponent(ref RewardItemIcon);
		ResetItemList();
		SelectedPointItem = null;
		CoroutineIncreaseAnimation = null;
		HideSpeak();
	}

	private void ResetItemList()
	{
		FactoryBase.ReleaseAndClearList(PointItems);
	}

	public override void UpdateUI(bool bSelectHero, bool BaseSpeakAction = true)
	{
		if (base.HeroInfo != null && base.CreatureData != null)
		{
			InitItemCountInput();
			NPCFriendlyPointData data = GameRoot.Instance.NPCFriendlyPointDataContainer.GetData(NPCFriendlyInfo.HeroIndex, NPCFriendlyInfo.Step);
			int maxPoint = CalcMaxFriendlyPoint(NPCFriendlyInfo.HeroIndex, NPCFriendlyInfo.Step);
			InitFriendshipPoint(NPCFriendlyInfo.FriendlyPoint, maxPoint, NPCFriendlyInfo.Step);
			InitRewardItem(NPCFriendlyInfo, data);
			InitGiftItemList(NPCFriendlyInfo.HeroIndex);
		}
	}

	protected virtual void InitRewardItem(NPCFriendlyInfo npcFriendlyInfo, NPCFriendlyPointData npcFriendlyPointData)
	{
		string text = LocalizationManager.GetString(NPCRecruit.NPC_FRIENDLY_GRADE_REWARD, npcFriendlyPointData.Step);
		UIHelper.SetLabelText(RewardTitleLabel, text);
		FactoryBase.SafeReleaseComponent(ref RewardItemIcon);
		RewardItemIcon = UIHelper.CreateRewardMainItemIconComponent(npcFriendlyPointData.RewardIndex, ItemIconPosition, ItemName);
		bool enable = IsMaxPoint();
		UIHelper.EnableButton(TakeRewardButton, enable, grayscaleIfDisable: true);
	}

	protected void InitFriendshipPoint(int currentPoint, int maxPoint, int step)
	{
		CurrentFriedshipPoint = currentPoint;
		MaxFriedshipPoint = maxPoint;
		AddedFriedshipPoint = 0;
		CalcProgress(currentPoint, step, CurrentFriendshipPointProgress);
		string text = "";
		UIHelper.SetLabelText(text: (_friendlyType == NpcFriendlyType.Recruit) ? LocalizationManager.GetString(NPCRecruit.NPC_FRIENDLY_GRADE2, step) : ((_friendlyType != NpcFriendlyType.EventChristmas) ? LocalizationManager.GetString(NPCRecruit.NPC_CONTRIBUTION_GRADE2, step) : LocalizationManager.GetString(EVENTCHRISTMAS.TITLE_INFO_EVENT_ITEM_LEVEL, step)), label: FriendshipGradeLabel);
		UpdateFriendshipPoint();
	}

	protected void CalcProgress(int curr, int step, UIProgressBar progress)
	{
		if (!(progress == null))
		{
			int num = 0;
			int heroIndex = NPCFriendlyInfo.HeroIndex;
			if (step > 1)
			{
				num = CalcMaxFriendlyPoint(heroIndex, step - 1);
			}
			int num2 = CalcMaxFriendlyPoint(heroIndex, step);
			float value = (float)(curr - num) / (float)(num2 - num);
			progress.value = value;
		}
	}

	protected void InitGiftItemList(int heroIndex, int selectItemindex = 0)
	{
		ResetItemList();
		SelectedPointItem = null;
		IList<NPCFriendlyPointItemData> itemListByHeroIndex = GameRoot.Instance.NPCFriendlyPointItemDataContainer.GetItemListByHeroIndex(heroIndex);
		if (__Extension_Method_ICollection__.COUNT(itemListByHeroIndex) == 0)
		{
			UIHelper.EnableCollider(ItemCountInput, isEnable: false);
			UIHelper.EnableGameObject(NoneGiftItemLabel, enable: true);
			InitItemCountInput();
			return;
		}
		foreach (NPCFriendlyPointItemData item in itemListByHeroIndex)
		{
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(item.ItemCode);
			if (dataByCode == null)
			{
				continue;
			}
			ItemInfo itemInfo = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(dataByCode.Index);
			if (itemInfo != null && itemInfo.Count > 0)
			{
				NPCFriendshipItemComponent nPCFriendshipItemComponent = UIHelper.AddGridItem(ItemGrid, NPCFriendshipItemComponent.PrefabAuto);
				nPCFriendshipItemComponent.Open(item, NPCFriendlyInfo, itemInfo.Count, _friendlyType, this);
				PointItems.Add(nPCFriendshipItemComponent);
				if (selectItemindex != 0 && selectItemindex == nPCFriendshipItemComponent.ItemIndex)
				{
					SelectedPointItem = nPCFriendshipItemComponent;
				}
			}
		}
		SelectPointItemComponent(SelectedPointItem);
		bool flag = PointItems.Count > 0;
		UIHelper.EnableGameObject(NoneGiftItemLabel, !flag);
		UIHelper.EnableCollider(ItemCountInput, flag);
		UIHelper.RepositionGrid(ItemGrid);
	}

	private void SelectPointItemComponent(NPCFriendshipItemComponent selectComp)
	{
		if (SelectedPointItem != null)
		{
			SelectedPointItem.Select(isSelect: false);
		}
		SelectedPointItem = selectComp;
		if (selectComp != null)
		{
			selectComp.Select(isSelect: true);
		}
		if (IsMaxPoint() || selectComp == null)
		{
			SetItemCount(0);
		}
		else
		{
			SetItemCount(1);
		}
	}

	protected void SetPointLabel(int point, int max)
	{
		string text = $"{point} / {max}";
		UIHelper.SetLabelText(FriendshipPointLabel, text);
	}

	public virtual void UpdateFriendshipPoint()
	{
		CalcProgress(TotalFriedshipPoint, NPCFriendlyInfo.Step, AddedFriendshipPointProgress);
		int num = 0;
		if (NPCFriendlyInfo.Step > 1)
		{
			num = CalcMaxFriendlyPoint(NPCFriendlyInfo.HeroIndex, NPCFriendlyInfo.Step - 1);
		}
		int point = TotalFriedshipPoint - num;
		int max = MaxFriedshipPoint - num;
		SetPointLabel(point, max);
		SetAddedPointLabel(AddedFriedshipPoint);
	}

	protected void SetAddedPointLabel(int value)
	{
		if (value == 0)
		{
			UIHelper.SetLabelText(AddedFriendshipPointLabel, "");
		}
		else
		{
			UIHelper.SetLabelText(AddedFriendshipPointLabel, "+" + value);
		}
	}

	protected void InitItemCountInput()
	{
		SetItemCount(0);
	}

	private void SetItemCount(int count)
	{
		ItemCount = count;
		ItemCountInput.value = count.ToString();
		if (SelectedPointItem != null)
		{
			AddedFriedshipPoint = SelectedPointItem.FriendshipPoint * ItemCount;
		}
		else
		{
			AddedFriedshipPoint = 0;
		}
		UpdateFriendshipPoint();
	}

	public void OnSubmitItemCountInput()
	{
		if (ItemCountInput == null || SelectedPointItem == null)
		{
			return;
		}
		int result = 0;
		if (!int.TryParse(ItemCountInput.value, out result))
		{
			SetItemCount(1);
			return;
		}
		int num = 0;
		if (SelectedPointItem != null)
		{
			num = CalcMaxCount(SelectedPointItem.FriendshipPoint, SelectedPointItem.ItemMaxCount);
		}
		if (result <= 0)
		{
			SetItemCount(1);
		}
		else if (num <= result)
		{
			SetItemCount(num);
		}
		else
		{
			SetItemCount(result);
		}
	}

	public void OnClickCountDecrease()
	{
		if (!IsPlayingAnimation() && !(SelectedPointItem == null) && ItemCount > 1)
		{
			SetItemCount(ItemCount - 1);
		}
	}

	public void OnClickCountIncrease()
	{
		if (!IsPlayingAnimation() && !(SelectedPointItem == null))
		{
			int num = 0;
			if (SelectedPointItem != null)
			{
				num = CalcMaxCount(SelectedPointItem.FriendshipPoint, SelectedPointItem.ItemMaxCount);
			}
			if (ItemCount < num)
			{
				SetItemCount(ItemCount + 1);
			}
		}
	}

	public void OnClickCountMax()
	{
		if (!IsPlayingAnimation() && !(SelectedPointItem == null))
		{
			int itemCount = 0;
			if (SelectedPointItem != null)
			{
				itemCount = CalcMaxCount(SelectedPointItem.FriendshipPoint, SelectedPointItem.ItemMaxCount);
			}
			SetItemCount(itemCount);
		}
	}

	private int CalcMaxCount(int onceItemPoint, int itemCount)
	{
		if (IsMaxPoint())
		{
			return 0;
		}
		int num = (int)Mathf.Ceil((float)(MaxFriedshipPoint - CurrentFriedshipPoint) / (float)onceItemPoint);
		if (num > itemCount)
		{
			num = itemCount;
		}
		return num;
	}

	protected bool IsMaxPoint()
	{
		return CurrentFriedshipPoint >= MaxFriedshipPoint;
	}

	public void OnClickOpenRewardList()
	{
		if (_listener != null)
		{
			_listener.OnOpenRewardList(NPCFriendlyInfo, _friendlyType);
		}
	}

	public void OnClickGiveGift()
	{
		if (IsPlayingAnimation())
		{
			return;
		}
		if (SelectedPointItem == null)
		{
			CenterMessage.OpenError(NPCRecruit.NO_SELECT_ITEM_MESSAGE);
		}
		else if (SelectedPointItem.CanGiftItem)
		{
			if (IsMaxPoint())
			{
				MessagePopup.OpenError(NPCRecruit.FRIENDLY_POINT_FULL_MESSAGE);
			}
			else if (_listener != null)
			{
				_listener.OnGiveGift(NPCFriendlyInfo.HeroIndex, SelectedPointItem.ItemIndex, ItemCount);
			}
		}
	}

	public void OnClickTakeRewardItem()
	{
		if (_listener != null)
		{
			_listener.OnTakeRewardItem(NPCFriendlyInfo.HeroIndex, NPCFriendlyInfo.Step, _friendlyType);
		}
	}

	public void OnClickShowDetailHero()
	{
		if (_listener != null)
		{
			_listener.OnShowDetail(NPCData.HeroIndex);
		}
	}

	private void ReceiveGiftAction(int itemGrade)
	{
		NPCFriendlyActionData data = GameRoot.Instance.NPCFriendlyActionDataContainer.GetData(NPCData.CreatureIndex, itemGrade);
		if (data == null)
		{
			return;
		}
		if (ParentType == ParentWindowType.Cinema)
		{
			if (_isFirstReceiveGift)
			{
				_isFirstReceiveGift = false;
				AnimationData.StockData currentMotionStock = _cinemaRenderFacade.Action.CurrentMotionStock;
				_prevAnimationStockData = new AnimationData.StockData
				{
					AnimationKey = currentMotionStock.AnimationKey,
					MotionEnd = currentMotionStock.MotionEnd,
					State = currentMotionStock.State
				};
			}
			ResponseAction(_cinemaRenderFacade, ref _heroSound, data.GetMotion(), data.GetSound(), bBlendAction: false);
			if (!_isReserveIdleAnimation)
			{
				_isReserveIdleAnimation = true;
			}
		}
		else
		{
			ResponseAction(data.GetMotion(), data.GetSound(), bBlendAction: false);
		}
		string stringDirect = LocalizationManager.GetStringDirect(data.GetSpeech());
		ShowSpeak(stringDirect);
	}

	public void ShowSpeak(string message)
	{
		UIHelper.EnableGameObject(SpeakBubble, enable: true);
		UIHelper.SetLabelText(SpeakLabel, message);
		if (BubbleTweenAlpha != null)
		{
			BubbleTweenAlpha.ResetToBeginning();
			BubbleTweenAlpha.PlayForward();
			BubbleTweenAlpha.SetOnFinished(delegate
			{
				UIHelper.EnableGameObject(SpeakBubble, enable: false);
			});
		}
	}

	public void HideSpeak()
	{
		UIHelper.EnableGameObject(SpeakBubble, enable: false);
		if (BubbleTweenAlpha != null)
		{
			BubbleTweenAlpha.ResetToBeginning();
		}
	}

	void NPCFriendshipItemComponent.Listener.OnClickComponent(NPCFriendshipItemComponent component)
	{
		if (!IsPlayingAnimation())
		{
			SelectPointItemComponent(component);
		}
	}

	void IUserObserver<ItemInfo>.OnChanged(ItemInfo changedInfo)
	{
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(changedInfo.ItemIndex);
		if (data.Type != ItemType.NPCFriendlyPoint)
		{
			return;
		}
		ReceiveGiftAction(data.Grade);
		if (changedInfo.Count == 0)
		{
			ResetItemList();
			InitGiftItemList(NPCFriendlyInfo.HeroIndex, SelectedPointItem.ItemIndex);
			return;
		}
		NPCFriendshipItemComponent nPCFriendshipItemComponent = PointItems.Find((NPCFriendshipItemComponent p) => p.ItemIndex == changedInfo.ItemIndex);
		if (nPCFriendshipItemComponent != null)
		{
			nPCFriendshipItemComponent.UpdateCount(changedInfo.Count);
		}
	}

	private bool IsPlayingAnimation()
	{
		return CoroutineIncreaseAnimation != null;
	}

	void IUserObserver<NPCFriendlyInfo>.OnChanged(NPCFriendlyInfo changedInfo)
	{
		NPCFriendlyInfo nPCFriendlyInfo = NPCFriendlyInfo;
		NPCFriendlyInfo = changedInfo;
		bool flag = false;
		if (nPCFriendlyInfo.Step != changedInfo.Step || nPCFriendlyInfo.FriendlyPoint >= changedInfo.FriendlyPoint)
		{
			flag = true;
		}
		if (flag)
		{
			int maxPoint = CalcMaxFriendlyPoint(NPCFriendlyInfo.HeroIndex, NPCFriendlyInfo.Step);
			NPCFriendlyPointData data = GameRoot.Instance.NPCFriendlyPointDataContainer.GetData(NPCFriendlyInfo.HeroIndex, NPCFriendlyInfo.Step);
			InitFriendshipPoint(NPCFriendlyInfo.FriendlyPoint, maxPoint, NPCFriendlyInfo.Step);
			InitRewardItem(NPCFriendlyInfo, data);
			InitGiftItemList(NPCFriendlyInfo.HeroIndex);
			return;
		}
		CurrentFriedshipPoint = changedInfo.FriendlyPoint;
		AddedFriedshipPoint = 0;
		if (CoroutineIncreaseAnimation != null)
		{
			StopCoroutine(CoroutineIncreaseAnimation);
		}
		CoroutineIncreaseAnimation = ExtensionMethod_VespaCoroutine.StartCoroutineThis(this, CoIncreaseProgressAnimtion(nPCFriendlyInfo.FriendlyPoint, changedInfo.FriendlyPoint, changedInfo.Step));
	}

	private IEnumerator CoIncreaseProgressAnimtion(int start, int end, int step)
	{
		int prevMax = 0;
		int max = CalcMaxFriendlyPoint(NPCFriendlyInfo.HeroIndex, step);
		if (step > 1)
		{
			prevMax = CalcMaxFriendlyPoint(NPCFriendlyInfo.HeroIndex, step - 1);
		}
		float AnimationTime = (float)(end - start) / (float)(max - prevMax);
		float startTime = Time.time;
		float value = (float)(end - prevMax) / (float)(max - prevMax);
		AddedFriendshipPointProgress.value = value;
		while (true)
		{
			float num = Time.time - startTime;
			float num2 = Mathf.Lerp(start, end, num / AnimationTime);
			float num3 = num2 - (float)prevMax;
			int num4 = max - prevMax;
			float value2 = num3 / (float)num4;
			CurrentFriendshipPointProgress.value = value2;
			SetAddedPointLabel(end - (int)num2);
			SetPointLabel((int)num3, num4);
			if (num2 >= (float)end)
			{
				break;
			}
			yield return null;
		}
		NPCFriendlyPointData data = GameRoot.Instance.NPCFriendlyPointDataContainer.GetData(NPCFriendlyInfo.HeroIndex, NPCFriendlyInfo.Step);
		InitRewardItem(NPCFriendlyInfo, data);
		SelectPointItemComponent(null);
		CoroutineIncreaseAnimation = null;
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	private int CalcMaxFriendlyPoint(int heroIndex, int step)
	{
		int num = 0;
		List<NPCFriendlyPointData> pointDataList = GameRoot.Instance.NPCFriendlyPointDataContainer.GetPointDataList(heroIndex);
		pointDataList.Sort((NPCFriendlyPointData a, NPCFriendlyPointData b) => a.Step.CompareTo(b.Step));
		foreach (NPCFriendlyPointData item in pointDataList)
		{
			if (item.Step > step)
			{
				break;
			}
			num += item.ReqFriendlyPoint;
		}
		return num;
	}
}
