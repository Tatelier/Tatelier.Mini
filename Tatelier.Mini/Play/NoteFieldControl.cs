using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DxLibDLL.DX;

namespace Tatelier.Mini.Play
{
    /// <summary>
    /// 音符描画下地管理クラス
    /// </summary>
    internal class NoteFieldControl
        : IJudgeFramePoint
    {
        public int X { get; set; } = 92;
        public int Y { get; set; } = 164;

        public int Width { get; set; } = 420;
        public int Height { get; set; } = 56;

        public float NoteCX => 121;
        public float NoteCY => Y + Height / 2;

        float IJudgeFramePoint.CX => NoteCX;

        float IJudgeFramePoint.CY => NoteCY;

        int fieldHandle = -1;

        int handle = -1;

        public void DrawField()
        {
            if(fieldHandle == -1)
            {
                DrawBox(X, Y, X + Width, Y + Height, 0x141414, TRUE);
            }

            DrawRotaGraphFastF(NoteCX, NoteCY, 1.0F, 0.0F, handle, TRUE);
        }

        public NoteFieldControl(string folder)
        {
            int xNum = 15;
            int yNum = 1;

            int divWidth = 48;
            int height = 48;

            int[] handles = new int[xNum * yNum];

            string imageFilePath = Path.Combine(folder, "notes.png");

            LoadDivGraph(imageFilePath, xNum * yNum, xNum, yNum, divWidth, height, handles);

            handle = handles[0];

            for(int i = 1; i < handles.Length; i++)
            {
                DeleteGraph(handles[i]);
            }
        }
    }
}
