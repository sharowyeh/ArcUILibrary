using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace WPFArcUILibrary
{
    /// <summary>
    /// The basic class of arc component, inherited from Viewport3D drawing mesh geometry
    /// </summary>
    public abstract class ArcVisual : Viewport3D
    {
        #region Dependency properties for XAML styling, data binding and animation

        public static readonly DependencyProperty StartAngleProperty = DependencyProperty.Register(
            "StartAngle", typeof(double), typeof(ArcVisual),
            new FrameworkPropertyMetadata(-90.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty EndAngleProperty = DependencyProperty.Register(
            "EndAngle", typeof(double), typeof(ArcVisual),
            new FrameworkPropertyMetadata(90.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register(
            "Radius", typeof(double), typeof(ArcVisual),
            new FrameworkPropertyMetadata(50.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ArcWidthProperty = DependencyProperty.Register(
            "ArcWidth", typeof(double), typeof(ArcVisual),
            new FrameworkPropertyMetadata(5.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(
            "Foreground", typeof(Brush), typeof(ArcVisual),
            new FrameworkPropertyMetadata(Brushes.White, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
            "Background", typeof(Brush), typeof(ArcVisual),
            new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender));

        #endregion

        public double StartAngle
        {
            get { return (double)GetValue(StartAngleProperty); }
            set { SetValue(StartAngleProperty, value); }
        }

        public double EndAngle
        {
            get { return (double)GetValue(EndAngleProperty); }
            set { SetValue(EndAngleProperty, value); }
        }

        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        public double ArcWidth
        {
            get { return (double)GetValue(ArcWidthProperty); }
            set { SetValue(ArcWidthProperty, value); }
        }

        /// <summary>
        /// Readonly: The length of the arc visual is calculated from Radius, StartAngle and EndAngle<para></para>
        /// Notice: For mesh geometry drawing triangles, refer to OuterArcLength and InnerArcLength properties considered with ArcWidth
        /// </summary>
        public double ArcLength
        {
            get { return Radius * Math.Abs(EndAngle - StartAngle) * Math.PI / 180; }
        }

        /// <summary>
        /// Readonly: Inner length of arc visual with its ArcWidth
        /// </summary>
        public double InnerArcLength
        {
            get { return (Radius - (ArcWidth / 2)) * Math.Abs(EndAngle - StartAngle) * Math.PI / 180; }
        }

        /// <summary>
        /// Readonly: Outer length of arc visual with its ArcWidth
        /// </summary>
        public double OuterArcLength
        {
            get { return (Radius + (ArcWidth / 2)) * Math.Abs(EndAngle - StartAngle) * Math.PI / 180; }
        }

        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        #region PerspectiveCamera properties of Visual3D rendering (for debug)

        public static readonly DependencyProperty FieldOfViewProperty = DependencyProperty.Register(
            "FieldOfView", typeof(double), typeof(ArcVisual),
            new FrameworkPropertyMetadata(120.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ZPositionProperty = DependencyProperty.Register(
            "ZPosition", typeof(double), typeof(ArcVisual),
            new FrameworkPropertyMetadata(30.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public double FieldOfView
        {
            get { return (double)GetValue(FieldOfViewProperty); }
            set { SetValue(FieldOfViewProperty, value); }
        }

        public double ZPosition
        {
            get { return (double)GetValue(ZPositionProperty); }
            set { SetValue(ZPositionProperty, value); }
        }

        #endregion

        static ArcVisual()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ArcVisual), new System.Windows.FrameworkPropertyMetadata(typeof(ArcVisual)));
        }

        protected PerspectiveCamera camera;
        protected ModelVisual3D modelVisual3D;
        protected List<Viewport2DVisual3D> viewportList;
        protected List<Visual> visualList;
        protected List<MeshGeometry3D> meshList;

        protected override Size MeasureOverride(Size availableSize)
        {
            /// The element's actual size should be get from ArrangeOverride/OnRender for final size, but
            /// ArrangeOverride/OnRender will be fired each of timeline that increased CPU loading if element applied animation effect.
            /// So use MeasureOverride to get measure size when parent UI layout completed, it just fired once if layout fixed.
            /// https://msdn.microsoft.com/en-us/library/ms745058(v=vs.110).aspx

            /// Use element width multiple 0.3 for camera's Z position(and fixed field of view to 120) 
            /// will get best element display size by given variables number.
            if (availableSize.IsEmpty == false)
                camera.Position = new Point3D(0, 0, availableSize.Width * 0.3);

            return base.MeasureOverride(availableSize);
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            initializeVisualChildren();
        }

        protected void initializeVisualChildren()
        {
            if (visualList == null || visualList.Count == 0)
                throw new Exception("Visual list must has visual for rendering");

            camera = new PerspectiveCamera();
            camera.FieldOfView = 120;

            modelVisual3D = new ModelVisual3D();
            modelVisual3D.Content = new DirectionalLight(System.Windows.Media.Colors.White, new Vector3D(0, 0, -1));

            viewportList = new List<Viewport2DVisual3D>();
            meshList = new List<MeshGeometry3D>();

            foreach (var visual in visualList)
            {
                var viewport = new Viewport2DVisual3D();
                viewport.Material = new DiffuseMaterial();
                viewport.Material.SetValue(Viewport2DVisual3D.IsVisualHostMaterialProperty, true);
                var meshGeometry = new MeshGeometry3D();
                viewport.Geometry = meshGeometry;
                viewport.Visual = visual;
                viewportList.Add(viewport);
                meshList.Add(meshGeometry);
            }

            this.Camera = camera;
            this.Children.Add(modelVisual3D);
            foreach (var viewport in viewportList)
                this.Children.Add(viewport);
        }

        /// <summary>
        /// Numbers of visual's mesh vertex which will be drawn for a arc visual, <para></para>
        /// Default mesh vertex value is ArcLength if greater(included) then 100.
        /// Notice: Large vertex count can gets more smooth curve but also increases computing effort.
        /// </summary>
        public double MeshVertexCount
        {
            get { return (this.ArcLength > 100) ? this.ArcLength : 100; }
            //get { return this.ArcLength / 2; }
        }

        /// <summary>
        /// Fill mesh geometry for specific visual
        /// </summary>
        /// <param name="meshIndex">Given mesh(visual) index</param>
        /// <param name="fillRatio">Clockwise fill ratio 0~1, default 1 is fill whole arc</param>
        protected void fillMeshGeometry(int meshIndex, double fillRatio = 1)
        {
            if (meshList == null || meshList.Count <= meshIndex)
                return;
            /// Fill mesh geometry also force update mesh coordinates for storke brush
            updateMeshGeometry(meshIndex, this.MeshVertexCount * fillRatio, true);
        }

        /// <summary>
        /// Mesh position list needs to rebuild if StartAngle, EndAngle, Radius changed
        /// </summary>
        private List<Point3D> meshPositions = new List<Point3D>();
        /// <summary>
        /// Mesh coordinate list needs to rebuild if MeshVertexCount changed
        /// </summary>
        private List<Point> meshCoordinates = new List<Point>();

        /// <summary>
        /// Update visual's mesh geometry by given specific arc length<para></para>
        /// Notice: Not all caller will get the maximum length of the arc visual, <para></para>
        /// please consider using fillMeshGeometry(int meshIndex, double fillRatio)
        /// </summary>
        /// <param name="meshIndex">Given mesh(visual) index</param>
        /// <param name="drawLength">Given drawn arc length</param>
        protected void updateMeshGeometry(int meshIndex, double drawLength, bool forceUpdateMesh = false)
        {
            if (meshList == null || meshList.Count <= meshIndex)
                return;

            /// In 2D phases of 3D coordinates, the first phase is minus X with plus Y, second phase is plus X with plus Y,
            /// third phase is plus X with minus Y and four phase is minus X with minus Y.
            /// If the start point in first or four phase, start angle should be minus
            double startAngle = this.StartAngle / 180 * Math.PI;
            double endAngle = this.EndAngle / 180 * Math.PI;
            double outerRadius = this.Radius + (this.ArcWidth / 2);
            double innerRadius = this.Radius - (this.ArcWidth / 2);

            /// Calculate MeshGeometry3D by given angles and radius
            if (meshPositions.Count == 0 || meshCoordinates.Count == 0 || forceUpdateMesh == true)
            {
                List<Point3D> positions = new List<Point3D>();
                List<Point> coordinates = new List<Point>();
                /// Mesh geometry must be included both begin(0) and end(MeshVertexCount), 
                /// the total positions/coordinates = ((int)MeshVertexCount + 1) * 2 if step is 1
                for (int i = 0; i < this.MeshVertexCount + 1; i++)
                {
                    /// Get arc length ratio divided by mesh vertex(a whole arc's length or 100)
                    double lengthRatio = (double)i / this.MeshVertexCount;
                    if (lengthRatio > 1)
                        lengthRatio = 1;
                    /// Get angle of each length ratio in arc visual
                    double angle = startAngle + ((endAngle - startAngle) * lengthRatio);
                    /// Calculate outer position of each angles
                    double posX2Coord0 = Math.Sin(angle) * outerRadius;
                    double posY2Coord0 = Math.Cos(angle) * outerRadius;
                    positions.Add(new Point3D(posX2Coord0, posY2Coord0, 0));
                    /// Calculate inner position of each angles
                    double posX2Coord1 = Math.Sin(angle) * innerRadius;
                    double posY2Coord1 = Math.Cos(angle) * innerRadius;
                    positions.Add(new Point3D(posX2Coord1, posY2Coord1, 0));
                    /// Coordinates begins 0 to 1 just can represented by length ratio
                    double coordX = lengthRatio;
                    coordinates.Add(new Point(coordX, 0));
                    coordinates.Add(new Point(coordX, 1));
                }
                meshPositions = positions;
                meshCoordinates = coordinates;
                meshList[meshIndex].Positions = new Point3DCollection(positions);
                meshList[meshIndex].TextureCoordinates = new System.Windows.Media.PointCollection(coordinates);
            }


            List<int> triangles = new List<int>();
            int triangleCount = 0;
            for (int i = 0; i < drawLength; i++)
            {

                /// First triangle=0,1,2 and second triangle=1,3,2
                int[] firstTriangle = new int[] { triangleCount * 2 + 0, triangleCount * 2 + 1, triangleCount * 2 + 2 };
                int[] secondTriangle = new int[] { triangleCount * 2 + 1, triangleCount * 2 + 3, triangleCount * 2 + 2 };
                triangles.AddRange(firstTriangle);
                triangles.AddRange(secondTriangle);
                triangleCount++;
            }
            /// Following is example of what for-loop works
            //positions.Clear();
            //positions.Add(new Point3D(0, 15, 0));
            //positions.Add(new Point3D(0, 10, 0));
            //positions.Add(new Point3D(15, 15, 0));
            //positions.Add(new Point3D(15, 10, 0));
            //coordinates.Clear();
            //coordinates.Add(new Point(0, 0));
            //coordinates.Add(new Point(0, 1));
            //coordinates.Add(new Point(1, 0));
            //coordinates.Add(new Point(1, 1));
            //triangles.Clear();
            //triangles.AddRange(new int[] { 0, 1, 2 });
            //triangles.AddRange(new int[] { 1, 3, 2 });

            /// Apply to given mesh geometry
            if (triangleCount == 0)
            {
                meshList[meshIndex].Positions.Clear();
                meshList[meshIndex].TriangleIndices.Clear();
            }
            else
            {
                meshList[meshIndex].Positions = new Point3DCollection(meshPositions);
                meshList[meshIndex].TriangleIndices = new System.Windows.Media.Int32Collection(triangles);
            }
        }
    }
}
