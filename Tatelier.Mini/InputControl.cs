using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tatelier.Mini
{
	interface IInputControlItem
	{
		bool Enabled { get; set; }
		bool GetKey(int key);
		bool GetKeyDown(int key);
		bool GetKeyUp(int key);
		int GetCount(int key);
	}

	class InputControlItem : IInputControlItem
	{
		bool enabled;
		public bool Enabled
		{
			get
			{
				return enabled && InputControl.Singleton.Enabled;
			}
			set
			{
				enabled = value;
			}
		}
		public bool GetKey(int key) => Enabled ? Input.Singleton.GetKey(key) : false;
		public bool GetKeyUp(int key) => Enabled ? Input.Singleton.GetKeyUp(key) : false;
		public bool GetKeyDown(int key) => Enabled ? Input.Singleton.GetKeyDown(key) : false;

		public int GetCount(int key) => Enabled ? Input.Singleton.GetCount(key) : 0;
	}
	class InputControl
	{
		public static InputControl Singleton { get; set; }

		Dictionary<string, IInputControlItem> InputMap = new Dictionary<string, IInputControlItem>();

		string[] prevInputNames = new string[0];
		string[] nowInputNames = new string[0];

		public bool Enabled => !NowCommandInput;

		public bool NowCommandInput { get; set; } = false;

		public bool NowMyMessageboxInput { get; set; } = false;

		static InputControl()
		{
			Singleton = new InputControl();
		}

		public void Remove(string name)
		{
			if (InputMap.ContainsKey(name))
			{
				InputMap.Remove(name);
			}
		}

		public T Regist<T>(string name)
			where T : IInputControlItem, new()
		{
			var item = new T
			{
				Enabled = false
			};
			InputMap[name] = item;

			return item;
		}

		public void Regist(string name, IInputControlItem item)
		{
			item.Enabled = false;
			InputMap[name] = item;
		}

		public void ChangeInput(params string[] names)
		{
			if (nowInputNames.Length > 0)
			{
				prevInputNames = nowInputNames;
				foreach (var item in nowInputNames)
				{
					InputMap[item].Enabled = false;
				}
			}
			nowInputNames = names.ToArray();
			if (nowInputNames.Length == 0)
			{
			}
			foreach (var item in nowInputNames)
			{
				InputMap[item].Enabled = true;
			}
		}

		public void ChangePrevInput()
		{
			ChangeInput(prevInputNames);
		}
	}
}
