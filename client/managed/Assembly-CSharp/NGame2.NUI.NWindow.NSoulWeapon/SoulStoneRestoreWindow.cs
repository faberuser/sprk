using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NNodeTree;
using NGame2.NRender;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWindow.NGlobalTopmost;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NSoulWeapon;

public class SoulStoneRestoreWindow : BaseWindow
{
	public enum ENodeTreeState
	{
		None,
		NotEnoughSoulStone,
		EnoughSoulStone,
		GetBonus,
		SOULWEAPON_ETHER_1,
		SOULWEAPON_ETHER_2,
		SOULWEAPON_ETHER_3,
		SOULWEAPON_ETHER_4,
		SoulStoneWait,
		SoulStoneAction,
		SoulStoneSelect,
		SoulStoneWait2
	}

	[Serializable]
	public class NodeTreeStock
	{
		public ENodeTreeState State;

		public string PackCode;

		public string TreeName;

		public bool IsPlayOnce;
	}

	public interface IListener
	{
		void OnClickRestoreButton();

		void GetBonus();

		void OpenSoulStoneRestoreRewardPopup(RestoreSoulStoneRewardInfo rewardInfo, bool isGetBonus);

		void NotEnoughSoulStoneFragment(int itemIndex);

		void OnClickSelectedSoulStoneButton(int itemIndex);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/SoulWeapon/SoulStoneRestoreWindow";

	public RestoreSoulStoneInfo RestoreSoulStoneInfo;

	public RestoreBonusInfo RestoreBonusInfo;

	public RestoreBuffInfo RestoreBuffInfo;

	public UIButton SpecificItemButton;

	public GameObject GoContentRoot;

	public NodeTreeStock[] NodeTreeStocks;

	private IListener _listener;

	private RestoreSoulStoneRewardInfo _restoreSoulStoneRewardInfo;

	public NodeTreeUIPlayer BgNodeTreeUIPlayer;

	public GameObject SideBlackSpritesRoot;

	private ENodeTreeState _nodeTreeMainState;

	private const float DefaultDelayTime = 1.5f;

	private const float SpecificItemDelayTime = 3.8f;

	private int SelectedSoulStoneIndex;

	private IAsyncLoad<IEffect> _effectAsync;

	private Item3dModelPopup2 SelectingPopup;

	private SceneChanger SceneChanger;

	private bool IsPossibleRestoreSoulStone
	{
		get
		{
			if (RestoreSoulStoneInfo.IsPossibleRestoreSoulStone && !RestoreBonusInfo.IsPossibleGetBonus && !IsItemResultSoulStone)
			{
				return !IsSelectingSoulStone;
			}
			return false;
		}
	}

	private bool IsSelectingSoulStone
	{
		get
		{
			if (_restoreSoulStoneRewardInfo == null)
			{
				return false;
			}
			if (__Extension_Method_ICollection__.IsNullOrEmpty(_restoreSoulStoneRewardInfo.SelectSoulStoneIndices) || _restoreSoulStoneRewardInfo.SelectSoulStoneIndices.Length == 0)
			{
				return false;
			}
			bool isSelected = false;
			ItemData itemData = null;
			_restoreSoulStoneRewardInfo.SelectSoulStoneIndices.ToList().ForEach(delegate(int item)
			{
				itemData = GameRoot.Instance.ItemDataContainer.GetData(item);
				if (itemData != null && itemData.Type == ItemType.SoulStone)
				{
					isSelected = true;
				}
			});
			return isSelected;
		}
	}

	private bool IsItemResultSoulStone
	{
		get
		{
			if (_restoreSoulStoneRewardInfo == null)
			{
				return false;
			}
			if (__Extension_Method_ICollection__.IsNullOrEmpty(_restoreSoulStoneRewardInfo.ItemResults) || _restoreSoulStoneRewardInfo.ItemResults.Length == 0)
			{
				return false;
			}
			bool isSpecificItem = false;
			ItemData itemData = null;
			_restoreSoulStoneRewardInfo.ItemResults.ToList().ForEach(delegate(ItemResultInfo item)
			{
				itemData = GameRoot.Instance.ItemDataContainer.GetData(item.ItemIndex);
				if (itemData != null && itemData.Type == ItemType.SoulStone)
				{
					isSpecificItem = true;
				}
			});
			return isSpecificItem;
		}
	}

