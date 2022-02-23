using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DxLibDLL.DX;

namespace Tatelier.Mini.Engine
{
    /// <summary>
    /// 画像読込管理
    /// </summary>
    public class ImageLoadControl : BaseLoadControl
    {
        MemoryStream dbDebugMemory = null;
        object dbDebugMemoryLockObj = null;

        MemoryStream dbFailMemory = null;
        object dbFailMemoryLockObj = null;

        bool isDebug = false;
        public bool IsDebug
        {
            get => isDebug;
            set
            {
                isDebug = value;
                if (value)
                {
                    if (dbDebugMemory == null)
                    {
                        dbDebugMemory = new MemoryStream();
                        dbDebugMemoryLockObj = new object();
                    }
                    if (dbFailMemory == null)
                    {
                        dbFailMemory = new MemoryStream();
                        dbFailMemoryLockObj = new object();
                    }
                }
            }
        }

        public void OutputLog()
        {
            DateTime now = DateTime.Now;

            string logOutputFolder = "Log";

            string outputFilePath = Path.Combine(Environment.CurrentDirectory, logOutputFolder, $"{now:yyyyMMdd_HHmmss}", $"{now:yyyyMMdd_HHmmss}_ImageLoadControl.csv");

            lock (dbMemoryLockObj)
            {
                if (dbDebugMemory == null)
                {
                    dbDebugMemory = new MemoryStream();
                    dbDebugMemoryLockObj = new object();
                }

                lock (dbDebugMemoryLockObj)
                {
                    using (var db = new LiteDB.LiteDatabase(dbMemory))
                    using (var dbDebug = new LiteDB.LiteDatabase(dbDebugMemory))
                    {
                        var sb = new StringBuilder();
                        try
                        {
                            var collection = db.GetCollection<LoadContentCommonColumn>();
                            var collectionDebug = dbDebug.GetCollection<ImageLoadDebugDBColumn>();

                            var all = collection.FindAll();

                            var dbColumnProperties = typeof(LoadContentCommonColumn).GetProperties();
                            var dbDebugColumnProperties = typeof(ImageLoadDebugDBColumn).GetProperties();

                            // ヘッダー部
                            sb.Append("#");
                            sb.AppendLine(string.Join(",", dbColumnProperties
                                .Concat(dbDebugColumnProperties.Where(v => v.Name != nameof(LoadContentCommonColumn.Handle)))
                                .Where(v => v.Name != "id")
                                .Select(v => v.Name)));

                            foreach (var item in all)
                            {
                                var debugOne = collectionDebug.FindOne(v => v.Handle == item.Handle);

                                var normalContent = dbColumnProperties.Where(v => v.Name != "id")
                                    .Select(v => v.Name.Contains("Hash") ? $"0x{v.GetValue(item):X8}" : v.GetValue(item));


                                IEnumerable<object> debugContent;
                                if (debugOne == null)
                                {
                                    debugContent = new object[dbDebugColumnProperties.Where(v => v.Name != nameof(LoadContentCommonColumn.Handle)).Count()];
                                }
                                else
                                {
                                    debugContent = dbDebugColumnProperties.Where(v => v.Name != "id")
                                        .Where(v => v.Name != nameof(ImageLoadDebugDBColumn.Handle))
                                    .Select(v => v.GetValue(debugOne));
                                }

                                sb.AppendLine(string.Join(",", normalContent.Concat(debugContent)));
                            }
                        }
                        catch (Exception e)
                        {
                            sb.AppendLine($"{e}");
                        }

                        var fi = new FileInfo(outputFilePath);
                        fi.Directory.Create();
                        using (var sw = fi.CreateText())
                        {
                            sw.Write($"{sb}");
                        }
                    }
                }
            }

            if (dbFailMemory != null)
            {
                // 読み込みに失敗した画像一覧
                lock (dbFailMemoryLockObj)
                {
                    //using (var db = new LiteDB.LiteDatabase(dbFailMemory))
                    //{
                    //    var collection = db.GetCollection<ImageLoadFailDBColumn>();

                    //    var all = collection.FindAll();

                    //    foreach (var item in all)
                    //    {

                    //    }
                    //}
                }
            }
        }

