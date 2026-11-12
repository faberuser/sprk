using NGame2.NBattleView;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NGame2.NRender;
using UnityEngine;

namespace NGame2.NNodeTree.NUtil;

public class UidCache
{
	private RenderFacade _renderFacade;

	private Creature _battleCreature;

	public void SetRenderFacade(RenderFacade renderFacade)
	{
		_renderFacade = renderFacade;
	}

	public RenderFacade GetRenderFacade(IVariable variable, ArgumentSingleGeneric<long> uidArgument)
	{
		if (_renderFacade != null)
		{
			if (_renderFacade.Opened)
			{
				return _renderFacade;
			}
			_renderFacade = null;
		}
		_renderFacade = FindRenderFacade(variable, uidArgument);
		return _renderFacade;
	}

	public static RenderFacade FindRenderFacade(IVariable variable, ArgumentSingleGeneric<long> uidArgument)
	{
		if (variable == null || uidArgument == null)
		{
			return null;
		}
		long value = uidArgument.GetValue(variable);
		if (value == 0L)
		{
			return null;
		}
		return variable.RUNTIMEINSTANCE().SCENECONTAINER().GETRENDERFACADE((int)value);
	}

	public Creature GetBattleCreature(IVariable variable, ArgumentSingleGeneric<long> uidArgument)
	{
		if (_battleCreature != null)
		{
			return _battleCreature;
		}
		_battleCreature = FindBattleCreature(variable, uidArgument);
		return _battleCreature;
	}

	public static Creature FindBattleCreature(IVariable variable, ArgumentSingleGeneric<long> uidArgument)
	{
		if (variable == null || uidArgument == null)
		{
			return null;
		}
		long value = uidArgument.GetValue(variable);
		if (value == 0L)
		{
			return null;
		}
		return variable.RUNTIMEINSTANCE().SCENECONTAINER().GETBATTLECREATURE((int)value);
	}

	public bool TryGetPosition(IVariable variable, ArgumentSingleGeneric<long> uidArgument, out Vector3 oPos)
	{
		Creature battleCreature = GetBattleCreature(variable, uidArgument);
		if (battleCreature != null)
		{
			oPos = battleCreature.Position;
			return true;
		}
		RenderFacade renderFacade = GetRenderFacade(variable, uidArgument);
		if (renderFacade != null)
		{
			oPos = renderFacade.transform.position;
			return true;
		}
		oPos = Vector3.zero;
		return false;
	}
}
