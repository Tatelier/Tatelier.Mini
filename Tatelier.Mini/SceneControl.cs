using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tatelier.Mini.Scene;
using static DxLibDLL.DX;

namespace Tatelier.Mini
{
    internal class SceneControl
        : ISceneControl
    {
        /// <summary>
        /// コンストラクタを引数付きで実行する
        /// </summary>
        /// <typeparam name="T">クラスの型</typeparam>
        /// <typeparam name="P1">コンストラクタの仮引数1の型</typeparam>
        /// <param name="p1">仮引数1</param>
        /// <returns>インスタンス</returns>
        static T Construct<T, P1>(P1 p1)
             where T : IScene, new()
        {
            T instance;
            try
            {
                instance = (T)typeof(T).GetConstructor(new Type[] { typeof(P1) }).Invoke(new object[] { p1 });
            }
            catch
            {
                instance = new T();
            }

            return instance;
        }

        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static SceneControl Singleton { get; private set; } = new SceneControl();

        IScene nowScene;

        readonly Queue<IScene> destroyList = new Queue<IScene>();
        readonly Queue<IScene> createList = new Queue<IScene>();
        readonly LinkedList<(IScene, IEnumerator)> startEnumeratorList = new LinkedList<(IScene, IEnumerator)>();
        readonly List<(float, IScene)> updateAddList = new List<(float, IScene)>();
        readonly SortedDictionary<float, IScene> updateList2 = new SortedDictionary<float, IScene>();

        /// <summary>
        /// 指定された型のシーンが現在表示中のシーンかどうか
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool IsNowCurrent<T>()
            where T : IScene
        {
            return nowScene is T;
        }

        /// <summary>
        /// 指定された型のシーンが現在表示中のシーンかどうか
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool ISceneControl.IsCurrentScene<T>() => IsNowCurrent<T>();

        public int Create<T>(out T scene, params object[] args) where T : IScene, new()
        {
            return Create(null, out scene, args);
        }

        public int Create<T>(string name, out T scene, params object[] args) where T : IScene, new()
        {
#if DEBUG
            scene = name != null ? Construct<T, string>(name) : new T();
#else
			scene = new T();
#endif

            int ret = scene.PreStart(args);

            if (ret != 0)
            {
                scene = default;
                return -2;
            }

            createList.Enqueue(scene);
            startEnumeratorList.AddLast((scene, scene.GetStartIterator()));

            return 0;
        }

        /// <summary>
        /// シーンを破棄する
        /// </summary>
        /// <param name="scene">破棄するシーン</param>
        /// <param name="sender">指示を出したシーン</param>
        public void Destroy(IScene scene, IScene sender = null)
        {
            Unregist(scene);
            destroyList.Enqueue(scene);
        }

        /// <summary>
        /// カレントシーンを切り替える
        /// </summary>
        /// <param name="scene">カレントにするシーン</param>
        /// <param name="sender">指示したシーン</param>
        public void Enter(IScene scene, IScene sender)
        {
            nowScene.OnLeave(sender);
            nowScene = scene;
            scene.OnEnter(sender);
        }

        /// <summary>
        /// シーンを登録する
        /// </summary>
        /// <param name="layer">表示レイヤー</param>
        /// <param name="scene">登録するシーン</param>
        public void Regist(float layer, SceneBase scene)
        {
            updateAddList.Add((layer, scene));
        }

        /// <summary>
        /// シーンを解除する
        /// </summary>
        /// <param name="scene"></param>
        public void Unregist(IScene scene)
        {
            updateAddList.Add((-1.0F, scene));
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        public void Start<T>(object[] args = null)
            where T : IScene, new()
        {
            Create<T>(out var s);
            nowScene = s;

            Update();
        }

        void UpdateSystem()
        {
            // 破棄
            while (destroyList.Any())
            {
                var item = destroyList.Dequeue();
                item.Finish();
            }

            // 生成
            while (createList.Any())
            {
                var item = createList.Dequeue();
                item.Start();
            }

            // 初期化列挙&
            if (startEnumeratorList.Any())
            {
                var node = startEnumeratorList.First;

                while (node != null)
                {
                    if (!node.Value.Item2.MoveNext())
                    {
                        var next = node.Next;
                        startEnumeratorList.Remove(node);
                        node = next;
                    }
                    else
                    {
                        node = node.Next;
                    }
                }
            }

            if (updateAddList.Any())
            {
                foreach (var item in updateAddList)
                {
                    var first = updateList2.FirstOrDefault(v => v.Value == item.Item2);
                    if (first.Value != default)
                    {
                        updateList2.Remove(first.Key);
                    }

                    if (item.Item1 > 0)
                    {
                        updateList2[item.Item1] = item.Item2;
                    }
                }
                updateAddList.Clear();
            }
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Update()
        {

            UpdateSystem();

            if (updateList2.Any())
            {
                foreach (var item in updateList2.Values)
                {
                    item.Update();
                }
            }


#if DEBUG
            if (Input.Singleton.GetKeyDown(KEY_INPUT_BACKSLASH))
            {
                throw new NotImplementedException();
            }
#endif
        }

        /// <summary>
        /// 描画処理
        /// </summary>
        public void Draw()
        {
            if (updateList2.Any())
            {
                foreach (var item in updateList2.Values)
                {
                    item.Draw();
                }
            }
        }

        public void Reset()
        {
        }

        int ISceneControl.CreateScene<T>(string name, out T scene, params object[] args)
        {
            int result = Create(name, out scene, args);
            return result;
        }

        public SceneControl()
        {
        }
    }
}