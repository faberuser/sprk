using System;

namespace NGame2;

public class GameLayerAttribute : Attribute
{
	public int Depth { get; set; }

	public bool IsUI { get; set; }
}
