using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DxLibDLL.DX;

namespace Tatelier.Mini.Play
{
	interface ISong
	{
		int CurrentTime { get; set; }

		void Play();

		void Update();

		void Stop();

		void Pause();

		bool IsNowPause { get; }

		bool IsEnd { get; }
	}

	class EmptySong : ISong
	{
		int startTime;

		int currentTime;

		int total;

		bool nowPlaying;

		public int CurrentTime
		{
			get => currentTime;
			set
			{
				currentTime = value;
				startTime = Program.NowMilliSec - currentTime;
			}
		}

		public bool IsEnd => currentTime >= (total);

		public bool IsNowPause { get; private set; }

		public void Pause()
		{
			nowPlaying = false;
			IsNowPause = true;
		}

		public void Play()
		{
			nowPlaying = true;
			IsNowPause = false;
		}

		public void Update()
		{
			if (nowPlaying)
			{
				currentTime = Program.NowMilliSec - startTime;
			}
			else
			{
				startTime = Program.NowMilliSec - currentTime;
			}
		}

		public void Stop()
		{

		}

		public EmptySong(int totalTime)
		{
			total = totalTime;
		}
	}

	/// <summary>
	/// 音楽クラス
	/// </summary>
	public class Song : ISong, IDisposable
	{
		int handle = -1;

		bool isCurrentChange = false;
		bool nowPlaying;

		int currentTime = 0;
		bool disposed = false;

		int startPaddingTime = 0;

		int StartMilliTime;

		public int CurrentTime
		{
			get
			{
				return currentTime;
			}
			set
			{
				currentTime = value;
				StartMilliTime = Program.NowMilliSec - currentTime - startPaddingTime;
				isCurrentChange = true;
				StopSoundMem(handle);
			}
		}

		public bool IsNowPause { get; private set; }

		int cnt;

		public int PlayCount = 0;

		public int PlayMaxCount = 1;

		public long TotalTime;

		public string DebugText
		{
			get
			{
				return $"{currentTime}:{IsEnd}:{GetSoundTotalTime(handle)},Check={CheckSoundMem(handle)}";
			}
		}

		/// <summary>
		/// 音楽が終了してるかどうか
		/// true: 終了済, false: 未終了
		/// </summary>
		public bool IsEnd => currentTime > TotalTime;

		int time;

		/// <summary>
		/// 初期化
		/// </summary>
		/// <param name="handle">音源ハンドル</param>
		/// <returns></returns>
		public int Init(int handle)
		{
			if (this.handle != -1)
			{
				StopSoundMem(this.handle);
				DeleteSoundMem(this.handle);
			}

			this.handle = handle;

			currentTime = 0;
			StartMilliTime = Program.NowMilliSec;
			TotalTime = GetSoundTotalTime(handle);
			return 0;
		}
		public void Play()
		{
			if (!nowPlaying)
			{
				nowPlaying = true;
				IsNowPause = false;
			}
		}

		int nowTime;

		public void Update()
		{
			if (isCurrentChange)
			{
				if (CheckSoundMem(handle) == 1)
				{
					StopSoundMem(handle);
				}
				StartMilliTime = Program.NowMilliSec - currentTime - startPaddingTime;
				SetSoundCurrentTime(Program.NowMilliSec - StartMilliTime, handle);
				isCurrentChange = false;
			}

			if (nowPlaying)
			{
				nowTime = Program.NowMilliSec - StartMilliTime;
				if (CheckSoundMem(handle) == 0)
				{
					if (PlayCount < PlayMaxCount)
					{
						if (nowTime >= 0)
						{
							SetSoundCurrentTime(nowTime, handle);
							PlaySoundMem(handle, DX_PLAYTYPE_BACK, DX_FALSE);
							PlayCount++;
						}
					}
					else
					{

					}
				}

				if (nowTime >= 0
					&& time + 1000 < Program.NowMilliSec)
				{
					if (GetSoundTotalTime(handle) - 500 > GetSoundCurrentTime(handle))
					{
						nowTime = (int)GetSoundCurrentTime(handle);
					}
					StartMilliTime = Program.NowMilliSec - nowTime;
					time = Program.NowMilliSec;
				}

				currentTime = nowTime - startPaddingTime;
			}
			else
			{
				if (CheckSoundMem(handle) == 1)
				{
					StopSoundMem(handle);
					currentTime = (int)GetSoundCurrentTime(handle) - startPaddingTime;
				}
				StartMilliTime = Program.NowMilliSec - currentTime - startPaddingTime;
			}
		}

		public void Pause()
		{
			nowPlaying = false;
			IsNowPause = true;
			PlayCount--;
		}

		public void Stop()
		{
			StopSoundMem(handle);
			nowPlaying = false;
			currentTime = 0;
		}

		public void Dispose()
		{
			Dispose(true);
		}

		void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{

				}
				disposed = true;
			}
		}

		~Song()
		{
			Dispose();
		}

		public Song()
		{

		}
	}
}
