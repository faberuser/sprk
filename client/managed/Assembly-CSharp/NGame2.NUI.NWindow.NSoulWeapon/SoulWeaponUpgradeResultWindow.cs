using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using NGameAsset;
using NNguiExtension;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NUI.NWindow.NSoulWeapon;

public class SoulWeaponUpgradeResultWindow : BaseWindow
{
	public interface IListener
	{
		void CloseUpgradeResultWindow();
	}

	[Serializable]
	public class NodeTreeStock
	{
		public int Grade;

		public string PackCode;

		public string TreeName;
	}

	[CompilerGenerated]
	private sealed class _003CCoroutineEnableUpgradeInfo_003Ed__20 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public SoulWeaponUpgradeResultWindow _003C_003E4__this;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCoroutineEnableUpgradeInfo_003Ed__20(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			SoulWeaponUpgradeResultWindow soulWeaponUpgradeResultWindow = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = new WaitForSecondsRealtime(4.5f);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				soulWeaponUpgradeResultWindow.EnableUpgradeInfo(enable: true);
				_003C_003E2__current = new WaitForSecondsRealtime(1f);
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				GlobalTopmostWindow.instance.RemoveTouchDisabler(soulWeaponUpgradeResultWindow);
				_003C_003E2__current = null;
				_003C_003E1__state = 3;
				return true;
			case 3:
				_003C_003E1__state = -1;
				return false;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	public const string PrefabPath = "Resources/Prefab2/Window/SoulWeapon/SoulWeaponUpgradeResultWindow";

	public NodeTreeUIPlayer BgNodeTreeUIPlayer;

	public NodeTreeStock[] NodeTreeStocks = new NodeTreeStock[2]
	{
		new NodeTreeStock
		{
			Grade = 1,
			PackCode = "Common/Cinema",
			TreeName = "SoulWeapon_RankUP_1"
		},
		new NodeTreeStock
		{
			Grade = 2,
			PackCode = "Common/Cinema",
			TreeName = "SoulWeapon_RankUP_2"
		}
	};

	public SoulWeaponGradeInfo SoulWeaponGradeInfo;

	public SoulWeaponGradeInfo UpgradeSoulWeaponGradeInfo;

	public SoulWeaponStatInfo SoulWeaponStatInfo;

	public SoulWeaponStatInfo UpgradeSoulWeaponStatInfo;

	public SoulWeaponSkillInfo SoulWeaponSkillInfo;

	public Animator Animator;

	public UISpriteSelector BgSpriteSelector;

	public Transform UpgradeInfoTransform;

	public Transform TouchLabelTransform;

	private IListener _iListener;

	private bool _isWaitingResult;

	private Action _onClose;

	public void Open(IListener iListener, SoulWeaponInfo originSoulWeaponInfo, EquipItemInfo equipItemInfo, Action onClose)
	{
		Open(delegate
		{
			Setup(iListener, originSoulWeaponInfo, equipItemInfo, onClose);
		}, null);
	}

	public override void Close()
	{
		if (!_isWaitingResult)
		{
			SprkSoulUpgradePlayback.End(this);
			base.Close();
			SoulWeaponSkillInfo.Close();
			if (BgNodeTreeUIPlayer != null)
			{
				BgNodeTreeUIPlayer.Close();
			}
		}
	}

	private void Setup(IListener iListener, SoulWeaponInfo originSoulWeaponInfo, EquipItemInfo equipItemInfo, Action onClose)
	{
		SprkSoulUpgradePlayback.Begin(this);
		_iListener = iListener;
		_onClose = onClose;
		SetSoulWeaponNodeTree(equipItemInfo);
		SoulWeaponInfo soulWeaponInfo = null;
		if (equipItemInfo != null)
		{
			soulWeaponInfo = equipItemInfo.SoulWeaponInfo;
		}
		SetBgSpriteSelector(soulWeaponInfo);
		SoulWeaponGradeInfo.Open(originSoulWeaponInfo?.Grade ?? 0, 0, isLocalize: true);
		SoulWeaponStatInfo.Open(originSoulWeaponInfo);
		UpgradeSoulWeaponGradeInfo.Open(soulWeaponInfo?.Grade ?? 0, 0, isLocalize: true);
		UpgradeSoulWeaponStatInfo.Open(soulWeaponInfo);
		SoulWeaponSkillInfo.OpenUpgrade(soulWeaponInfo);
		GlobalTopmostWindow.instance.AddTouchDisabler(this);
		EnableUpgradeInfo(enable: false);
		VespaCoroutine.Start(CoroutineEnableUpgradeInfo());
	}

	[IteratorStateMachine(typeof(_003CCoroutineEnableUpgradeInfo_003Ed__20))]
	private IEnumerator CoroutineEnableUpgradeInfo()
	{
		return SprkSoulUpgradePlayback.Guard(new _003CCoroutineEnableUpgradeInfo_003Ed__20(0)
		{
			_003C_003E4__this = this
		}, this);
	}

	private void EnableUpgradeInfo(bool enable)
	{
		_isWaitingResult = !enable;
		UIHelper.EnableGameObject(UpgradeInfoTransform, enable);
		UIHelper.EnableGameObject(TouchLabelTransform, enable);
	}

	private void SetBgSpriteSelector(SoulWeaponInfo soulWeaponInfo)
	{
		if (soulWeaponInfo != null && BgSpriteSelector != null)
		{
			BgSpriteSelector.SelectByIndex(soulWeaponInfo.Grade);
		}
	}

	private void SetBg(EquipItemInfo equipItemInfo)
	{
		UIHelper.PlaySoulWeaponNodeTree(BgNodeTreeUIPlayer, equipItemInfo, SoulWeaponNodeTreeType.Upgrade, null, null);
	}

	public void SetSoulWeaponNodeTree(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo != null && equipItemInfo.IsSoulWeapon())
		{
			SoulWeaponInfo soulWeaponInfo = equipItemInfo.SoulWeaponInfo;
			int grade = soulWeaponInfo.Grade;
			CreateSoulWeapon(equipItemInfo.ItemData.Code, grade, isAdd: true);
			NodeTreeStock nodeTreeStock = GetNodeTreeStock(soulWeaponInfo.Grade);
			BgNodeTreeUIPlayer.PlayOnce(nodeTreeStock.PackCode, nodeTreeStock.TreeName);
		}
	}

