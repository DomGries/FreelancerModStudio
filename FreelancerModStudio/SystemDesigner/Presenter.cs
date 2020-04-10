namespace FreelancerModStudio.SystemDesigner
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Media3D;

    using FreelancerModStudio.Data;
    using FreelancerModStudio.Properties;
    using FreelancerModStudio.SystemDesigner.Content;

    using HelixEngine;

    using ContextMenu = System.Windows.Controls.ContextMenu;
    using MenuItem = System.Windows.Controls.MenuItem;
    using Sys = System;

    public class ContentBaseList : List<ContentBase>
    {
        public static void PresentSelect(Presenter present)
        {
            present.SetSelectionBox();
            present.SetTrackedLine(true);
            present.SetManipulatorLines();
            present.SetTitle();
        }

        public static void AddItem(Presenter present, ContentBase content)
        {

            if (present.Manipulating && present.SelectedContent.Any(x => x != content))
                present.StopManipulating(true);

            present.SelectedContent.Add(content);
            PresentSelect(present);
        }

        public static void ClearAll(Presenter present)
        {
            present.SelectedContent.Clear();
            PresentSelect(present);
        }

        public static void EditAt(Presenter present, ContentBase content, int index)
        {
            present.SelectedContent[index] = content;
            PresentSelect(present);
        }

        public static void RemoveAt(Presenter present, int index)
        {
            present.SelectedContent.RemoveAt(index);
            PresentSelect(present);
        }
    }

    public class Presenter
    {
        public delegate void SelectionChangedType(TableBlock block, bool toggle);
        public delegate void FileOpenType(string file);
        public delegate void DataManipulatedType(List<TableBlock> newBlock, List<TableBlock> oldBlock);

        internal DataManipulatedType DataManipulated;
        internal HelixViewport3D Viewport;
        internal ViewerType ViewerType;
        internal bool IsModelMode;
        internal string DataPath;
        internal SelectionChangedType SelectionChanged;
        internal FileOpenType FileOpen;
        internal ContentBaseList SelectedContent = new ContentBaseList();
        internal bool Manipulating;
        internal SystemEditorForm SystemEditorForm;

        private readonly Dictionary<string, Model3D> modelCache = new Dictionary<string, Model3D>(StringComparer.OrdinalIgnoreCase);
        private int secondLayerId;
        private Visual3D lighting;
        private BoundingBoxWireFrameVisual3D selectionBox;
        private LineVisual3D trackedLine;

        private ContentBase trackedContent;
        private FixedLineVisual3D manipulatorX;
        private FixedLineVisual3D manipulatorY;
        private FixedLineVisual3D manipulatorZ;
        private ManipulationMode manipulationMode;
        private ManipulationAxis manipulationAxis;

        // A dictionary of index to last pos
        private Dictionary<int, Point3D?> manipulationLastPosition = new Dictionary<int, Point3D?>();

        private void OnSelectionChanged(TableBlock block, bool toggle) => this.SelectionChanged?.Invoke(block, toggle);
        private void OnFileOpen(string file) => this.FileOpen?.Invoke(file);
        private void OnDataManipulated(List<TableBlock> newBlock, List<TableBlock> oldBlock) => this.DataManipulated?.Invoke(newBlock, oldBlock);

        public Visual3D Lighting
        {
            get => this.lighting;

            set
            {
                this.AddOrReplace(this.lighting, value);
                this.lighting = value;
            }
        }

        public BoundingBoxWireFrameVisual3D SelectionBox
        {
            get => this.selectionBox;

            set
            {
                this.AddOrReplace(this.selectionBox, value);
                this.selectionBox = value;
            }
        }

        public LineVisual3D TrackedLine
        {
            get => this.trackedLine;

            set
            {
                this.AddOrReplace(this.trackedLine, value);
                this.trackedLine = value;
            }
        }

        public FixedLineVisual3D ManipulatorLineX
        {
            get => this.manipulatorX;

            set
            {
                this.AddOrReplace(this.manipulatorX, value);
                this.manipulatorX = value;
            }
        }

        public FixedLineVisual3D ManipulatorLineY
        {
            get => this.manipulatorY;

            set
            {
                this.AddOrReplace(this.manipulatorY, value);
                this.manipulatorY = value;
            }
        }

        public FixedLineVisual3D ManipulatorLineZ
        {
            get => this.manipulatorZ;

            set
            {
                this.AddOrReplace(this.manipulatorZ, value);
                this.manipulatorZ = value;
            }
        }

        public ContentBase TrackedContent
        {
            get => this.trackedContent;

            set
            {
                this.trackedContent = value;

                // track content visually
                this.SetTrackedLine(false);
                this.SetTitle();
            }
        }

        public ManipulationMode ManipulationMode
        {
            get => this.manipulationMode;

            set
            {
                if (this.Manipulating && this.manipulationMode != value) this.StopManipulating(false);
                this.manipulationMode = value;
                this.SetManipulatorLines();
            }
        }

        public Presenter(HelixViewport3D viewport, SystemEditorForm form)
        {
            this.SystemEditorForm = form;

            this.Viewport = viewport;
            this.Viewport.MouseDown += this.ViewportMouseDown;
            this.Viewport.MouseUp += this.ViewportMouseUp;
            this.Viewport.MouseMove += this.ViewportMouseMove;
            this.Viewport.KeyDown += this.ViewportKeyDown;
            this.Viewport.KeyUp += this.ViewportKeyUp;

            this.Viewport.ViewCubeLeftBrush = new SolidColorBrush(SharedMaterials.ManipulatorZ);
            this.Viewport.ViewCubeRightBrush = this.Viewport.ViewCubeLeftBrush;
            this.Viewport.ViewCubeTopBrush = new SolidColorBrush(SharedMaterials.ManipulatorY);
            this.Viewport.ViewCubeBottomBrush = this.Viewport.ViewCubeTopBrush;
            this.Viewport.ViewCubeFrontBrush = new SolidColorBrush(SharedMaterials.ManipulatorX);
            this.Viewport.ViewCubeBackBrush = this.Viewport.ViewCubeFrontBrush;
            this.Viewport.ViewCubeLeftText = "F";
            this.Viewport.ViewCubeRightText = "B";
            this.Viewport.ViewCubeFrontText = "R";
            this.Viewport.ViewCubeBackText = "L";

            this.Lighting = new SystemLightsVisual3D();
        }

        public void LookAt(ContentBase content) => this.Viewport.LookAt(content.GetPositionPoint(), Animator.AnimationDuration.TimeSpan.TotalMilliseconds);

        public void LookAt(Point3D point) => this.Viewport.LookAt(point, Animator.AnimationDuration.TimeSpan.TotalMilliseconds);

        public void LookAtAndZoom(ContentBase content, double zoomFactor, bool animate)
        {
            Rect3D bounds = this.GetBounds(content);
            Matrix3D matrix = content.Transform.Value;

            // prepend translation to account for model scale
            matrix.TranslatePrepend(new Vector3D(bounds.X + bounds.SizeX * 0.5f, bounds.Y + bounds.SizeY * 0.5f, bounds.Z + bounds.SizeZ * 0.5f));
            Point3D point = new Point3D(matrix.OffsetX, matrix.OffsetY, matrix.OffsetZ);

            // get distance based on model transform
            matrix.TranslatePrepend(new Vector3D(bounds.SizeX, bounds.SizeY, bounds.SizeZ));
            double distance = Math.Max(Math.Max(Math.Abs(matrix.OffsetX - point.X), Math.Abs(matrix.OffsetY - point.Y)), Math.Abs(matrix.OffsetZ - point.Z));

            this.Viewport.ZoomExtents(point, distance * 0.5 * zoomFactor, animate ? Animator.AnimationDuration.TimeSpan.TotalMilliseconds : 0);
        }

        public void Add(List<TableBlock> blocks)
        {
            Animator.AnimationDuration = new Duration(TimeSpan.Zero);

            foreach (TableBlock block in blocks)
            {
                if (block.Visibility)
                {
                    this.AddBlock(block);
                }
            }

            Animator.AnimationDuration = new Duration(TimeSpan.FromMilliseconds(500));
        }

        public void Add(TableBlock block)
        {
            Animator.AnimationDuration = new Duration(TimeSpan.Zero);

            this.AddBlock(block);

            Animator.AnimationDuration = new Duration(TimeSpan.FromMilliseconds(500));
        }

        public void Delete(ContentBase content)
        {
            // if we delete a system also delete all universe connections to and from it
            if (this.ViewerType == ViewerType.Universe)
            {
                System system = content as Content.System;
                if (system != null)
                {
                    this.DeleteConnections(system);
                }
            }

            this.RemoveModel(content);
        }

        public ContentBase GetSelection(Point position, bool farthest, bool checkManipulators, out Point3D point)
        {
            PointHitTestParameters hitParams = new PointHitTestParameters(position);

            ContentBase visual = null;
            Point3D hitPoint = new Point3D();
            double distance = 0.0;
            int selectionPriority = -1;

            VisualTreeHelper.HitTest(
                this.Viewport.Viewport,
                null,
                delegate(HitTestResult hit)
                {
                    RayMeshGeometry3DHitTestResult rayHit = hit as RayMeshGeometry3DHitTestResult;
                    if (rayHit != null)
                    {
                        if (checkManipulators)
                        {
                            // start manipulation on manipulator selection
                            if (rayHit.VisualHit == this.manipulatorX)
                            {
                                this.StartManipulation(ManipulationAxis.X, this.manipulatorX, position);
                                visual = null;
                                return HitTestResultBehavior.Stop;
                            }

                            if (rayHit.VisualHit == this.manipulatorY)
                            {
                                this.StartManipulation(ManipulationAxis.Y, this.manipulatorY, position);
                                visual = null;
                                return HitTestResultBehavior.Stop;
                            }

                            if (rayHit.VisualHit == this.manipulatorZ)
                            {
                                this.StartManipulation(ManipulationAxis.Z, this.manipulatorZ, position);
                                visual = null;
                                return HitTestResultBehavior.Stop;
                            }
                        }

                        ContentBase newHit = rayHit.VisualHit as ContentBase;
                        if (newHit == null || newHit.Block == null)
                        {
                            // prevent selecting wirebox + universe connection
                            return HitTestResultBehavior.Continue;
                        }

                        MeshGeometry3D mesh = rayHit.MeshHit;
                        if (mesh != null)
                        {
                            Point3D p1 = mesh.Positions[rayHit.VertexIndex1];
                            Point3D p2 = mesh.Positions[rayHit.VertexIndex2];
                            Point3D p3 = mesh.Positions[rayHit.VertexIndex3];
                            double x = p1.X * rayHit.VertexWeight1 + p2.X * rayHit.VertexWeight2
                                       + p3.X * rayHit.VertexWeight3;
                            double y = p1.Y * rayHit.VertexWeight1 + p2.Y * rayHit.VertexWeight2
                                       + p3.Y * rayHit.VertexWeight3;
                            double z = p1.Z * rayHit.VertexWeight1 + p2.Z * rayHit.VertexWeight2
                                       + p3.Z * rayHit.VertexWeight3;

                            // point in local coordinates
                            Point3D p = new Point3D(x, y, z);

                            // transform to global coordinates

                            // first transform the Model3D hierarchy
                            GeneralTransform3D t2 = Viewport3DHelper.GetTransform(rayHit.VisualHit, rayHit.ModelHit);
                            if (t2 != null)
                            {
                                p = t2.Transform(p);
                            }

                            // then transform the Visual3D hierarchy up to the Viewport3D ancestor
                            GeneralTransform3D t = Viewport3DHelper.GetTransform(this.Viewport.Viewport, rayHit.VisualHit);
                            if (t != null)
                            {
                                p = t.Transform(p);
                            }

                            double newDistance = (this.Viewport.Camera.Position - p).LengthSquared;
                            int newSelectionPriority = GetSelectionPriority(newHit);

                            if (newSelectionPriority > selectionPriority ||
                                (newSelectionPriority == selectionPriority &&
                                 (farthest ? newDistance > distance : newDistance < distance)))
                            {
                                visual = newHit;
                                hitPoint = p;
                                distance = newDistance;
                                selectionPriority = newSelectionPriority;
                            }
                        }
                    }

                    return HitTestResultBehavior.Continue;
                },
                hitParams);

            point = hitPoint;
            return visual;
        }

        public Rect3D GetAllBounds()
        {
            Rect3D bounds = Rect3D.Empty;
            for (int i = this.GetContentStartId(); i < this.Viewport.Children.Count; ++i)
            {
                Rect3D contentBounds = Visual3DHelper.FindBounds(this.Viewport.Children[i], Transform3D.Identity);
                bounds.Union(contentBounds);
            }

            return bounds;
        }

        internal void StopManipulating(bool cancelled)
        {
            if (!this.Manipulating)
                return;

            switch (this.manipulationAxis)
            {
                case ManipulationAxis.X:
                    this.ManipulatorLineX.Color = SharedMaterials.ManipulatorX;
                    break;
                case ManipulationAxis.Y:
                    this.ManipulatorLineY.Color = SharedMaterials.ManipulatorY;
                    break;
                case ManipulationAxis.Z:
                    this.ManipulatorLineZ.Color = SharedMaterials.ManipulatorZ;
                    break;
            }

            if (cancelled)
            {
                foreach (var content in this.SelectedContent)
                    SystemParser.SetValues(content, content.Block, true);
            }
            else
            {
                // update data globally
                this.UpdateSelectedBlock();
            }

            // stop manipulating
            this.Manipulating = false;
            this.Viewport.Viewport.ReleaseMouseCapture();
        }

        private void AddContent(ContentBase content)
        {
            // load model if it was not loaded yet
            if (content.Content == null)
                this.LoadModel(content);

            this.AddModel(content);

            // reset reference of previously invisible selected content
            int index = this.SelectedContent.FindIndex(x => x.Block == content.Block);
            if (index != -1)
                this.SelectedContent[index] = content;
        }

        private void AddBlock(TableBlock block)
        {
            ContentBase content = this.CreateContent(block);
            if (content != null)
            {
                this.AddContent(content);
            }
        }

        private void AddModel(ContentBase content)
        {
            if (content.IsEmissive())
            {
                this.Viewport.Children.Add(content);
            }
            else
            {
                this.Viewport.Children.Insert(this.secondLayerId, content);
                ++this.secondLayerId;
            }
        }

        private void RemoveModel(ContentBase content)
        {
            this.Viewport.Children.Remove(content);

            if (!content.IsEmissive())
            {
                --this.secondLayerId;
            }

            if (content == this.trackedContent)
            {
                this.TrackedContent = null;
            }
        }

        private static int GetSelectionPriority(ContentBase content)
        {
            switch (content.Block.ObjectType)
            {
                case ContentType.LightSource:
                case ContentType.Construct:
                case ContentType.Depot:
                case ContentType.DockingRing:
                case ContentType.JumpGate:
                case ContentType.JumpHole:
                case ContentType.Planet:
                case ContentType.Satellite:
                case ContentType.Ship:
                case ContentType.Station:
                case ContentType.Sun:
                case ContentType.TradeLane:
                case ContentType.WeaponsPlatform:
                    return 2;
                case ContentType.ZonePath:
                case ContentType.ZonePathTrade:
                case ContentType.ZonePathTradeLane:
                    return 1;
                default:
                    return 0;
            }
        }

        private void ViewportKeyDown(object sender,  Sys.Windows.Input.KeyEventArgs e) => this.ViewportKeyEvent(e, false);

        private void ViewportKeyUp(object sender, Sys.Windows.Input.KeyEventArgs e) => this.ViewportKeyEvent(e, true);

        private void ViewportKeyEvent(Sys.Windows.Input.KeyEventArgs e, bool isKeyUp)
        {
            bool isCtrl = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            bool isAlt = (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt;
            bool isShift = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
            bool isCaps = Keyboard.IsKeyToggled(Key.CapsLock);

            // catch key short-cuts on the viewport(though arrow keys still do not work due to docking panel)
            switch (e.Key)
            {
                case Key.W:
                    if (isCaps)
                    {
                        if (isKeyUp || (!isAlt && !isCtrl))
                        {
                            this.StartOffsetManipulation(CameraDirection.Up, isKeyUp, isShift);
                        }
                    }
                    else if (isKeyUp || (!e.IsRepeat && !isCtrl && !isAlt))
                    {
                        this.Fly(CameraDirection.Forward, isKeyUp);
                    }

                    break;
                case Key.A:
                    if (isCaps)
                    {
                        if (isKeyUp || (!isAlt && !isCtrl))
                        {
                            this.StartOffsetManipulation(CameraDirection.Left, isKeyUp, isShift);
                        }
                    }
                    else if (isKeyUp || (!e.IsRepeat && !isCtrl && !isAlt))
                    {
                        this.Fly(CameraDirection.Left, isKeyUp);
                    }

                    break;
                case Key.S:
                    if (isCaps)
                    {
                        if (isKeyUp || (!isAlt && !isCtrl))
                        {
                            this.StartOffsetManipulation(CameraDirection.Down, isKeyUp, isShift);
                        }
                    }
                    else if (isKeyUp || (!e.IsRepeat && !isCtrl && !isAlt))
                    {
                        this.Fly(CameraDirection.Backward, isKeyUp);
                    }

                    break;
                case Key.D:
                    if (isCaps)
                    {
                        if (isKeyUp || (!isAlt && !isCtrl))
                        {
                            this.StartOffsetManipulation(CameraDirection.Right, isKeyUp, isShift);
                        }
                    }
                    else if (isKeyUp || (!e.IsRepeat && !isCtrl && !isAlt))
                    {
                        this.Fly(CameraDirection.Right, isKeyUp);
                    }

                    break;
                case Key.Space:
                    if (!isCaps && (isKeyUp || (!e.IsRepeat && !isCtrl && !isAlt)))
                    {
                        this.Fly(CameraDirection.Up, isKeyUp);
                    }

                    break;
                case Key.E:
                    if (!isCaps && (isKeyUp || (!e.IsRepeat && !isCtrl && !isAlt)))
                    {
                        this.Fly(CameraDirection.Down, isKeyUp);
                    }

                    break;
                case Key.F:
                    if (!isKeyUp && !e.IsRepeat && !isCtrl && !isAlt)
                    {
                        if (isShift)
                        {
                            this.LookAtSelected();
                        }
                        else
                        {
                            this.FocusSelected();
                        }
                    }

                    break;
                case Key.T:
                    if (!isKeyUp && !e.IsRepeat && !isCtrl && !isAlt)
                    {
                        this.TrackSelected();
                    }

                    break;
            }
        }

        private void StartOffsetManipulation(CameraDirection direction, bool complete, bool small)
        {
            if (this.ViewerType != ViewerType.System && this.ViewerType != ViewerType.Universe)
                return;

            if (complete)
                // update data globally
                this.UpdateSelectedBlock();

            else
            {
                Vector3D offset = this.Viewport.CameraController.GetDirection(direction);

                if (this.ViewerType == ViewerType.Universe)
                    offset *= (small ? 0.1 : 1) * SystemParser.UniverseScale;
                else
                    offset *= (small ? 1 : 25) * SystemParser.SystemScale;

                // update transform
                foreach (ContentBase content in this.SelectedContent)
                {
                    this.ManipulateOffset(offset, content);
                    content.UpdateTransform(false);
                    ContentBaseList.PresentSelect(this);
                }
            }
        }

        private void Fly(CameraDirection direction, bool stop)
        {
            if (stop)
                this.Viewport.CameraController.StopFly(direction);
            else
                this.Viewport.CameraController.StartFly(direction);
        }

        private void ViewportMouseDown(object sender, MouseButtonEventArgs e)
        {
            // workaround to focus viewport for key events
            this.Viewport.Focus();

            bool isDoubleClick = e.ClickCount > 1;

            bool isCtrl = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            bool isAlt = (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt;
            bool isShift = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;

            // reset camera
            if (isDoubleClick && e.ChangedButton == MouseButton.Middle && isShift)
            {
                this.Viewport.CameraController.ResetCamera();
                CameraHelper.ZoomExtents(this.Viewport.Camera, this.Viewport.Viewport, this.GetAllBounds(), 0);

                return;
            }

            bool isSelect = e.ChangedButton == MouseButton.Left && !isAlt;
            bool isLookAt = e.ChangedButton == MouseButton.Right && isDoubleClick;

            if (isSelect || isLookAt)
            {
                Point3D point;
                ContentBase visual = this.GetSelection(e.GetPosition(this.Viewport.Viewport), isShift, !isLookAt, out point);
                if (visual != null)
                {
                    if (isLookAt)
                        // change the 'lookat' point
                        this.LookAt(point);
                    else
                        this.Select(visual, isCtrl);

                    return;
                }

            }
        }

        private void ViewportMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                Point p = e.GetPosition(this.Viewport.Viewport);
                this.SystemEditorForm.OpenContextMenu(new global::System.Drawing.Point(Convert.ToInt32(Math.Round(p.X)), Convert.ToInt32(Math.Round(p.Y))));
            }

            this.StopManipulating(false);
        }

        private void UpdateSelectedBlock()
        {
            List<TableBlock> oldBlocks = new List<TableBlock>();
            List<TableBlock> newBlocks = new List<TableBlock>();

            for (var index = 0; index < this.SelectedContent.Count; index++)
            {
                var content = this.SelectedContent[index];
                TableBlock oldBlock = content.Block;
                TableBlock newBlock = ObjectClone.Clone(oldBlock);
                newBlock.SetModifiedChanged();

                content.Block = newBlock;
                oldBlocks.Add(oldBlock);
                newBlocks.Add(newBlock);

                SystemParser.WriteBlock(content);
            }
            this.OnDataManipulated(newBlocks, oldBlocks);
        }

        private Point3D? GetMousePoint(Point mousePosition, ContentBase content) => this.Viewport.CameraController.UnProject(mousePosition, content.GetPositionPoint(), this.Viewport.CameraController.Camera.LookDirection);

        private Vector3D GetMouseDelta(Point mousePosition, ContentBase content, int index)
        {
            // get mouse delta
            Point3D? thisPoint3D = this.GetMousePoint(mousePosition, content);
            Vector3D delta3D = thisPoint3D.Value - this.manipulationLastPosition[index].Value;
            this.manipulationLastPosition[index] = thisPoint3D;

            // transform mouse delta using matrix
            Matrix3D matrix = ContentBase.RotationMatrix(content.Rotation);
            matrix.Invert();
            delta3D = matrix.Transform(delta3D);

            if (this.manipulationMode == ManipulationMode.Rotate)
            {
                double length = delta3D.Length;
                if (length > 0)
                {
                    delta3D *= 2 / length;
                }
            }
            else if (this.manipulationMode == ManipulationMode.Scale)
            {
                if (this.ManipulatorLineY == null)
                {
                    if (this.ManipulatorLineZ == null)
                    {
                        return new Vector3D(delta3D.X, delta3D.X, delta3D.X);
                    }

                    if (this.manipulationAxis == ManipulationAxis.X)
                    {
                        return new Vector3D(delta3D.X, 0d, delta3D.X);
                    }
                }
            }

            switch (this.manipulationAxis)
            {
                default:
                    return new Vector3D(delta3D.X, 0d, 0d);
                case ManipulationAxis.Y:
                    return new Vector3D(0d, 0d, delta3D.Z);
                case ManipulationAxis.Z:
                    return new Vector3D(0d, delta3D.Y, 0d);
            }
        }

        private void ViewportMouseMove(object sender, Sys.Windows.Input.MouseEventArgs e)
        {
            if (!this.Manipulating || this.SelectedContent.Count == 0)
                return;

            ContentBase first = this.SelectedContent[0];

            Vector3D delta = this.GetMouseDelta(e.GetPosition(this.Viewport.Viewport), first, 0);
            Matrix3D originTM = ContentBase.RotationMatrix(first.Rotation);
            Matrix3D resultTM = new Matrix3D();

            switch (this.manipulationMode)
            {
                case ManipulationMode.Translate:
                    delta = originTM.Transform(delta);
                    break;
                case ManipulationMode.Rotate:
                    originTM.Translate(first.Position);

                    // TODO: Replace line below with proper Clone method.
                    Matrix3D originInverseTM = new Matrix3D(
                        originTM.M11,
                        originTM.M12,
                        originTM.M13,
                        originTM.M14,
                        originTM.M21,
                        originTM.M22,
                        originTM.M23,
                        originTM.M24,
                        originTM.M31,
                        originTM.M32,
                        originTM.M33,
                        originTM.M34,
                        originTM.OffsetX, 
                        originTM.OffsetY, 
                        originTM.OffsetZ, 
                        originTM.M44);
                    originInverseTM.Invert();

                    resultTM = Matrix3D.Multiply(originInverseTM, ContentBase.RotationMatrix(delta));
                    resultTM = Matrix3D.Multiply(resultTM, originTM);

                    break;
            }

            foreach (var target in this.SelectedContent)
            {
                switch (this.manipulationMode)
                {
                    case ManipulationMode.Translate:
                        target.Position += delta;
                        break;
                    case ManipulationMode.Rotate:
                        Matrix3D targetTM = ContentBase.RotationMatrix(target.Rotation);
                        targetTM.Translate(target.Position);
                        targetTM.Append(resultTM);

                        target.Rotation = ContentBase.GetRotation(targetTM);
                        target.Position = new Vector3D(targetTM.OffsetX, targetTM.OffsetY, targetTM.OffsetZ);

                        break;
                    case ManipulationMode.Scale:
                        // relative scale
                        this.ManipulateScale(delta, target);
                        break;
                }

                // update transform
                target.UpdateTransform(false);
            }

            ContentBaseList.PresentSelect(this);
        }

        private void ManipulateOffset(Vector3D offset, ContentBase content) => content.Position += offset;

        private void ManipulateScale(Vector3D delta, ContentBase content)
        {
            // update position
            content.Scale += delta;

            // prevent negative scaling
            const double MinScale = 10 * SystemParser.SystemScale;

            if (content.Scale.X < MinScale || content.Scale.Y < MinScale || content.Scale.Z < MinScale)
                content.Scale -= delta;
        }

        private void StartManipulation(ManipulationAxis axis, ScreenSpaceVisual3D line, Point mousePosition)
        {
            this.manipulationLastPosition.Clear();
            this.Manipulating = true;
            this.manipulationAxis = axis;

            for (var index = 0; index < this.SelectedContent.Count; index++)
                this.manipulationLastPosition[index] = this.GetMousePoint(mousePosition, this.SelectedContent[index]);

            line.Color = SharedMaterials.Selection;
            this.Viewport.Viewport.CaptureMouse();
        }

        private void Select(ContentBase content, bool toggle)
        {
            if (!toggle && this.SelectedContent.Any(x => x == content))
            {
                if (this.ViewerType == ViewerType.Universe)
                {
                    if (content is System system)
                    {
                        this.DisplayContextMenu(system.Path);
                    }
                }
            }

            this.OnSelectionChanged(content.Block, toggle);
        }

        private void DisplayContextMenu(string path)
        {
            ContextMenu menu = new ContextMenu();
            MenuItem item = new MenuItem
                {
                    Header = string.Format(Strings.SystemPresenterOpen, Path.GetFileName(path)),
                    Tag = path
                };
            item.Click += this.ItemClick;

            menu.Items.Add(item);
            menu.IsOpen = true;
        }

        private void ItemClick(object sender, RoutedEventArgs e)
        {
            this.OnFileOpen((string)((MenuItem)sender).Tag);
        }

        internal void SetSelectionBox()
        {
            if (this.SelectedContent.Count == 0)
            {
                this.SelectionBox = null;
                return;
            }

            Color color = SharedMaterials.Selection;

            if (this.trackedContent == this.SelectedContent[0]) 
                color = SharedMaterials.TrackedLine;

            if (this.SelectionBox == null)
                this.SelectionBox = new BoundingBoxWireFrameVisual3D();

            // Initial boundary for first object (don't init empty Rect3D as its extents are at zero)
            Rect3D bounds = Visual3DHelper.FindBounds(this.SelectedContent[0], Transform3D.Identity);

            // Add all other objects in selection
            for (int i = 1; i < this.SelectedContent.Count; i++)
                bounds.Union(Visual3DHelper.FindBounds(this.SelectedContent[i], Transform3D.Identity));

            this.SelectionBox.Color = color;
            this.SelectionBox.BoundingBox = bounds;
            this.SelectionBox.Transform = Transform3D.Identity;
        }

        internal void SetTrackedLine(bool update)
        {
            if (this.SelectedContent.Count == 0)
            {
                this.TrackedLine = null;
                return;
            }

            if (!update)
            {
                BoundingBoxWireFrameVisual3D selectionBox = this.SelectionBox;
                if (this.trackedContent == this.SelectedContent[0])
                {
                    selectionBox.Color = SharedMaterials.TrackedLine;
                }
                else
                {
                    selectionBox.Color = SharedMaterials.Selection;
                }
            }

            if (this.trackedContent == null)
            {
                this.TrackedLine = null;
                return;
            }

            if (this.TrackedLine != null)
            {
                LineVisual3D trackedLine = this.TrackedLine;
                trackedLine.Point1 = this.SelectedContent[0].GetPositionPoint();
                trackedLine.Point2 = this.trackedContent.GetPositionPoint();
            }
            else
            {
                this.TrackedLine = new LineVisual3D
                    {
                        Point1 = this.SelectedContent[0].GetPositionPoint(),
                        Point2 = this.trackedContent.GetPositionPoint(),
                        Color = SharedMaterials.TrackedLine,
                        DepthOffset = 1,
                    };
            }
        }

        public void SetManipulatorLines()
        {
            if (this.ViewerType != ViewerType.System && this.ViewerType != ViewerType.Universe)
                return;

            if (this.SelectedContent.Count == 0 || this.manipulationMode == ManipulationMode.None)
            {
                this.ManipulatorLineX = null;
                this.ManipulatorLineY = null;
                this.ManipulatorLineZ = null;
                return;
            }

            if (this.ViewerType == ViewerType.Universe && this.manipulationMode != ManipulationMode.Translate)
                return;

            int axisCount = 3;
            if (this.manipulationMode == ManipulationMode.Scale || this.ViewerType == ViewerType.Universe)
            {
                axisCount = GetAxisCount(this.SelectedContent[0].Block.ObjectType);
                if (axisCount == 0)
                {
                    this.ManipulatorLineX = null;
                    this.ManipulatorLineY = null;
                    this.ManipulatorLineZ = null;
                    return;
                }
            }

            Matrix3D matrix = new Matrix3D();

            // scale used as workaround for fixed screen space line visual glitches
            matrix.Scale(new Vector3D(0.0001, 0.0001, 0.0001));
            matrix *= ContentBase.RotationMatrix(this.SelectedContent[0].Rotation);
            matrix.Translate(this.SelectedContent[0].Position);

            Transform3D transform = new MatrixTransform3D(matrix);

            if (axisCount >= 3)
            {
                if (this.ManipulatorLineY != null)
                {
                    FixedLineVisual3D manipulatorLine = this.ManipulatorLineY;
                    manipulatorLine.Point2 = new Point3D(0, 0, 1);
                    manipulatorLine.Transform = transform;
                }
                else
                {
                    this.ManipulatorLineY = new FixedLineVisual3D
                                                {
                                                    Point2 = new Point3D(0, 0, 1),
                                                    Transform = transform,
                                                    Color = SharedMaterials.ManipulatorY,
                                                    Thickness = 5,
                                                    DepthOffset = 0.5,
                                                    FixedLength = 100,
                                                };
                }
            }
            else
            {
                this.ManipulatorLineY = null;
            }

            if (axisCount >= 2)
            {
                if (this.ManipulatorLineZ != null)
                {
                    FixedLineVisual3D manipulatorLine = this.ManipulatorLineZ;
                    manipulatorLine.Point2 = new Point3D(0, 1, 0);
                    manipulatorLine.Transform = transform;
                }
                else
                {
                    this.ManipulatorLineZ = new FixedLineVisual3D
                                                {
                                                    Point2 = new Point3D(0, 1, 0),
                                                    Transform = transform,
                                                    Color = SharedMaterials.ManipulatorZ,
                                                    Thickness = 5,
                                                    DepthOffset = 0.5,
                                                    FixedLength = 100,
                                                };
                }
            }
            else
            {
                this.ManipulatorLineZ = null;
            }

            if (this.ManipulatorLineX != null)
            {
                FixedLineVisual3D manipulatorLine = this.ManipulatorLineX;
                manipulatorLine.Point2 = new Point3D(1, 0, 0);
                manipulatorLine.Transform = transform;
            }
            else
            {
                this.ManipulatorLineX = new FixedLineVisual3D
                                            {
                                                Point2 = new Point3D(1, 0, 0),
                                                Transform = transform,
                                                Color = SharedMaterials.ManipulatorX,
                                                Thickness = 5,
                                                DepthOffset = 0.5,
                                                FixedLength = 100,
                                            };
            }
        }

        private static int GetAxisCount(ContentType type)
        {
            switch (type)
            {
                case ContentType.ZoneSphere:
                case ContentType.ZoneSphereExclusion:
                case ContentType.ZoneVignette:
                    return 1;
                case ContentType.ZonePath:
                case ContentType.ZonePathTrade:
                case ContentType.ZoneRing: // move back to 3 axis if we support ring mesh
                case ContentType.System:
                    return 2;
                case ContentType.ZoneEllipsoid:
                case ContentType.ZoneEllipsoidExclusion:
                case ContentType.ZoneCylinder:
                case ContentType.ZoneCylinderExclusion:
                case ContentType.ZoneBox:
                case ContentType.ZoneBoxExclusion:
                case ContentType.ZonePathTradeLane:
                    return 3;
                default:
                    return 0;
            }
        }

        private Rect3D GetBounds(ContentBase content)
        {
            if (this.IsModelMode && content.Block.IsRealModel())
            {
                // return bounds of shown model
                if (content.Block.Visibility && content.Content != null)
                {
                    return content.Content.Bounds;
                }

                // try to find bounds of cached model
                if (content.Block.Archetype != null)
                {
                    Model3D contentModel;
                    if (this.modelCache.TryGetValue(content.Block.Archetype.ModelPath, out contentModel))
                    {
                        return contentModel.Bounds;
                    }
                }
            }

            // return shape model by default
            return content.GetShapeBounds();
        }

        public void ClearDisplay(bool light)
        {
            this.Viewport.Children.Clear();

            if (light || this.Lighting == null)
            {
                this.secondLayerId = 0;
            }
            else
            {
                this.Viewport.Children.Add(this.Lighting);
                this.secondLayerId = 1;
            }
        }

        public int GetContentStartId()
        {
            int index = 0;
            if (this.Lighting != null)
            {
                ++index;
            }

            if (this.SelectionBox != null)
            {
                ++index;
            }

            if (this.TrackedLine != null)
            {
                ++index;
            }

            if (this.ManipulatorLineX != null)
            {
                ++index;
            }

            if (this.ManipulatorLineY != null)
            {
                ++index;
            }

            if (this.ManipulatorLineZ != null)
            {
                ++index;
            }

            return index;
        }

        public void DisplayUniverse(string path, int systemTemplate, List<TableBlock> blocks, ArchetypeManager archetype)
        {
            // filter the systems to improve speed as we need to loop them often in the analyzer
            List<TableBlock> systems = new List<TableBlock>();
            foreach (TableBlock block in blocks)
            {
                if (block.ObjectType == ContentType.System)
                {
                    systems.Add(block);
                }
            }

            Analyzer analyzer = new Analyzer
                {
                    Universe = systems,
                    UniversePath = path,
                    SystemTemplate = systemTemplate,
                    Archetype = archetype
                };
            analyzer.Analyze();

            this.DisplayUniverseConnections(analyzer.Connections);
        }

        private void DisplayUniverseConnections(Dictionary<int, UniverseConnection> connections)
        {
            this.Viewport.Dispatcher.Invoke((MethodInvoker)(delegate
                {
                    foreach (UniverseConnection connection in connections.Values)
                    {
                        this.Viewport.Children.Add(this.GetConnection(connection));
                    }
                }));
        }

        private void DeleteConnections(System system)
        {
            foreach (Connection connection in system.Connections)
            {
                this.Delete(connection);
            }
        }

        private void UpdateConnections(System system)
        {
            foreach (Connection connection in system.Connections)
            {
                SetConnection(connection);
            }
        }

        private Connection GetConnection(UniverseConnection connection)
        {
            Connection line = new Connection();
            this.SetConnection(line, connection);

            line.LoadModel();
            return line;
        }

        private void SetConnection(Connection line, UniverseConnection connection)
        {
            int count = 2;

            for (int i = this.GetContentStartId(); i < this.Viewport.Children.Count && count > 0; ++i)
            {
                ContentBase content = (ContentBase)this.Viewport.Children[i];
                if (content.Block.Index == connection.From.Id)
                {
                    line.From = content;
                    --count;
                }
                else if (content.Block.Index == connection.To.Id)
                {
                    line.To = content;
                    --count;
                }
            }

            line.FromType = GetConnectionType(connection.From.JumpGate, connection.From.JumpHole);
            line.ToType = GetConnectionType(connection.To.JumpGate, connection.To.JumpHole);

            ((Content.System)line.From).Connections.Add(line);
            ((Content.System)line.To).Connections.Add(line);

            SetConnection(line);
        }

        private static void SetConnection(Connection line)
        {
            Vector3D fromPosition = line.From.Position;
            Vector3D toPosition = line.To.Position;

            line.Position = (fromPosition + toPosition) / 2;
            line.Scale = new Vector3D(SystemParser.UniverseConnectionScale, (fromPosition - toPosition).Length, 1);

            if (line.FromType == ConnectionType.JumpGateAndHole || line.ToType == ConnectionType.JumpGateAndHole)
            {
                line.Scale.X = SystemParser.UniverseDoubleConnectionScale;
            }

            Vector v1 = new Vector(fromPosition.X, fromPosition.Y);
            Vector v2 = new Vector(toPosition.X, toPosition.Y);

            double a = Difference(v2.X, v1.X);
            double b = Difference(v2.Y, v1.Y);
            double factor = 1;
            double angleOffset = 90;

            if (v2.X < v1.X)
            {
                factor = -1;
            }

            if (v2.Y < v1.Y)
            {
                angleOffset = -90;
                factor *= -1;
            }

            const double RadToDeg = 180 / Math.PI;

            double c = Math.Sqrt(a * a + b * b);
            double angle = Math.Acos(a / c) * RadToDeg;

            line.Rotation = new Vector3D(0, 0, (angle + angleOffset) * factor);
            line.UpdateTransform(false);
        }

        private static ConnectionType GetConnectionType(bool jumpgate, bool jumphole)
        {
            if (jumpgate && jumphole)
            {
                return ConnectionType.JumpGateAndHole;
            }

            if (jumpgate)
            {
                return ConnectionType.JumpGate;
            }

            if (jumphole)
            {
                return ConnectionType.JumpHole;
            }

            return ConnectionType.None;
        }

        private static double Difference(double x, double y)
        {
            if (x > y)
            {
                return x - y;
            }

            return y - x;
        }

        public void ChangeValues(ContentBase content, TableBlock block)
        {
            if (block.ObjectType == ContentType.None)
            {
                // delete content if it was changed back to an invalid type
                this.Delete(content);
                int index = this.SelectedContent.IndexOf(content);
                if (index != -1)
                    ContentBaseList.RemoveAt(this, index);
            }
            else
            {
                this.SetValues(content, block);
            }
        }

        private void SetValues(ContentBase content, TableBlock block)
        {
            bool modelChanged;
            if (this.ViewerType == ViewerType.SolarArchetype || this.ViewerType == ViewerType.ModelPreview)
            {
                modelChanged = SystemParser.SetModelPreviewValues(content, block);
            }
            else
            {
                modelChanged = SystemParser.SetValues(content, block, true);
            }

            if (modelChanged && content.Content != null)
            {
                this.LoadModel(content);
            }

            if (this.ViewerType == ViewerType.Universe)
            {
                System system = content as Content.System;
                if (system != null)
                {
                    this.UpdateConnections(system);
                }
            }

            int index = this.SelectedContent.IndexOf(content);
            if (index != -1)
                // update selection if changed content is selected
                ContentBaseList.EditAt(this, content, index);
        }

        private Model3D LoadModel(string modelPath)
        {
            string extension = Path.GetExtension(modelPath);

            if (extension != null &&
                (extension.Equals(".cmp", StringComparison.OrdinalIgnoreCase) ||
                 extension.Equals(".3db", StringComparison.OrdinalIgnoreCase)))
            {
                // try to get the cached model
                Model3D contentModel;
                if (this.ViewerType == ViewerType.System && this.modelCache.TryGetValue(modelPath, out contentModel))
                {
                    return contentModel;
                }

                string file = Path.Combine(this.DataPath, modelPath);
                if (File.Exists(file))
                {
                    contentModel = UtfModel.LoadModel(file);

                    // cache model
                    if (this.ViewerType == ViewerType.System)
                    {
                        this.modelCache[modelPath] = contentModel;
                    }

                    return contentModel;
                }
            }

            return null;
        }

        private void LoadModel(ContentBase content)
        {
            if (this.IsModelMode && content.Block.IsRealModel())
            {
                if (content.Block.Archetype != null && content.Block.Archetype.ModelPath != null)
                {
                    content.Content = this.LoadModel(content.Block.Archetype.ModelPath);

                    // return if model was loaded successfully
                    if (content.Content != null)
                    {
                        return;
                    }
                }
            }

            content.LoadModel();
        }

        public void ReloadModels()
        {
            for (int i = this.GetContentStartId(); i < this.Viewport.Children.Count; ++i)
            {
                ContentBase content = (ContentBase)this.Viewport.Children[i];
                if (content.Block.IsRealModel())
                {
                    this.LoadModel(content);
                }
            }

            if (this.SelectedContent.Count != -1 && this.SelectedContent.Any(x => x.Block.IsRealModel()))
                ContentBaseList.PresentSelect(this);
        }

        private ContentBase CreateContent(TableBlock block)
        {
            ContentBase content = CreateContent(block.ObjectType);
            if (content == null)
                return null;

            this.SetValues(content, block);
            return content;
        }

        public static ContentBase CreateContent(ContentType type)
        {
            switch (type)
            {
                case ContentType.None:
                    return null;
                case ContentType.System:
                    return new Content.System();
                case ContentType.LightSource:
                    return new LightSource();
                case ContentType.Construct:
                case ContentType.Depot:
                case ContentType.DockingRing:
                case ContentType.JumpGate:
                case ContentType.JumpHole:
                case ContentType.Planet:
                case ContentType.Satellite:
                case ContentType.Ship:
                case ContentType.Station:
                case ContentType.Sun:
                case ContentType.TradeLane:
                case ContentType.WeaponsPlatform:
                case ContentType.ModelPreview:
                    return new SystemObject();
                default: // zone
                    return new Zone();
            }
        }

        private void AddOrReplace(Visual3D visual, Visual3D value)
        {
            int index = this.Viewport.Children.IndexOf(visual);
            if (index != -1)
            {
                if (value != null)
                {
                    this.Viewport.Children[index] = value;
                }
                else
                {
                    this.Viewport.Children.RemoveAt(index);
                    --this.secondLayerId;
                }
            }
            else if (value != null)
            {
                this.Viewport.Children.Insert(0, value);
                ++this.secondLayerId;
            }
        }

        private void AddOrReplace(ScreenSpaceVisual3D visual, ScreenSpaceVisual3D value)
        {
            if (visual != null && value == null)
            {
                visual.StopRendering();
            }

            this.AddOrReplace(visual, (Visual3D)value);

            if (visual == null)
            {
                value?.StartRendering();
            }
        }

        public void SetTitle()
        {
            if (this.SelectedContent.Count == 0)
            {
                this.Viewport.Title = null;
                return;
            }

            Helper.String.StringBuilder.Length = 0;
            Helper.String.StringBuilder.AppendLine(this.SelectedContent[0].Block.Name);

            if (this.trackedContent != null && this.trackedContent != this.SelectedContent[0])
            {
                this.AddTrackInfo();
            }

            this.Viewport.Title = Helper.String.StringBuilder.ToString();
        }

        private void AddTrackInfo()
        {
            Helper.String.StringBuilder.AppendLine();
            Helper.String.StringBuilder.AppendLine(this.trackedContent.Block.Name);

            Vector3D a = this.SelectedContent[0].Position / SystemParser.SystemScale;
            Vector3D b = this.trackedContent.Position / SystemParser.SystemScale;
            Vector3D delta = b - a;

            Helper.String.StringBuilder.Append(Strings.SystemPresenterTrackedDistance);
            Helper.String.StringBuilder.Append(Math.Round(delta.Length));

            Helper.String.StringBuilder.AppendLine();
            Helper.String.StringBuilder.Append(Strings.SystemPresenterTrackedAngles);

            const double RadToDeg = 180 / Math.PI;

            if (delta.Y == 0.0)
            {
                Helper.String.StringBuilder.Append("0, 0, ");
            }
            else
            {
                Helper.String.StringBuilder.Append(Math.Round(-Math.Atan(delta.Z / delta.Y) * RadToDeg));
                Helper.String.StringBuilder.Append(", ");
                Helper.String.StringBuilder.Append(Math.Round(-Math.Atan(delta.X / delta.Y) * RadToDeg));
                Helper.String.StringBuilder.Append(", ");
            }

            if (delta.X == 0.0)
            {
                Helper.String.StringBuilder.Append('0');
            }
            else
            {
                Helper.String.StringBuilder.Append(Math.Round(Math.Atan(delta.Z / delta.X) * RadToDeg));
            }
        }

        public ContentBase FindContent(TableBlock block)
        {
            for (int i = this.GetContentStartId(); i < this.Viewport.Children.Count; ++i)
            {
                ContentBase content = (ContentBase)this.Viewport.Children[i];
                if (content.Block != null && content.Block.Id == block.Id)
                {
                    return content;
                }
            }

            return null;
        }

        public void LookAtSelected()
        {
            if (this.SelectedContent.Count == 0)
            {
                return;
            }

            this.LookAt(this.SelectedContent[0]);
        }

        public void FocusSelected()
        {
            if (this.SelectedContent.Count == 0)
            {
                return;
            }

            double zoomFactor;
            switch (this.ViewerType)
            {
                case ViewerType.Universe:
                    zoomFactor = 20;
                    break;
                case ViewerType.System:
                    zoomFactor = 1.75;
                    break;
                default:
                    zoomFactor = 1.25;
                    break;
            }

            this.LookAtAndZoom(this.SelectedContent[0], zoomFactor, true);
        }

        public void TrackSelected()
        {
            if (this.ViewerType != ViewerType.System || this.SelectedContent.Count == 0)
            {
                return;
            }

            // change tracked object
            if (this.SelectedContent[0] == this.TrackedContent)
            {
                this.TrackedContent = null;
            }
            else
            {
                this.TrackedContent = this.SelectedContent[0];
            }
        }
    }
}
