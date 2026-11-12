using System;
using System.Collections.Generic;
using System.Text;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NSubStory;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWorldMap;
using NGameAsset;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class SubStoryNodeInfo : BaseWindow
{
	public enum NodeRequireType
	{
		Hero,
		ClearDungeon,
		HeroRequire
	}

	public const string PrefabPath = "Resources/Prefab2/Window/SubStory/SubStoryNodeInfo";

	private const string LOCALE_TITLE = "SUBSTORY_MAIN_TITLE_STORY";

	public TweenAlpha[] TweenAlpha_Ary;

	public TweenPosition[] TweenPosition_Ary;

	public TweenPosition DisplayTweener;

	public UILabel Label_Title;

	public UISimpleGrid Grid_RequireCondtion;

	public UIScrollGrid Grid_FirstReward;

	public Transform Root_RequireCondition;

	public Transform Root_Rewards;

	public Transform Root_Costume;

	public Transform Root_Tactic;

	public UIScrollView ScrollView_Info;

	public UITexture Texture_CostumePortrait;

	public UILabel Label_CostumeName;

	public UILabel Label_Tactic;

	[Header("[InfoWindow Position]")]
	public Vector3 Gap_RequireCondition;

	public Vector3 Gap_Rewards;

	public Vector3 Gap_Costume;

	public Vector3 StartPositionOfFirst;

	[Header("[RequireCondition Text Color]")]
	public Color TextColor;

	public Color OutlineColor;

	public Color FailTextColor;

	public Color FailOutlineColor;

	private List<NodeInfoComponent> _componentList = new List<NodeInfoComponent>();

	private List<Component> _rewardCmponentList = new List<Component>();

	private SubStoryData _subStoryData;

	private CampaignNodeData _nodeData;

	private int _chapterIndex;

	private int _dungeonIndex;

	private bool _needUpdate;

	private bool _canShowNodeInfo;

	private bool _isClosed;

	private bool _isCompleted;

	public void OpenWindow()
	{
		base.Open();
		Init();
		SetDungeonInfo();
	}

	private void Init()
	{
		_chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		_dungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
		_nodeData = GameRoot.Instance.CampaignNodeDataContainer.GetData(_chapterIndex, _dungeonIndex);
		_subStoryData = GameRoot.Instance.SubStoryDataContainer.GetData(_chapterIndex, _dungeonIndex);
		_needUpdate = true;
	}

	private void SetDungeonInfo()
	{
		SetStoryTitle();
		SetRequireCondition();
		SetFirstReward();
		SetBuyableCostume();
		SetTacticInfo();
		SetPositionOfInfoElement();
	}

	private void SetStoryTitle()
	{
		if (_subStoryData != null)
		{
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(_subStoryData.CreatureIndex);
			if (data != null)
			{
				UIHelper.SetLabelText(Label_Title, LocalizationManager.GetStringDirect("SUBSTORY_MAIN_TITLE_STORY", data.Name));
			}
		}
	}

	private void SetRequireCondition()
	{
		UIHelper.EnableGameObject(Grid_RequireCondtion, _subStoryData != null);
		if (_subStoryData == null || _subStoryData.ReqStar == 0)
		{
			UIHelper.EnableGameObject(Root_RequireCondition, enable: false);
			return;
		}
		UIHelper.EnableGameObject(Root_RequireCondition, enable: true);
		FactoryBase.ReleaseAndClearList(_componentList);
		if (_subStoryData.ReqStar != 0)
		{
			NodeInfoComponent nodeInfoComponent = UIHelper.AddGridItem(Grid_RequireCondtion, NodeInfoComponent.PrefabAuto);
			nodeInfoComponent.Init(NodeRequireType.HeroRequire, _subStoryData);
			_componentList.Add(nodeInfoComponent);
		}
		Grid_RequireCondtion.RepositionGrid();
	}

	private void SetTacticInfo()
	{
		UIHelper.EnableGameObject(Root_Tactic, enable: true);
		if (_subStoryData == null)
		{
			UIHelper.EnableGameObject(Root_Tactic, enable: false);
			return;
		}
		if (_subStoryData.SubStoryTactic == null || _subStoryData.SubStoryTactic.Length < 1)
		{
			UIHelper.EnableGameObject(Root_Tactic, enable: false);
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		string[] subStoryTactic = _subStoryData.SubStoryTactic;
		foreach (string key in subStoryTactic)
		{
			stringBuilder.AppendLine(LocalizationManager.GetStringDirect(key));
		}
		UIHelper.SetLabelText(Label_Tactic, stringBuilder.ToString());
	}

	private void SetFirstReward()
	{
		UIHelper.EnableGameObject(Grid_FirstReward, _subStoryData != null);
		if (_subStoryData == null)
		{
			return;
		}
		FactoryBase.ReleaseAndClearList(_rewardCmponentList);
		int num = _subStoryData.FirstRewardIndex;
		bool flag = false;
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(_subStoryData.ChapterIndex, _subStoryData.DungeonIndex);
		if (data != null)
		{
			switch ((DungeonDifficultyType)Helper.GetDifficulty(_subStoryData.ChapterIndex, _subStoryData.DungeonIndex))
			{
			case DungeonDifficultyType.Easy:
				num = data.FirstClearRewardIndex_Easy;
				flag = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(_subStoryData.ChapterIndex, _subStoryData.DungeonIndex);
				break;
			case DungeonDifficultyType.Normal:
				num = data.FirstClearRewardIndex_Normal;
				flag = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(_subStoryData.ChapterIndex, _subStoryData.DungeonIndex, DungeonDifficultyType.Normal);
				break;
			case DungeonDifficultyType.Hard:
				num = data.FirstClearRewardIndex_Hard;
				flag = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(_subStoryData.ChapterIndex, _subStoryData.DungeonIndex, DungeonDifficultyType.Hard);
				break;
			case DungeonDifficultyType.Hell:
				num = data.FirstClearRewardIndex_Hell;
				flag = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(_subStoryData.ChapterIndex, _subStoryData.DungeonIndex, DungeonDifficultyType.Hell);
				break;
			}
			if (num == 0)
			{
				num = data.FirstClearRewardIndex_Easy;
				flag = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(_subStoryData.ChapterIndex, _subStoryData.DungeonIndex);
			}
		}
		RewardData data2 = GameRoot.Instance.RewardDataContainer.GetData(num);
		if (data2 != null)
		{
			int GroupCount;
			foreach (RewardElementData item in data2.GetRewardItemListWithGroupCount(out GroupCount))
			{
				if (data2 == null)
				{
					continue;
				}
				CurrencyType currencyType = item.GetCurrencyType();
				if (currencyType != CurrencyType.None)
				{
					CurrencyIconComponent currencyIconComponent = UIHelper.AddGridItem(Grid_FirstReward, CurrencyIconComponent.PrefabAuto);
					int itemMin = item.ItemMin;
					CurrencyIconComponent.Listener listener = null;
					int num2 = itemMin;
					currencyIconComponent.Open(currencyType, num2, listener);
					_rewardCmponentList.Add(currencyIconComponent);
					continue;
				}
				ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(item.ItemCode);
				if (dataByCode != null)
				{
					int itemMin2 = item.ItemMin;
					long count = itemMin2;
					int itemMax = item.ItemMax;
					long maxCount = itemMax;
					ItemIconComponent itemIconComponent = UIHelper.AddGridItem(Grid_FirstReward, ItemIconComponent.PrefabAuto);
					itemIconComponent.Open(dataByCode);
					if (dataByCode.Type != ItemType.Equip)
					{
						itemIconComponent.SetItemMinMax(count, maxCount);
					}
					if (flag)
					{
						itemIconComponent.SetSelected(selected: true);
					}
					_rewardCmponentList.Add(itemIconComponent);
				}
			}
		}
		UIHelper.RepositionGrid(Grid_FirstReward);
	}

	private void SetBuyableCostume()
	{
		CostumeData costumeData = null;
		bool flag = _subStoryData == null || (costumeData = GameRoot.Instance.CostumeDataContainer.GetData(_subStoryData.UnlockCostumeIndex)) == null;
		UIHelper.EnableGameObject(Label_CostumeName, !flag);
		UIHelper.EnableGameObject(Root_Costume, !flag);
		if (!flag)
		{
			UIHelper.SetLabelText(Label_CostumeName, costumeData.CostumeName);
			if (!String_Extension_Method.IsNullOrEmpty(costumeData.IconPortraitPath))
			{
				AssetTexture plainTexture = UIHelper.GetPlainTexture(costumeData.IconPortraitPath);
				UIHelper.ResetPlaneAssetTextureAsync(this, Texture_CostumePortrait, plainTexture);
			}
		}
	}

	private void SetPositionOfInfoElement()
	{
		bool num = UIHelper.IsEnabledGameObjet(Root_RequireCondition);
		bool flag = UIHelper.IsEnabledGameObjet(Root_Rewards);
		bool flag2 = UIHelper.IsEnabledGameObjet(Root_Costume);
		bool flag3 = UIHelper.IsEnabledGameObjet(Root_Tactic);
		Root_RequireCondition.localPosition = Vector3.zero;
		Root_Rewards.localPosition = Vector3.zero;
		Root_Costume.localPosition = Vector3.zero;
		Root_Tactic.localPosition = Vector3.zero;
		Dictionary<string, Transform> dictionary = new Dictionary<string, Transform>();
		if (num)
		{
			dictionary.Add("Root_RequireCondition", Root_RequireCondition);
		}
		if (flag)
		{
			dictionary.Add("Root_Rewards", Root_Rewards);
		}
		if (flag2)
		{
			dictionary.Add("Root_Costume", Root_Costume);
		}
		if (flag3)
		{
			dictionary.Add("Root_Tactic", Root_Tactic);
		}
		int num2 = 0;
		Vector3 vector = Vector3.zero;
		Vector3 vector2 = Vector3.zero;
		foreach (KeyValuePair<string, Transform> item in dictionary)
		{
			Vector3 vector3 = Vector3.zero;
			if (item.Key == "Root_RequireCondition")
			{
				vector3 = Gap_RequireCondition;
			}
			else if (item.Key == "Root_Rewards")
			{
				vector3 = Gap_Rewards;
			}
			else if (item.Key == "Root_Costume")
			{
				vector3 = Gap_Costume;
			}
			if (num2 == 0)
			{
				item.Value.localPosition = StartPositionOfFirst;
				vector = StartPositionOfFirst;
				vector2 = vector3;
			}
			else
			{
				item.Value.localPosition = vector + vector2;
				vector = item.Value.localPosition;
				vector2 = vector3;
			}
			num2++;
		}
		ScrollView_Info.ResetPosition();
		ScrollView_Info.enabled = flag3 || dictionary.Count >= 3;
	}

	public override void OnSystemBackButton()
	{
		if (SceneAutoBehaviour<SubStoryManagement>.instance.EntryChapterNode != null)
		{
			MoveChapterMap(SceneAutoBehaviour<SubStoryManagement>.instance.EntryChapterNode, delegate
			{
				SceneAutoBehaviour<SubStoryManagement>.instance.SetEntryChapterNode(null);
			});
			return;
		}
		if (SceneAutoBehaviour<SubStoryManagement>.instance.EntryNode != null)
		{
			OpenSubStoryCharacterScene(SceneAutoBehaviour<SubStoryManagement>.instance.EntryNode);
			return;
		}
		List<CampaignNodeData> allDataOfContntType = GameRoot.Instance.CampaignNodeDataContainer.GetAllDataOfContntType(ContentsStatusType.Library);
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(allDataOfContntType))
		{
			CampaignNodeData entryNode = allDataOfContntType[0];
			OpenSubStoryCharacterScene(entryNode);
		}
	}

	private void OpenSubStoryCharacterScene(CampaignNodeData entryNode)
	{
		SceneAutoBehaviour<SubStoryManagement>.instance.OpenSubStoryCharacterScene(delegate
		{
			SceneAutoBehaviour<LobbyManagement>.instance.MoveMap(entryNode.ChapterIndex, entryNode.DungeonIndex);
		});
	}

	private void MoveChapterMap(CampaignNodeData entryNode, Action onEnd = null)
	{
		SceneAutoBehaviour<LobbyManagement>.instance.MoveMap(entryNode.ChapterIndex, entryNode.DungeonIndex, onEnd);
	}

	public override void Reset()
	{
		base.Reset();
		Release();
	}

	private void Release()
	{
		_chapterIndex = 0;
		_dungeonIndex = 0;
		_subStoryData = null;
		_needUpdate = false;
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		UpdataNodeInfo();
	}

	private void UpdataNodeInfo()
	{
		if (_dungeonIndex != NGame2.NAccount.UserCampaignManager.DungeonIndex)
		{
			_dungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
			_nodeData = GameRoot.Instance.CampaignNodeDataContainer.GetData(_chapterIndex, _dungeonIndex);
			_subStoryData = GameRoot.Instance.SubStoryDataContainer.GetData(_chapterIndex, _dungeonIndex);
			_isCompleted = false;
			_needUpdate = true;
		}
		if (_subStoryData != null && !_needUpdate && !_isCompleted)
		{
			bool flag = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(_subStoryData.ChapterIndex, _subStoryData.DungeonIndex);
			if (_isCompleted != flag)
			{
				_isCompleted = flag;
				_needUpdate = true;
			}
		}
		if (_needUpdate)
		{
			_canShowNodeInfo = CanShowNodeInfo();
			UpdateTween();
			if (_canShowNodeInfo)
			{
				SetDungeonInfo();
				_needUpdate = false;
			}
		}
	}

	private void UpdateTween()
	{
		if (_canShowNodeInfo)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(TweenAlpha_Ary); i++)
			{
				TweenAlpha_Ary[i].ResetToBeginning();
				TweenAlpha_Ary[i].PlayForward();
			}
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(TweenPosition_Ary); j++)
			{
				TweenPosition_Ary[j].ResetToBeginning();
				TweenPosition_Ary[j].PlayForward();
			}
		}
	}

	private bool CanShowNodeInfo()
	{
		if (_subStoryData == null)
		{
			return false;
		}
		if (IsWorldMapPlayerMoving())
		{
			return false;
		}
		Helper.FunctionStateData functionState = Helper.GetFunctionState(_nodeData, containsDisableMsg: false);
		if (functionState.Type == NShared.NodeFunctionType.None)
		{
			return false;
		}
		if (functionState.State == Helper.FunctionStateData.EFunctionState.Disabled)
		{
			return false;
		}
		return true;
	}

	private bool IsWorldMapPlayerMoving()
	{
		if (!SceneAutoBehaviour<WorldMapManagement>.isValidInstance)
		{
			return false;
		}
		if (SceneAutoBehaviour<WorldMapManagement>.instance.IsMapLoading)
		{
			return false;
		}
		MapLayout instance = SceneAutoBehaviour<WorldMapManagement>.instance.GetInstance<MapLayout>();
		if (instance == null)
		{
			return false;
		}
		return instance.IsPlayerMoving();
	}

	public void OnClickFold()
	{
		if (!DisplayTweener.enabled)
		{
			if (_isClosed)
			{
				DisplayTweener.PlayForward();
				DisplayTweener.onFinished.Clear();
			}
			else
			{
				DisplayTweener.PlayReverse();
				DisplayTweener.onFinished.Clear();
			}
			_isClosed = !_isClosed;
		}
	}
}
