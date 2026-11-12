using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NMessage;
using NGame2.NMessage.NFastHandler;
using NGame2.NPet;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.ClassBuff;
using NGame2.NUI.NRender3D;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class Item3dModelPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/Item3dModelPopup";

	public const string Command_ShowExtraDesc = "Item/ShowExtraDesc";

	public float HeroStar_3StarInterval = 0.3f;

	public float HeroStar_5StarInterval = 0.3f;

	public float HeroStar_3StarCellWidth = 200f;

	public float HeroStar_5StarCellWidth = 100f;

	public TransformData extraTransform = new TransformData(100f);

	public UITexture CinemaTexture;

	public GameObject ItemNameRoot;

	public GameObject ItemNameSpriteRoot;

	public UILabel NameLabel;

	public TweenAlpha TweenAlpha_Label;

	public TweenAlpha TweenAlpha_Sprite;

	public TweenPosition TweenPos_Label;

	public TweenPosition TweenPos_Sprite;

	public UIGrid StarGrid;

	[Header("[Hero Info Group]")]
	public GameObject HeroInfoRoot;

	public UILabel HeroNameLabel;

	public UILabel SoulStoneNameLabel;

	public ClassTagIconComponent HeroClassIcon;

	public UIBundleLoader HeroPortrait;

	[HideInInspector]
	public List<UIGame3DWidget> StarEffectList = new List<UIGame3DWidget>();

	[Header("ExtraDesc")]
	public int ExtraDescHeightPadding = 172;

	public float ExtraDescOpenDelay = 0.3f;

	public GameObject ExtraDescRoot;

	public UISprite SpriteExtraDescFrame;

	public UILabel LabelExtraDescTitle;

	public UITable TableExtraDesc;

	private List<Item3dModelExtraDescComponent> _listExtraDescComponent = new List<Item3dModelExtraDescComponent>();

	private ItemData _itemData;

	private string[] _nodeTree;

	private Action _onClose;

	private int _numStar;

	private RenderTargetCinema _cinema;

	private bool _canClose;

	private bool _hasExtraDesc;

	private bool _canCloseExtraDesc;

	private WindowPrefab<Item3dModelPopup> _windowPrefab;

	private GlobalWindowPrefab<Item3dModelPopup> _globalWindowPrefab;

	private IClassBuffListener _iClassBuffListener;

	private Holder _receiverHolder = new Holder();

	public static Item3dModelPopup PreLoad(ItemData itemData, string[] nodeTree)
	{
		WindowPrefab<Item3dModelPopup> windowPrefab = new WindowPrefab<Item3dModelPopup>("Resources/Prefab2/Window/MessagePopup/Item3dModelPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance._windowPrefab = windowPrefab;
		windowPrefab.instance.PreLoadInternal(itemData, nodeTree);
		return windowPrefab.instance;
	}

	public static Item3dModelPopup PreLoadGlobal(ItemData itemData, string[] nodeTree)
	{
		GlobalWindowPrefab<Item3dModelPopup> globalWindowPrefab = new GlobalWindowPrefab<Item3dModelPopup>("Resources/Prefab2/Window/MessagePopup/Item3dModelPopup", GlobalUIRoot.ELayerType.GlobalUI);
		globalWindowPrefab.instance._globalWindowPrefab = globalWindowPrefab;
		globalWindowPrefab.instance.PreLoadInternal(itemData, nodeTree);
		return globalWindowPrefab.instance;
	}

	public void OpenPopup(Action onClose, int numStar = 0)
	{
		if (_windowPrefab != null)
		{
			_windowPrefab.instance.OpenInternal(onClose);
		}
		if (_globalWindowPrefab != null)
		{
			_globalWindowPrefab.instance.OpenInternal(onClose);
		}
		_iClassBuffListener = SceneAutoBehaviour<ClassBuffManagement>.instance.ClassBuffWindowManager;
		_receiverHolder.Add<CommandMessage>(new ObjVerifier(this), OnMessageFast);
		_numStar = numStar;
		UIHelper.EnableGameObject(StarGrid, enable: false);
	}

	private void OnMessageFast(CommandMessage message)
	{
		if (message.IsValidMessage() && message.IsValidScene(_cinema) && message.Command == "Item/ShowExtraDesc")
		{
			VespaCoroutine.Start(this, coShowExtraDesc());
		}
	}

	private IEnumerator coPlayStarEffect()
	{
		UIHelper.EnableGameObject(StarGrid, enable: true);
		for (int i = 0; i < StarEffectList.Count; i++)
		{
			UIHelper.EnableGameObject(StarEffectList[i], i < _numStar);
			StarEffectList[i].Stop(immediatelyClear: true);
		}
		float t = Mathf.Clamp01(0.5f * (float)(_numStar - 3));
		float interval = Mathf.Lerp(HeroStar_3StarInterval, HeroStar_5StarInterval, t);
		float cellWidth = Mathf.Lerp(HeroStar_3StarCellWidth, HeroStar_5StarCellWidth, t);
		yield return null;
		StarGrid.cellWidth = cellWidth;
		StarGrid.Reposition();
		yield return null;
		for (int index = 0; index < _numStar && index < StarEffectList.Count; index++)
		{
			StarEffectList[index].Play(null);
			yield return new WaitForSeconds(interval);
		}
	}

	private IEnumerator coShowExtraDesc()
	{
		if (_hasExtraDesc)
		{
			UIHelper.EnableGameObject(ExtraDescRoot, enable: true);
			yield return null;
			RepositionExtraDescTable();
			yield return null;
			_canCloseExtraDesc = true;
		}
	}

	private void OpenInternal(Action onClose)
	{
		OpenAndPlay(onClose);
	}

	private void OpenAndPlay(Action onClose)
	{
		_onClose = onClose;
		if (_itemData != null)
		{
			if (_itemData.Type == ItemType.SoulStone)
			{
				InitHeroInfo();
				UIHelper.EnableGameObject(HeroInfoRoot, enable: true);
				UIHelper.SetLabelTextLocalizeDirect(NameLabel, "SOULSTONE_GAIN");
				UIHelper.SetLabelText(SoulStoneNameLabel, _itemData.Name);
			}
			else
			{
				UIHelper.EnableGameObject(HeroInfoRoot, enable: false);
				UIHelper.SetLabelText(NameLabel, _itemData.Name);
			}
		}
		if (CinemaTexture != null && __Extension_Method_ICollection__.COUNT(_nodeTree) >= 2)
		{
			_canClose = false;
			VespaCoroutine.Start(this, coStartNodeTree(delegate
			{
				SetCanClose();
			}));
		}
		else
		{
			SetCanClose();
		}
	}

	private void InitHeroInfo()
	{
		int valueInt = _itemData.GetValueInt(0);
		if (valueInt != 0)
		{
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(valueInt);
			if (data != null)
			{
				UIHelper.SetLabelText(HeroNameLabel, data.Name);
				string battleHeroPortraitPath = UIHelper.GetBattleHeroPortraitPath(data.CodeName, 5, 0);
				HeroPortrait.SetAssetPath(battleHeroPortraitPath);
				HeroClassIcon.Open(data.TagType, null);
			}
		}
	}

	private void PreLoadInternal(ItemData itemData, string[] nodeTree)
	{
		VespaCoroutine.Start(coPreLoad(itemData, nodeTree));
	}

	private IEnumerator coPreLoad(ItemData itemData, string[] nodeTree)
	{
		_itemData = itemData;
		_nodeTree = nodeTree;
		SetExtraDesc(itemData);
		RenderTargetCinema.StockData stock = default(RenderTargetCinema.StockData);
		SingletonAuto<ResolutionInfo>.instance.GetScreenRenderTargetSize(out stock.RenderTargetWidth, out stock.RenderTargetHeight);
		stock.NodeTreePackCode = Vespa_String_Extension_Method.TRIM(nodeTree[0]);
		stock.NodeTreeName = Vespa_String_Extension_Method.TRIM(nodeTree[1]);
		RenderTargetCinema cinema = RenderTargetCinema.CreateInstance(stock);
		if (!(cinema == null))
		{
			do
			{
				yield return null;
			}
			while (!cinema.IsCompleteLoad());
			_cinema = cinema;
		}
	}

	private IEnumerator PlayItemNameAnimation()
	{
		float seconds = (float)GameRoot.Instance.ConstantDataContainer.GetValueInt("Item3DModelNameAnimationDelay", 500) / 1000f;
		yield return new WaitForSeconds(seconds);
		if (ItemNameRoot != null && ItemNameSpriteRoot != null && TweenAlpha_Label != null && TweenAlpha_Sprite != null && TweenPos_Label != null && TweenPos_Sprite != null)
		{
			UIHelper.EnableGameObject(ItemNameRoot.gameObject, enable: true);
			UIHelper.EnableGameObject(ItemNameSpriteRoot.gameObject, enable: true);
			TweenAlpha_Label.PlayForward();
			TweenAlpha_Sprite.PlayForward();
			TweenPos_Label.PlayForward();
			TweenPos_Sprite.PlayForward();
		}
		yield return null;
	}

	private void SetCanClose()
	{
		_canClose = true;
	}

	private IEnumerator coStartNodeTree(Action onComplete)
	{
		do
		{
			yield return null;
		}
		while (!(_cinema != null) || !_cinema.IsCompleteLoad());
		UIHelper.EnableGameObject(CinemaTexture, enable: false);
		base.Open();
		if (_globalWindowPrefab != null)
		{
			GlobalAutoBehaviour<PopupManegement>.instance.AddGlobal(_globalWindowPrefab);
		}
		if (_windowPrefab != null)
		{
			GlobalAutoBehaviour<PopupManegement>.instance.Add(_windowPrefab);
		}
		VespaCoroutine.Start(this, PlayItemNameAnimation());
		if (_numStar > 0)
		{
			VespaCoroutine.Start(this, coPlayStarEffect());
		}
		_cinema.StartNodeTree();
		UIHelper.EnableGameObject(CinemaTexture, enable: true);
		CinemaTexture.mainTexture = _cinema.RenderTarget;
		CinemaTexture.alpha = 1f;
		Vespa_System_Action_Extension_Method.fire(onComplete);
	}

	public override void Reset()
	{
		base.Reset();
		if (ItemNameRoot != null && ItemNameSpriteRoot != null && TweenAlpha_Label != null && TweenAlpha_Sprite != null && TweenPos_Label != null && TweenPos_Sprite != null)
		{
			UIHelper.EnableGameObject(ItemNameRoot.gameObject, enable: false);
			UIHelper.EnableGameObject(ItemNameSpriteRoot.gameObject, enable: false);
			TweenAlpha_Label.ResetToBeginning();
			TweenAlpha_Sprite.ResetToBeginning();
			TweenPos_Label.ResetToBeginning();
			TweenPos_Sprite.ResetToBeginning();
		}
		if (CinemaTexture != null)
		{
			CinemaTexture.mainTexture = null;
		}
		_canCloseExtraDesc = false;
		UIHelper.EnableGameObject(ExtraDescRoot, enable: false);
		UIHelper.EnableGameObject(StarGrid, enable: false);
		VespaCoroutine.StopAll(this);
	}

	private new void CloseInternal()
	{
		if (_canClose)
		{
			Vespa_System_Action_Extension_Method.fire(_onClose);
			_onClose = null;
			if (_windowPrefab != null)
			{
				_windowPrefab = null;
			}
			if (_globalWindowPrefab != null)
			{
				_globalWindowPrefab = null;
			}
			if (_cinema != null)
			{
				RenderTargetCinema.ReleaseInstance(_cinema);
				_cinema = null;
			}
			for (int i = 0; i < _numStar && i < StarEffectList.Count; i++)
			{
				StarEffectList[i].Stop(immediatelyClear: true);
			}
			FactoryBase.ReleaseAndClearList(_listExtraDescComponent);
			Close();
			if (_iClassBuffListener != null)
			{
				_iClassBuffListener.AddPoint(_itemData);
			}
		}
	}

	public void OnClickClose()
	{
		CloseInternal();
	}

	public override void OnSystemBackButton()
	{
		CloseInternal();
	}

	public void OnClickExtraDescClose()
	{
		if (_canCloseExtraDesc)
		{
			HideExtraDesc();
		}
	}

	private void SetExtraDesc(ItemData itemData)
	{
		FactoryBase.ReleaseAndClearList(_listExtraDescComponent);
		_hasExtraDesc = false;
		if (itemData != null)
		{
			if (itemData.Type == ItemType.Pet)
			{
				UIHelper.SetLabelTextLocalizeDirect(LabelExtraDescTitle, "RAIDER_ENGRAVE_DESC_TITLE");
				AddPetBuffDesc(itemData.Index);
			}
			TableExtraDesc.Reposition();
			TableExtraDesc.repositionNow = true;
		}
	}

	private void AddExtraDescComponent(UITable table, string title, string value)
	{
		Item3dModelExtraDescComponent item3dModelExtraDescComponent = Item3dModelExtraDescComponent.PrefabAuto.Factory.Create(table.transform);
		item3dModelExtraDescComponent.Open(title, value);
		_listExtraDescComponent.Add(item3dModelExtraDescComponent);
	}

	private void HideExtraDesc()
	{
		UIHelper.EnableGameObject(ExtraDescRoot, enable: false);
	}

	private void RepositionExtraDescTable()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listExtraDescComponent); i++)
		{
			Item3dModelExtraDescComponent item3dModelExtraDescComponent = _listExtraDescComponent[i];
			if (!(item3dModelExtraDescComponent == null))
			{
				item3dModelExtraDescComponent.Resize();
			}
		}
		TableExtraDesc.Reposition();
		TableExtraDesc.repositionNow = true;
		UIHelper.TableCalcBounds(TableExtraDesc, SpriteExtraDescFrame, isFixedWidth: true, ExtraDescHeightPadding);
	}

	private void AddPetBuffDesc(int petIndex)
	{
		List<PetManager.PetBuffDesc> petBuffDesc = GlobalAutoBehaviour<PetManager>.instance.GetPetBuffDesc(petIndex);
		_hasExtraDesc = __Extension_Method_ICollection__.COUNT(petBuffDesc) > 0;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(petBuffDesc); i++)
		{
			PetManager.PetBuffDesc petBuffDesc2 = petBuffDesc[i];
			if (petBuffDesc2 != null)
			{
				AddExtraDescComponent(TableExtraDesc, petBuffDesc2.OptionName, petBuffDesc2.OptionValue);
			}
		}
	}
}
