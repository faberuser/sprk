using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NContents;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class GuideMenu : BaseWindow, LobbyTopmostMenu.IMiniGuideButtonObserver, GuideMenuComponent.Listener
{
	public interface Listener
	{
		void OnClickShortCut(GameGuideData data);
	}

	[Serializable]
	public class QA
	{
		public const int MaxLabelWidth = 1280;

		public const int MinLabelWidth = 1190;

		public Transform Root;

		public UILabel Question;

		public UILabel Answer;

		public void SetContents(string qKey, string aKey)
		{
			string stringDirect = LocalizationManager.GetStringDirect(qKey);
			string stringDirect2 = LocalizationManager.GetStringDirect(aKey);
			bool flag = String_Extension_Method.IsNullOrEmpty(stringDirect) || String_Extension_Method.IsNullOrEmpty(stringDirect2);
			UIHelper.EnableGameObject(Root, !flag);
			UIHelper.SetLabelText(Question, stringDirect);
			UIHelper.SetLabelText(Answer, stringDirect2);
		}

		public void SetAnswerLabelWidth(int width)
		{
			Answer.width = width;
		}
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Tutorial/GuideMenu";

	public UIBaseGrid Grid_GuideMenu;

	private List<GuideMenuComponent> _guideMenuList = new List<GuideMenuComponent>();

	public Transform Texture_Root;

	public UIBundleLoader TextureBundle_GuideImage;

	public Transform Summary_Root;

	public UILabel Label_Summary;

	public QA[] QAContents;

	public UIScrollView Scroll_Qa;

	public UITable Table_Qa;

	public UIButton Button_ShortCut;

	private int _guideType;

	private Listener _listener;

	private GameGuideData _selectedGuideData;

	private GuideMenuComponent _selectedComponent;

	private int _selectedIndex;

	private bool _openByMiniGuide;

	public new int WindowGuideIndex => 0;

	public override void Reset()
	{
		base.Reset();
		_listener = null;
		_selectedIndex = 0;
		_openByMiniGuide = false;
		_selectedComponent = null;
		ClearList();
	}

	private void ClearList()
	{
		FactoryBase.ReleaseAndClearList(_guideMenuList);
	}

	public void Open(Listener listener, GameGuideType guidType)
	{
		Open(delegate
		{
			_listener = listener;
			_guideType = (int)guidType;
			Init();
		}, delegate
		{
			ScrollToMenu(_selectedComponent);
		});
	}

	public void Open(Listener listener, int selectIndex)
	{
		Open(delegate
		{
			_listener = listener;
			GameGuideData data = GameRoot.Instance.GameGuideDataContainer.GetData(selectIndex);
			if (data != null)
			{
				_openByMiniGuide = true;
				_selectedIndex = selectIndex;
				_guideType = data.GuideType;
			}
			else
			{
				_openByMiniGuide = false;
				_selectedIndex = 0;
				_guideType = 1;
			}
			Init();
		}, delegate
		{
			ScrollToMenu(_selectedComponent);
		});
	}

	private void Init()
	{
		IList<GameGuideData> listByGuideType = GameRoot.Instance.GameGuideDataContainer.GetListByGuideType(_guideType);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(listByGuideType); i++)
		{
			GuideMenuComponent guideMenuComponent = UIHelper.AddGridItem(Grid_GuideMenu, GuideMenuComponent.PrefabAuto);
			guideMenuComponent.Open(this, listByGuideType[i]);
			_guideMenuList.Add(guideMenuComponent);
			bool flag = ((_selectedIndex != 0) ? (listByGuideType[i].Index == _selectedIndex) : (i == 0));
			guideMenuComponent.SetSelect(flag);
			if (flag)
			{
				_selectedComponent = guideMenuComponent;
			}
		}
		UIHelper.RepositionGrid(Grid_GuideMenu);
	}

	private void SetContents(GameGuideData data)
	{
		if (data != null)
		{
			_selectedGuideData = data;
			SetGuideTexture(_selectedGuideData.GuideImage);
			SetSummary(data.SummaryKEY);
			SetQATable();
			SetShortCutButton();
			SetLastLabelWidth();
		}
	}

	private void SetShortCutButton()
	{
		bool enable = !_openByMiniGuide && !String_Extension_Method.IsNullOrEmpty(_selectedGuideData.MoveTaskType);
		UIHelper.EnableGameObject(Button_ShortCut, enable);
	}

	private void SetQATable()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(QAContents); i++)
		{
			_selectedGuideData.GetQandA(i, out var q, out var a);
			QAContents[i].SetContents(q, a);
		}
		ResetPositionNextFrame();
	}

	private void SetLastLabelWidth()
	{
		bool flag = UIHelper.IsEnabledGameObjet(Button_ShortCut);
		int num = -1;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(QAContents); i++)
		{
			if (UIHelper.IsEnabledGameObjet(QAContents[i].Root))
			{
				QAContents[i].SetAnswerLabelWidth(1280);
				num = i;
			}
		}
		if (num != -1)
		{
			QAContents[num].SetAnswerLabelWidth(flag ? 1190 : 1280);
		}
	}

	private void ResetPositionNextFrame()
	{
		VespaCoroutine.Start(coResetPostionNextFrame(), null, null);
	}

	private IEnumerator coResetPostionNextFrame()
	{
		for (int i = 0; i < 3; i++)
		{
			yield return null;
			Table_Qa.Reposition();
			Scroll_Qa.ResetPosition();
		}
	}

	private void SetGuideTexture(string path)
	{
		UIHelper.EnableGameObject(Texture_Root, !String_Extension_Method.IsNullOrEmpty(path));
		UIHelper.SetBundleTexture(TextureBundle_GuideImage, path);
	}

	private void SetSummary(string key)
	{
		string stringDirect = LocalizationManager.GetStringDirect(key);
		UIHelper.EnableGameObject(Summary_Root, !String_Extension_Method.IsNullOrEmpty(stringDirect));
		UIHelper.SetLabelText(Label_Summary, stringDirect);
	}

	private bool IsOpenedContents(string moveTaskType, out string errorMsg)
	{
		errorMsg = string.Empty;
		if (moveTaskType == Vespa_String_Extension_Method.GetEnumString(ClientActivityType.OpenRaid) || moveTaskType == Vespa_String_Extension_Method.GetEnumString(ClientActivityType.OpenChallengeRaid))
		{
			if (!SingletonAuto<MenuOpenCheckManager>.instance.IsOpen(ERightMenuButton.Raid))
			{
				errorMsg = LocalizationManager.GetStringDirect("FAILED_MISSION_MOVE_NOT_OPEN_RAID");
				return false;
			}
		}
		else if (moveTaskType == Vespa_String_Extension_Method.GetEnumString(ClientActivityType.OpenWorldBoss))
		{
			if (!SingletonAuto<MenuOpenCheckManager>.instance.IsOpen(ERightMenuButton.WorldBoss))
			{
				errorMsg = LocalizationManager.GetStringDirect("CONTENT_NOT_OPEN_WORLDBOSS");
				return false;
			}
		}
		else if (moveTaskType == Vespa_String_Extension_Method.GetEnumString(ClientActivityType.OpenEclipse))
		{
			if (!SingletonAuto<MenuOpenCheckManager>.instance.IsOpen(ERightMenuButton.Eclipse))
			{
				errorMsg = LocalizationManager.GetStringDirect("SOULWEAPON_UI_QUICKMENU_DESC");
				return false;
			}
		}
		else if (moveTaskType == Vespa_String_Extension_Method.GetEnumString(ClientActivityType.OpenGodkingTrial) && !SingletonAuto<MenuOpenCheckManager>.instance.IsOpen(ERightMenuButton.GodkingTrial))
		{
			errorMsg = LocalizationManager.GetStringDirect("SOULWEAPON_UI_QUICKMENU_DESC");
			return false;
		}
		return true;
	}

	private void ScrollToMenu(GuideMenuComponent selectedComponent)
	{
		if (!(selectedComponent == null))
		{
			VespaCoroutine.Start(coScrollToMenu(selectedComponent), null, null);
		}
	}

	private IEnumerator coScrollToMenu(GuideMenuComponent selectedComponent)
	{
		yield return null;
		yield return null;
		yield return null;
		Grid_GuideMenu.ScrollTo(selectedComponent, 0f, 0f);
	}

	void GuideMenuComponent.Listener.OnClick(GuideMenuComponent caller)
	{
		SetContents(caller.GameGuideData);
	}

	public void OnClickClose()
	{
		Close();
	}

	public override void OnSystemBackButton()
	{
		OnClickClose();
	}

	public void OnClickShortCut()
	{
		if (_selectedGuideData == null || _listener == null)
		{
			return;
		}
		string errorMsg;
		if (TopmostMenuType == ETopmostMenuType.Tutorial_Guide)
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("TUTORIAL_RAIDPARTYSETTING_3"));
		}
		else if (!IsOpenedContents(_selectedGuideData.MoveTaskType, out errorMsg))
		{
			if (!String_Extension_Method.IsNullOrEmpty(errorMsg))
			{
				CenterMessage.Open(errorMsg);
			}
		}
		else
		{
			_listener.OnClickShortCut(_selectedGuideData);
			Close();
		}
	}
}
