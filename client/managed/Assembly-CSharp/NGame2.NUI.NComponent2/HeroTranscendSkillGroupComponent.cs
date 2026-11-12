using System.Collections.Generic;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class HeroTranscendSkillGroupComponent : UIScrollItemBehaviour, InitOnCreate, HeroTranscendSkillIconComponent.Listener
{
	public interface Listener
	{
		void OnSelectComponent(int transcended, int skillSlot, int skillIndex);
	}

	public const string PrefabPath = "Prefab2/Component/Hero/HeroTranscendSkillGroupComponent";

	public static PrefabAuto<HeroTranscendSkillGroupComponent> PrefabAuto = new PrefabAuto<HeroTranscendSkillGroupComponent>("Prefab2/Component/Hero/HeroTranscendSkillGroupComponent");

	public UIWidget RootWidget;

	public UILabel Label_HeroTranscended;

	public UISimpleGrid SkillListGrid;

	public UIWidget WidgetClosed;

	public float widthPadding;

	private List<HeroTranscendSkillIconComponent> _listSkillIConComponent = new List<HeroTranscendSkillIconComponent>();

	private HeroInfo _heroInfo;

	private CreatureData _CreatureData;

	private Listener _listener;

	private int LineCount = 1;

	public int Transcended { get; private set; }

	private int Page { get; set; }

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		if (LineCount > 1)
		{
			RootWidget.height -= (int)SkillListGrid.Grid.cellHeight;
		}
		ResetSkillIconList();
	}

	private void ResetSkillIconList()
	{
		FactoryBase.ReleaseAndClearList(_listSkillIConComponent);
	}

	public void Open(HeroInfo heroInfo, int page, int transcended, Listener listener, bool readOnly = false)
	{
		Init(heroInfo, page, transcended, listener, readOnly);
	}

	public void OpenForArchive(HeroInfo heroInfo, int transcended, Listener listener)
	{
		Init(heroInfo, 0, transcended, listener, readOnly: true, isViewer: true);
	}

	public void OpenForRecommend(HeroInfo heroInfo, int transcended, List<int> recommendTranscendIndexs)
	{
		if (heroInfo == null)
		{
			return;
		}
		_heroInfo = heroInfo;
		Transcended = transcended;
		_CreatureData = GameRoot.Instance.CreatureDataContainer.GetData(_heroInfo.HeroIndex);
		if (_CreatureData != null)
		{
			UIHelper.EnableGameObject(WidgetClosed, enable: false);
			SetupGroupName();
			SetupRecommendTranscendSkillIconList(recommendTranscendIndexs);
			LineCount = (_listSkillIConComponent.Count - 1) / SkillListGrid.Grid.maxPerLine + 1;
			if (LineCount > 1)
			{
				RootWidget.height += (int)SkillListGrid.Grid.cellHeight;
			}
		}
	}

	private void Init(HeroInfo heroInfo, int page, int transcended, Listener listener, bool readOnly, bool isViewer = false)
	{
		if (heroInfo == null)
		{
			return;
		}
		_heroInfo = heroInfo;
		Transcended = transcended;
		_listener = listener;
		Page = page;
		_CreatureData = GameRoot.Instance.CreatureDataContainer.GetData(_heroInfo.HeroIndex);
		if (_CreatureData != null)
		{
			UIHelper.EnableGameObject(WidgetClosed, enable: false);
			SetupGroupName();
			SetupTranscendSkillIconList(readOnly, isViewer);
			LineCount = (_listSkillIConComponent.Count - 1) / SkillListGrid.Grid.maxPerLine + 1;
			if (LineCount > 1)
			{
				RootWidget.height += (int)SkillListGrid.Grid.cellHeight;
			}
		}
	}

	public void Refresh()
	{
		if (_heroInfo != null)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listSkillIConComponent); i++)
			{
				_listSkillIConComponent[i].Refresh();
			}
		}
	}

	public void OpenDisabled(int transcended)
	{
		_heroInfo = null;
		_listener = null;
		Transcended = transcended;
		UIHelper.EnableGameObject(WidgetClosed, enable: true);
		SetupGroupName();
	}

	private void SetupGroupName()
	{
		string text = "";
		text = Transcended.ToString();
		UIHelper.SetLabelText(Label_HeroTranscended, LocalizationManager.GetString(HeroTranscend.TRANSCEND_SKILL_LINE_NUM, text));
	}

	private int GetSkillGridMaxLine()
	{
		string format = "MaxTranscendLine{0}";
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt(string.Format(format, Transcended));
		if (valueInt != 0)
		{
			return valueInt;
		}
		return 4;
	}

	private bool IsLockIconCreate(int index)
	{
		string key = $"TranscendLockIcon{Transcended}_{index + 1}";
		return GameRoot.Instance.ConstantDataContainer.GetValueBool(key);
	}

	private void SetupTranscendSkillIconList(bool readOnly, bool isViewer = false)
	{
		int[] transcendSkill = _CreatureData.GetTranscendSkill(Transcended);
		if (transcendSkill == null)
		{
			return;
		}
		ChangeTranscendSkillScrollScaleFix();
		for (int i = 0; i < transcendSkill.Length; i++)
		{
			HeroTranscendSkillIconComponent heroTranscendSkillIconComponent = UIHelper.AddGridItem(SkillListGrid, HeroTranscendSkillIconComponent.PrefabAuto);
			CreatureData.TranscendSkill transcendSkillData = _CreatureData.GetTranscendSkillData(Transcended, i);
			if (transcendSkillData == null)
			{
				continue;
			}
			heroTranscendSkillIconComponent.Open(_heroInfo, Page, transcendSkillData, this, readOnly);
			_listSkillIConComponent.Add(heroTranscendSkillIconComponent);
			if (IsLockIconCreate(i))
			{
				HeroTranscendLockIconComponent heroTranscendLockIconComponent = UIHelper.AddGridItem(SkillListGrid, HeroTranscendLockIconComponent.PrefabAuto);
				if (heroTranscendLockIconComponent != null)
				{
					_listSkillIConComponent.Add(heroTranscendLockIconComponent);
				}
			}
		}
		UIHelper.RepositionGrid(SkillListGrid);
	}

	private void SetupRecommendTranscendSkillIconList(List<int> recommendTranscendIndexs)
	{
		int[] transcendSkill = _CreatureData.GetTranscendSkill(Transcended);
		if (transcendSkill == null)
		{
			return;
		}
		ChangeTranscendSkillScrollScaleFix();
		for (int i = 0; i < transcendSkill.Length; i++)
		{
			CreatureData.TranscendSkill transcendSkillData = _CreatureData.GetTranscendSkillData(Transcended, i);
			if (transcendSkillData != null && !__Extension_Method_ICollection__.IsNullOrEmpty(recommendTranscendIndexs))
			{
				HeroTranscendSkillIconComponent heroTranscendSkillIconComponent = UIHelper.AddGridItem(SkillListGrid, HeroTranscendSkillIconComponent.PrefabAuto);
				heroTranscendSkillIconComponent.Open(_heroInfo, Page, transcendSkillData, this, readOnly: true);
				if (recommendTranscendIndexs.Contains(transcendSkillData.TranscendSkillIndex))
				{
					heroTranscendSkillIconComponent.SetSkillStatus(HeroTranscendSkillIconComponent.ETranscendSkillStatus.Learned);
				}
				else
				{
					heroTranscendSkillIconComponent.SetSkillStatus(HeroTranscendSkillIconComponent.ETranscendSkillStatus.NotLearn);
				}
				_listSkillIConComponent.Add(heroTranscendSkillIconComponent);
			}
		}
		UIHelper.RepositionGrid(SkillListGrid);
	}

	private void ChangeTranscendSkillScrollScaleFix()
	{
		int skillGridMaxLine = GetSkillGridMaxLine();
		SkillListGrid.Grid.maxPerLine = skillGridMaxLine;
		SkillListGrid.AutoFitScale = false;
	}

	private void ChangeTranscendSkillScroll()
	{
		int skillGridMaxLine = GetSkillGridMaxLine();
		SkillListGrid.Grid.maxPerLine = skillGridMaxLine;
		UIWidget component = SkillListGrid.GetComponent<UIWidget>();
		if (!(component != null))
		{
			return;
		}
		int num = 1;
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_listSkillIConComponent))
		{
			UIWidget component2 = _listSkillIConComponent[0].GetComponent<UIWidget>();
			if (component2 != null)
			{
				num = component2.width;
			}
		}
		SkillListGrid.AutoFitScale = false;
		int b = __Extension_Method_ICollection__.COUNT(_listSkillIConComponent);
		int num2 = Mathf.Min(skillGridMaxLine, b);
		int num3 = (component.height = Mathf.Min(component.width / num2, component.height));
		Vector3 localScale = Vector3.one * ((float)num3 / (float)num);
		SkillListGrid.Grid.cellWidth = (float)num3 + widthPadding;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listSkillIConComponent); i++)
		{
			HeroTranscendSkillIconComponent heroTranscendSkillIconComponent = _listSkillIConComponent[i];
			if (!(heroTranscendSkillIconComponent == null))
			{
				heroTranscendSkillIconComponent.transform.localScale = localScale;
			}
		}
	}

	void HeroTranscendSkillIconComponent.Listener.OnSelectSkillComponent(HeroTranscendSkillIconComponent comp)
	{
		if (_listener != null)
		{
			_listener.OnSelectComponent(comp.TranscendSkill.Transcended, comp.TranscendSkill.Index, comp.TranscendSkill.TranscendSkillIndex);
			Debug.Log("### comp.TranscendSkill.Transcended = " + comp.TranscendSkill.Transcended);
			Debug.Log("### comp.TranscendSkill.Index = " + comp.TranscendSkill.Index);
			Debug.Log("### comp.TranscendSkill.TranscendSkillIndex = " + comp.TranscendSkill.TranscendSkillIndex);
		}
	}
}
