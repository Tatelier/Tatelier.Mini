using System.Collections.Generic;

namespace Tatelier.Mini.Scene
{
    /// <summary>
    /// シーンインターフェース
    /// </summary>
    public interface IScene
    {
        /// <summary>
        /// レイヤー
        /// </summary>
        float Layer { get; set; }

        /// <summary>
        /// 初期化前初期化
        /// </summary>
        /// <param name="args"></param>
        /// <returns>0:正常, 以外:異常</returns>
        int PreStart(params object[] args);

        /// <summary>
        /// 初期化処理イテレーター
        /// 重い処理を実施するとき用
        /// </summary>
        /// <returns></returns>
        IEnumerator<float> GetStartIterator();

        /// <summary>
        /// 初期化処理
        /// </summary>
        void Start();

        /// <summary>
        /// 更新処理
        /// </summary>
        void Update();

        /// <summary>
        /// 描画処理
        /// </summary>
        void Draw();

        /// <summary>
        /// 終了処理
        /// </summary>
        void Finish();

        /// <summary>
        /// 引数カレントシーンにする
        /// </summary>
        /// <param name="to"></param>
        /// <returns></returns>
        int EnterTo(IScene to);

        /// <summary>
        /// カレントシーンになったときの処理
        /// </summary>
        /// <param name="sender"></param>
        void OnEnter(IScene sender);

        /// <summary>
        /// カレントシーンを抜けたときの処理
        /// </summary>
        /// <param name="sender"></param>
        void OnLeave(IScene sender);
    }
}
