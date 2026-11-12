using System;

namespace NShared;

public class CommentAttribute : Attribute
{
	public string Comment { get; set; }

	public CommentAttribute(string comment = "")
	{
		Comment = comment;
	}
}