	public void CreateSoulWeapon(string itemCode, int grade, bool isAdd)
	{
		SoulWeaponNodeTreeData data = GameRoot.Instance.SoulWeaponNodeTreeDataContainer.GetData(itemCode);
		if (data != null)
		{
			float[] position = null;
			float[] rotation = null;
			float scale = 0f;
			data.GetModleViewValues(ESoulWeaponModelViewType.UpgradeResult, out position, out rotation, out scale);
			int weaponCreatureIndex = data.WeaponCreatureIndex;
			Vector3 arrayToVec = GetArrayToVec3(position);
			Vector3 arrayToVec2 = GetArrayToVec3(rotation);
			if (isAdd)
			{
				BgNodeTreeUIPlayer.AddCreatureStock(weaponCreatureIndex, grade, arrayToVec, arrayToVec2, scale);
			}
			else
			{
				BgNodeTreeUIPlayer.SpawnCreature(weaponCreatureIndex, grade, arrayToVec, arrayToVec2, scale);
			}
		}
	}

	private void PlayAnim()
	{
		if (!(Animator == null))
		{
			Animator.Play("Anim_UpGrade01");
		}
	}

	private void PlaySound(string soundCode)
	{
		SoundStock stock = new SoundStock
		{
			Code = soundCode
		};
		SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 10f);
	}

	private Vector3 GetArrayToVec3(float[] vectorArray)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(vectorArray) || __Extension_Method_ICollection__.COUNT(vectorArray) < 3)
		{
			return Vector3.zero;
		}
		float x = vectorArray[0];
		float y = vectorArray[1];
		float z = vectorArray[2];
		return new Vector3(x, y, z);
	}

	public NodeTreeStock GetNodeTreeStock(int grade)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(NodeTreeStocks); i++)
		{
			NodeTreeStock nodeTreeStock = NodeTreeStocks[i];
			if (nodeTreeStock != null && nodeTreeStock.Grade == grade)
			{
				return nodeTreeStock;
			}
		}
		return new NodeTreeStock();
	}

	public void OnClickClose()
	{
		if (_iListener != null)
		{
			_iListener.CloseUpgradeResultWindow();
			Close();
			Vespa_System_Action_Extension_Method.fire(_onClose);
		}
	}
}
