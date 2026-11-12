using NGame2.NBattleView;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class PatternAlarm : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/Battle/PatternAlarm";

	public Transform PatternTextAlarm;

	public Transform PatternImageAlarm;

	public UILabel PatternAlarmLabel;

	public Animator PatternImageAnimator;

	private Creature _boss;

	private BossPatternAlarm _bossPatternAlarm = new BossPatternAlarm();

	public void Open(Creature bossCreature)
	{
		Open(delegate
		{
			Init(bossCreature);
		}, null);
	}

	public void Init(Creature bossCreature)
	{
		_boss = bossCreature;
		_bossPatternAlarm.Init(_boss, PatternTextAlarm, PatternAlarmLabel, PatternImageAlarm, PatternImageAnimator, "Anim_RaidWarning_Start");
	}

	public override void Close()
	{
		_bossPatternAlarm.Close();
		base.Close();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		_bossPatternAlarm.Update();
	}
}
