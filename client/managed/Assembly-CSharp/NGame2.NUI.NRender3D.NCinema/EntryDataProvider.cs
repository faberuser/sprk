using System.Collections.Generic;
using NGameAsset.NCreature;
using NShared;
using UnityEngine;

namespace NGame2.NUI.NRender3D.NCinema;

public class EntryDataProvider : IDataProvider
{
	private List<ICommandMessageHandler> _handlerList = new List<ICommandMessageHandler>();

	private List<NodeTreeKeyword> _keywordList = new List<NodeTreeKeyword>();

	private List<IInteractionDataProvider> _interactionDataProviderList = new List<IInteractionDataProvider>();

	private HashSet<AnimationKeyData> _preloadAnimationSet = new HashSet<AnimationKeyData>();

	private LinkedList<UITexture> _linkedListTexture = new LinkedList<UITexture>();

	private CampaignNodeData _nodeData;

	List<ICommandMessageHandler> IDataProvider.HandlerList => _handlerList;

	List<NodeTreeKeyword> IDataProvider.KeywordList => _keywordList;

	HashSet<AnimationKeyData> IDataProvider.PreloadAnimationSet => _preloadAnimationSet;

	public int TextureWidth { get; set; }

	public int TextureHeight { get; set; }

	public string NodeTreePack { get; set; }

	public string NodeTreeName { get; set; }

	public bool EnableInteraction { get; set; }

	public UITexture Texture => _linkedListTexture.Last?.Value;

	public EntryDataProvider(EntryMenuSettingData menuAppearance, CampaignNodeData nodeData)
	{
		string cinemaWindowName = menuAppearance.CinemaWindowName;
		if (!String_Extension_Method.IsNullOrEmpty(cinemaWindowName))
		{
			CinemaWindowData data = GameRoot.Instance.CinemaWindowDataContainer.GetData(cinemaWindowName);
			if (data != null)
			{
				_handlerList.Add(new BgmHandler());
				NodeTreePack = data.OpenNodeTreePack;
				NodeTreeName = data.OpenNodeTreeName;
				_nodeData = nodeData;
			}
		}
	}

	public RenderFacadeInteractionProvider GetRenderFacadeInteractionDataProvider(string code)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_interactionDataProviderList); i++)
		{
			if (_interactionDataProviderList[i] is RenderFacadeInteractionProvider renderFacadeInteractionProvider && !(renderFacadeInteractionProvider.Code != code))
			{
				return renderFacadeInteractionProvider;
			}
		}
		return null;
	}

	public void SetTexture(UITexture texture)
	{
		if (!(texture == null))
		{
			_linkedListTexture.AddLast(texture);
		}
	}

	public void RemoveTexture(UITexture texture)
	{
		if (!(texture == null))
		{
			_linkedListTexture.Remove(texture);
		}
	}

	public void SetRenderTexture(RenderTexture renderTexture)
	{
		if (renderTexture == null)
		{
			return;
		}
		LinkedList<UITexture>.Enumerator enumerator = _linkedListTexture.GetEnumerator();
		while (enumerator.MoveNext())
		{
			UITexture current = enumerator.Current;
			if (!(current == null))
			{
				current.mainTexture = renderTexture;
			}
		}
	}

	public void Close()
	{
		_handlerList.Clear();
		_keywordList.Clear();
		_interactionDataProviderList.Clear();
		_preloadAnimationSet.Clear();
		_linkedListTexture.Clear();
	}
}
