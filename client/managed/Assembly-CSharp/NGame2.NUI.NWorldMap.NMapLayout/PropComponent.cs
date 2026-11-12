using System;
using System.Collections.Generic;
using NShared;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout;

public class PropComponent : MonoBehaviour
{
	protected static Dictionary<PropManager.PropActionType, Func<IPropDecorator>> propDecoratorCreator;

	private IPropDecorator _propDecorator;

	public bool IsValid()
	{
		return _propDecorator != null;
	}

	public void Setup(string[] propActionArgs, Mediator mapMediator)
	{
		RegistPropDecoratorCreator();
		Release();
		_propDecorator = CreatePropDecorator(propActionArgs, mapMediator);
		_propDecorator.SetPropComponent(this, propActionArgs, mapMediator);
	}

	private void RegistPropDecoratorCreator()
	{
		if (propDecoratorCreator == null)
		{
			propDecoratorCreator = new Dictionary<PropManager.PropActionType, Func<IPropDecorator>>();
			propDecoratorCreator.Add(PropManager.PropActionType.SetRenderFacade, () => new NGame2.NUI.NWorldMap.NMapLayout.RenderFacadePropDecorator());
			propDecoratorCreator.Add(PropManager.PropActionType.SetHonorRenderFacade, () => new NGame2.NUI.NWorldMap.NMapLayout.HonorRenderFacadePropDecorator());
			propDecoratorCreator.Add(PropManager.PropActionType.SetOrdealArena, () => new OrdealArenaRewardPropDecorator());
		}
	}

	private IPropDecorator CreatePropDecorator(string[] propActionArgs, Mediator mapMediator)
	{
		PropManager.PropActionType valueEnum = ArgParser.GetValueEnum<string, PropManager.PropActionType>((IList<string>)propActionArgs, 0, PropManager.PropActionType.None);
		Func<IPropDecorator> value = null;
		if (!propDecoratorCreator.TryGetValue(valueEnum, out value))
		{
			return new NGame2.NUI.NWorldMap.NMapLayout.NullPropDecorator();
		}
		return Vespa_System_Action_Extension_Method.fire(value);
	}

	public void SetExtraAlpha(float alpha)
	{
		if (_propDecorator != null)
		{
			_propDecorator.SetExtraAlpha(alpha);
		}
	}

	public virtual void ChainUpdate()
	{
		if (_propDecorator != null)
		{
			_propDecorator.ChainUpdate();
		}
	}

	public void Release()
	{
		if (_propDecorator != null)
		{
			_propDecorator.Release();
			_propDecorator = null;
		}
	}

	public void Refresh()
	{
		if (_propDecorator != null)
		{
			_propDecorator.Refresh();
		}
	}
}
