using System;
using System.Collections;
using NGame2.NAccount.NExtension;
using NGame2.NMessage;
using NGame2.NMessage.NFastHandler;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NStat;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.ClassBuff;
using NGame2.NUI.NRender3D;
using NGame2.NUI.NWindow.NGlobalTopmost;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class EquipAwakenScene : BaseWindow, ITouchDisabler, LateUpdaterBeforePanel.IReceiver, IInspectorGuiDrawer
{
	public class StockData
	{
		public string LogHeader => $"[EquipAwakenScene({0},{0})]";
	}

	public interface IDataProvider
	{
		StatData GetBeforeStat();

		StatData GetAfterStat();

		int GetBeforeStar();

		int GetAfterStar();

		EquipItemData GetTargetEquipItemData();

		EquipItemInfo GetTargetEquipItemInfo();

		Color GetFieldColor();

		Color GetBeforeColor();

		Color GetAfterColor();
	}

	public enum CinemaType
	{
		NONE,
		Equip_Awaken_Tutorial,
		Equip_Awaken,
		Equip_Enchant,
		Equip_OptionRenew
	}

	public const string Command_ShowItemIcon = "EquipAwakenScene/ShowItemIcon";

	public const string PrefabPath = "Resources/Prefab2/Window/FullScene/EquipAwakenScene";

	public UIWidget RootWidget;

	public UITexture CinemaTexture;

	public UISimpleGrid ItemGrid;

	public Transform SuccessTransform;

	public Transform FailTransform;

	public UIButton ExitButton;

	public UIPanel StatChangePanel;

	public UIScrollView StatChangeScrollView;

	private StatListComponent _statListComponent;

	private RenderTargetCinema _cinema;

	private ItemIconComponent _itemIconComponent;

	private Holder _receiverHolder = new Holder();

	protected IDataProvider _dataProvider;

	private CinemaType _cinemaType;

	private EquipItemDetailData _detailData;

	private string _nodeTreeName = "";

	private IClassBuffListener _iClassBuffListener;

	public EquipItemInfo EquipItemInfo { get; private set; }

	public bool IsSuccess { get; private set; }

	bool ITouchDisabler.IsValid => IsOpen();

	public void Open(EquipItemInfo itemInfo, IDataProvider dataProvider, CinemaType cinemaType, bool isSuccess, Action onComplete)
	{
		if (IsOpen())
		{
			Close();
		}
		base.Open();
		if (CinemaTexture == null)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return;
		}
		itemInfo.LinkSoulWeaponInfo();
		EquipItemInfo = itemInfo;
		IsSuccess = isSuccess;
		ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(EquipItemInfo.ItemIndex);
		if (data == null)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return;
		}
		_detailData = GameRoot.Instance.EquipItemDetailDataContainer.GetData(data.SubData.DetailIndex);
		if (_detailData == null)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return;
		}
		_cinemaType = cinemaType;
		_nodeTreeName = GetNodeTreeName(_cinemaType);
		if (string.IsNullOrEmpty(_nodeTreeName))
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return;
		}
		_receiverHolder.Add<CommandMessage>(new ObjVerifier(this), OnMessageFast);
		_dataProvider = dataProvider;
		UIHelper.EnableGameObject(SuccessTransform, enable: false);
		UIHelper.EnableGameObject(FailTransform, enable: false);
		UIHelper.EnableGameObject(ExitButton, enable: false);
		VespaCoroutine.Start(this, coOpen(onComplete));
	}

	private IEnumerator coOpen(Action onComplete)
	{
		UIHelper.SetWidgetAlpha(RootWidget, 0f);
		SceneChanger sceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(5f, GlobalTopmostWindow.ESceneChangeType.BlackScreen);
		while (!sceneChanger.IsCompleteAnimation())
		{
			yield return null;
		}
		UIHelper.SetWidgetAlpha(RootWidget, 1f);
		RenderTargetCinema.StockData stock = default(RenderTargetCinema.StockData);
		SingletonAuto<ResolutionInfo>.instance.GetScreenRenderTargetSize(out stock.RenderTargetWidth, out stock.RenderTargetHeight);
		stock.NodeTreePackCode = "Common/Item";
		stock.NodeTreeName = _nodeTreeName;
		_cinema = RenderTargetCinema.CreateInstance(stock);
		if (_cinema == null)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			yield break;
		}
		do
		{
			yield return null;
		}
		while (!_cinema.IsCompleteLoad());
		_cinema.StartNodeTree(OnEndCinema);
		CinemaTexture.mainTexture = _cinema.RenderTarget;
		CinemaTexture.alpha = 1f;
		sceneChanger.Close();
		while (!sceneChanger.IsCompleteAnimation())
		{
			yield return null;
		}
		Vespa_System_Action_Extension_Method.fire(onComplete);
		_iClassBuffListener = SceneAutoBehaviour<ClassBuffManagement>.instance.ClassBuffWindowManager;
	}

	public override void Awake()
	{
		base.Awake();
		UIPanel.lateUpdateBeforePanelList.Add(base.gameObject);
	}

	private void OnEndCinema()
	{
		if (!(_cinema == null))
		{
			_ = _cinema.MainCamera == null;
		}
	}

	void LateUpdaterBeforePanel.IReceiver.OnUpdate()
	{
	}

	public override void Reset()
	{
		base.Reset();
		_receiverHolder.Clear();
		FactoryBase.SafeReleaseComponent(ref _itemIconComponent);
		if (CinemaTexture != null)
		{
			CinemaTexture.mainTexture = null;
		}
		if (_cinema != null)
		{
			RenderTargetCinema.ReleaseInstance(_cinema);
			_cinema = null;
		}
		ClearStatChange();
		UIHelper.EnableGameObject(SuccessTransform, enable: false);
		UIHelper.EnableGameObject(FailTransform, enable: false);
		UIHelper.EnableGameObject(ExitButton, enable: false);
	}

	private void ClearStatChange()
	{
		FactoryBase.SafeReleaseComponent(ref _statListComponent);
	}

	public override void OnClosing()
	{
		base.OnClosing();
		VespaCoroutine.StopAll(this);
		HeroEquipAwaken instance = SceneAutoBehaviour<BlacksmithManagement>.instance.GetInstance<HeroEquipAwaken>();
		if (instance != null && instance.IsOpen())
		{
			instance.RefreshItemList();
			instance.SetTargetItem(EquipItemInfo);
		}
	}

	public void OnClickClose()
	{
		VespaCoroutine.Start(coClose(this));
	}

	private static IEnumerator coClose(EquipAwakenScene This)
	{
		SceneChanger sceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(5f, GlobalTopmostWindow.ESceneChangeType.BlackScreen);
		while (!sceneChanger.IsCompleteAnimation())
		{
			yield return null;
		}
		if (This != null && This.IsOpen())
		{
			UIHelper.SetWidgetAlpha(This.RootWidget, 0f);
			This.Close();
		}
		sceneChanger.Close();
	}

	private void OnMessageFast(CommandMessage message)
	{
		if (message.IsValidMessage() && message.IsValidScene(_cinema) && !(message.Command != "EquipAwakenScene/ShowItemIcon") && !(ItemGrid == null))
		{
			ShowItemIcon();
			ShowAwakenResult();
		}
	}

	private void ShowItemIcon()
	{
		if (!_itemIconComponent)
		{
			_itemIconComponent = UIHelper.AddGridItem(ItemGrid, ItemIconComponent.PrefabAuto);
			if (_itemIconComponent != null)
			{
				_itemIconComponent.Open(EquipItemInfo, null);
			}
		}
	}

	private string GetNodeTreeName(CinemaType cinemaType)
	{
		string result = "";
		switch (_cinemaType)
		{
		case CinemaType.Equip_Awaken_Tutorial:
		case CinemaType.Equip_Awaken:
			result = (IsSuccess ? _detailData.ForgeAwakeSuccessNodeTree : _detailData.ForgeAwakeFailNodeTree);
			break;
		case CinemaType.Equip_Enchant:
			result = _detailData.ForgeEnchantSuccessNodeTree;
			break;
		case CinemaType.Equip_OptionRenew:
			result = _detailData.ForgeRenewSuccessNodeTree;
			break;
		}
		return result;
	}

	protected virtual void ShowAwakenResult()
	{
		if (IsSuccess)
		{
			UIHelper.EnableGameObject(SuccessTransform, enable: true);
			ClearStatChange();
			if (_dataProvider != null)
			{
				_statListComponent = StatListComponent.Create(StatChangePanel, 5);
				bool flag = _statListComponent.AddStatChange(_dataProvider.GetBeforeStat(), _dataProvider.GetAfterStat(), all: false, StatListComponent.EStatChangeTemplate.ForStarUpgradeResult, new StatListComponent.ColorInfo(_dataProvider.GetFieldColor()), new StatListComponent.ColorInfo(_dataProvider.GetBeforeColor()), new StatListComponent.ColorInfo(_dataProvider.GetAfterColor()), new StatListComponent.ColorInfo(_dataProvider.GetAfterColor()));
				EquipItemData targetEquipItemData = _dataProvider.GetTargetEquipItemData();
				if (targetEquipItemData != null)
				{
					int num = 1;
					while (true)
					{
						int fixedOptionIndex = targetEquipItemData.GetFixedOptionIndex(num);
						if (fixedOptionIndex == 0)
						{
							break;
						}
						bool flag2 = _statListComponent.AddOptionChangeSkill(fixedOptionIndex, _dataProvider.GetBeforeStar(), _dataProvider.GetAfterStar(), StatListComponent.EOptionChangeTemplate.ForStarUpgradeResult, new StatListComponent.ColorInfo(_dataProvider.GetBeforeColor()), new StatListComponent.ColorInfo(_dataProvider.GetAfterColor()));
						flag = flag || flag2;
						num++;
					}
				}
				EquipItemInfo targetEquipItemInfo = _dataProvider.GetTargetEquipItemInfo();
				if (targetEquipItemInfo != null)
				{
					foreach (EquipItemOptionInfo optionInfo in targetEquipItemInfo.GetOptionInfoList())
					{
						_statListComponent.AddOptionChange(optionInfo, _dataProvider.GetBeforeStar(), _dataProvider.GetAfterStar(), all: false, StatListComponent.EStatChangeTemplate.ForStarUpgrade, new StatListComponent.ColorInfo(_dataProvider.GetFieldColor()), new StatListComponent.ColorInfo(_dataProvider.GetBeforeColor()), new StatListComponent.ColorInfo(_dataProvider.GetAfterColor()), new StatListComponent.ColorInfo(_dataProvider.GetAfterColor()));
					}
				}
			}
			if ((bool)StatChangeScrollView)
			{
				StatChangeScrollView.ResetPosition();
			}
			if (_iClassBuffListener != null)
			{
				_iClassBuffListener.AddPoint();
			}
		}
		else
		{
			UIHelper.EnableGameObject(FailTransform, enable: true);
		}
		UIHelper.EnableGameObject(ExitButton, enable: true);
	}
}
