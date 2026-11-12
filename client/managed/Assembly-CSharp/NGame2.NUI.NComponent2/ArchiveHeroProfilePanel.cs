using System.Collections;
using System.Collections.Generic;
using System.Text;
using NGame2.NAccount;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class ArchiveHeroProfilePanel : MonoBehaviour, IHeroPanel, IArchiveHeroSubPanel
{
	public interface Listener
	{
		void OpenMonthlyHeroWindow(int creatureIndex);
	}

	public const string PrefabPath = "Resources/Prefab2/Component/Hero/ArchiveHeroProfilePanel";

	public static PrefabAuto<ArchiveHeroProfilePanel> PrefabAuto = new PrefabAuto<ArchiveHeroProfilePanel>("Resources/Prefab2/Component/Hero/ArchiveHeroProfilePanel");

	public UIScrollView StatScroll;

	public UITable HeroStatTable;

	public UITable HeroProfileTable;

	public UISprite SpriteProfileBg;

	public CachedHeroInfo CachedHeroInfo;

	public UILabel Label_BaseInfo;

	public UIWidget RootWidget;

	public UIButton MoveSubStoryButton;

	private List<LabelFieldComponent> _baseStatList = new List<LabelFieldComponent>();

	private Listener _listener;

	private bool _isEnableButtonInfo;

	public void Open(CachedHeroInfo cachedHeroInfo, Listener listener, bool isEnableButtonInfo = false)
	{
		Reset();
		base.gameObject.SetActive(value: true);
		_listener = listener;
		Setup(cachedHeroInfo, isEnableButtonInfo);
	}

	public void Reset()
	{
		_listener = null;
		ClearProfileComponentList();
	}

	public void Close()
	{
		base.gameObject.SetActive(value: false);
		Reset();
	}

	public void Enable(bool enable, CachedHeroInfo cachedHeroInfo, IArchiveHeroSubPanelListener listener = null)
	{
		if (enable)
		{
			Reset();
			base.gameObject.SetActive(value: true);
			Setup(cachedHeroInfo);
		}
		else
		{
			Close();
		}
	}

	public void Change(CachedHeroInfo cachedHeroInfo, IArchiveHeroSubPanelListener listener = null)
	{
		Setup(cachedHeroInfo);
	}

	private void Setup(CachedHeroInfo cachedHeroInfo, bool isEnableButtonInfo = false)
	{
		CachedHeroInfo = cachedHeroInfo;
		_isEnableButtonInfo = isEnableButtonInfo;
		SetupProfile();
		CalculateTableBounds();
		ResetAndUpdateScroll();
		VespaCoroutine.Start(coRepositionTable(), null, null);
	}

	private IEnumerator coRepositionTable()
	{
		yield return null;
		RepositionTable();
	}

	private void CalculateTableBounds()
	{
		if ((bool)HeroProfileTable && SpriteProfileBg != null)
		{
			UIHelper.TableCalcBounds(HeroProfileTable, SpriteProfileBg, isFixedWidth: true);
		}
	}

	private void RepositionTable()
	{
		if ((bool)HeroProfileTable)
		{
			HeroProfileTable.Reposition();
			HeroProfileTable.repositionNow = true;
		}
		if ((bool)HeroStatTable)
		{
			HeroStatTable.Reposition();
			HeroStatTable.repositionNow = true;
		}
	}

	private void ResetAndUpdateScroll()
	{
		StatScroll.ResetPosition();
		StatScroll.UpdatePosition();
	}

	private void ClearProfileComponentList()
	{
		FactoryBase.ReleaseAndClearList(_baseStatList);
	}

	private void SetupProfile()
	{
		ClearProfileComponentList();
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(CachedHeroInfo.HeroIndex);
		HeroProfileData data2 = GameRoot.Instance.HeroProfileDataContainer.GetData(CachedHeroInfo.HeroIndex);
		if (data != null && data2 != null)
		{
			SetupHeroProfile(data, data2);
			SetupHeroStory(data2.Story);
			SetMoveSubStoryButtonState();
		}
	}

	private void SetupHeroProfile(CreatureData creatureData, HeroProfileData heroProfileData)
	{
		string stringDirect = LocalizationManager.GetStringDirect("ARCHIVE_HERO_NAME");
		AddProfileComponent(HeroProfileTable, stringDirect, creatureData.Name);
		string stringDirect2 = LocalizationManager.GetStringDirect("ARCHIVE_HERO_TITLE");
		AddProfileComponent(HeroProfileTable, stringDirect2, creatureData.Title);
		string stringDirect3 = LocalizationManager.GetStringDirect("ARCHIVE_HERO_GENDER");
		string stringDirect4 = LocalizationManager.GetStringDirect(heroProfileData.Gender);
		AddProfileComponent(HeroProfileTable, stringDirect3, stringDirect4);
		string stringDirect5 = LocalizationManager.GetStringDirect("ARCHIVE_HERO_RACE");
		string stringDirect6 = LocalizationManager.GetStringDirect(heroProfileData.Race);
		AddProfileComponent(HeroProfileTable, stringDirect5, stringDirect6);
		string stringDirect7 = LocalizationManager.GetStringDirect("ARCHIVE_HERO_AGE");
		string empty = string.Empty;
		AddProfileComponent(content: (!int.TryParse(heroProfileData.Age, out var result)) ? LocalizationManager.GetStringDirect(heroProfileData.Age) : LocalizationManager.GetStringDirect("ARCHIVE_HERO_AGE_DESC", result), table: HeroProfileTable, title: stringDirect7);
		string stringDirect8 = LocalizationManager.GetStringDirect("ARCHIVE_HERO_LENGTH");
		string stringDirect9 = LocalizationManager.GetStringDirect("ARCHIVE_HERO_LENGTH_DESC", heroProfileData.Length);
		AddProfileComponent(HeroProfileTable, stringDirect8, stringDirect9);
		SetBirthDayInfo(HeroProfileTable, heroProfileData);
		string stringDirect10 = LocalizationManager.GetStringDirect("ARCHIVE_HERO_CONSTELL");
		string stringDirect11 = LocalizationManager.GetStringDirect(heroProfileData.Constellation);
		AddProfileComponent(HeroProfileTable, stringDirect10, stringDirect11);
		string stringDirect12 = LocalizationManager.GetStringDirect("ARCHIVE_HERO_LIKE");
		string stringDirect13 = LocalizationManager.GetStringDirect(heroProfileData.Like);
		AddProfileComponent(HeroProfileTable, stringDirect12, stringDirect13);
		string stringDirect14 = LocalizationManager.GetStringDirect("ARCHIVE_HERO_UNLIKE");
		string stringDirect15 = LocalizationManager.GetStringDirect(heroProfileData.Unlike);
		AddProfileComponent(HeroProfileTable, stringDirect14, stringDirect15);
		HeroProfileTable.Reposition();
		HeroProfileTable.repositionNow = true;
	}

	private void SetupHeroStory(string story)
	{
		string stringDirect = LocalizationManager.GetStringDirect("ARCHIVE_HERO_STORY");
		string stringDirect2 = LocalizationManager.GetStringDirect(story);
		LabelFieldComponent labelFieldComponent = LabelFieldComponent.PrefabAuto3.Factory.Create(HeroStatTable.transform);
		labelFieldComponent.Open(stringDirect, stringDirect2);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(labelFieldComponent.transform);
		_baseStatList.Add(labelFieldComponent);
		HeroStatTable.Reposition();
		HeroStatTable.repositionNow = true;
	}

	private void AddProfileComponent(UITable table, string title, string content)
	{
		LabelFieldResizedComponent labelFieldResizedComponent = LabelFieldResizedComponent.PrefabAutoResized.Factory.Create(table.transform);
		labelFieldResizedComponent.Open(title, content);
		_baseStatList.Add(labelFieldResizedComponent);
	}

	private void SetBirthDayInfo(UITable table, HeroProfileData heroProfileData)
	{
		if (heroProfileData == null)
		{
			return;
		}
		string stringDirect = LocalizationManager.GetStringDirect("ARCHIVE_HERO_BIRTH");
		string localMonthName = NGame2.NUI.Common.GetLocalMonthName(heroProfileData.Birth[0]);
		string ordinalizeDay = NGame2.NUI.Common.GetOrdinalizeDay(heroProfileData.Birth[1]);
		string stringDirect2 = LocalizationManager.GetStringDirect("ARCHIVE_HERO_BIRTH_DESC", localMonthName, ordinalizeDay);
		string stringDirect3 = LocalizationManager.GetStringDirect(heroProfileData.MonthText);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(stringDirect2).Append(", ").Append(stringDirect3);
		LabelFieldComponent.FieldButtonInfo fieldButtonInfo = null;
		if (_isEnableButtonInfo)
		{
			fieldButtonInfo = new LabelFieldComponent.FieldButtonInfo(delegate
			{
				if (_listener != null)
				{
					_listener.OpenMonthlyHeroWindow(heroProfileData.Index);
				}
			}, LocalizationManager.GetStringDirect("SUBSTORY_SUB_BUTTON_NAME"), GlobalAutoBehaviour<AccountManager>.instance.MonthlyHeroManager.IsExistCurrentMonthlyHero(heroProfileData.Index));
		}
		LabelFieldResizedComponent labelFieldResizedComponent = LabelFieldResizedComponent.PrefabAutoResized.Factory.Create(table.transform);
		labelFieldResizedComponent.Open(stringDirect, stringBuilder.ToString(), fieldButtonInfo);
		_baseStatList.Add(labelFieldResizedComponent);
	}

	private void SetMoveSubStoryButtonState()
	{
		bool enable = false;
		if (CachedHeroInfo != null)
		{
			List<SubStoryData> onlySubStoryDataList = GameRoot.Instance.SubStoryDataContainer.GetOnlySubStoryDataList();
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(onlySubStoryDataList))
			{
				enable = onlySubStoryDataList.Find((SubStoryData data) => data.CreatureIndex == CachedHeroInfo.HeroIndex) != null;
			}
		}
		UIHelper.EnableButton(MoveSubStoryButton, enable);
	}

	public void OnClickEnterStory()
	{
		SceneAutoBehaviour<SubStoryManagement>.instance.OpenSubStoryCharacterScene(CachedHeroInfo.HeroIndex);
	}
}
