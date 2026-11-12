using System;
using System.Collections;
using NGame2.NAccount;
using NGame2.NMessage;
using NGame2.NMessage.NFastHandler;
using NGame2.NUI.NManager;
using NGame2.NUI.NRender3D;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWindow.NGlobalTopmost;
using NShared;
using NShared.UsePackageItem;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class CinemaItemScene : BaseWindow, ITouchDisabler
{
	public interface Listener
	{
		void OnClickUse(CinemaItemScene caller, Action<NShared.UsePackageItem.Response> OnResponse);
	}

	public const string Command_ShowItemIcon = "CinemaItemScene/ShowItemIcon";

	public const string PrefabPath = "Resources/Prefab2/Window/FullScene/CinemaItemScene";

	public UIButton Btn_Return;

	public UIButton Btn_Use;

	public UIButton Btn_Close;

	public UILabel Lbl_RemainText;

	public UILabel Lbl_RemainCount;

	private NShared.UsePackageItem.Response _response;

	public UIWidget Widget_Root;

	public UITexture Texture_Cinema;

	private RenderTargetCinema _cinema;

	private CinemaItemData _cinemaItemData;

	private Listener _listener;

	protected Holder _receiverHolder = new Holder();

	public ItemData ItemData { get; private set; }

	bool ITouchDisabler.IsValid => IsOpen();

	public override void Awake()
	{
		base.Awake();
		UIPanel.lateUpdateBeforePanelList.Add(base.gameObject);
	}

	public override void Reset()
	{
		base.Reset();
		_receiverHolder.Clear();
		if (Texture_Cinema != null)
		{
			Texture_Cinema.mainTexture = null;
		}
		if (_cinema != null)
		{
			RenderTargetCinema.ReleaseInstance(_cinema);
			_cinema = null;
		}
	}

	public override void OnSystemBackButton()
	{
		if (UIHelper.IsEnabledGameObjet(Btn_Return))
		{
			OnClickBack();
		}
		else if (UIHelper.IsEnabledGameObjet(Btn_Close))
		{
			VespaCoroutine.Start(coClose(this));
		}
	}

	public override void OnClosing()
	{
		base.OnClosing();
		VespaCoroutine.StopAll(this);
	}

	protected void Play()
	{
		if (!(Texture_Cinema == null))
		{
			_receiverHolder.Add<CommandMessage>(new ObjVerifier(this), OnMessageFast);
			SetUIs(show: false);
			UIHelper.EnableGameObject(Btn_Return, enable: false);
			VespaCoroutine.Start(this, coAppear());
		}
	}

	public void Open(ItemData itemData, Listener listener)
	{
		if (IsOpen())
		{
			Close();
		}
		if (itemData != null && listener != null)
		{
			ItemData = itemData;
			_listener = listener;
			base.Open();
			Play();
		}
	}

	protected virtual CinemaItemData GetCinemaData()
	{
		return GameRoot.Instance.CinemaItemDataContainer.GetData(ItemData.Code);
	}

	private IEnumerator coAppear()
	{
		UIHelper.SetWidgetAlpha(Widget_Root, 0f);
		SceneChanger sceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(5f, GlobalTopmostWindow.ESceneChangeType.BlackScreen);
		while (!sceneChanger.IsCompleteAnimation())
		{
			yield return null;
		}
		UIHelper.SetWidgetAlpha(Widget_Root, 1f);
		_cinemaItemData = GetCinemaData();
		if (_cinemaItemData == null)
		{
			yield break;
		}
		string packCode = _cinemaItemData.PackCode;
		string appearName = _cinemaItemData.AppearName;
		if (_cinema == null)
		{
			RenderTargetCinema.StockData stock = default(RenderTargetCinema.StockData);
			SingletonAuto<ResolutionInfo>.instance.GetScreenRenderTargetSize(out stock.RenderTargetWidth, out stock.RenderTargetHeight);
			stock.NodeTreePackCode = packCode;
			stock.NodeTreeName = appearName;
			_cinema = RenderTargetCinema.CreateInstance(stock);
		}
		else
		{
			_cinema.LoadNodeTree(packCode, appearName);
		}
		if (!(_cinema == null))
		{
			do
			{
				yield return null;
			}
			while (!_cinema.IsCompleteLoad());
			_cinema.StartNodeTree(OnEndAppear);
			Texture_Cinema.mainTexture = _cinema.RenderTarget;
			Texture_Cinema.alpha = 1f;
			sceneChanger.Close();
			while (!sceneChanger.IsCompleteAnimation())
			{
				yield return null;
			}
		}
	}

	protected virtual void OnEndAppear()
	{
		SetItemRemainCount();
		SetUIs(show: true);
	}

	protected virtual void OnEndReturn()
	{
		SetItemRemainCount();
		SetUIs(show: true);
	}

	private void UsePackageCallback(NShared.UsePackageItem.Response response)
	{
		_response = response;
		VespaCoroutine.Start(this, coReaction());
	}

	protected IEnumerator coReaction()
	{
		if (!(_cinema == null))
		{
			_cinema.LoadNodeTree(_cinemaItemData.PackCode, _cinemaItemData.ReactionName);
			do
			{
				yield return null;
			}
			while (!_cinema.IsCompleteLoad());
			_cinema.StartNodeTree();
		}
	}

	protected void OnMessageFast(CommandMessage message)
	{
		if (message.IsValidMessage() && message.IsValidScene(_cinema) && message.Command == "CinemaItemScene/ShowItemIcon")
		{
			UIHelper.EnableGameObject(Btn_Return, enable: true);
			ShowRewardPopup();
		}
	}

	public virtual void OnClickBack()
	{
		BaseRewardPopup baseRewardPopup = GlobalAutoBehaviour<PopupManegement>.instance.PopUpExist<BaseRewardPopup>();
		if (baseRewardPopup != null)
		{
			baseRewardPopup.Close();
		}
		UIHelper.EnableGameObject(Btn_Return, enable: false);
		StartCoReturn();
	}

	private void StartCoReturn()
	{
		VespaCoroutine.Start(this, coReturn());
	}

	protected virtual void ShowRewardPopup()
	{
		if (_response != null)
		{
			BaseRewardPopup.PopupOpenData popupOpenData = new BaseRewardPopup.PopupOpenData(_response);
			popupOpenData.SetShowOnlyReward(value: true);
			BaseRewardPopup.OpenPopup(popupOpenData, isGlobal: false);
			_response = null;
		}
	}

	private IEnumerator coReturn()
	{
		if (!(_cinema == null))
		{
			_cinema.LoadNodeTree(_cinemaItemData.PackCode, _cinemaItemData.ReturnName);
			do
			{
				yield return null;
			}
			while (!_cinema.IsCompleteLoad());
			_cinema.StartNodeTree(delegate
			{
				OnEndReturn();
			});
		}
	}

	protected void SetItemRemainCount()
	{
		int itemCount = Helper.GetItemCount(ItemData.Index);
		string stringDirect = LocalizationManager.GetStringDirect("ITEM.COMMON_GROWTH_REWARD_COUNT", itemCount);
		UIHelper.SetLabelText(Lbl_RemainCount, stringDirect);
	}

	protected virtual void SetUIs(bool show)
	{
		if (ItemData != null)
		{
			int itemCount = Helper.GetItemCount(ItemData.Index);
			UIHelper.EnableGameObject(Lbl_RemainCount, show);
			UIHelper.EnableGameObject(Btn_Use, show && itemCount > 0);
			UIHelper.EnableGameObject(Btn_Close, show);
		}
	}

	public virtual void OnClickUse()
	{
		SetUIs(show: false);
		if (_listener != null)
		{
			_listener.OnClickUse(this, UsePackageCallback);
		}
	}

	public void OnClickClose()
	{
		BaseRewardPopup baseRewardPopup = GlobalAutoBehaviour<PopupManegement>.instance.PopUpExist<BaseRewardPopup>();
		if (baseRewardPopup != null)
		{
			baseRewardPopup.Close();
		}
		VespaCoroutine.Start(coClose(this));
	}

	private static IEnumerator coClose(CinemaItemScene This, Action AfterClose = null)
	{
		SceneChanger sceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(5f, GlobalTopmostWindow.ESceneChangeType.BlackScreen);
		while (!sceneChanger.IsCompleteAnimation())
		{
			yield return null;
		}
		if (This != null && This.IsOpen())
		{
			UIHelper.SetWidgetAlpha(This.Widget_Root, 0f);
			This.Close();
		}
		sceneChanger.Close();
		Vespa_System_Action_Extension_Method.fire(AfterClose);
	}
}
