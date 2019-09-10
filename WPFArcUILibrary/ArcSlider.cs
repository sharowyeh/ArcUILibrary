using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WPFArcUILibrary
{
    /// <summary>
    /// This class just bending Silder according to ArcVisual(Viewport3D) implementaion
    /// TODO: It should be inherited or customized Silder class changes default behavior
    /// </summary>
    public class ArcSlider : ArcVisual
    {
        public static readonly DependencyProperty SliderStyleProperty = DependencyProperty.Register(
            "SliderStyle", typeof(Style), typeof(ArcSlider),
            new FrameworkPropertyMetadata(new Style(typeof(Slider)), FrameworkPropertyMetadataOptions.AffectsRender));

        public Style SliderStyle
        {
            get { return (Style)GetValue(SliderStyleProperty); }
            set { SetValue(SliderStyleProperty, value); }
        }

        public Slider Visual
        {
            get { return (visualList != null && visualList.Count > 0) ? visualList[0] as Slider : null; }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            /// The element's actual size should be get from ArrangeOverride/OnRender for final size, but
            /// ArrangeOverride/OnRender will be fired each of timeline that increased CPU loading if element applied animation effect.
            /// So use MeasureOverride to get measure size when parent UI layout completed, it just fired once if layout fixed.
            /// https://msdn.microsoft.com/en-us/library/ms745058(v=vs.110).aspx

            if (visualList != null && visualList.Count > 0)
            {
                /// Bypass silder's style
                var slider = visualList[0] as Slider;
                slider.Style = this.SliderStyle;
                slider.Background = this.Background;
                slider.Height = this.ArcWidth;
                /// Re-calculate angle from slider extended width
                /// slider needs to added thumb height(defined in its style) for extended width to display thumb in front of begin and back of end
                var extendLength = 16;/*slider.Height*/;
                var extendAngle = extendLength / (this.Radius * 2);
                var startAngle = this.StartAngle / 180 * Math.PI;
                var endAngle = this.EndAngle / 180 * Math.PI;
                startAngle -= extendAngle;
                endAngle += extendAngle;
                /// Use middle of arc length for silder's thumb looks fine after bending it
                slider.Width = this.ArcLength;
            }
            /// Default update visual's mesh to fill whole arc
            fillMeshGeometry(0);

            return base.MeasureOverride(availableSize);
        }

        protected override void OnInitialized(EventArgs e)
        {
            initializeSlider();

            base.OnInitialized(e);
        }

        private void initializeSlider()
        {
            /// Create a default slider and its styles
            visualList = new List<Visual>();
            var slider = new Slider();
            var tip = new ToolTip();
            tip.Background = Brushes.Black;
            tip.Foreground = Brushes.Red;
            tip.FontFamily = new FontFamily("Segoe UI");
            tip.FontSize = 12;
            tip.FontWeight = FontWeights.SemiBold;
            slider.ToolTip = tip;
            slider.Style = this.SliderStyle;
            slider.Background = this.Background;
            visualList.Add(slider);
        }
    }
}
