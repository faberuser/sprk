using System;
using System.Collections;
using NGame2.NAccount.NExtension;
using NGame2.NMessage;
using NGame2.NMessage.NFastHandler;
using NGame2.NUI;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NRender3D;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NGlobalTopmost;
using NGame2.NUI3.NEmbed;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI3.NEquipLimitBreak;

public class LimitBreakResultScene : BaseWindow, ITouchDisabler
{
	[Serializable]
	public class RenderSceneUi
	{
		public UITexture SceneTexture;

		private RenderTargetCinema _cinema;

		private Holder _receiverHolder = new Holder();

		private LimitBreakResultScene _scene;

		public void ResetUi()
		{
			_receiverHolder.Clear();
			if (SceneTexture != null)
			{
				SceneTexture.mainTexture = null;
				SceneTexture.alpha = 0f;
			}
			if (_cinema != null)
			{
				RenderTargetCinema.ReleaseInstance(_cinema);
				_cinema = null;
			}
		}

		public IEnumerator coPlayForgeNodeTree(LimitBreakResultScene scene, string nodeTreeName, Action callbackOnSceneChangeComplete)
		{
			_scene = scene;
			_receiverHolder.Add<CommandMessage>(new ObjVerifier(scene), OnMessageFast);
			UIHelper.SetWidgetAlpha(scene.Widget_Root, 0f);
			SceneChanger sceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(5f, GlobalTopmostWindow.ESceneChangeType.BlackScreen);
			while (!sceneChanger.IsCompleteAnimation())
			{
				yield return null;
			}
			UIHelper.SetWidgetAlpha(scene.Widget_Root, 1f);
			Vespa_System_Action_Extension_Method.fire(callbackOnSceneChangeComplete);
			RenderTargetCinema.StockData stock = default(RenderTargetCinema.StockData);
			SingletonAuto<ResolutionInfo>.instance.GetScreenRenderTargetSize(out stock.RenderTargetWidth, out stock.RenderTargetHeight);
			stock.NodeTreePackCode = "Common/Item";
			stock.NodeTreeName = nodeTreeName;
			_cinema = RenderTargetCinema.CreateInstance(stock);
			if (_cinema == null)
			{
				sceneChanger.Close();
				scene.InitUi_ResultScene();
				yield break;
			}
			while (!_cinema.IsCompleteLoad())
			{
				yield return null;
			}
			_cinema.StartNodeTree();
			SceneTexture.mainTexture = _cinema.RenderTarget;
			SceneTexture.alpha = 1f;
			sceneChanger.Close();
			while (!sceneChanger.IsCompleteAnimation())
			{
				yield return null;
			}
		}

		private void OnMessageFast(CommandMessage message)
		{
			if (message.IsValidMessage() && !(message.Command != "EquipAwakenScene/ShowItemIcon"))
			{
				_scene.InitUi_ResultScene();
			}
		}
	}

	[Serializable]
	public class ResultItemUi
	{
		public UIWidget Widget_Pivot_Item;

		private ItemIconComponent _resultComp;

		public void InitUi(EquipItemInfo resultInfo)
		{
			ResetUi();
			if (resultInfo != null)
			{
				_resultComp = ItemIconComponent.PrefabAuto.Factory.Create(Widget_Pivot_Item.transform);
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_resultComp.transform);
				_resultComp.Open(resultInfo, null);
			}
		}

