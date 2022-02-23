//#define ALPHA_WINDOW

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tatelier.Mini.Engine;
using Tatelier.Mini.Scene;
using static DxLibDLL.DX;

namespace Tatelier.Mini
{
    class Program
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static Program Singleton { get; private set; }

        /// <summary>
        /// 画像ロード管理インスタンス
        /// </summary>
        public ImageLoadControl ImageLoadControl { get; private set; }

        bool IsExit = false;

        bool IsReboot = false;

        public int ScreenWidth { get; } = 512;

        public int ScreenHeight { get; } = 384;

        /// <summary>
        /// 現在時刻(ms)
        /// </summary>
        public static int NowMilliSec { get; private set; }

        /// <summary>
        /// 現在時刻(μs)
        /// </summary>
        public static long NowMicroSec { get; private set; }

        /// <summary>
        /// 現在時間をリフレッシュする
        /// </summary>
        void RefreshNowTime()
        {
            NowMicroSec = GetNowHiPerformanceCount();
            NowMilliSec = (int)(NowMicroSec / 1000);
        }

        /// <summary>
        /// 初期のDrawModeをセットする
        /// </summary>
        /// <param name="json">json</param>
        void SetInitDrawMode()
        {
            SetGraphMode(ScreenWidth, ScreenHeight, 32);
        }

        void Run()
        {

            RefreshNowTime();

            SetOutApplicationLogValidFlag(DX_FALSE);
            SetEnableXAudioFlag(false ? DX_TRUE : DX_FALSE);

            SetInitDrawMode();

            ChangeWindowMode(1);
            SetAlwaysRunFlag(1);

            SetWindowStyleMode(7);
            SetWindowSizeChangeEnableFlag(1);

            SetWindowVisibleFlag(DX_FALSE);

            SetDoubleStartValidFlag(DX_TRUE);

            SetWindowUserCloseEnableFlag(DX_TRUE);

            int ret = 0;
            ret = DxLib_Init();
            SetUseGraphBaseDataBackup(DX_FALSE);

            int screen = -1;

            int Screen;
            int SoftImage;

            int Alpha = 255;

            // ウインドウモードで起動
            ChangeWindowMode(TRUE);


#if ALPHA_WINDOW

            // 透過ウインドウ設定
            SetUseBackBufferTransColorFlag(TRUE);

            // 画面取り込み用のソフトウエア用画像を作成
            SoftImage = MakeARGB8ColorSoftImage(ScreenWidth, ScreenHeight);

            // 画像を読み込む際にアルファ値をRGB値に乗算するように設定する
            SetUsePremulAlphaConvertLoad(TRUE);
#endif
            // 描画対象にできるアルファチャンネル付き画面を作成
            Screen = MakeScreen(ScreenWidth, ScreenHeight, TRUE);

            // 描画先を描画対象にできるアルファチャンネル付き画面にする
            SetDrawScreen(Screen);

            do
            {
                try
                {
                    SetWindowVisibleFlag(DX_FALSE);

                    IsExit = false;
                    IsReboot = false;

                    SetDrawScreen(DX_SCREEN_BACK);

                    int prevX, prevY;

                    GetWindowSize(out prevX, out prevY);

                    GetDefaultState(out int desktopWidth, out int desktopHeight, out _, out _, out _, out _, out _, out _, out _, out _);

                    SetMainWindowText("Tatelier.Mini");

                    screen = MakeScreen(ScreenWidth, ScreenHeight);

                    SetWindowVisibleFlag(DX_TRUE);
                    SetDragFileValidFlag(DX_TRUE);

                    ImageLoadControl = new ImageLoadControl();
                    SceneControl.Singleton.Start<Scene.Play>();

                    RefreshNowTime();

                    while (!IsExit)
                    {
                        if (ProcessMessage() != 0)
                        {
                            break;
                        }

                        ClearDrawScreen();

                        SetDrawScreen(screen);
                        ClearDrawScreen();

                        RefreshNowTime();

                        // 更新処理
                        Input.Singleton.Update();
                        SceneControl.Singleton.Update();

                        // 描画処理
                        SceneControl.Singleton.Draw();

#if ALPHA_WINDOW
                        // アルファ値の変更
                        //Alpha += AlphaAdd;
                        //if (Alpha <= 0)
                        //{
                        //    Alpha = 0;
                        //    AlphaAdd = 8;
                        //}
                        //else
                        //if (Alpha >= 255)
                        //{
                        //    Alpha = 255;
                        //    AlphaAdd = -8;
                        //}

                        // 描画ブレンドモードを乗算済みアルファにセット
                        SetDrawBlendMode(DX_BLENDMODE_PMA_ALPHA, Alpha);

                        DrawStringF(0, 0, "aiueo", 0xFFFFFF);

                        GetDrawScreenSoftImage(0, 0, ScreenWidth, ScreenHeight, SoftImage);

                        UpdateLayerdWindowForPremultipliedAlphaSoftImage(SoftImage);
#endif

                        SetDrawScreen(DX_SCREEN_BACK);
                        DrawGraph(0, 0, screen, DX_TRUE);

                        ScreenFlip();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"例外が発生しました。太鼓さん次郎のリソースが置いてあるか確認してください。\n\n{ex}", "FATAL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            } while (IsReboot);
        }

        static void Main(string[] args)
        {
            Singleton = new Program();
            Singleton.Run();
        }
    }
}
