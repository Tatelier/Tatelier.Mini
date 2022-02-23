using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Tatelier.Score.Component.NoteSystem;
using static DxLibDLL.DX;

namespace Tatelier.Mini.Play
{
	/// <summary>
	/// 小節線画像管理クラス
	/// </summary>
    internal class MeasureLineImageControl
    {

		int handle = -1;

		public int CreateBox(int width, int height, uint color)
		{
			int handle = -1;

			using (Bitmap bmp = new Bitmap(width, height))
			using (Graphics gps = Graphics.FromImage(bmp))
			{
				gps.TextRenderingHint = TextRenderingHint.AntiAlias;
				gps.CompositingQuality = CompositingQuality.HighQuality;
				gps.CompositingMode = CompositingMode.SourceOver;
				//(アンチエイリアス処理されたレタリング)を指定する
				gps.SmoothingMode = SmoothingMode.AntiAlias;

				Brush fillBrush = new SolidBrush(Color.FromArgb((int)(color | 0xFF000000)));

				gps.FillRectangle(fillBrush, new Rectangle()
				{
					X=0,
					Y=0,
					Width = width,
					Height = height
				});

				// 動的に作成したBitmapからグラフィックを作成
				using (var ms = new MemoryStream())
				{
					bmp.Save(ms, ImageFormat.Bmp);
					byte[] buff = ms.ToArray();

					IntPtr parray = Marshal.AllocCoTaskMem(buff.Length);
					Marshal.Copy(buff, 0, parray, buff.Length);
					SetDrawValidGraphCreateFlag(DX_TRUE);
					SetDrawValidAlphaChannelGraphCreateFlag(DX_TRUE);

					handle = CreateGraphFromMem(parray, buff.Length);

					SetDrawValidGraphCreateFlag(DX_FALSE);
					SetDrawValidAlphaChannelGraphCreateFlag(DX_FALSE);

					Marshal.FreeCoTaskMem(parray);
				}

				return handle;
			}
		}


		public int GetHandle(MeasureLineType measureType)
		{
			switch (measureType)
			{
				case MeasureLineType.Normal:
				default:
					return handle;
				case MeasureLineType.BranchStart:
					return handle;
			}
		}

		public MeasureLineImageControl(string folderPath)
        {
			handle = CreateBox(1, 42, 0xFFFFFF);
        }
    }
}
