using System;
using System.Collections.Generic;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

[Obsolete("사용안함")]
public class SkillInspectComponent : UIScrollItemBehaviour, InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/SkillInspectComponent";

	public static PrefabAuto<SkillInspectComponent> PrefabAuto = new PrefabAuto<SkillInspectComponent>("Prefab2/Component/SkillInspectComponent");

	public UILabel Label_SkillType;

	public UILabel Label_SkillLevel;

	public UILabel Label_SkillDesc;

	public UILabel Label_CoolTime;

	public UIWidget ManaWidget;

	public UIGrid ManaGrid;

	public GameObject ManaGaugeTemplate;

	public UISimpleGrid SkillIconGrid;

	private HeroSkillIconComponent _SkillComponent;

	private List<GameObject> _manaGaugeList = new List<GameObject>();

	public HeroInfo HeroInfo { get; private set; }

	public int SkillSlotIndex { get; private set; }

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}
}
