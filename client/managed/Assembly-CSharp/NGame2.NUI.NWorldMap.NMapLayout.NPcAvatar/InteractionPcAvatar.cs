using System.Collections.Generic;
using NShared;

namespace NGame2.NUI.NWorldMap.NMapLayout.NPcAvatar;

public class InteractionPcAvatar : RoamingPcAvatar
{
	public enum EInteractionType
	{
		None,
		ClickReward,
		ClickAvatar
	}

	private class InteractionWrapper
	{
		private readonly EInteractionType Type;

		private readonly IInteractiveAction Action;

		public InteractionWrapper(EInteractionType type, IInteractiveAction action)
		{
			Type = type;
			Action = action;
		}

		public bool ActionIfEqual(EInteractionType type)
		{
			if (Action == null)
			{
				return false;
			}
			if (type != Type)
			{
				return false;
			}
			if (!Action.CanAction())
			{
				return false;
			}
			Action.DoAction();
			return true;
		}

		public void Update()
		{
			if (Action != null)
			{
				Action.Update();
			}
		}
	}

	protected long Uid;

	protected string Name;

	private List<InteractionWrapper> _interactionWrapperList = new List<InteractionWrapper>();

	protected override void SetAvatarBehavior(BasePcMediator mediator, PcAvatarData pcAvatarData)
	{
		base.SetAvatarBehavior(mediator, pcAvatarData);
	}

	public override void SetInteractionComponent(PcAvatarData data)
	{
		base.SetInteractionComponent(data);
		EInteractionType type = EInteractionType.None;
		IInteractiveAction interactiveAction = null;
		if (data.RewardOpen)
		{
			type = EInteractionType.ClickReward;
			interactiveAction = new GuildMemeberReward();
			interactiveAction.Init(data, base.AiInteractionComponent);
		}
		else if (!__Extension_Method_ICollection__.IsNullOrEmpty(data.TouchText))
		{
			type = EInteractionType.ClickAvatar;
			interactiveAction = new TextBallon();
			interactiveAction.Init(data, base.AiInteractionComponent);
		}
		InteractionWrapper item = new InteractionWrapper(type, interactiveAction);
		_interactionWrapperList.Add(item);
	}

	public override void SetPcAvatarInfo(long uid, string name)
	{
		base.SetPcAvatarInfo(uid, name);
		Uid = uid;
		Name = name;
	}

	protected override void OnClickReward()
	{
		_interactionWrapperList.ForEach(delegate(InteractionWrapper wrapper)
		{
			wrapper.ActionIfEqual(EInteractionType.ClickReward);
		});
	}

	protected override void ClickAvatarDerived()
	{
		_interactionWrapperList.ForEach(delegate(InteractionWrapper wrapper)
		{
			wrapper.ActionIfEqual(EInteractionType.ClickAvatar);
		});
	}

	public override void ChainUpdate()
	{
		base.ChainUpdate();
		if (!Hide)
		{
			_interactionWrapperList.ForEach(delegate(InteractionWrapper wrapper)
			{
				wrapper.Update();
			});
		}
	}
}
