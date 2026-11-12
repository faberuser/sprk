using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NStat;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NPayShop;
using NGameAsset.NCreature;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NPet;

public class PetDetailView : HeroFullSceneViewBase, HeroViewAnimationComponent.Listener
{
	public class PetDetailViewInfo
	{
		public PetInfo PetInfo { get; set; }

		public bool IsShowPetStat { get; set; }

		public Listener IBaseListner { get; set; }

		public Action OpenAction { get; set; }

		public Action CloseAction { get; set; }

		public bool UseSceneChanger { get; set; }

		public PetDetailViewInfo()
		{
			OpenAction = null;
			CloseAction = null;
			UseSceneChanger = true;
		}
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Pet/PetDetailView";

	[Header("[PetDetailView]")]
	public UIScrollGrid PetViewGrid;

	public GameObject PetStatObject;

	public UILabel PetGradeLabel;

	public UILabel PetNameLabel;

	public UILabel PetStoryLabel;

	public UIPanel PetStatPanel;

	public UIScrollView PetStatScrollView;

	private List<HeroViewAnimationComponent> _petViewAnimationComponentList = new List<HeroViewAnimationComponent>();

	private StatListComponent _statComponent;

	private bool _isInitPetAnimation;

	private bool _petAnimationInitStart;

	private Action _closeAction;

	protected override string _nodeTreePackCode => base._nodeTreePackCode;

	protected override string _nodeTreeName => base._nodeTreeName;

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_petViewAnimationComponentList);
		FactoryBase.SafeReleaseComponent(ref _statComponent);
		_isInitPetAnimation = false;
		_petAnimationInitStart = false;
	}

	public void Open(PetDetailViewInfo info)
	{
		if (info == null)
		{
			return;
		}
		_closeAction = info.CloseAction;
		Action onEnd = delegate
		{
			Init(info.IsShowPetStat);
			if (info.OpenAction != null)
			{
				info.OpenAction();
			}
		};
		Open(info.PetInfo, info.IBaseListner, onEnd, info.UseSceneChanger);
	}

	public override void Close()
	{
		base.Close();
		if (_closeAction != null)
		{
			Vespa_System_Action_Extension_Method.fire(_closeAction);
		}
	}

	private void Init(bool isShowPetStat)
	{
		UIHelper.EnableGameObject(PetStatObject, isShowPetStat);
		if (isShowPetStat)
		{
			InitPetStat();
		}
		_petAnimationInitStart = true;
	}

	private void InitPetStat()
	{
		UIHelper.SetLabelText(PetGradeLabel, base.CreatureData.CreatureGrade);
		UIHelper.SetLabelText(PetNameLabel, base.CreatureData.Name);
		string stringDirect = LocalizationManager.GetStringDirect(base.CreatureData.ArchiveStory);
		UIHelper.SetLabelText(PetStoryLabel, stringDirect);
		SetupPetStatList();
	}

	private void SetupPetStatList()
	{
		if (_statComponent == null)
		{
			_statComponent = StatListComponent.PetStatCreate(PetStatPanel, 5);
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(base.CreatureData.PetOptionIndices); i++)
		{
			int optionIndex = base.CreatureData.PetOptionIndices[i];
			_statComponent.AddPetStat(optionIndex, 0);
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(base.CreatureData.PetBonusIndices); j++)
		{
			int optionIndex2 = base.CreatureData.PetBonusIndices[j];
			_statComponent.AddPetBonusStat(optionIndex2, 0);
		}
		PetStatScrollView.ResetPosition();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (_petAnimationInitStart)
		{
			InitPetAnimation();
		}
	}

	private void InitPetAnimation()
	{
		if (_motionSequence == null || _isInitPetAnimation)
		{
			return;
		}
		_isInitPetAnimation = true;
		FactoryBase.ReleaseAndClearList(_petViewAnimationComponentList);
		HeroAnimationViewData[] datas = GameRoot.Instance.HeroAnimationViewDataContainer.GetDatas(base.PetInfo.PetIndex);
		if (datas == null)
		{
			return;
		}
		Dictionary<string, Tuple<HeroAnimationViewData, int>> dictionary = new Dictionary<string, Tuple<HeroAnimationViewData, int>>();
		MotionSequenceData[] motionSequenceDatas = _motionSequence.GetMotionSequenceDatas();
		for (int i = 0; i < motionSequenceDatas.Length; i++)
		{
			MotionSequenceData motionSequenceData = motionSequenceDatas[i];
			string name = motionSequenceData.MotionList[0].Key.PlayName;
			HeroAnimationViewData heroAnimationViewData = Array.Find(datas, (HeroAnimationViewData e) => string.Equals(e.Animation, name));
			if (heroAnimationViewData != null)
			{
				Tuple<HeroAnimationViewData, int> value = new Tuple<HeroAnimationViewData, int>(heroAnimationViewData, i);
				if (!dictionary.ContainsKey(name))
				{
					dictionary.Add(name, value);
				}
			}
		}
		List<Tuple<HeroAnimationViewData, int>> list = dictionary.Values.ToList();
		list.Sort((Tuple<HeroAnimationViewData, int> l, Tuple<HeroAnimationViewData, int> r) => l.Item1.Index.CompareTo(r.Item1.Index));
		foreach (Tuple<HeroAnimationViewData, int> item in list)
		{
			HeroViewAnimationComponent heroViewAnimationComponent = UIHelper.AddGridItem(PetViewGrid, HeroViewAnimationComponent.PetPrefabAuto);
			heroViewAnimationComponent.Open(this, item.Item2, item.Item1, null);
			heroViewAnimationComponent.SetSelected(selected: false);
			_petViewAnimationComponentList.Add(heroViewAnimationComponent);
		}
		UIHelper.RepositionGrid(PetViewGrid);
		HeroViewAnimationComponent heroViewAnimationComponent2 = _petViewAnimationComponentList[0];
		heroViewAnimationComponent2.SetSelected(selected: true);
		_motionSequence.Play(heroViewAnimationComponent2.SequenceIndex);
	}

	void HeroViewAnimationComponent.Listener.PlayAnimation(HeroViewAnimationComponent component)
	{
		_motionSequence.Play(component.SequenceIndex);
		foreach (HeroViewAnimationComponent petViewAnimationComponent in _petViewAnimationComponentList)
		{
			petViewAnimationComponent.SetSelected(selected: false);
		}
		component.SetSelected(selected: true);
		UpdatePauseButtonState();
	}

	void HeroViewAnimationComponent.Listener.PlayCinema(HeroViewAnimationComponent component)
	{
	}

	public override void OnClickCloseNormal()
	{
		if (SceneAutoBehaviour<PayShopManagement>.isValidInstance)
		{
			PayShopHeroInfoPopup instance = SceneAutoBehaviour<PayShopManagement>.instance.GetInstance<PayShopHeroInfoPopup>(openedOnly: true);
			if (instance != null && instance.IsOpen())
			{
				instance.EnableCreatureWidget(enable: true);
			}
		}
		if (SceneAutoBehaviour<InventoryManagement>.isValidInstance)
		{
			PayShopHeroInfoPopup instance2 = SceneAutoBehaviour<InventoryManagement>.instance.GetInstance<PayShopHeroInfoPopup>(openedOnly: true);
			if (instance2 != null && instance2.IsOpen())
			{
				instance2.EnableCreatureWidget(enable: true);
			}
		}
		base.OnClickCloseNormal();
	}
}