        /// <summary>
        /// 初期化
        /// </summary>
        public void Start()
        {
        }

        void LoadDebug(string filePath, int handle)
        {
            if (!IsDebug)
            {
                return;
            }
            if (dbDebugMemory == null)
            {
                dbDebugMemory = new MemoryStream();
            }
            if (handle == -1)
            {
                lock (dbFailMemoryLockObj)
                {
                    using (var db = new LiteDB.LiteDatabase(dbFailMemory))
                    {
                        var collection = db.GetCollection<ImageLoadFailDBColumn>();
                        var one = collection.FindOne(x => x.FilePath == filePath);

                        if (one == null)
                        {
                            collection.Insert(new ImageLoadFailDBColumn()
                            {
                                FilePath = filePath,
                            });

                            db.Commit();
                        }
                    }
                }
            }
            else
            {
                lock (dbDebugMemoryLockObj)
                {
                    using (var db = new LiteDB.LiteDatabase(dbDebugMemory))
                    {
                        var collection = db.GetCollection<ImageLoadDebugDBColumn>();

                        var one = collection.FindOne(x => x.Handle == handle);

                        if (one == null)
                        {
                            collection.Insert(new ImageLoadDebugDBColumn()
                            {
                                Handle = handle,
                                FilePath = filePath,
                                FirstLoadDateTime = DateTime.Now,
                                LastLoadDateTime = DateTime.Now
                            });

                            db.Commit();
                        }
                        else
                        {
                            one.LastLoadDateTime = DateTime.Now;
                            collection.Update(one);
                            db.Commit();
                        }
                    }
                }
            }
        }

        void DeleteDebug(int handle)
        {
            if (!IsDebug)
            {
                return;
            }

            if (handle != -1)
            {
                if (dbDebugMemory == null)
                {
                    dbDebugMemory = new MemoryStream();
                }
                lock (dbDebugMemoryLockObj)
                {
                    using (var db = new LiteDB.LiteDatabase(dbDebugMemory))
                    {
                        var collection = db.GetCollection<ImageLoadDebugDBColumn>();

                        var one = collection.FindOne(x => x.Handle == handle);

                        if (one != null)
                        {
                            collection.Delete(one.id);
                            db.Commit();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 画像を読込
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>ハンドル</returns>
        public int Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                LoadDebug(filePath, -1);
                return -1;
            }
            var bytes = sha256.ComputeHash(File.OpenRead(filePath));

            ulong hash01 = BitConverter.ToUInt64(bytes, 0);
            ulong hash02 = BitConverter.ToUInt64(bytes, 16);

            lock (dbMemoryLockObj)
            {
                using (var db = new LiteDB.LiteDatabase(dbMemory))
                {
                    int handle = UpdateFromLoad(db, hash01, hash02, () => LoadGraph(filePath));

                    if (handle == -1
                        || IsDebug)
                    {
                        LoadDebug(filePath, handle);
                    }

                    return handle;
                }
            }
        }

        /// <summary>
        /// 画像を削除
        /// </summary>
        /// <param name="handle">画像ハンドル</param>
        /// <returns></returns>
        public int Delete(int handle)
        {
            lock (dbMemoryLockObj)
            {
                using (var db = new LiteDB.LiteDatabase(dbMemory))
                {
                    int ret = UpdateFromDelete(db, handle, () => DeleteGraph(handle));

                    DeleteDebug(handle);

                    return ret;
                }
            }
        }

        public void Reset()
        {
            dbMemory?.Dispose();
            dbDebugMemory?.Dispose();

            dbMemory = new MemoryStream();
            dbDebugMemory = null;
        }

        /// <summary>
        /// 画像読込デバッグ用カラム
        /// </summary>
        internal class ImageLoadDebugDBColumn
        {
            public int id { get; set; }

            public int Handle { get; set; }

            public DateTime FirstLoadDateTime { get; set; }

            public DateTime LastLoadDateTime { get; set; }

            public string FilePath { get; set; }
        }

        internal class ImageLoadFailDBColumn
        {
            public int id { get; set; }

            public string FilePath { get; set; }
        }
    }
}
