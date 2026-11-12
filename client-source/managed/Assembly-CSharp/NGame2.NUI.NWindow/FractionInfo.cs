using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow;

public class FractionInfo : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Lobby/FractionInfo";

	public UISimpleGrid FractionComponentGrid;

	public TweenPosition DisplayTweener;

	private List<FractionInfoComponent> _fractionCompList = new List<FractionInfoComponent>();

	[NonSerialized]
	public static LocalAccountBool IsClosed = new LocalAccountBool(i_default: false, typeof(FractionInfo).FullName + "IsClosed");

	public new void Open()
	{
		base.Open();
		DisplayTweener.enabled = false;
		if (IsClosed.Value)
		{
			DisplayTweener.PlayForward();
		}
		else
		{
			WindowRoot.localPosition = DisplayTweener.from;
		}
		for (CreatureTagType creatureTagType = CreatureTagType.Warrior; creatureTagType <= CreatureTagType.Mechanic; creatureTagType++)
		{
			FractionInfoComponent fractionInfoComponent = UIHelper.AddGridItem(FractionComponentGrid, FractionInfoComponent.PrefabAuto);
			fractionInfoComponent.Open(creatureTagType);
			_fractionCompList.Add(fractionInfoComponent);
		}
		UIHelper.RepositionGrid(FractionComponentGrid);
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_fractionCompList);
	}

	public void OnClickFold()
	{
		if (!DisplayTweener.enabled)
		{
			IsClosed.Value = !IsClosed.Value;
			if (IsClosed.Value)
			{
				DisplayTweener.PlayForward();
			}
			else
			{
				DisplayTweener.PlayReverse();
			}
		}
	}
}
