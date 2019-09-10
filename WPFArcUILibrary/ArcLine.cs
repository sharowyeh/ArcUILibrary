using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace WPFArcUILibrary
{
    /// <summary>
    /// The arc line UI component with drawing animation
    /// </summary>
    public sealed class ArcLine : ArcVisual, IAnimatable
    {
        private Storyboard storyBoard = null;
        private DoubleAnimation lineLengthAnimation = null;

        #region Dependency properties for XAML styling and animation, animation behavior is implemented in property changed callback 

        public static readonly DependencyProperty LineLengthProperty = DependencyProperty.Register(
            "LineLength", typeof(double), typeof(ArcLine),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty AnimatedLineLengthProperty = DependencyProperty.Register(
            "AnimatedLineLength", typeof(double), typeof(ArcLine),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender,
                new PropertyChangedCallback((sender, e) =>
                {
                    /// Use property changed callback for binding value changed
                    if ((double)e.NewValue == (double)e.OldValue)
                        return;
                    var inst = sender as ArcLine;
                    if (inst.LineLength == (double)e.NewValue)
                        return;
                    /// If there has animation setting, fire animation for LineLength property 
                    if (inst.AnimationTime > 0 && inst.storyBoard != null && inst.lineLengthAnimation != null)
                    {
                        inst.lineLengthAnimation.From = (double)e.OldValue;
                        inst.lineLengthAnimation.To = (double)e.NewValue;
                        inst.storyBoard.Begin();
                    }
                    /// Otherwise change LineLength directly
                    else
                    {
                        inst.LineLength = (double)e.NewValue;
                    }
                })));

        public static readonly DependencyProperty AnimationTimeProperty = DependencyProperty.Register(
            "AnimationTime", typeof(double), typeof(ArcLine),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender,
                new PropertyChangedCallback((sender, e) =>
                {
                    if ((double)e.NewValue == (double)e.OldValue)
                        return;
                    var inst = sender as ArcLine;
                    if (inst.lineLengthAnimation != null)
                        inst.lineLengthAnimation.Duration = new Duration(TimeSpan.FromMilliseconds((double)e.NewValue));
                }),
                new CoerceValueCallback((sender, e) =>
                {
                    if ((double)e < 0)
                        return 0;
                    else
                        return e;
                })));

        public static readonly DependencyProperty FillLineRatioProperty = DependencyProperty.Register(
            "FillLineRatio", typeof(double), typeof(ArcLine),
            new FrameworkPropertyMetadata(1.1, FrameworkPropertyMetadataOptions.AffectsRender,
                new PropertyChangedCallback((sender, e) =>
                {
                    /// Use property changed callback for binding value changed
                    if ((double)e.NewValue == (double)e.OldValue)
                        return;
                    var inst = sender as ArcLine;
                    /// If there has animation setting, use AnimatedLineLength for LineLength property 
                    if (inst.AnimationTime > 0 && inst.storyBoard != null && inst.lineLengthAnimation != null)
                        inst.AnimatedLineLength = (double)e.NewValue * inst.MeshVertexCount;
                    /// Otherwise change LineLength directly
                    else
                        inst.LineLength = (double)e.NewValue * inst.MeshVertexCount;
                })));

        #endregion

        /// <summary>
        /// Length of arc line will be drawn, the maximum is MeshVertexCount(inherited from ArcVisual) <para></para>
        /// Notice: If caller use AnimatedLineLength, this value will present length controlled by animation during animation time
        /// </summary>
        public double LineLength
        {
            get { return (double)GetValue(LineLengthProperty); }
            set { SetValue(LineLengthProperty, value); }
        }

        /// <summary>
        /// Length of arc line will be effected with liner animation by animation time<para></para>
        /// Notice: It changes LineLength property to approach given value during the AnimationTime(ms) property, <para></para>
        /// or changes LineLength property immediately if AnimationTime is 0
        /// </summary>
        public double AnimatedLineLength
        {
            get { return (double)GetValue(AnimatedLineLengthProperty); }
            set { SetValue(AnimatedLineLengthProperty, value); }
        }
        
        /// <summary>
        /// Animation effected time (ms)
        /// </summary>
        public double AnimationTime
        {
            get { return (double)GetValue(AnimationTimeProperty); }
            set { SetValue(AnimationTimeProperty, value); }
        }
        
        /// <summary>
        /// Length ratio of arc line will be drawn, it changes AnimatedLineLength or LineLength denpends on AnimationTime property<para></para>
        /// If value is 1, drawing line length will be MeshVertexCount(inherited from ArcVisual)
        /// </summary>
        public double FillLineRatio
        {
            get { return (double)GetValue(FillLineRatioProperty); }
            set { SetValue(FillLineRatioProperty, value); }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            updateMeshGeometry(1, LineLength);

            base.OnRender(drawingContext);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (visualList != null && visualList.Count > 1)
            {
                var backLine = visualList[0] as Line;
                backLine.Stroke = this.Background;
                var foreLine = visualList[1] as Line;
                foreLine.Stroke = this.Foreground;
            }
            /// Default update first visual's mesh to fill whole arc as ArcLine's background
            fillMeshGeometry(0);
            fillMeshGeometry(1, 0);

            return base.MeasureOverride(availableSize);
        }

        protected override void OnInitialized(EventArgs e)
        {
            initializeLine();

            base.OnInitialized(e);
        }

        private void initializeLine()
        {
            /// Build 2 lines to visual list for foreground and background represents ArcLine length animation
            visualList = new List<Visual>();
            var backLine = new Line();
            backLine.X2 = 1;
            backLine.StrokeThickness = 1;
            backLine.Stroke = this.Background;
            var foreLine = new Line();
            foreLine.X2 = 1;
            foreLine.StrokeThickness = 1;
            foreLine.Stroke = this.Foreground;

            /// Background must be ordering before foreground
            visualList.Add(backLine);
            visualList.Add(foreLine);

            lineLengthAnimation = new DoubleAnimation(0, 0, new Duration(TimeSpan.FromMilliseconds(this.AnimationTime)));
            lineLengthAnimation.AutoReverse = false;
            lineLengthAnimation.RepeatBehavior = new RepeatBehavior(1);
            /// Binding animation's target property to this
            lineLengthAnimation.SetValue(Storyboard.TargetProperty, this);
            /// Binding target property's property to LineLengthProperty
            lineLengthAnimation.SetValue(Storyboard.TargetPropertyProperty, new PropertyPath(ArcLine.LineLengthProperty));
            storyBoard = new Storyboard();
            storyBoard.Children.Add(lineLengthAnimation);
        }
    }
}
