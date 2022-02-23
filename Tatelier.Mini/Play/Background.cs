using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tatelier.Mini.Engine;
using static DxLibDLL.DX;

namespace Tatelier.Mini.Play
{
    /// <summary>
    /// 背景画像クラス
    /// </summary>
    internal class Background
    {
        int handle = -1;

        public void Draw()
        {
            DrawGraph(0, 0, handle, TRUE);
        }

        public Background(string folder)
        {
            handle = Program.Singleton.ImageLoadControl.Load(Path.Combine(folder, "bg.png"));
        }
    }
}