		public void ResetUi()
		{
			FactoryBase.SafeReleaseComponent(ref _resultComp);
		}
	}

	[Serializable]
	public class ChangingStarUi
	{
		public Color Color_Text_Success;

		public Color Color_Text_Fail;

		public Color Color_Text_Unchanged;

		public UILabel Label_StarBefore;

		public UILabel Label_StarAfter;

		public UISprite Sprite_Arrow_Success;

		public UISprite Sprite_Arrow_Fail;

		public void InitUi(int starBefore, int starAfter)
		{
			UIHelper.SetLabelText(Label_StarBefore, starBefore.ToString());
			UIHelper.SetLabelText(Label_StarAfter, starAfter.ToString());
			UIHelper.SetLabelColor(Label_StarBefore, Color_Text_Unchanged);
			if (starBefore < starAfter)
			{
				UIHelper.SetLabelColor(Label_StarAfter, Color_Text_Success);
			}
			else if (starBefore > starAfter)
			{
				UIHelper.SetLabelColor(Label_StarAfter, Color_Text_Fail);
			}
			else
			{
				UIHelper.SetLabelColor(Label_StarAfter, Color_Text_Unchanged);
			}
			bool flag = starBefore < starAfter;
			UIHelper.EnableGameObject(Sprite_Arrow_Success, flag);
			UIHelper.EnableGameObject(Sprite_Arrow_Fail, !flag);
		}
	}

	public const string Command_ShowItemIcon = "EquipAwakenScene/ShowItemIcon";

	public const string PrefabPath = "Resources/Prefab3/EquipLimitBreak/LimitBreakResultScene";

	[Header("[연출 화면]")]
	public RenderSceneUi RenderScene;

	[Header("[전체 Root]")]
	public UIWidget Widget_Root;

	[Header("[결과 Root]")]
	public UIWidget Widget_ResultRoot;

	[Header("[결과 Background Success]")]
	public UIWidget Widget_Result_Background_Success;

	[Header("[결과 Background Fail]")]
	public UIWidget Widget_Result_Background_Fail;

	[Header("[결과 아이템 아이콘]")]
	public ResultItemUi ResultItem;

	[Header("[결과 Star 표기]")]
	public ChangingStarUi ChangingStar;

	[Header("[변경되는 Stat 표기]")]
	public Embed_ChangingStatGrid_Comp ChangingStat;

	private bool _success;

	private EquipItemInfo _oldTargetInfo;

	private EquipItemInfo _resultInfo;

	bool ITouchDisabler.IsValid => IsOpen();

	public void InitScene(bool success, EquipItemInfo oldTargetInfo, EquipItemInfo resultInfo, Action callbackOnSceneChangeComplete)
	{
		ResetUi();
		if (resultInfo == null)
		{
			Vespa_System_Action_Extension_Method.fire(callbackOnSceneChangeComplete);
			Close();
			return;
		}
		ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(resultInfo.ItemIndex);
		if (data == null)
		{
			Vespa_System_Action_Extension_Method.fire(callbackOnSceneChangeComplete);
			Close();
			return;
		}
		EquipItemDetailData data2 = GameRoot.Instance.EquipItemDetailDataContainer.GetData(data.SubData.DetailIndex);
		if (data2 == null)
		{
			Vespa_System_Action_Extension_Method.fire(callbackOnSceneChangeComplete);
			Close();
			return;
		}
		_success = success;
		_oldTargetInfo = oldTargetInfo;
		_resultInfo = resultInfo;
		_resultInfo.LinkSoulWeaponInfo();
		string text = ((!success) ? data2.ForgeAwakeFailNodeTree : data2.ForgeAwakeSuccessNodeTree);
		if (String_Extension_Method.IsNullOrEmpty(text))
		{
			Vespa_System_Action_Extension_Method.fire(callbackOnSceneChangeComplete);
			InitUi_ResultScene();
		}
		else
		{
			VespaCoroutine.Start(this, RenderScene.coPlayForgeNodeTree(this, text, callbackOnSceneChangeComplete));
		}
	}

	public override void OnClosing()
	{
		base.OnClosing();
		ResetUi();
	}

	public override void Reset()
	{
		base.Reset();
		ResetUi();
	}

	private void ResetUi()
	{
		VespaCoroutine.StopAll(this);
		RenderScene.ResetUi();
		UIHelper.SetWidgetAlpha(Widget_Root, 0f);
		UIHelper.EnableGameObject(Widget_ResultRoot, enable: false);
		UIHelper.EnableGameObject(Widget_Result_Background_Success, enable: false);
		UIHelper.EnableGameObject(Widget_Result_Background_Fail, enable: false);
		ResultItem.ResetUi();
		ChangingStat.ResetUi();
		_oldTargetInfo = null;
		_resultInfo = null;
	}

	public void InitUi_ResultScene()
	{
		UIHelper.EnableGameObject(Widget_ResultRoot, enable: true);
		UIHelper.EnableGameObject(Widget_Result_Background_Success, _success);
		UIHelper.EnableGameObject(Widget_Result_Background_Fail, !_success);
		ResultItem.InitUi(_resultInfo);
		ChangingStar.InitUi(_oldTargetInfo.Star, _resultInfo.Star);
		StatData equipItemStatData = StatCalculator.GetEquipItemStatData(_oldTargetInfo);
		StatData equipItemStatData2 = StatCalculator.GetEquipItemStatData(_resultInfo);
		ChangingStat.InitUi(_oldTargetInfo, equipItemStatData, _resultInfo, equipItemStatData2);
	}

	public void OnClick_Close()
	{
		VespaCoroutine.Start(coClose(this));
	}

	private static IEnumerator coClose(LimitBreakResultScene This)
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
	}
}