	public void Open(IListener listener)
	{
		SceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(2f, GlobalTopmostWindow.ESceneChangeType.BlackScreen);
		UIHelper.EnableGameObject(GoContentRoot, enable: false);
		Open(delegate
		{
			Init(listener);
		});
	}

	private void Init(IListener listener)
	{
		_listener = listener;
		_restoreSoulStoneRewardInfo = null;
		SetRestoreSoulStoneInfo();
		VespaCoroutine.Start(InitAsync());
	}

	private IEnumerator InitAsync()
	{
		SetMainBgNodeTreePlay();
		while (!BgNodeTreeUIPlayer.IsCompletedLoad())
		{
			yield return null;
		}
		SceneChanger.Close();
		while (!BgNodeTreeUIPlayer.IsCompletedPlay())
		{
			yield return null;
		}
		UIHelper.EnableGameObject(GoContentRoot, enable: true);
		CheckSelectingSoulStone();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (_effectAsync != null && _effectAsync.Value != null)
		{
			_effectAsync.Value.Update();
		}
	}

	private void SetRestoreSoulStoneInfo()
	{
		RestoreSoulStoneInfo.Open();
		RestoreBonusInfo.Open();
		RestoreBuffInfo.Open();
	}

	private void SetMainBgNodeTreePlay()
	{
		if (BgNodeTreeUIPlayer == null)
		{
			return;
		}
		bool isPossibleRestoreSoulStone = RestoreSoulStoneInfo.IsPossibleRestoreSoulStone;
		bool isPossibleGetBonus = RestoreBonusInfo.IsPossibleGetBonus;
		bool isSelectingSoulStone = GlobalAutoBehaviour<AccountManager>.instance.UserManager.IsSelectingSoulStone;
		ENodeTreeState nodeTreeMainState = GetNodeTreeMainState(isPossibleGetBonus || isSelectingSoulStone, isPossibleRestoreSoulStone);
		if (_nodeTreeMainState == nodeTreeMainState)
		{
			return;
		}
		NodeTreeStock nodeTreeStock = GetNodeTreeStock(nodeTreeMainState);
		if (nodeTreeStock != null && !String_Extension_Method.IsNullOrEmpty(nodeTreeStock.PackCode) && !String_Extension_Method.IsNullOrEmpty(nodeTreeStock.TreeName))
		{
			SetNodeTreeState(nodeTreeMainState);
			if (nodeTreeStock.IsPlayOnce)
			{
				BgNodeTreeUIPlayer.PlayOnce(nodeTreeStock.PackCode, nodeTreeStock.TreeName);
			}
			else
			{
				BgNodeTreeUIPlayer.PlaySequence(nodeTreeStock.PackCode, nodeTreeStock.TreeName, null, null);
			}
		}
	}

