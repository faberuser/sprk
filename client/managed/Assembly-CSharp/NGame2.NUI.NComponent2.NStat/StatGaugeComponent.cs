using System;
using System.Collections.Generic;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NStat;

public class StatGaugeComponent : LinkFactoryComponent, InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/Stat/StatGaugeComponent";

	public const string ButtonPrefabPath = "Prefab2/Component/Stat/StatGaugeButtonComponent";

	public const string DisplayPrefabPath = "Prefab2/Component/Stat/StatDisplayComponent";

	public const string ValanceButtonPrefabPath = "Prefab2/Component/Valance/ValanceStatButtonComponent";

	public const string ValanceButtonPrefabPath2 = "Prefab2/Component/Valance/ValanceSkillButtonComponent";

	public static PrefabAuto<StatGaugeComponent> PrefabAuto = new PrefabAuto<StatGaugeComponent>("Prefab2/Component/Stat/StatGaugeComponent");

	public static PrefabAuto<StatGaugeComponent> ButtonPrefabAuto = new PrefabAuto<StatGaugeComponent>("Prefab2/Component/Stat/StatGaugeButtonComponent");

	public static PrefabAuto<StatGaugeComponent> DisplayPrefabAuto = new PrefabAuto<StatGaugeComponent>("Prefab2/Component/Stat/StatDisplayComponent");

	public static PrefabAuto<StatGaugeComponent> ValanceButtonPrefabAuto = new PrefabAuto<StatGaugeComponent>("Prefab2/Component/Valance/ValanceStatButtonComponent");

	public static PrefabAuto<StatGaugeComponent> ValanceSkillButtonPrefabAuto = new PrefabAuto<StatGaugeComponent>("Prefab2/Component/Valance/ValanceSkillButtonComponent");

	public UIGrid GaugeGrid;

	public StatGaugeInfo GaugeTemplate;

	public UILabel Label_Title;

	public UISprite Sprite_BG;

	private List<StatGaugeInfo> _instancedGaugeList = new List<StatGaugeInfo>();

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		Clear();
	}

	private new void Awake()
	{
		UIHelper.EnableGameObject(GaugeTemplate, enable: false);
	}

	public void AddStat(Enum statType, int current, int max, int curRatio, int maxRatio, string splitter = "/", Action onClickSelect = null)
	{
		if (!(GaugeTemplate == null) && !(GaugeGrid == null))
		{
			GameObject obj = UnityEngine.Object.Instantiate(GaugeTemplate.gameObject);
			UIHelper.EnableGameObject(obj, enable: true);
			StatGaugeInfo component = obj.GetComponent<StatGaugeInfo>();
			if (!(component == null))
			{
				Vespa_Unity_Transform_Extension_Method.ParentResetTM(component.transform, GaugeGrid.transform);
				component.Open(statType, current, max, curRatio, maxRatio, onClickSelect);
				component.SetSplitterLabel(splitter);
				_instancedGaugeList.Add(component);
				GaugeGrid.Reposition();
			}
		}
	}

	public void AddStat(Enum statType, int current, int max, string splitter = "/")
	{
		AddStat(statType, current, max, current, max, splitter);
	}

	public void AddEmptyStat(string empty)
	{
		if (!(GaugeTemplate == null) && !(GaugeGrid == null))
		{
			GameObject obj = UnityEngine.Object.Instantiate(GaugeTemplate.gameObject);
			UIHelper.EnableGameObject(obj, enable: true);
			StatGaugeInfo component = obj.GetComponent<StatGaugeInfo>();
			if (!(component == null))
			{
				Vespa_Unity_Transform_Extension_Method.ParentResetTM(component.transform, GaugeGrid.transform);
				component.OpenEmpty(empty);
				_instancedGaugeList.Add(component);
				GaugeGrid.Reposition();
			}
		}
	}

	public void AddExtraSkill(string text, Action onClickSelect = null)
	{
		if (!(GaugeTemplate == null) && !(GaugeGrid == null))
		{
			GameObject obj = UnityEngine.Object.Instantiate(GaugeTemplate.gameObject);
			UIHelper.EnableGameObject(obj, enable: true);
			StatGaugeInfo component = obj.GetComponent<StatGaugeInfo>();
			if (!(component == null))
			{
				Vespa_Unity_Transform_Extension_Method.ParentResetTM(component.transform, GaugeGrid.transform);
				component.OpenExtraSkill(text, onClickSelect);
				_instancedGaugeList.Add(component);
				GaugeGrid.Reposition();
			}
		}
	}

	public StatGaugeInfo GetStatGaugeInfo(int index)
	{
		if (index >= __Extension_Method_ICollection__.COUNT(_instancedGaugeList))
		{
			return null;
		}
		return _instancedGaugeList[index];
	}

	private void Clear()
	{
		UIHelper.EnableGameObject(Sprite_BG, enable: true);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_instancedGaugeList); i++)
		{
			VespaObject.DestroyGoAndDetachFromParent(_instancedGaugeList[i].gameObject, base.transform);
		}
		_instancedGaugeList.Clear();
	}

	public void SetTitle(string title)
	{
		UIHelper.SetLabelText(Label_Title, title);
	}

	public void OnOffBGSprite(bool onOff)
	{
		UIHelper.EnableGameObject(Sprite_BG, onOff);
	}
}
