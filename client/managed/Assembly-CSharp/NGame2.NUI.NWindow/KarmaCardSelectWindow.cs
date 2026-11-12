using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NWindow.NBattle.NAnim;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class KarmaCardSelectWindow : BaseWindow, IInspectorGuiDrawer
{
	public interface IListener
	{
		void OnClickSelect(int slotIndex);
	}

	[Serializable]
	public class CardPlate
	{
		public bool Select;

		public Transform Transform;

		public int CardPlateIndex;

		public UIBundleLoader IconTexture;

		public UIGrid ClassTagGrid;

		public UISpriteSelector CardGradeSpriteSelector;

		public UILabel NameLabel;

		public UILabel DescLabel;

		public UIButton ButtonComponent;

		private List<ClassTagIconComponent> _classTagComp = new List<ClassTagIconComponent>();

		public void SetCard(int index)
		{
			KarmaCardData data = GameRoot.Instance.KarmaCardDataContainer.GetData(index);
			if (data != null)
			{
				UIHelper.SetBundleTexture(IconTexture, data.IconPath);
				SetClassTagTypes(data.ClassTypes);
				if (CardGradeSpriteSelector != null)
				{
					CardGradeSpriteSelector.Select(data.Rarity.ToString());
				}
				UIHelper.SetLabelTextLocalizeDirect(NameLabel, data.RarityDesc);
				string stringDirect = LocalizationManager.GetStringDirect(data.CardDescCode, data.BuffValue);
				UIHelper.SetLabelTextLocalizeDirect(DescLabel, stringDirect);
				UIHelper.EnableGameObject(Transform.gameObject, enable: true);
			}
		}

		public void Close()
		{
			ReleaseClassTagTypes();
		}

		private bool IsAllTagTypes(CreatureTagType[] creatureTagTypes)
		{
			HashSet<CreatureTagType> hashSet = new HashSet<CreatureTagType>(creatureTagTypes);
			for (int i = 1; i < 8; i++)
			{
				CreatureTagType item = (CreatureTagType)i;
				if (!hashSet.Contains(item))
				{
					return false;
				}
			}
			return true;
		}

		private void SetClassTagTypes(CreatureTagType[] creatureTagTypes)
		{
			ReleaseClassTagTypes();
			if (IsAllTagTypes(creatureTagTypes))
			{
				ClassTagIconComponent classTagIconComponent = ClassTagIconComponent.PrefabAuto.Factory.Create(ClassTagGrid.transform);
				classTagIconComponent.OpenAllClassTag(null);
				_classTagComp.Add(classTagIconComponent);
				ClassTagGrid.Reposition();
				return;
			}
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(creatureTagTypes); i++)
			{
				CreatureTagType creatureTagType = creatureTagTypes[i];
				ClassTagIconComponent classTagIconComponent2 = ClassTagIconComponent.PrefabAuto.Factory.Create(ClassTagGrid.transform);
				if (classTagIconComponent2 == null)
				{
					break;
				}
				if (creatureTagType == CreatureTagType.None)
				{
					classTagIconComponent2.OpenAllClassTag(null);
				}
				else
				{
					classTagIconComponent2.Open(creatureTagType, null);
				}
				_classTagComp.Add(classTagIconComponent2);
			}
			ClassTagGrid.Reposition();
		}

		public void ReleaseClassTagTypes()
		{
			FactoryBase.ReleaseAndClearList(_classTagComp);
		}
	}

	public class BuffInfo : Tuple<string, string, int, int, int>
	{
		public string IconPath => Item1;

		public string DescKey => Item2;

		public int BuffValue => Item3;

		public int Priority => Item4;

		public int StackCount => Item5;

		public string BuffValueUnit { get; private set; }

		public BuffInfo(string iconPath, string descKey, int buffValue, int priority, string buffValueUnit)
			: base(iconPath, descKey, buffValue, priority, 1)
		{
			BuffValueUnit = buffValueUnit;
		}

		public void AddBuffValue(int buffValue)
		{
			Item3 += buffValue;
			IncreaseStackCount();
		}

		private void IncreaseStackCount()
		{
			Item5++;
		}
	}

	public static string PrefabPath = "Resources/Prefab2/Window/Raid/KarmaCardSelectWindow";

	public CardPlate[] CardPlates;

	public GameObject CardPlateRoot;

	public UIGrid CardPlateGrid;

	public UILabel SelectTimeLabel;

	public UIGame3DWidget BuffIconEffect;

	public UIButton ButtonBuffToMax;

	public UIButton ButtonBuffoMin;

	public UILabel LabelBuffCount;

	public UISimpleGrid BuffGrid;

	public UIGrid SelectGrid;

	public Animator Animator;

	public AnimationCurve AnimationCurve;

	public UIGame3DWidget FoldOutHitEffect;

	public Transform TempPos;

	public int SelectCardIndex;

	private IListener _listener;

	private StateUpdater _animStateUpdater = new StateUpdater();

	private bool _selectCard;

	private bool _isSuffled;

	private KarmaBuffComponent _lastBuffComponent;

	private Dictionary<int, BuffInfo> _selectBuff = new Dictionary<int, BuffInfo>();

	private List<KarmaBuffComponent> _listComp = new List<KarmaBuffComponent>();

	private int[] _cardIndices;

	private bool _isExpandBuff;

	public void Openbase(IListener listener)
	{
		Open(delegate
		{
			_listener = listener;
			Set();
		}, null);
	}

	public override void OnClosing()
	{
		base.OnClosing();
	}

	private void Set()
	{
		ShowCardRoot(show: false);
		if (SelectGrid != null)
		{
			SelectGrid.onCustomSort = CompareBuffComponent;
		}
		UIHelper.EnableGameObject(ButtonBuffoMin, enable: false);
		UIHelper.EnableGameObject(ButtonBuffToMax, enable: false);
	}

	public void ShowCardRoot(bool show)
	{
		UIHelper.EnableGameObject(CardPlateRoot, show);
	}

	public void SetCardPlateGridReposition()
	{
		if (!(CardPlateGrid == null))
		{
			CardPlateGrid.Reposition();
		}
	}

	public float GetCardSelectLeftTime()
	{
		return 10f;
	}

	public void SetCardSelectLeftTime(int time)
	{
		UIHelper.SetLabelText(SelectTimeLabel, time);
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (_animStateUpdater != null)
		{
			_animStateUpdater.OnUpdate();
		}
	}

	public void SetCardList(int[] cardIndices)
	{
		SetCardPlates(cardIndices);
	}

	public void StartAnimation()
	{
		_animStateUpdater.SetState(new KarmaCardInit(this));
	}

	public void StartAnimation(int[] cardIndices)
	{
		SetCardPlates(cardIndices);
		_animStateUpdater.SetState(new KarmaCardInit(this));
	}

	public void SetCardPlates(int[] cardIndices)
	{
		if (__Extension_Method_ICollection__.COUNT(cardIndices) < 3)
		{
			return;
		}
		_cardIndices = cardIndices;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(CardPlates); i++)
		{
			CardPlate cardPlate = CardPlates[i];
			int num = cardIndices[i];
			if (cardPlate != null && num != 0)
			{
				cardPlate.SetCard(num);
			}
		}
		_isSuffled = true;
	}

	public bool GetSelectCard()
	{
		return _selectCard;
	}

	public void SetSelectCard(bool isSelect)
	{
		_selectCard = isSelect;
	}

	public Transform GetTargetTransform()
	{
		if (_isExpandBuff)
		{
			if (_lastBuffComponent == null)
			{
				return TempPos;
			}
			return _lastBuffComponent.transform;
		}
		return TempPos;
	}

	public void PlayCardHitEffect()
	{
		if (!_isExpandBuff)
		{
			string stringDirect = LocalizationManager.GetStringDirect("RESCOLLECT_ITEMCOUNT", _listComp.Count);
			UIHelper.SetLabelText(LabelBuffCount, stringDirect);
			FoldOutHitEffect.Play(null);
		}
		else if (!(_lastBuffComponent == null))
		{
			_lastBuffComponent.PlayEffectCardHit();
			_lastBuffComponent.Refresh();
			_lastBuffComponent.RefreshTooltip();
		}
	}

	public float GetCurve(float time)
	{
		if (AnimationCurve == null)
		{
			return time;
		}
		return AnimationCurve.Evaluate(time);
	}

	public void OnClickCard(UIButton button)
	{
		if (!_isSuffled)
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(CardPlates); i++)
		{
			CardPlate cardPlate = CardPlates[i];
			if (cardPlate != null && !(cardPlate.ButtonComponent != button))
			{
				AddBuffComponent(_cardIndices[i]);
				SelectCardIndex = i;
				SetSelectCard(isSelect: true);
				_isSuffled = false;
				if (_listener != null)
				{
					_listener.OnClickSelect(i);
					break;
				}
			}
		}
	}

	public void AutoSelect(int index)
	{
		if (Vespa_Generic_IList_Extension_Method.IsValidIndex(CardPlates, index))
		{
			CardPlate cardPlate = CardPlates[index];
			if (cardPlate != null)
			{
				OnClickCard(cardPlate.ButtonComponent);
			}
		}
	}

	public void ForCardPlate(Action<CardPlate> action)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(CardPlates); i++)
		{
			CardPlate cardPlate = CardPlates[i];
			if (cardPlate != null)
			{
				Vespa_System_Action_Extension_Method.fire(action, cardPlate);
			}
		}
	}

	private void AddBuffComponent(int cardIndex)
	{
		KarmaCardData data = GameRoot.Instance.KarmaCardDataContainer.GetData(cardIndex);
		if (data == null)
		{
			return;
		}
		int groupIndex = data.GroupIndex;
		bool flag = false;
		if (!_selectBuff.TryGetValue(groupIndex, out var value))
		{
			value = new BuffInfo(data.BuffIconPath, data.CardDescCode, data.BuffValue, data.BuffIconIndex, data.BuffValueUnit);
			_selectBuff.Add(groupIndex, value);
			flag = true;
		}
		else
		{
			value.AddBuffValue(data.BuffValue);
		}
		KarmaBuffComponent karmaBuffComponent = null;
		if (flag)
		{
			karmaBuffComponent = KarmaBuffComponent.PrefabAuto.Factory.Create(SelectGrid.transform);
			if (karmaBuffComponent == null)
			{
				return;
			}
			karmaBuffComponent.Open(groupIndex, value);
			_listComp.Add(karmaBuffComponent);
			SelectGrid.Reposition();
		}
		else
		{
			for (int i = 0; i < _listComp.Count; i++)
			{
				KarmaBuffComponent karmaBuffComponent2 = _listComp[i];
				if (!(karmaBuffComponent2 == null) && karmaBuffComponent2.GroupIndex == groupIndex)
				{
					karmaBuffComponent = karmaBuffComponent2;
				}
			}
		}
		_lastBuffComponent = karmaBuffComponent;
		karmaBuffComponent.RefreshTooltip();
		MoveToButton();
		if (__Extension_Method_ICollection__.COUNT(_listComp) == 1 && value.StackCount == 1)
		{
			OnClickButtonBuffMax();
		}
	}

	public Vector3 GetButtonPos()
	{
		int num = __Extension_Method_ICollection__.COUNT(_listComp);
		float num2 = ((SelectGrid != null) ? SelectGrid.cellWidth : 0f);
		int num3 = ((SelectGrid != null) ? SelectGrid.maxPerLine : 0);
		if (num > num3)
		{
			num = num3;
		}
		return new Vector3(num2 * (float)num, 0f, 0f);
	}

	public bool IsCardPickEnded()
	{
		return _animStateUpdater.IsEnded();
	}

	private void MoveToButton()
	{
		if (!(ButtonBuffoMin == null))
		{
			ButtonBuffoMin.transform.localPosition = GetButtonPos();
		}
	}

	public void OnClickButtonBuffMax()
	{
		if (_listComp.Count >= 1)
		{
			_isExpandBuff = true;
			UIHelper.EnableGameObject(BuffGrid, enable: true);
			UIHelper.EnableGameObject(ButtonBuffToMax, enable: false);
			UIHelper.EnableGameObject(ButtonBuffoMin, enable: true);
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listComp); i++)
		{
			KarmaBuffComponent karmaBuffComponent = _listComp[i];
			if (!(karmaBuffComponent == null))
			{
				karmaBuffComponent.Refresh();
			}
		}
		SelectGrid.Reposition();
	}

	public void OnClickButtonBuffMin()
	{
		_isExpandBuff = false;
		UIHelper.EnableGameObject(BuffGrid, enable: false);
		UIHelper.EnableGameObject(ButtonBuffToMax, enable: true);
		UIHelper.EnableGameObject(ButtonBuffoMin, enable: false);
		if (BuffIconEffect != null)
		{
			BuffIconEffect.Play(null);
		}
		string stringDirect = LocalizationManager.GetStringDirect("RESCOLLECT_ITEMCOUNT", _listComp.Count);
		UIHelper.SetLabelText(LabelBuffCount, stringDirect);
	}

	public int CompareBuffComponent(Transform lhs, Transform rhs)
	{
		KarmaBuffComponent component = lhs.GetComponent<KarmaBuffComponent>();
		KarmaBuffComponent component2 = rhs.GetComponent<KarmaBuffComponent>();
		if (component == null || component2 == null)
		{
			return 0;
		}
		return component.Priority.CompareTo(component2.Priority);
	}
}
