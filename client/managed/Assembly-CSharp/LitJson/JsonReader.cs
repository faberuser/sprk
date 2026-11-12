using System;
using System.Collections.Generic;
using System.IO;

namespace LitJson;

public class JsonReader
{
	private static IDictionary<int, IDictionary<int, int[]>> parse_table;

	private Stack<int> automaton_stack;

	private int current_input;

	private int current_symbol;

	private bool end_of_json;

	private bool end_of_input;

	private LitJson.Lexer lexer;

	private bool parser_in_string;

	private bool parser_return;

	private bool read_started;

	private TextReader reader;

	private bool reader_is_owned;

	private bool skip_non_members;

	private object token_value;

	private JsonToken token;

	public bool AllowComments
	{
		get
		{
			return lexer.AllowComments;
		}
		set
		{
			lexer.AllowComments = value;
		}
	}

	public bool AllowSingleQuotedStrings
	{
		get
		{
			return lexer.AllowSingleQuotedStrings;
		}
		set
		{
			lexer.AllowSingleQuotedStrings = value;
		}
	}

	public bool SkipNonMembers
	{
		get
		{
			return skip_non_members;
		}
		set
		{
			skip_non_members = value;
		}
	}

	public bool EndOfInput => end_of_input;

	public bool EndOfJson => end_of_json;

	public JsonToken Token => token;

	public object Value => token_value;

	static JsonReader()
	{
		PopulateParseTable();
	}

	public JsonReader(string json_text)
		: this(new StringReader(json_text), owned: true)
	{
	}

	public JsonReader(TextReader reader)
		: this(reader, owned: false)
	{
	}

