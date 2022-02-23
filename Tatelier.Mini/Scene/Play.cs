using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tatelier.Mini.Play;
using Tatelier.Score.Component.NoteSystem;
using Tatelier.Score.Play;
using Tatelier.Score.Play.Chart.TJA;
using static DxLibDLL.DX;

namespace Tatelier.Mini.Scene
{
	class CommonInfo
	{
		public int GreatRange = 30;
		public int GoodRange = 50;
		public int BadRange = 65;
	}

	[DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    internal class Play
        : SceneBase
    {
        public override SceneType SceneType => SceneType.Play;

        Background background;

		CommonInfo info;

		Player[] players = new Player[1];

        TJA tja;

        int bgm = -1;

        ISong song;

        readonly string tjaPath = @"sample\sample\sample.tja";

        int nowMillisec = 0;

        const int StartOffset = -2000;

        readonly string imageFolder = "img";
        readonly string soundFolder = "snd";

        public override void Start()
        {
            background = new Background(imageFolder);
			info = new CommonInfo();

			tja = new TJA();
            tja.Load(new TJALoadInfo()
            {
                FilePath = tjaPath,
                TJALoadPlayerInfoList = new TJALoadPlayerInfo[]
                {
                    new TJALoadPlayerInfo("Oni"),
                },

            });

			for (int i = 0; i < players.Length; i++)
            {
                players[i] = new Player
                {
                    NoteImageControl = new NoteImageControl("img\\notes.png"),
                    NoteFieldControl = new NoteFieldControl(imageFolder),
                    TaikoSEControl = new TaikoSEControl(soundFolder),
                    Input = new InputControlItemPlay(),
                    Score = tja.Scores[i]
                };

                var player = players[i];
                
                if (player.Score.ScoreType == Score.Play.Chart.ScoreType.Normal)
                {
                    player.ScoreRenderer = new NormalScoreRenderer(player);
                }
                else
                {
                    player.ScoreRenderer = new HBScrollScoreRenderer(player);
                }
            }

			string[] inputList = new string[players.Length];
			for (int i = 0; i < players.Length; i++)
			{
				string name = $"Player{i + 1:000}";
				inputList[i] = name;
				InputControl.Singleton.Regist(name, players[i].Input);
			}
			InputControl.Singleton.ChangeInput(inputList);

			tja.BuildScoreRendererData(420, 640, -120, 1.0F);

            var tjaDir = Path.GetDirectoryName(tjaPath);
            string waveFullPath = Path.Combine(tjaDir, tja.WaveFileName);

            bgm = LoadSoundMem(waveFullPath);

            if (bgm != -1)
            {
                Song s = new Song();
                s.Init(bgm);
                song = s;
            }
            else
            {
                int total = 2000 + (tja.Scores.Select(v => v.Notes.Max(w => w.FinishMillisec)).OrderByDescending(v => v)?.FirstOrDefault() ?? 0) + 5000;
                song = new EmptySong(total);
            }

            song.CurrentTime = (tja.Scores[0].OffsetMillisec);

            song.CurrentTime = StartOffset;

            Regist(1.0F);

            song.Play();
        }

		void UpdateAuto()
		{
			nowMillisec = song.CurrentTime + tja.Scores[0].OffsetMillisec;

			int diffTime = 0;

			foreach (var player in players)
			{
				foreach (var bscore in player.Score.BranchScoreControl.GetBranchScoreList())
				{
					foreach (var noteList1 in bscore.NoteList)
					{
						foreach (var noteList2 in noteList1)
						{
							foreach (var item in noteList2)
							{
								// すでにヒットしている場合は次に進む
								if (item.Hit) continue;

								diffTime = item.StartMillisec - nowMillisec;

								if (item.NoteType != NoteType.End && diffTime > 100) break;

								if (diffTime <= 0)
								{
									switch (item.NoteType)
									{
										case NoteType.Don:
										case NoteType.DonBig:
											player.TaikoSEControl.Play(TaikoSEType.Don);
											//player.State.Don = true;
											item.Hit = true;
											item.Visible = false;
											//player.JudgeType = JudgeType.Great;
											//player.NoteFlyEffect.Fly(item.NoteType, nowMillisec);
											//player.ResultData.AddCount(JudgeType.Great);
											//judgeType = JudgeType.Great;
											//player.SoulGauge.Add(JudgeType.Great);
											//player.HitImageControl.Set(nowMillisec, item.NoteType, JudgeType.Great);
											//player.BranchCondition.AddCount(judgeType);
											break;
										case NoteType.Kat:
										case NoteType.KatBig:
											player.TaikoSEControl.Play(TaikoSEType.Kat);
											//player.State.Kat = true;
											item.Hit = true;
											item.Visible = false;
											//player.JudgeType = JudgeType.Great;
											//player.NoteFlyEffect.Fly(item.NoteType, nowMillisec);
											//player.ResultData.AddCount(JudgeType.Great);
											//judgeType = JudgeType.Great;
											//player.SoulGauge.Add(JudgeType.Great);
											//player.HitImageControl.Set(nowMillisec, item.NoteType, JudgeType.Great);
											//player.BranchCondition.AddCount(judgeType);
											break;
										case NoteType.Roll:
										//player.RollNumberImageControl.NowRoll = true;

										//if (AutoRoll.Current)
										//{
										//    player.NoteFlyEffect.Fly(NoteType.Don, nowMillisec);
										//    player.ResultData.AddRollCount();
										//    player.BranchCondition.AddRollCount();
										//    player.RollNumberImageControl.Number++;
										//    player.TaikoSEControl.Play(TaikoSEType.Don);
										//}
										//break;
										case NoteType.RollBig:
											//player.RollNumberImageControl.NowRoll = true;

											//if (AutoRoll.Current)
											//{
											//    player.NoteFlyEffect.Fly(NoteType.DonBig, nowMillisec);
											//    player.ResultData.AddRollCount();
											//    player.BranchCondition.AddRollCount();
											//    player.RollNumberImageControl.Number++;
											//    player.TaikoSEControl.Play(TaikoSEType.Don);
											//}
											break;
										case NoteType.Balloon:
											{
												//player.BalloonNumberImageControl.NowBalloon = true;
												//var balloon = player.BalloonControl.GetBalloon(item);

												//player.BalloonNumberImageControl.Count = balloon.Count;

												//if (AutoRoll.Current)
												//{
												//    if (balloon.AddCount())
												//    {
												//        item.Hit = true;
												//        item.Visible = false;
												//        player.NoteFlyEffect.Fly(NoteType.DonBig, nowMillisec);
												//        player.TaikoSEControl.Play(TaikoSEType.Balloon);
												//        player.BalloonNumberImageControl.NowBalloon = false;
												//    }
												//    player.TaikoSEControl.Play(TaikoSEType.Don);
												//    player.BalloonNumberImageControl.Number++;
												//}
											}
											break;
										case NoteType.End:
											item.PrevNote.Hit = true;
											item.Hit = true;
											//player.RollNumberImageControl.NowRoll = false;
											//if (item.PrevNote.NoteType == NoteType.Balloon)
											//{
											//    item.PrevNote.Visible = false;
											//    player.BalloonNumberImageControl.NowBalloon = false;
											//    //player.NoteFlyEffect.Fly(NoteType.DonBig, nowTime);
											//}
											break;
									}

									//player.JudgeImageControl.Update(judgeType, nowMillisec);
								}
							}
						}
					}
				}
			}
		}

        void UpdateYourself(int playerIndex)
        {
            var player = players[playerIndex];


            player.State.Reset();

            // 入力時に音声を流す
            if (player.Input.GetKeyDown(InputControlItemPlay.LDon))
            {
                player.State.LDon = true;
            }
            if (player.Input.GetKeyDown(InputControlItemPlay.RDon))
            {
                player.State.RDon = true;
            }
            if (player.Input.GetKeyDown(InputControlItemPlay.LKat))
            {
                player.State.LKat = true;
            }
            if (player.Input.GetKeyDown(InputControlItemPlay.RKat))
            {
                player.State.RKat = true;
            }
            if (player.Input.GetKeyDown(InputControlItemPlay.Don))
            {
                player.TaikoSEControl.Play(TaikoSEType.Don);
            }
            if (player.Input.GetKeyDown(InputControlItemPlay.Kat))
            {
                player.TaikoSEControl.Play(TaikoSEType.Kat);
            }

            nowMillisec = song.CurrentTime + tja.Scores[0].OffsetMillisec;

			INote nearNote = null;
			JudgeType judgeType = JudgeType.None;
			int diffTime = 0;
			// 分岐譜面層
			foreach (var bscore in player.Score.BranchScoreControl.GetBranchScoreList())
			{
				// レイヤー層
				foreach (var layer in bscore.NoteList)
				{
					// セクション層
					foreach (var section in layer)
					{
						// 音符層
						foreach (var item in section)
						{
							// すでにヒットしている場合は次に進む
							if (item.Hit) continue;

							diffTime = item.StartMillisec - nowMillisec;

							// 音符の時間が現在時間より250ミリ秒後の場合は、以降の音符の処理をしない
							if (item.NoteType != NoteType.End && diffTime > 250) break;

							// すでに判定で使う音符が特殊音符以外で確定していたら以下の処理はしない
							if (nearNote != null)
							{
								bool isContinue = false;

								switch (nearNote.NoteType)
								{
									case NoteType.Don:
									case NoteType.DonBig:
									case NoteType.Kat:
									case NoteType.KatBig:
										isContinue = true;
										break;
								}
								if (isContinue) continue;
							}

							// 過ぎ去った不可はさよなら
							if (diffTime <= -info.BadRange)
							{
								switch (item.NoteType)
								{
									case NoteType.Don:
									case NoteType.DonBig:
									case NoteType.Kat:
									case NoteType.KatBig:
										item.Hit = true;
										player.TaikoSEControl.Play(TaikoSEType.Miss);
										break;
								}
							}
							// 過ぎ去った可
							else if (diffTime <= -info.GoodRange)
							{
								if (player.State.Don)
								{
									switch (item.NoteType)
									{
										case NoteType.Don:
										case NoteType.DonBig:
											nearNote = item;
											judgeType = JudgeType.Good;
											break;
									}
								}
								if (player.State.Kat)
								{
									switch (item.NoteType)
									{
										case NoteType.Kat:
										case NoteType.KatBig:
											nearNote = item;
											judgeType = JudgeType.Good;
											break;
									}
								}
							}
							// 良
							else if (-info.GreatRange <= diffTime && diffTime <= info.GreatRange)
							{
								if (player.State.Don)
								{
									switch (item.NoteType)
									{
										case NoteType.Don:
										case NoteType.DonBig:
											nearNote = item;
											judgeType = JudgeType.Great;
											break;
									}
								}

								if (player.State.Kat)
								{
									switch (item.NoteType)
									{
										case NoteType.Kat:
										case NoteType.KatBig:
											nearNote = item;
											judgeType = JudgeType.Great;
											break;
									}
								}
							}
							// 可
							else if (diffTime <= info.GoodRange)
							{
								if (player.State.Don)
								{
									switch (item.NoteType)
									{
										case NoteType.Don:
										case NoteType.DonBig:
											nearNote = item;
											judgeType = JudgeType.Good;
											break;
									}
								}
								if (player.State.Kat)
								{
									switch (item.NoteType)
									{
										case NoteType.Kat:
										case NoteType.KatBig:
											nearNote = item;
											judgeType = JudgeType.Good;
											break;
									}
								}
							}
							// 不可
							else if (diffTime <= info.BadRange)
							{
								if (player.State.Don)
								{
									switch (item.NoteType)
									{
										case NoteType.Don:
										case NoteType.DonBig:
											nearNote = item;
											judgeType = JudgeType.Bad;
											break;
									}
								}
								if (player.State.Kat)
								{
									switch (item.NoteType)
									{
										case NoteType.Kat:
										case NoteType.KatBig:
											nearNote = item;
											judgeType = JudgeType.Bad;
											break;
									}
								}
							}

							// 特殊音符を処理する
							if (diffTime < 0)
							{
								switch (item.NoteType)
								{
									case NoteType.Roll:
									case NoteType.RollBig:
									case NoteType.Balloon:
									case NoteType.Dull:
										nearNote = item;
										judgeType = JudgeType.None;
										break;
									case NoteType.End:
										if (!item.PrevNote.Hit)
										{

										}
										item.PrevNote.Hit = true;
										item.Hit = true;

										break;
								}
							}
						}

					}
				}
			}


			// 叩かれた音符が確定した段階で判定処理
			if (nearNote != null)
			{
				if (!nearNote.Hit)
				{
					switch (judgeType)
					{
						case JudgeType.Great: // 良
						case JudgeType.Good: // 可
							{
								switch (nearNote.NoteType)
								{
									case NoteType.Don:
									case NoteType.Kat:
									case NoteType.DonBig:
									case NoteType.KatBig:
										nearNote.Hit = true;
										nearNote.Visible = false;
										break;
								}
							}
							break;
						case JudgeType.Bad: // 不可
							{
								switch (nearNote.NoteType)
								{
									case NoteType.Don:
									case NoteType.Kat:
									case NoteType.DonBig:
									case NoteType.KatBig:
										nearNote.Hit = true;
										nearNote.Visible = false;
										player.TaikoSEControl.Play(TaikoSEType.Miss);
										break;
								}
							}
							break;
						default:
							switch (nearNote.NoteType)
							{
								case NoteType.Roll:
									break;
								case NoteType.RollBig:
									break;
								case NoteType.Balloon:
									if (player.State.Don)
									{
									}
									break;
								case NoteType.End:
									break;
								case NoteType.Dull:
									nearNote.Hit = true;
									break;
							}
							break;
					}
				}

			}

		}

		public override void Update()
        {
			UpdateYourself(0);

            song.Update();
        }

        public override void Draw()
        {
            background.Draw();
            foreach (var player in players)
            {
                player.NoteFieldControl.DrawField();
                foreach(var bscore in player.Score.BranchScoreControl.GetBranchScoreList())
                {
                    player.ScoreRenderer.DrawNoteBranchScore(bscore, nowMillisec);
                }
            }

            DrawGraph(0, 0, players[0].NoteImageControl.GetImageHandle(Score.Component.NoteSystem.NoteType.Roll), TRUE);
        }

        private string GetDebuggerDisplay()
        {
            return ToString();
        }

        public override void Finish()
        {

        }
    }
}
