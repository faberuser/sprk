using NGame2.N3D;
using NGame2.NRender.NRenderFacade;
using NGameAsset;
using NGameAsset.NCreature;
using NShared;
using NVespa.NGlobal;

namespace NGame2.NUI.NWorldMap.NMapLayout;

public class NpcAvatar : Avatar
{
	private CampaignNodeNPCData.NPCAvatarState _state;

	private bool _hasOffset;

	public string NPCCode { get; private set; }

	public NPCData NpcData { get; private set; }

	public MapNode MapNode { get; private set; }

	public int ChapterIndex
	{
		get
		{
			if (!(MapNode != null))
			{
				return 0;
			}
			return MapNode.ChapterIndex;
		}
	}

	public int DungeonIndex
	{
		get
		{
			if (!(MapNode != null))
			{
				return 0;
			}
			return MapNode.DungeonIndex;
		}
	}

	public void Open(AvatarController controller, MapComponent mapComponent, RenderFacadeBuilder renderFacadeBuilder, string npcCode, MapNode mapNode)
	{
		Open(new AvatarInfo
		{
			AvatarController = controller,
			MapComponent = mapComponent,
			RenderFacadeBuilder = renderFacadeBuilder
		});
		NPCCode = npcCode;
		MapNode = mapNode;
		NpcData = GameRoot.Instance.NPCDataContainer.GetData(NPCCode);
		if (NpcData != null)
		{
			if (!String_Extension_Method.IsNullOrEmpty(NpcData.NpcHudTextCode))
			{
				string stringDirect = LocalizationManager.GetStringDirect(NpcData.NpcHudTextCode);
				SetHudName(stringDirect);
			}
			_hasOffset = HasOffset();
		}
	}

	public void SetState(CampaignNodeNPCData.NPCAvatarState state, string[] motionValues, bool unUseBlenMotion = false)
	{
		if (state != CampaignNodeNPCData.NPCAvatarState.None && !__Extension_Method_ICollection__.IsNullOrEmpty(motionValues))
		{
			_state = state;
			SetMotion(state, motionValues, unUseBlenMotion);
		}
	}

	private void SetMotion(CampaignNodeNPCData.NPCAvatarState state, string[] motionValues, bool unUseBlenMotion)
	{
		DefineCreature.EMotionEnd valueEnum = ArgParser.GetValueEnum(motionValues, 0, DefineCreature.EMotionEnd.End);
		DefineCreature.EMotion valueEnum2 = ArgParser.GetValueEnum(motionValues, 1, DefineCreature.EMotion.Idle);
		string valueString = ArgParser.GetValueString(motionValues, 2);
		NGame2.NRender.NRenderFacade.AnimationData.StockData motionStock = new NGame2.NRender.NRenderFacade.AnimationData.StockData
		{
			State = NGame2.NRender.NRenderFacade.AnimationData.EState.Action,
			AnimationKey = new AnimationKeyData(valueEnum2, valueString),
			MotionEnd = valueEnum
		};
		SetMotion(motionStock, unUseBlenMotion);
	}

	public override int GetLastDungeonIndex()
	{
		int num = base.GetLastDungeonIndex();
		if (num == 0)
		{
			num = MapNode.DungeonIndex;
		}
		return num;
	}

	private bool HasOffset()
	{
		if (NpcData != null)
		{
			int[] npcOffset = NpcData.NpcOffset;
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(npcOffset); i++)
			{
				if (npcOffset[i] != 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public override bool CanBeRearranged()
	{
		if (_hasOffset)
		{
			return false;
		}
		if (NpcData != null)
		{
			if (_state == CampaignNodeNPCData.NPCAvatarState.Dead)
			{
				return false;
			}
			if (_state == CampaignNodeNPCData.NPCAvatarState.Lose)
			{
				return false;
			}
		}
		return base.CanBeRearranged();
	}
}