	private JsonReader(TextReader reader, bool owned)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		parser_in_string = false;
		parser_return = false;
		read_started = false;
		automaton_stack = new Stack<int>();
		automaton_stack.Push(65553);
		automaton_stack.Push(65543);
		lexer = new LitJson.Lexer(reader);
		end_of_input = false;
		end_of_json = false;
		skip_non_members = true;
		this.reader = reader;
		reader_is_owned = owned;
	}

	private static void PopulateParseTable()
	{
		parse_table = new Dictionary<int, IDictionary<int, int[]>>();
		TableAddRow(LitJson.ParserToken.Array);
		TableAddCol(LitJson.ParserToken.Array, 91, 91, 65549);
		TableAddRow(LitJson.ParserToken.ArrayPrime);
		TableAddCol(LitJson.ParserToken.ArrayPrime, 34, 65550, 65551, 93);
		TableAddCol(LitJson.ParserToken.ArrayPrime, 91, 65550, 65551, 93);
		TableAddCol(LitJson.ParserToken.ArrayPrime, 93, 93);
		TableAddCol(LitJson.ParserToken.ArrayPrime, 123, 65550, 65551, 93);
		TableAddCol(LitJson.ParserToken.ArrayPrime, 65537, 65550, 65551, 93);
		TableAddCol(LitJson.ParserToken.ArrayPrime, 65538, 65550, 65551, 93);
		TableAddCol(LitJson.ParserToken.ArrayPrime, 65539, 65550, 65551, 93);
		TableAddCol(LitJson.ParserToken.ArrayPrime, 65540, 65550, 65551, 93);
		TableAddRow(LitJson.ParserToken.Object);
		TableAddCol(LitJson.ParserToken.Object, 123, 123, 65545);
		TableAddRow(LitJson.ParserToken.ObjectPrime);
		TableAddCol(LitJson.ParserToken.ObjectPrime, 34, 65546, 65547, 125);
		TableAddCol(LitJson.ParserToken.ObjectPrime, 125, 125);
		TableAddRow(LitJson.ParserToken.Pair);
		TableAddCol(LitJson.ParserToken.Pair, 34, 65552, 58, 65550);
		TableAddRow(LitJson.ParserToken.PairRest);
		TableAddCol(LitJson.ParserToken.PairRest, 44, 44, 65546, 65547);
		TableAddCol(LitJson.ParserToken.PairRest, 125, 65554);
		TableAddRow(LitJson.ParserToken.String);
		TableAddCol(LitJson.ParserToken.String, 34, 34, 65541, 34);
		TableAddRow(LitJson.ParserToken.Text);
		TableAddCol(LitJson.ParserToken.Text, 91, 65548);
		TableAddCol(LitJson.ParserToken.Text, 123, 65544);
		TableAddRow(LitJson.ParserToken.Value);
		TableAddCol(LitJson.ParserToken.Value, 34, 65552);
		TableAddCol(LitJson.ParserToken.Value, 91, 65548);
		TableAddCol(LitJson.ParserToken.Value, 123, 65544);
		TableAddCol(LitJson.ParserToken.Value, 65537, 65537);
		TableAddCol(LitJson.ParserToken.Value, 65538, 65538);
		TableAddCol(LitJson.ParserToken.Value, 65539, 65539);
		TableAddCol(LitJson.ParserToken.Value, 65540, 65540);
		TableAddRow(LitJson.ParserToken.ValueRest);
		TableAddCol(LitJson.ParserToken.ValueRest, 44, 44, 65550, 65551);
		TableAddCol(LitJson.ParserToken.ValueRest, 93, 65554);
	}

	private static void TableAddCol(LitJson.ParserToken row, int col, params int[] symbols)
	{
		parse_table[(int)row].Add(col, symbols);
	}

	private static void TableAddRow(LitJson.ParserToken rule)
	{
		parse_table.Add((int)rule, new Dictionary<int, int[]>());
	}

	private void ProcessNumber(string number)
	{
		long result2;
		if ((number.IndexOf('.') != -1 || number.IndexOf('e') != -1 || number.IndexOf('E') != -1) && double.TryParse(number, out var result))
		{
			token = JsonToken.Double;
			token_value = result;
		}
		else if (long.TryParse(number, out result2))
		{
			token = JsonToken.Long;
			token_value = result2;
		}
		else
		{
			token = JsonToken.Long;
			token_value = 0;
		}
	}

	private void ProcessSymbol()
	{
		if (current_symbol == 91)
		{
			token = JsonToken.ArrayStart;
			parser_return = true;
		}
		else if (current_symbol == 93)
		{
			token = JsonToken.ArrayEnd;
			parser_return = true;
		}
		else if (current_symbol == 123)
		{
			token = JsonToken.ObjectStart;
			parser_return = true;
		}
		else if (current_symbol == 125)
		{
			token = JsonToken.ObjectEnd;
			parser_return = true;
		}
		else if (current_symbol == 34)
		{
			if (parser_in_string)
			{
				parser_in_string = false;
				parser_return = true;
				return;
			}
			if (token == JsonToken.None)
			{
				token = JsonToken.String;
			}
			parser_in_string = true;
		}
		else if (current_symbol == 65541)
		{
			token_value = lexer.StringValue;
		}
		else if (current_symbol == 65539)
		{
			token = JsonToken.Boolean;
			token_value = false;
			parser_return = true;
		}
		else if (current_symbol == 65540)
		{
			token = JsonToken.Null;
			parser_return = true;
		}
		else if (current_symbol == 65537)
		{
			ProcessNumber(lexer.StringValue);
			parser_return = true;
		}
		else if (current_symbol == 65546)
		{
			token = JsonToken.PropertyName;
		}
		else if (current_symbol == 65538)
		{
			token = JsonToken.Boolean;
			token_value = true;
			parser_return = true;
		}
	}

	private ExceptionBool ReadToken()
	{
		if (end_of_input)
		{
			return ExceptionBool.False;
		}
		if (lexer.NextToken() == ExceptionBool.Exception)
		{
			return ExceptionBool.Exception;
		}
		if (lexer.EndOfInput)
		{
			Close();
			return ExceptionBool.False;
		}
		current_input = lexer.Token;
		return ExceptionBool.True;
	}

	public void Close()
	{
		if (!end_of_input)
		{
			end_of_input = true;
			end_of_json = true;
			if (reader_is_owned)
			{
				reader.Close();
			}
			reader = null;
		}
	}

	public ExceptionBool Read()
	{
		if (end_of_input)
		{
			return ExceptionBool.False;
		}
		if (end_of_json)
		{
			end_of_json = false;
			automaton_stack.Clear();
			automaton_stack.Push(65553);
			automaton_stack.Push(65543);
		}
		parser_in_string = false;
		parser_return = false;
		token = JsonToken.None;
		token_value = null;
		if (!read_started)
		{
			read_started = true;
			ExceptionBool exceptionBool = ReadToken();
			if (exceptionBool != ExceptionBool.True)
			{
				return exceptionBool;
			}
		}
		while (true)
		{
			if (parser_return)
			{
				if (automaton_stack.Peek() == 65553)
				{
					end_of_json = true;
				}
				return ExceptionBool.True;
			}
			current_symbol = automaton_stack.Pop();
			ProcessSymbol();
			if (current_symbol == current_input)
			{
				switch (ReadToken())
				{
				case ExceptionBool.Exception:
					return ExceptionBool.Exception;
				case ExceptionBool.False:
					if (automaton_stack.Peek() != 65553)
					{
						return ExceptionBool.Exception;
					}
					if (parser_return)
					{
						return ExceptionBool.True;
					}
					return ExceptionBool.False;
				}
				continue;
			}
			if (!parse_table.TryGetValue(current_symbol, out var value))
			{
				return ExceptionBool.Exception;
			}
			if (!value.TryGetValue(current_input, out var value2))
			{
				break;
			}
			if (value2[0] != 65554)
			{
				for (int num = value2.Length - 1; num >= 0; num--)
				{
					automaton_stack.Push(value2[num]);
				}
			}
		}
		return ExceptionBool.Exception;
	}
}
