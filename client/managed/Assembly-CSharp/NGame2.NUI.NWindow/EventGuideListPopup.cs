using System.Collections;
using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class EventGuideListPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/Help/EventGuideListPopup";

	public UITable Table;

	public UIScrollView ScrollView;

	public UILabel LabelTitle;

	public UIBundleLoader BundleLoader;

	public UIButton ButtonPrev;

	public UISprite SpritePrev;

	public UIButton ButtonNext;

	public UISprite SpriteNext;

	private string _code;

	private int _currentPageNum;

	private int _maxPageNum;

	private List<BaseHelpComponent> _elementCompList = new List<BaseHelpComponent>();

	public static void Open(string code)
	{
		WindowPrefab<EventGuideListPopup> windowPrefab = new WindowPrefab<EventGuideListPopup>("Resources/Prefab2/Window/Help/EventGuideListPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(code);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	protected void OpenInternal(string code)
	{
		Open(delegate
		{
			Init(code);
		}, null);
	}

	public override void Close()
	{
		if (ScrollView != null)
		{
			ScrollView.ResetPosition();
		}
		ClearPageElement();
		_currentPageNum = 0;
		_code = "";
		base.Close();
	}

	private void Init(string code)
	{
		_maxPageNum = GameRoot.Instance.HelpListDataContainer.GetMaxPageCountByCode(code);
		if (_maxPageNum != 0)
		{
			_code = code;
			SetPage(code, 1);
		}
	}

	private void SetHelpTitle(string titleKey)
	{
		string stringDirect = LocalizationManager.GetStringDirect(titleKey);
		UIHelper.SetLabelText(LabelTitle, stringDirect);
	}

	private void SetHelpBackgroundTexuture(string backgroundTexturePath)
	{
		UIHelper.SetBundleTexture(BundleLoader, backgroundTexturePath);
	}

	private void SetPage(string code, int pageNumber)
	{
		HelpListData data = GameRoot.Instance.HelpListDataContainer.GetData(new HelpListDataKey(code, pageNumber));
		if (data != null && _currentPageNum != pageNumber)
		{
			_currentPageNum = pageNumber;
			SetButton();
			SetHelpTitle(data.MainTitleKey);
			SetHelpBackgroundTexuture(data.BackgroundTexturePath);
			SetScrollView(data.ScrollMovement);
			SetPageElemnt(data.GetAllElements());
			VespaCoroutine.Start(TableRepostion());
		}
	}

	private IEnumerator TableRepostion()
	{
		yield return null;
		if (Table != null)
		{
			Table.Reposition();
		}
		Table.repositionNow = true;
		yield return null;
		if (Table != null)
		{
			Table.Reposition();
		}
		Table.repositionNow = true;
	}

	private void SetButton()
	{
		if (SpritePrev != null)
		{
			SpritePrev.isGrayscale = _currentPageNum == 1;
			UIHelper.EnableGameObject(SpritePrev, enable: false);
			UIHelper.EnableGameObject(SpritePrev, enable: true);
		}
		if (SpriteNext != null)
		{
			SpriteNext.isGrayscale = _currentPageNum == _maxPageNum;
			UIHelper.EnableGameObject(SpriteNext, enable: false);
			UIHelper.EnableGameObject(SpriteNext, enable: true);
		}
	}

	private void SetScrollView(string strMovementType)
	{
		if (!(Table == null) && !(ScrollView == null))
		{
			UIScrollView.Movement movement = EnumParser.Parse<UIScrollView.Movement>(strMovementType);
			switch (movement)
			{
			case UIScrollView.Movement.Horizontal:
				ScrollView.ResetPosition();
				ScrollView.movement = movement;
				Table.columns = 0;
				break;
			case UIScrollView.Movement.Vertical:
				ScrollView.ResetPosition();
				ScrollView.movement = movement;
				Table.columns = 1;
				break;
			}
		}
	}

	private void ClearPageElement()
	{
		FactoryBase.ReleaseAndClearList(_elementCompList);
		_elementCompList.Clear();
	}

	private void SetPageElemnt(List<HelpListPageElement> listPageElements)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(listPageElements))
		{
			return;
		}
		ClearPageElement();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(listPageElements); i++)
		{
			HelpListPageElement helpListPageElement = listPageElements[i];
			if (helpListPageElement != null)
			{
				BaseHelpComponent elemntComponent = GetElemntComponent(helpListPageElement.ComponentName);
				if (!(elemntComponent == null))
				{
					elemntComponent.Open(helpListPageElement);
					_elementCompList.Add(elemntComponent);
				}
			}
		}
	}

	private BaseHelpComponent GetElemntComponent(string elementName)
	{
		if (ScrollView.movement == UIScrollView.Movement.Vertical)
		{
			if (elementName.Contains("Label"))
			{
				return HelpLabelComponent.PrefabAutoVertical.Factory.Create(Table.transform);
			}
			return HelpTextureComponent.PrefabAutoVertical.Factory.Create(Table.transform);
		}
		if (ScrollView.movement == UIScrollView.Movement.Horizontal)
		{
			if (elementName.Contains("Label"))
			{
				return HelpLabelComponent.PrefabAutoHorizontal.Factory.Create(Table.transform);
			}
			return HelpTextureComponent.PrefabAutoHorizontal.Factory.Create(Table.transform);
		}
		return null;
	}

	public void OnClickPrev()
	{
		if (_currentPageNum != 1)
		{
			SetPage(_code, _currentPageNum - 1);
		}
	}

	public void OnClickNext()
	{
		if (_currentPageNum != _maxPageNum)
		{
			SetPage(_code, _currentPageNum + 1);
		}
	}
}
