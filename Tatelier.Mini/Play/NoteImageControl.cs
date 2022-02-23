using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tatelier.Score.Component.NoteSystem;
using static DxLibDLL.DX;

namespace Tatelier.Mini.Play
{
	/// <summary>
	/// 音符画像管理クラス
	/// </summary>
    internal class NoteImageControl
		: IDisposable
	{
		bool disposed = true;

		int[] handles;

		/// <summary>
		/// 音符画像ハンドルを取得する
		/// </summary>
		/// <param name="noteType">音符種類</param>
		/// <returns>画像ハンドル</returns>
		public int GetImageHandle(NoteType noteType)
		{
			switch (noteType)
			{
				case NoteType.Don: return handles[1];
				case NoteType.Kat: return handles[2];
				case NoteType.DonBig: return handles[3];
				case NoteType.KatBig: return handles[4];
				case NoteType.Roll: return handles[5];
				case NoteType.RollBig: return handles[8];
				case NoteType.Balloon: return handles[11];
				default: return -1;
			}
		}

		/// <summary>
		/// 特殊音符の開始から終了までをつなぐ音符画像ハンドルを取得する	
		/// </summary>
		/// <param name="noteType">音符種類</param>
		/// <returns>画像ハンドル</returns>
		public int GetContentNoteImageHandle(NoteType noteType)
		{
			switch (noteType)
			{
				case NoteType.Roll: return handles[6];
				case NoteType.RollBig: return handles[9];
				default: return -1;
			}
		}

		/// <summary>
		/// 特殊音符の終了印譜画像ハンドルを取得する
		/// </summary>
		/// <param name="noteType">音符種類</param>
		/// <returns>画像ハンドル</returns>
		public int GetEndNoteImageHandle(NoteType noteType)
		{
			switch (noteType)
			{
				case NoteType.Roll: return handles[7];
				case NoteType.RollBig: return handles[10];
				case NoteType.Balloon: return handles[12];
				default: return -1;
			}
		}

		void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					// managed
				}


				for(int i = 0; i < handles.Length; i++)
                {
					DeleteGraph(handles[i]);
                }


				disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}

		~NoteImageControl()
		{
			Dispose();
		}


		public NoteImageControl(string imageFilePath)
        {
            int xNum = 15;
            int yNum = 1;

            int divWidth = 48;
            int height = 48;

            handles = new int[xNum * yNum];

            LoadDivGraph(imageFilePath, xNum * yNum, xNum, yNum, divWidth, height, handles);
        }
    }
}