	public void CheckSelectingSoulStone()
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.UserManager.IsSelectingSoulStone)
		{
			RestoreSoulStoneRewardInfo obj = new RestoreSoulStoneRewardInfo
			{
				CurrencyResults = null,
				ItemResults = null,
				EquipItemResults = null
			};
			int num = 0;
			obj.AddSoulStoneMileage = num;
			obj.SelectSoulStoneIndices = GlobalAutoBehaviour<AccountManager>.instance.UserManager.RestoreSoulStoneIndices;
			obj.IsConfirmResponse = false;
			RestoreSoulStoneRewardInfo rewardInfo = obj;
			Refresh(rewardInfo, isOpenWnd: true);
		}
	}

	public void SetResultBgNodePlay(string itemCode, ENodeTreeState nodeTreeState, Action onStart, Action onEnd)
	{
		NodeTreeStock nodeTreeResultStock = GetNodeTreeResultStock(itemCode, nodeTreeState);
		if (nodeTreeResultStock == null || String_Extension_Method.IsNullOrEmpty(nodeTreeResultStock.PackCode) || String_Extension_Method.IsNullOrEmpty(nodeTreeResultStock.TreeName))
		{
			return;
		}
		SetNodeTreeState(nodeTreeResultStock.State);
		bool orthographic = false;
		if (_restoreSoulStoneRewardInfo != null && ENodeTreeState.SoulStoneAction == _nodeTreeMainState)
		{
			ItemResultInfo[] itemResults = _restoreSoulStoneRewardInfo.ItemResults;
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(itemResults))
			{
				ItemResultInfo itemResultInfo = itemResults[0];
				BgNodeTreeUIPlayer.AddNodeTreeKeyWords(VariableKeyword.ItemCode, itemResultInfo.ItemIndex);
			}
		}
		if (_restoreSoulStoneRewardInfo != null && ENodeTreeState.SoulStoneSelect == _nodeTreeMainState)
		{
			int[] selectSoulStoneIndices = _restoreSoulStoneRewardInfo.SelectSoulStoneIndices;
			List<ItemData> list = new List<ItemData>();
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(selectSoulStoneIndices))
			{
				for (int i = 0; i < selectSoulStoneIndices.Length; i++)
				{
					int num = selectSoulStoneIndices[i];
					StringKey key = VariableKeyword.ItemCode;
					switch (i)
					{
					case 0:
						key = VariableKeyword.ItemCode;
						break;
					case 1:
						key = VariableKeyword.ItemCode2;
						break;
					case 2:
						key = VariableKeyword.ItemCode3;
						break;
					}
					BgNodeTreeUIPlayer.AddNodeTreeKeyWords(key, num);
					ItemData data = GameRoot.Instance.ItemDataContainer.GetData(num);
					if (data != null)
					{
						list.Add(data);
					}
				}
			}
			SelectedSoulStoneIndex = 0;
			UIHelper.EnableGameObject(SideBlackSpritesRoot, enable: false);
			Item3dModelPopup2 item3dModelPopup = Item3dModelPopup2.PreLoadGlobal(list.ToArray());
			item3dModelPopup.OpenPopup(delegate(int selectIndex)
			{
				if (BgNodeTreeUIPlayer != null)
				{
					RenderFacade runtimeSoulStoneRenderfacade = BgNodeTreeUIPlayer.GetRuntimeSoulStoneRenderfacade(selectIndex);
					if (runtimeSoulStoneRenderfacade != null)
					{
						if (_effectAsync != null)
						{
							EffectContainer.ReleaseEffect(_effectAsync, detachParent: true, immediatelyRelease: true);
							_effectAsync = null;
						}
						if (_effectAsync == null)
						{
							TransformData extraTransform = new TransformData
							{
								Position = Vector3.zero,
								RotationEuler = new Vector3(90f, 0f, 0f),
								Scale = 1f
							};
							_effectAsync = SingletonAuto<EffectContainer>.instance.CreateAsync("UI.Ascension.SoulStoneSelect", runtimeSoulStoneRenderfacade.gameObject.layer, extraTransform);
						}
						if (_effectAsync != null && _effectAsync.Value != null)
						{
							_effectAsync.Value.SetParent(runtimeSoulStoneRenderfacade.AssetCreature.FindTransform("Point001"));
						}
					}
				}
			}, delegate(int itemIndex)
			{
				SelectedSoulStoneIndex = itemIndex;
				if (_listener != null)
				{
					_listener.OnClickSelectedSoulStoneButton(SelectedSoulStoneIndex);
					SelectedSoulStoneIndex = 0;
				}
				UIHelper.EnableGameObject(SideBlackSpritesRoot, enable: true);
				EffectContainer.ReleaseEffect(_effectAsync, detachParent: true, immediatelyRelease: true);
				_effectAsync = null;
			});
			SelectingPopup = item3dModelPopup;
			orthographic = true;
		}
		if (nodeTreeResultStock.IsPlayOnce)
		{
			BgNodeTreeUIPlayer.PlayOnce(nodeTreeResultStock.PackCode, nodeTreeResultStock.TreeName, onStart, onEnd, orthographic);
		}
		else
		{
			BgNodeTreeUIPlayer.PlaySequence(nodeTreeResultStock.PackCode, nodeTreeResultStock.TreeName, onStart, onEnd, orthographic);
		}
	}

	public override void OnSystemBackButton()
	{
		if (_nodeTreeMainState != ENodeTreeState.SOULWEAPON_ETHER_1 && _nodeTreeMainState != ENodeTreeState.SOULWEAPON_ETHER_2 && _nodeTreeMainState != ENodeTreeState.SOULWEAPON_ETHER_3 && _nodeTreeMainState != ENodeTreeState.SOULWEAPON_ETHER_4 && _nodeTreeMainState != ENodeTreeState.SoulStoneAction && _nodeTreeMainState != ENodeTreeState.SoulStoneWait && _nodeTreeMainState != ENodeTreeState.SoulStoneWait2 && _nodeTreeMainState != ENodeTreeState.SoulStoneSelect)
		{
			base.OnSystemBackButton();
		}
	}

	public bool IsSoulStoneSelect()
	{
		return _nodeTreeMainState == ENodeTreeState.SoulStoneSelect;
	}

	private void SetNodeTreeState(ENodeTreeState nodeTreeState)
	{
		_nodeTreeMainState = nodeTreeState;
		SetClickButtonState(_nodeTreeMainState);
	}

	public NodeTreeStock GetNodeTreeStock(ENodeTreeState nodeTreeState)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(NodeTreeStocks); i++)
		{
			NodeTreeStock nodeTreeStock = NodeTreeStocks[i];
			if (nodeTreeStock != null && nodeTreeStock.State != ENodeTreeState.None && NodeTreeStocks[i].State == nodeTreeState)
			{
				return nodeTreeStock;
			}
		}
		return null;
	}

	public NodeTreeStock GetNodeTreeResultStock(string itemCode, ENodeTreeState nodeTreeState)
	{
		ENodeTreeState o_value = ENodeTreeState.None;
		if (!String_Extension_Method.IsNullOrEmpty(itemCode) && EnumLookup<ENodeTreeState>.TryParse(itemCode, out o_value))
		{
			return GetNodeTreeStock(o_value);
		}
		return GetNodeTreeStock(nodeTreeState);
	}

	public ENodeTreeState GetNodeTreeMainState(bool getBonus, bool isPossibleSoulStone)
	{
		if (getBonus)
		{
			return ENodeTreeState.GetBonus;
		}
		if (isPossibleSoulStone)
		{
			return ENodeTreeState.EnoughSoulStone;
		}
		return ENodeTreeState.NotEnoughSoulStone;
	}

	public void OnOffContentRoot(bool isOn)
	{
		UIHelper.EnableGameObject(GoContentRoot, isOn);
		if (isOn)
		{
			TopmostMenuType = ETopmostMenuType.SoulWeapon;
		}
		else
		{
			TopmostMenuType = ETopmostMenuType.HideAll;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (SelectingPopup != null)
		{
			SelectingPopup.Close();
		}
	}

	public override void Close()
	{
		base.Close();
		if (SelectingPopup != null)
		{
			SelectingPopup.Close();
		}
		_listener = null;
		_nodeTreeMainState = ENodeTreeState.None;
		EffectContainer.ReleaseEffect(_effectAsync, detachParent: true, immediatelyRelease: true);
		RestoreSoulStoneInfo.Close();
		RestoreBonusInfo.Close();
		RestoreBuffInfo.Close();
		if (BgNodeTreeUIPlayer != null)
		{
			BgNodeTreeUIPlayer.Close();
		}
		OnOffContentRoot(isOn: true);
	}

	private void SetClickButtonState(ENodeTreeState state)
	{
		bool enable = state == ENodeTreeState.EnoughSoulStone || state == ENodeTreeState.GetBonus || state == ENodeTreeState.SoulStoneWait || state == ENodeTreeState.SoulStoneWait2 || state == ENodeTreeState.SoulStoneSelect || state == ENodeTreeState.NotEnoughSoulStone;
		UIHelper.EnableButton(SpecificItemButton, enable);
	}

	private string GetRewardItemCode(ItemResultInfo[] resultItems)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(resultItems))
		{
			return null;
		}
		ItemResultInfo itemResultInfo = resultItems[0];
		return GameRoot.Instance.ItemDataContainer.GetData(itemResultInfo.ItemIndex)?.Code;
	}

	public void Refresh(RestoreSoulStoneRewardInfo rewardInfo, bool isOpenWnd = false)
	{
		_restoreSoulStoneRewardInfo = rewardInfo;
		if (_restoreSoulStoneRewardInfo != null)
		{
			string rewardItemCode = GetRewardItemCode(_restoreSoulStoneRewardInfo.ItemResults);
			bool isGetBonus = _restoreSoulStoneRewardInfo.IsGetBonus;
			bool isConfirmResponse = _restoreSoulStoneRewardInfo.IsConfirmResponse;
			ENodeTreeState eNodeTreeState = ENodeTreeState.None;
			if (IsSelectingSoulStone)
			{
				eNodeTreeState = (isGetBonus ? ENodeTreeState.SoulStoneSelect : ((!isOpenWnd) ? ENodeTreeState.SoulStoneWait : ENodeTreeState.SoulStoneWait2));
			}
			else if (isGetBonus)
			{
				eNodeTreeState = ENodeTreeState.SoulStoneAction;
			}
			else
			{
				if (isConfirmResponse)
				{
					OpenSoulStoneRestoreRewardPopup(3.8f);
					return;
				}
				if (!String_Extension_Method.IsNullOrEmpty(rewardItemCode))
				{
					eNodeTreeState = ENodeTreeState.SoulStoneWait;
				}
			}
			if (eNodeTreeState == ENodeTreeState.None)
			{
				return;
			}
			SetResultBgNodePlay(rewardItemCode, eNodeTreeState, delegate
			{
				OnOffContentRoot(isOn: false);
			}, delegate
			{
				if (!IsSelectingSoulStone)
				{
					if (isGetBonus)
					{
						OpenSoulStoneRestoreRewardPopup(3.8f);
					}
					else if (!IsItemResultSoulStone)
					{
						OpenSoulStoneRestoreRewardPopup(1.5f);
					}
				}
			});
		}
		else
		{
			SetRestoreSoulStoneInfo();
			SetMainBgNodeTreePlay();
		}
	}

	private void OpenSoulStoneRestoreRewardPopup(float duration)
	{
		OnOffContentRoot(isOn: true);
		if (_restoreSoulStoneRewardInfo != null)
		{
			if (BgNodeTreeUIPlayer != null)
			{
				BgNodeTreeUIPlayer.OrthographicMode(orthographic: false);
			}
			VespaCoroutine.Start(TouchDisableCoroutine(duration));
			string stringDirect = LocalizationManager.GetStringDirect("MULTIPLAY_REWARD_CLOSE");
			BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetItemResultInfos(_restoreSoulStoneRewardInfo.ItemResults).SetEquipResultInfos(_restoreSoulStoneRewardInfo.EquipItemResults).SetCurrencyResultInfos3(_restoreSoulStoneRewardInfo.CurrencyResults)
				.SetTitle(stringDirect), isGlobal: false);
			Refresh(null);
		}
	}

	private IEnumerator TouchDisableCoroutine(float duration)
	{
		GlobalTopmostWindow.instance.AddTouchDisabler(this);
		yield return new WaitForSeconds(duration);
		GlobalTopmostWindow.instance.RemoveTouchDisabler(this);
		yield return null;
	}

	public void OnClick()
	{
		if (_nodeTreeMainState == ENodeTreeState.NotEnoughSoulStone)
		{
			if (_listener != null)
			{
				_listener.NotEnoughSoulStoneFragment(RestoreSoulStoneInfo.SoulStonItemIndex);
			}
		}
		else if (IsPossibleRestoreSoulStone)
		{
			if (_listener != null)
			{
				_listener.OnClickRestoreButton();
			}
		}
		else if (IsSelectingSoulStone)
		{
			SetResultBgNodePlay(null, ENodeTreeState.SoulStoneSelect, delegate
			{
				OnOffContentRoot(isOn: false);
			}, delegate
			{
			});
		}
		else if (IsItemResultSoulStone)
		{
			string rewardItemCode = GetRewardItemCode(_restoreSoulStoneRewardInfo.ItemResults);
			if (!String_Extension_Method.IsNullOrEmpty(rewardItemCode))
			{
				SetResultBgNodePlay(rewardItemCode, ENodeTreeState.SoulStoneAction, delegate
				{
				}, delegate
				{
					OpenSoulStoneRestoreRewardPopup(3.8f);
				});
			}
		}
		else if (RestoreBonusInfo.IsPossibleGetBonus && _listener != null)
		{
			_listener.GetBonus();
		}
	}
}
