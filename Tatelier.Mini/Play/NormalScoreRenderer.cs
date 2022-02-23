using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tatelier.DxLibDLL;
using Tatelier.Score.Component.NoteSystem;
using Tatelier.Score.Play.Chart;
using static DxLibDLL.DX;

namespace Tatelier.Mini.Play
{
	public interface IJudgeFramePoint
	{
		float CX { get; }
		float CY { get; }
	}
	/// <summary>
	/// 標準譜面描画の対象データ
	/// </summary>
	interface INormalScoreRendererTarget
	{
		/// <summary>
		/// 判定枠座標情報
		/// </summary>
		IJudgeFramePoint JudgeFramePoint { get; }

		/// <summary>
		/// 音符画像管理
		/// </summary>
		NoteImageControl NoteImageControl { get; }

		/// <summary>
		/// 小節線画像管理
		/// </summary>
		MeasureLineImageControl BarLineImageControl { get; }
	}
	internal interface IScoreRenderer
	{
		/// <summary>
		/// 小節線描画処理
		/// </summary>
		/// <param name="bscore">譜面</param>
		/// <param name="nowMillisec">現在時間(ms)</param>
		void DrawMeasureBranchScore(BranchScore bscore, int nowMillisec);

		/// <summary>
		/// 音符描画クラス
		/// </summary>
		/// <param name="bscore">譜面</param>
		/// <param name="nowMillisec">現在時刻(ms)</param>
		void DrawNoteBranchScore(BranchScore bscore, int nowMillisec);
	}

	/// <summary>
	/// 標準譜面描画処理クラス
	/// </summary>
	class NormalScoreRenderer : IScoreRenderer

	{
		readonly INormalScoreRendererTarget module;


		void DrawNormalNote(INote note, int nowMillisec)
		{
			if (note.StartDrawMillisec <= nowMillisec
				&& nowMillisec < note.FinishDrawMillisec)
			{
				int diffMillisec = note.StartMillisec - nowMillisec;
				int handle = module.NoteImageControl.GetImageHandle(note.NoteType);
				float x = module.JudgeFramePoint.CX + (diffMillisec * note.MovementPerMillisec);
				float y = module.JudgeFramePoint.CY;

				DrawRotaGraphFastF(x, y, 1.0F, 0.0F, handle, DX_TRUE);
			}
		}

		void DrawBalloonNote(INote note, int nowMillisec)
		{
			if (note.StartDrawMillisec <= nowMillisec
				&& nowMillisec < note.FinishDrawMillisec)
			{
				int diffMillisec = note.StartMillisec - nowMillisec;

				int handle = module.NoteImageControl.GetImageHandle(note.NoteType);

				float x;
				float y = module.JudgeFramePoint.CY;

				if (diffMillisec < 0)
				{
					var finishDiffMillisec = (note.FinishMillisec - nowMillisec);
					x = finishDiffMillisec < 0 ? module.JudgeFramePoint.CX + (finishDiffMillisec * note.MovementPerMillisec) : module.JudgeFramePoint.CX;
				}
				else
				{
					x = module.JudgeFramePoint.CX + (diffMillisec * note.MovementPerMillisec);
				}

				DrawRotaGraphFastF(x, y, 1.0F, 0.0F, handle, DX_TRUE);
			}
		}

		void DrawRollNote(INote note, int nowMillisec)
		{
			var prevNote = note.PrevNote;

			// 前回音符によって処理を変える
			switch (prevNote.NoteType)
			{
				case NoteType.Roll:
				case NoteType.RollBig:
					{
						// 連打中身の描画
						if (prevNote.StartDrawMillisec <= nowMillisec
							&& nowMillisec < prevNote.FinishDrawMillisec)
						{
							int diffMillisec = note.StartMillisec - nowMillisec;

							int handle = module.NoteImageControl.GetContentNoteImageHandle(prevNote.NoteType);
							GetGraphSizeF(handle, out float w, out float h);

							float hHalf = h / 2;
							float x = module.JudgeFramePoint.CX + (diffMillisec * note.MovementPerMillisec);
							float y = module.JudgeFramePoint.CY;

							int prevDiffMillisec = (prevNote.StartMillisec - nowMillisec);
							float prevX = module.JudgeFramePoint.CX + (prevDiffMillisec * prevNote.MovementPerMillisec);

							using (DrawModeGuard.Create())
							{
								SetDrawMode(DX_DRAWMODE_NEAREST);
								DrawModiGraphF(prevX - 1, y - hHalf, x + 1, y - hHalf, x + 1, y + hHalf, prevX - 1, y + hHalf, handle, DX_TRUE);
							}

							// 終端音符の描画
							if (note.StartDrawMillisec <= nowMillisec
								&& nowMillisec < note.FinishDrawMillisec)
							{
								DrawRotaGraphFastF(x, y, 1.0F, 0.0F, module.NoteImageControl.GetEndNoteImageHandle(prevNote.NoteType), DX_TRUE, note.ScrollSpeedInfo.Value < 0 ? 1 : 0);
							}
						}
					}
					break;
			}
		}


		void IScoreRenderer.DrawMeasureBranchScore(BranchScore bscore, int nowTime)
		{
			// 小節線処理・描画
			foreach (var item in bscore.Measures.Reverse<IMeasureLine>().Where(v => v.Visible))
			{
				float x = module.JudgeFramePoint.CX + ((item.StartMillisec - nowTime) * item.MovementPerMillisec);
				float y = module.JudgeFramePoint.CY;

				//DrawRotaGraphFastF(x, y, 1.0F, 0.0F, module.BarLineImageControl.GetHandle(item.MeasureLineType), DX_TRUE);
				//DrawMeasureIdForDebug(x, y, item);
			}
		}

		void IScoreRenderer.DrawNoteBranchScore(BranchScore bscore, int nowTime)
		{
			// レイヤー層
			foreach (var layer in bscore.NoteList)
			{
				// セクション層(後ろから)
				foreach (var section in layer.Reverse())
				{
					// 音符層
					foreach (var note in section.Reverse())
					{
						if (note.Visible)
						{
							switch (note.NoteType)
							{
								case NoteType.Don:
								case NoteType.Kat:
								case NoteType.DonBig:
								case NoteType.KatBig:
								case NoteType.Roll:
								case NoteType.RollBig:
									DrawNormalNote(note, nowTime);
									break;
								case NoteType.Balloon:
									DrawBalloonNote(note, nowTime);
									break;
								case NoteType.End:
									DrawRollNote(note, nowTime);
									break;
							}
						}
					}
				}
			}
		}

		public NormalScoreRenderer(INormalScoreRendererTarget module)
		{
			this.module = module;
		}
	}
}
