using System.IO;
using System.Text;
using UnityEngine;

namespace Nanolod
{
	internal class UnityDebugWriter : TextWriter
	{
		private StringBuilder buffer = new StringBuilder();
		object _myLockToken = new object();

		public override void Flush()
		{
			lock (_myLockToken)
			{
				FlushInternal();
			}
		}

		private void FlushInternal()
		{
			Debug.Log(buffer.ToString());
			buffer.Clear();
		}

		public override void Write(string value)
		{
			lock (_myLockToken)
			{
				buffer.Append(value);
				if (value != null)
				{
					var len = value.Length;
					if (len > 0)
					{
						var lastChar = value[len - 1];
						if (lastChar == '\n')
						{
							FlushInternal();
						}
					}
				}
			}
		}

		public override void Write(char value)
		{
			lock (_myLockToken)
			{
				buffer.Append(value);
				if (value == '\n')
				{
					FlushInternal();
				}
			}
		}

		public override void Write(char[] value, int index, int count)
		{
			Write(new string(value, index, count));
		}

		public override Encoding Encoding
		{
			get { return Encoding.Default; }
		}
	}
}