using System;
using System.Collections.Generic;
using NGame2.NBattleField;
using NGame2.NUI.NComponent2.NBattle;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class DamageMeter : BaseWindowUpdater, IInspectorGuiDrawer
{
	[Serializable]
	public class DpsSetting
	{
		public ShowDpsType ShowDpsType;

		public UIWidget WidgetThis;

		public UISpriteSelector SpriteMode;
	}

	public interface Listener
	{
		void SetHighestTpsUid(int uid, bool forceRefresh);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Battle/DamageMeter";

	[NonSerialized]
	public static LocalAccountBool IsClosed = new LocalAccountBool(i_default: false, typeof(DamageMeter).FullName + "IsClosed");

	private EMeterType _meterType = EMeterType.Damage;

	private BattleType _battleType;

	private BattleDefineData _battleDefine;

	private bool _enableSqrt;

	public UIWidget DetailWidgetRoot;

	public TweenPosition DisplayTweener;

	public UISimpleGrid HeroGrid;

	public List<DpsSetting> DPSSettings;

	public GameObject RootOffset;

	public GameObject OffsetDefault;

	public GameObject OffsetArena;

	private long _playerId;

	private SortedDictionary<int, DamageMeterComponent> _dic = new SortedDictionary<int, DamageMeterComponent>();

	private List<int> _tempValidUIList = new List<int>();

	private List<int> _tempRemoveUIList = new List<int>();

	private List<DamageMeterComponent> _tempSortList = new List<DamageMeterComponent>();

	private DpsSetting _curDpsSetting;

	private bool _forceTpsRefresh;

	private ShowDpsType _showDpsType;

	private Listener _listener;

	public void Open(Listener listener, long playerId, BattleType battleType, BattleDefineData battleDefine)
	{
		SetUpdatePeriod(1f);
		_listener = listener;
		_playerId = playerId;
		_battleType = battleType;
		_battleDefine = battleDefine;
		DisplayTweener.enabled = false;
		_forceTpsRefresh = false;
		_enableSqrt = GameRoot.Instance.ConstantDataContainer.GetValueBool("ThreatMeterSqrt");
		InitDPSSetting();
		if (HeroGrid != null)
		{
			HeroGrid.ClearGridItem<DamageMeterComponent>();
		}
		if (IsClosed.Value)
		{
			DisplayTweener.PlayForward();
		}
		else
		{
			WindowRoot.localPosition = DisplayTweener.from;
		}
		Open(delegate
		{
			UpdateWidgetEnable();
		}, null);
		if (battleType == BattleType.Arena || battleType == BattleType.GuildArena || battleType == BattleType.OrdealArena)
		{
			RootOffset.transform.position = OffsetArena.transform.position;
		}
	}

	private void InitDPSSetting()
	{
		_showDpsType = ShowDpsType.Default;
		if ((_battleDefine.BattleMenu & EBattleMenu.TPSDamageMeter) != 0)
		{
			_showDpsType = ShowDpsType.TPS;
		}
		if ((_battleDefine.BattleMenu & EBattleMenu.MPSDamageMeter) != 0)
		{
			_showDpsType = ShowDpsType.MPS;
		}
		foreach (DpsSetting dPSSetting in DPSSettings)
		{
			if (dPSSetting != null)
			{
				bool flag = dPSSetting.ShowDpsType == _showDpsType;
				if (flag)
				{
					_curDpsSetting = dPSSetting;
				}
				UIHelper.EnableGameObject(dPSSetting.WidgetThis, flag);
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
		if (HeroGrid != null)
		{
			HeroGrid.ClearGridItem<DamageMeterComponent>();
		}
		_dic.Clear();
	}

	public override void UpdatePeriodically()
	{
		base.UpdatePeriodically();
		UpdateComponent();
	}

	private void UpdateComponent()
	{
		_tempRemoveUIList.Clear();
		_tempRemoveUIList.AddRange(_dic.Keys);
		for (int i = 0; i < SingletonAuto<BattleFacade>.instance.GetCreatureCount(); i++)
		{
			BattleFacade.CreatureCacheData creatureCacheByIndex = SingletonAuto<BattleFacade>.instance.GetCreatureCacheByIndex(i);
			if (creatureCacheByIndex == null || creatureCacheByIndex.Uid == 0)
			{
				continue;
			}
			if (_battleDefine != null)
			{
				if ((_battleDefine.DamageMeterFlag & DamageMeterFlag.ShowAll) != DamageMeterFlag.None)
				{
					if (!creatureCacheByIndex.Player.IsMe && !creatureCacheByIndex.Player.IsAlly)
					{
						continue;
					}
				}
				else if (((_battleDefine.DamageMeterFlag & DamageMeterFlag.ShowAllPlayer) == 0 && creatureCacheByIndex.Player.Id != _playerId) || ((_battleDefine.DamageMeterFlag & DamageMeterFlag.ShowAi) == 0 && creatureCacheByIndex.CreateInfo.IsAi))
				{
					continue;
				}
			}
			DamageMeterComponent vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_dic, creatureCacheByIndex.Uid, null);
			if (vALUE != null)
			{
				_tempRemoveUIList.Remove(creatureCacheByIndex.Uid);
				continue;
			}
			vALUE = UIHelper.AddGridItem(HeroGrid, DamageMeterComponent.PrefabAuto);
			if (!vALUE.Open(creatureCacheByIndex, _playerId, _showDpsType))
			{
				FactoryBase.ReleaseComponent(vALUE);
				continue;
			}
			_tempRemoveUIList.Remove(creatureCacheByIndex.Uid);
			_dic.Add(creatureCacheByIndex.Uid, vALUE);
		}
		foreach (int tempRemoveUI in _tempRemoveUIList)
		{
			DamageMeterComponent comp = _dic[tempRemoveUI];
			_dic.Remove(tempRemoveUI);
			FactoryBase.ReleaseComponent(comp);
		}
		long num = 1L;
		long num2 = 1L;
		long num3 = 1L;
		long num4 = 1L;
		int uid = 0;
		foreach (DamageMeterComponent value in _dic.Values)
		{
			value.UpdateTick();
			num = System.Math.Max(num, value.TotalDamage);
			num2 = System.Math.Max(num2, value.TotalHeal);
			num4 = System.Math.Max(num4, value.TotalManaRestoreInUnitTime);
			long num5 = value.TotalThreatInUnitTime;
			if (_enableSqrt)
			{
				num5 = Convert.ToInt64(System.Math.Sqrt(num5));
			}
			if (num5 != 0L && num5 > num3)
			{
				if (value.IsDead)
				{
					continue;
				}
				uid = value.CreatureUid;
			}
			num3 = System.Math.Max(num3, num5);
		}
		foreach (DamageMeterComponent value2 in _dic.Values)
		{
			value2.UpdateWidget(_meterType, num, num2, num3, num4);
		}
		if (_listener != null && _showDpsType == ShowDpsType.TPS)
		{
			_listener.SetHighestTpsUid(uid, _forceTpsRefresh);
			_forceTpsRefresh = false;
		}
		if (true)
		{
			SortGrid();
		}
	}

	private void SortGrid()
	{
		_tempSortList.Clear();
		_tempSortList.AddRange(_dic.Values);
		_tempSortList.Sort(delegate(DamageMeterComponent lhs, DamageMeterComponent rhs)
		{
			long value;
			long num2;
			if (_meterType == EMeterType.Damage)
			{
				value = lhs.TotalDamage;
				num2 = rhs.TotalDamage;
			}
			else if (_meterType == EMeterType.Heal)
			{
				value = lhs.TotalHeal;
				num2 = rhs.TotalHeal;
			}
			else if (_meterType == EMeterType.Status)
			{
				if (lhs.IsDead && rhs.IsDead)
				{
					value = lhs.CurrentTimeMs * -1;
					num2 = rhs.CurrentTimeMs * -1;
				}
				else
				{
					value = ((!lhs.IsDead) ? lhs.PositionScore : 0);
					num2 = ((!rhs.IsDead) ? rhs.PositionScore : 0);
				}
			}
			else
			{
				value = 0L;
				num2 = 0L;
			}
			int num3 = num2.CompareTo(value);
			return (num3 != 0) ? num3 : lhs.CreatureUid.CompareTo(rhs.CreatureUid);
		});
		for (int num = 0; num < __Extension_Method_ICollection__.COUNT(_tempSortList); num++)
		{
			_tempSortList[num].name = num.ToString();
		}
		UIHelper.RepositionGrid(HeroGrid);
	}

	public void SetHighestTpsComp(long highestUid)
	{
		foreach (DamageMeterComponent value in _dic.Values)
		{
			bool tpsDanger = value.CreatureUid == highestUid;
			value.SetTpsDanger(tpsDanger);
		}
	}

	public void OnClickFold()
	{
		if (!DisplayTweener.enabled)
		{
			_lastUpdateTime = 0f;
			IsClosed.Value = !IsClosed.Value;
			if (IsClosed.Value)
			{
				DisplayTweener.PlayForward();
			}
			else
			{
				DisplayTweener.PlayReverse();
			}
			UpdateWidgetEnable();
		}
	}

	public void OnClickMeterType()
	{
		if (_meterType == EMeterType.Damage)
		{
			_meterType = EMeterType.Heal;
		}
		else if (_meterType == EMeterType.Heal)
		{
			_meterType = EMeterType.Status;
		}
		else if (_meterType == EMeterType.Status)
		{
			_meterType = EMeterType.Damage;
		}
		else
		{
			_meterType = EMeterType.Damage;
		}
		_lastUpdateTime = 0f;
		UpdateWidgetEnable();
	}

	public void OnClickTPSMeterType()
	{
		if (_meterType == EMeterType.Damage)
		{
			_meterType = EMeterType.Heal;
		}
		else if (_meterType == EMeterType.Heal)
		{
			_meterType = EMeterType.Threat;
			_forceTpsRefresh = true;
		}
		else if (_meterType == EMeterType.Threat)
		{
			_meterType = EMeterType.Status;
		}
		else if (_meterType == EMeterType.Status)
		{
			_meterType = EMeterType.Damage;
		}
		else
		{
			_meterType = EMeterType.Damage;
		}
		_lastUpdateTime = 0f;
		UpdateWidgetEnable();
	}

	public void OnClickMPSMeterType()
	{
		if (_meterType == EMeterType.Damage)
		{
			_meterType = EMeterType.Heal;
		}
		else if (_meterType == EMeterType.Heal)
		{
			_meterType = EMeterType.ManaRestored;
		}
		else if (_meterType == EMeterType.ManaRestored)
		{
			_meterType = EMeterType.Status;
		}
		else if (_meterType == EMeterType.Status)
		{
			_meterType = EMeterType.Damage;
		}
		else
		{
			_meterType = EMeterType.Damage;
		}
		_lastUpdateTime = 0f;
		UpdateWidgetEnable();
	}

	private void UpdateWidgetEnable()
	{
		_curDpsSetting.SpriteMode.SelectEnum(_meterType);
	}
}
