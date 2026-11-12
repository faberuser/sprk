using System.Collections.Generic;
using NShared;
using NVespa;

namespace NGame2.NContents;

public class MenuOpenCheckManager : SingletonAuto<MenuOpenCheckManager>
{
	private readonly Dictionary<ERightMenuButton, IMenuOpenChecker> _checkerDictionary = new Dictionary<ERightMenuButton, IMenuOpenChecker>();

	public MenuOpenCheckManager()
	{
		Register(ERightMenuButton.Raid, new RaidMenuOpenChecker());
		Register(ERightMenuButton.TrialTower, new TrialTowerMenuOpenChecker());
		Register(ERightMenuButton.WorldBoss, new WorldBossOpenChecker());
		Register(ERightMenuButton.PartyPlayDungeon, new PartyPlayDungeonOpenChecker());
		Register(ERightMenuButton.ClassBuff, new ClassBuffOpenCheck());
		Register(ERightMenuButton.SoulWeapon, new SoulWeaponMenuOpenChecker());
		Register(ERightMenuButton.Eclipse, new EclipseMenuOpenChecker());
		Register(ERightMenuButton.GodkingTrial, new GodkingTrialMenuOpenChecker());
	}

	private void Register(ERightMenuButton menu, IMenuOpenChecker checker)
	{
		_checkerDictionary.Remove(menu);
		_checkerDictionary.Add(menu, checker);
	}

	public bool IsOpen(ERightMenuButton menu)
	{
		IMenuOpenChecker value = null;
		_checkerDictionary.TryGetValue(menu, out value);
		return value?.IsOpen() ?? false;
	}
}
