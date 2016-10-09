namespace GBlue
{
	public class TextParser
	{
		private string text;
		private int start;
		private int current;
		private int length;
		
		public TextParser()
		{
		}
		
		public TextParser(string text)
		{
			this.setText(text);
		}
		
		public void setText(string text)
		{
			this.setText(text, 0, -1);
		}
		
		public void setText(string text, int start)
		{
			this.setText(text, start, -1);
		}
		
		public void setText(string text, int start, int length)
		{
			this.text = text;
			this.start = this.current = start;
			if (length > 0)
				this.length = start + length;
			else
				this.length = text.Length;
		}
		
		public int leftLength
		{
			get { return this.length - (this.current - this.start); }
		}
		
		public bool isEndOfParsing
		{
			get { return this.current >= this.length; }
		}
		
		public bool isSpace
		{
			get
			{ 
				if (this.isEndOfParsing)
					return false;
				return this.text[this.current] == ' ' || this.text[this.current] == '\t';
			}
		}
		
		public bool isNewLine
		{
			get
			{ 
				if (this.isEndOfParsing)
					return false;
				return this.text[this.current] == '\n' || this.isCRnLF;
			}
		}
		
		public bool isCRnLF
		{
			get
			{
				var curr = this.current;
				if (curr + 1 >= this.length)
					return false;
				return this.text[curr] == '\r' && this.text[curr + 1] == '\n'; 
			}
		}
		
		public bool enableCommentParsing
		{
			get { return false; }
			set {  }
		}
		
		/// <summary>
		/// if value is -1 that means end of position
		/// </summary>
		public int pos
		{
			get { return this.current; }
			set { this.current = value < 0 ? this.length : value; }
		}
		
		public char ch
		{
			get
			{
				if (this.isEndOfParsing)
					return '\0';
				return this.text[this.current];
			}
		}
		
		public string line
		{
			get { return this.peekLine(); }
		}
		
		public void skipSpaceNewLineAndComment()
		{
			while (!this.isEndOfParsing)
			{
				if (this.isNewLine)
				{
					if (this.isCRnLF)
						this.current+=2;
					else
						this.current++;
					continue;
				}
				if (this.isSpace)
				{
					this.current++;
					continue;
				}
				break;
			}
		}
		
		public void skipSpaceAndComment()
		{
			while (!this.isEndOfParsing)
			{
				if (!this.isSpace)
					break;
				this.current++;
			}
		}
		
		public void skipSpaceAndNewLine()
		{
			while (!this.isEndOfParsing)
			{
				if (this.isNewLine)
				{
					if (this.isCRnLF)
						this.current+=2;
					else
						this.current++;
					continue;
				}
				if (this.isSpace)
				{
					this.current++;
					continue;
				}
				break;
			}
		}
		
		public void skipSpace()
		{
			while (!this.isEndOfParsing)
			{
				if (!this.isSpace)
					break;
				this.current++;
			}
		}
		
		public void skipNewLine()
		{
			while (!this.isEndOfParsing)
			{
				if (!this.isNewLine)
					break;
				this.current++;
			}
		}
		
		public void skipString(string text)
		{
			var length = text.Length;
			for (var i=0; i<length && !this.isEndOfParsing; ++i)
			{
				if (this.ch != text[i])
					break;
				this.current++;
			}
		}
		
		public int find(string text)
		{
			return this.text.IndexOf(text, this.current) - this.start;
		}
		
		public string block(char startCh, char endCh)
		{
			var first = 0;
			var last = 0;
			var i = this.current;
			for (; i<this.length; ++i)
			{
				var ch = this.text[i];
				if (ch == startCh)
				{
					first = ++i;
					break;
				}
			}
			for (; i<this.length; ++i)
			{
				var ch = this.text[i];
				if (ch == endCh)
				{
					last = i;
					break;
				}
			}
			this.current = last + 1;
			
			var len = last - first;
			if (len > 0)
				return this.text.Substring(first, len);
			return null;
		}
		
		public string block(string startStr, string endStr)
		{
			var first = this.text.IndexOf(startStr, this.current);
			if (first < 0)
				return null;
			first += startStr.Length;
			
			var last = this.text.IndexOf(endStr, first);
			if (last < 0)
				return this.text.Substring(first);
			
			this.current = last + endStr.Length;
			
			var len = last - first;
			if (len > 0)
				return this.text.Substring(first, len);
			return null;
		}
		
		public string peekText(int length)
		{
			var startIndex = this.current;
			return this.text.Substring(startIndex, length);
		}
		
		public string peekLine()
		{
			var oldPos = this.pos;
			var dst = this.seekLine();
			this.pos = oldPos;
			return dst;
		}
		
		public string seekText(int length)
		{
			var startIndex = this.current;
			
			if (this.current + length > this.length)
				length = this.length - this.current;
			this.current += length;
			
			return this.text.Substring(startIndex, length);
		}
		
		public string seekLine()
		{
			var dst = "";
			while (true)
			{
	//			if (this.isNewLineComment())
	//			{
	//				tpa.SkipLineComment();
	//				break;
	//			}
	//			if (tpa.IsStartComment())
	//			{
	//				tpa.SkipBlockComment();
	//				continue;
	//			}
				
				if (this.isNewLine || this.isEndOfParsing)
				{
					this.skipNewLine();
					break;
				}
				
				if (this.ch == '"')
				{
					dst += '"';
					dst += this.block('"', '"');
					dst += '"';
				}
				else
				{
					dst += this.ch;
					this.pos++;
				}
			}
			return dst;
		}
	};
}