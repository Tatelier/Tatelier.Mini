using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Tatelier.Mini.Engine
{
    public abstract class BaseLoadControl
    {
        protected SHA256 sha256 { get; } = SHA256.Create();

        protected MemoryStream dbMemory = new MemoryStream();
        protected object dbMemoryLockObj = new object();

        public int UpdateFromLoad(LiteDB.ILiteDatabase db, ulong hash01, ulong hash02, Func<int> createHandleMethod)
        {
            var collection = db.GetCollection<LoadContentCommonColumn>();

            var one = collection.FindOne(x => x.Hash01 == hash01 && x.Hash02 == hash02);

            int handle = -1;

            if (one == null)
            {
                handle = createHandleMethod();

                if (handle != -1)
                {
                    collection.Insert(new LoadContentCommonColumn()
                    {
                        Handle = handle,
                        Hash01 = hash01,
                        Hash02 = hash02,
                        UsedCount = 1,
                    });

                    db.Commit();
                }
            }
            else
            {
                handle = one.Handle;
                one.UsedCount++;
                collection.Update(one);
                db.Commit();
            }


            return handle;
        }

        public int UpdateFromDelete(LiteDB.ILiteDatabase db, int handle, Func<int> deleteHandleMethod)
        {
            var collection = db.GetCollection<LoadContentCommonColumn>();

            var one = collection.FindOne(x => x.Handle == handle);

            if (one == null)
            {
                // 無いのにコールされた
                deleteHandleMethod();
                return 0;
            }
            else
            {
                one.UsedCount--;
                if (one.UsedCount == 0)
                {
                    collection.Delete(one.id);
                    db.Commit();
                    deleteHandleMethod();
                }
                else
                {
                    collection.Update(one);
                    db.Commit();
                }

                return one.UsedCount;
            }
        }

        /// <summary>
        /// 読込データ共通カラム
        /// </summary>
        internal class LoadContentCommonColumn
        {
            public int id { get; set; }

            public int Handle { get; set; }

            public ulong Hash01 { get; set; }

            public ulong Hash02 { get; set; }

            public int UsedCount { get; set; }
        }

    }
}
