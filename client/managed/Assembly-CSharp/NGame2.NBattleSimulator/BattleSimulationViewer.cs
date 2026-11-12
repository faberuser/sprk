using System;
using System.Collections.Generic;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NGui;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NBattleSimulator;

public class BattleSimulationViewer : GlobalAutoBehaviour<BattleSimulationViewer>, InitOnCreate
{
	private BattleSimulationResultInfo _simulationResultInfo;

	private DateTime _simulationBeginTime = DateTime.Now;

	private DateTime _simulationEndTime = DateTime.Now;

	private bool _isSimulationPlay;

	private LocalStorageString simulationHeroIndices = new LocalStorageString("1,2,3,4", typeof(BattleSimulationViewer).FullName + "simulationHeroIndices");

	private LocalStorageString simulationChapterIndex = new LocalStorageString("1", typeof(BattleSimulationViewer).FullName + "simulationChapterIndex");

	private LocalStorageString simulationDungeonIndex = new LocalStorageString("1", typeof(BattleSimulationViewer).FullName + "simulationDungeonIndex");

	private LocalStorageString simulationDifficulty = new LocalStorageString("1", typeof(BattleSimulationViewer).FullName + "simulationDifficulty");

	private BattleSimulationInstance _currentSimulation;

	void InitOnCreate.InitializeSingleton()
	{
		GlobalAutoBehaviour<GuiManager>.instance.Add("BattleSimulationViewer", this, delegate
		{
			using (new BlockManager().VerticalBox(Color.black, 0.5f))
			{
				using (new BlockManager().Horizontal())
				{
					GUILayout.Label("HeroIndices");
					simulationHeroIndices.Value = GUILayout.TextField(simulationHeroIndices.Value);
					GUILayout.Label("chapter");
					simulationChapterIndex.Value = GUILayout.TextField(simulationChapterIndex.Value);
					GUILayout.Label("dungeon");
					simulationDungeonIndex.Value = GUILayout.TextField(simulationDungeonIndex.Value);
					GUILayout.Label("difficulty");
					simulationDifficulty.Value = GUILayout.TextField(simulationDifficulty.Value);
				}
				if (GUILayout.Button("battle Simulation"))
				{
					if (_isSimulationPlay)
					{
						return;
					}
					string[] array = simulationHeroIndices.Value.Split(',');
					if (__Extension_Method_ICollection__.IsNullOrEmpty(array))
					{
						return;
					}
					List<int> list = new List<int>();
					string[] array2 = array;
					for (int i = 0; i < array2.Length; i++)
					{
						int num = Vespa_String_Extension_Method.TOINT(array2[i], 0);
						if (num > 0)
						{
							list.Add(num);
						}
					}
					if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
					{
						return;
					}
					int num2 = Vespa_String_Extension_Method.TOINT(simulationChapterIndex.Value, 0);
					if (num2 == 0)
					{
						return;
					}
					int num3 = Vespa_String_Extension_Method.TOINT(simulationDungeonIndex.Value, 0);
					if (num3 == 0)
					{
						return;
					}
					int num4 = Vespa_String_Extension_Method.TOINT(simulationDifficulty.Value, -1);
					if (num4 == -1)
					{
						return;
					}
					_isSimulationPlay = true;
					_simulationBeginTime = DateTime.Now;
					_currentSimulation = BattleSimulationFacade.RunBattleSimulationTest("Test", list.ToArray(), num2, num3, num4, 1, delegate(BattleSimulationResultInfo result)
					{
						_isSimulationPlay = false;
						_simulationResultInfo = result;
						_simulationEndTime = DateTime.Now;
					});
				}
				if (_isSimulationPlay)
				{
					GUILayout.Label("Simulation Time " + (DateTime.Now - _simulationBeginTime).TotalSeconds);
				}
				else if (_simulationResultInfo != null)
				{
					GUILayout.Label((_simulationEndTime - _simulationBeginTime).TotalSeconds + " : " + _simulationResultInfo.ToString());
				}
				if (_currentSimulation != null && _currentSimulation.BattleInstance != null)
				{
					using (new BlockManager().Horizontal())
					{
						using (new BlockManager().VerticalBox(Color.black, 0.5f))
						{
							BattleCreatureList aliveCreaturesOfTeam = _currentSimulation.BattleInstance.CreatureContainer.GetAliveCreaturesOfTeam(TeamType.Left);
							for (int num5 = 0; num5 < aliveCreaturesOfTeam.Count; num5++)
							{
								BattleCreature battleCreature = aliveCreaturesOfTeam[num5];
								if (battleCreature == null)
								{
									return;
								}
								GUILayout.Label(battleCreature.CreatureData.Name + " " + battleCreature.StatController.Hp + " " + battleCreature.StatController.Mp);
							}
						}
						using (new BlockManager().VerticalBox(Color.black, 0.5f))
						{
							BattleCreatureList aliveCreaturesOfTeam2 = _currentSimulation.BattleInstance.CreatureContainer.GetAliveCreaturesOfTeam(TeamType.Right);
							for (int num6 = 0; num6 < aliveCreaturesOfTeam2.Count; num6++)
							{
								BattleCreature battleCreature2 = aliveCreaturesOfTeam2[num6];
								if (battleCreature2 == null)
								{
									break;
								}
								GUILayout.Label(battleCreature2.CreatureData.Name + " " + battleCreature2.StatController.Hp);
							}
							return;
						}
					}
				}
			}
		});
	}
}
