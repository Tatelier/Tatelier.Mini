using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DxLibDLL.DX;

namespace Tatelier.Mini.Play
{
	/// <summary>
	/// 演奏用入力管理項目クラス
	/// </summary>
	class InputControlItemPlay : IInputControlItem
	{
		public const int LDon = -11;
		public const int RDon = -12;
		public const int LKat = -13;
		public const int RKat = -14;

		public const int Don = -1;
		public const int Kat = -2;

		public const int Decision = -31;
		public const int Cancel = -32;

		public bool Enabled { get; set; }

		Input input;

		int[] ldon;
		int[] rdon;
		int[] lkat;
		int[] rkat;

		public InputControlItemPlay()
		{
			input = Input.Singleton;
			ldon = new int[1] { KEY_INPUT_F };
			rdon = new int[1] { KEY_INPUT_J };
			lkat = new int[1] { KEY_INPUT_D };
			rkat = new int[1] { KEY_INPUT_K };
		}

		public int GetCount(int key)
		{
			switch (key)
			{
				case Don:
					break;
				case Kat:
					break;
				default:
					return input.GetCount(key);
			}

			return 0;
		}

		public bool GetKey(int key)
		{
			if (!Enabled) return false;
			switch (key)
			{
				case LDon:
					return input.GetKey(KEY_INPUT_F);
				case RDon:
					return input.GetKey(KEY_INPUT_J);
				case LKat:
					return input.GetKey(KEY_INPUT_D);
				case RKat:
					return input.GetKey(KEY_INPUT_K);
				case Don:
					return input.GetKey(KEY_INPUT_F) || input.GetKey(KEY_INPUT_J);
				case Kat:
					return input.GetKey(KEY_INPUT_D) || input.GetKey(KEY_INPUT_K);
				default:
					return input.GetKey(key);
			}
		}

		public bool GetKeyDown(int key)
		{
			if (!Enabled) return false;

			switch (key)
			{
				case LDon:
					return input.GetKeyDown(ldon);
				case RDon:
					return input.GetKeyDown(rdon);
				case LKat:
					return input.GetKeyDown(lkat);
				case RKat:
					return input.GetKeyDown(rkat);
				case Don:
					return input.GetKeyDown(ldon) || input.GetKeyDown(rdon);
				case Kat:
					return input.GetKeyDown(lkat) || input.GetKeyDown(rkat);
				case Decision:
					return input.GetKeyDown(ldon) || input.GetKeyDown(rdon) || input.GetKeyDown(KEY_INPUT_SPACE);
				case Cancel:
					return input.GetKeyDown(KEY_INPUT_Q);
				default:
					return input.GetKeyDown(key);
			}
		}

		public bool GetKeyUp(int key)
		{
			switch (key)
			{
				case LDon:
					return input.GetKeyUp(KEY_INPUT_F);
				case RDon:
					return input.GetKeyUp(KEY_INPUT_J);
				case LKat:
					return input.GetKeyUp(KEY_INPUT_D);
				case RKat:
					return input.GetKeyUp(KEY_INPUT_K);
				case Don:
					return input.GetKeyUp(KEY_INPUT_F) || input.GetKeyUp(KEY_INPUT_J);
				case Kat:
					return input.GetKeyUp(KEY_INPUT_D) || input.GetKeyUp(KEY_INPUT_K);
				case Decision:
					return input.GetKeyUp(KEY_INPUT_F) || input.GetKeyUp(KEY_INPUT_J) || input.GetKeyUp(KEY_INPUT_SPACE);
				case Cancel:
					return input.GetKeyUp(KEY_INPUT_Q);
				default:
					return input.GetKeyUp(key);
			}
		}

	}
}
