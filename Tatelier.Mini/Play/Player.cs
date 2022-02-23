using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tatelier.Mini.Play
{
    internal class Player
         : INormalScoreRendererTarget
        , IHBScrollScoreRendererTarget
    {
        public float StartDrawPointX = 640;
        public float FinishDrawPointX = -120;

        public NoteImageControl NoteImageControl;

        public NoteFieldControl NoteFieldControl;

        public Tatelier.Score.Play.Chart.TJA.Score Score;

        public IScoreRenderer ScoreRenderer;

        public TaikoSEControl TaikoSEControl;

        public InputControlItemPlay Input;

        public UpdateState State = new UpdateState();

        #region INormalScoreRendererTarget
        IJudgeFramePoint INormalScoreRendererTarget.JudgeFramePoint => NoteFieldControl;

        NoteImageControl INormalScoreRendererTarget.NoteImageControl => NoteImageControl;

        MeasureLineImageControl INormalScoreRendererTarget.BarLineImageControl { get; } = new MeasureLineImageControl("");
        #endregion

        #region IHBScrollScoreRendererTarget
        IJudgeFramePoint IHBScrollScoreRendererTarget.JudgeFramePoint => NoteFieldControl;

        NoteImageControl IHBScrollScoreRendererTarget.NoteImageControl => NoteImageControl;

        MeasureLineImageControl IHBScrollScoreRendererTarget.BarLineImageControl { get; } = new MeasureLineImageControl("");

        float IHBScrollScoreRendererTarget.StartDrawPointX => StartDrawPointX;

        float IHBScrollScoreRendererTarget.FinishDrawPointX => FinishDrawPointX;

        double IHBScrollScoreRendererTarget.PlayOptionScrollSpeed => 1.0;
        #endregion
    }
}
