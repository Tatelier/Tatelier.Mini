using System;
using System.IO;
using static DxLibDLL.DX;

namespace Tatelier.Mini.Play
{
    /// <summary>
    /// 太鼓SE管理クラス
    /// </summary>
    class TaikoSEControl
		: IDisposable
	{

		bool disposed = false;

		public int don = -1;

		public int kat = -1;

		public int balloon = -1;

		public int miss = -1;

		public void Play(TaikoSEType seType)
		{
			switch (seType)
			{
				case TaikoSEType.Don:
					PlaySoundMem(don, DX_PLAYTYPE_BACK);
					break;
				case TaikoSEType.Kat:
					PlaySoundMem(kat, DX_PLAYTYPE_BACK);
					break;
				case TaikoSEType.Balloon:
					PlaySoundMem(balloon, DX_PLAYTYPE_BACK);
					break;
				case TaikoSEType.Miss:
					PlaySoundMem(miss, DX_PLAYTYPE_BACK);
					break;
			}
		}

		void Dispose(bool disposing)
		{
			if (disposed)
			{
				if (disposing)
				{
					// unmanaged
					DeleteSoundMem(don);
					DeleteSoundMem(kat);
					DeleteSoundMem(miss);
					DeleteSoundMem(balloon);
				}

				// managed

				disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}

		~TaikoSEControl()
		{
			Dispose();
		}

		public TaikoSEControl(string folder)
		{
			don = LoadSoundMem(Path.Combine(folder, "dong.wav"));
			kat = LoadSoundMem(Path.Combine(folder, "ka.wav"));
			balloon = LoadSoundMem(Path.Combine(folder, "balloon.wav"));
		}
	}
}
