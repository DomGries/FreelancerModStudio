using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using FreelancerModStudio.Data;
using FreelancerModStudio.Properties;
using FreelancerModStudio.SystemPresenter.Content;
using HelixEngine;
using ContextMenu = System.Windows.Controls.ContextMenu;
using MenuItem = System.Windows.Controls.MenuItem;

namespace FreelancerModStudio.SystemPresenter
{
    public class Presenter
    {
        public HelixViewport3D Viewport;
        public ViewerType ViewerType;
        public bool IsModelMode;
        public string DataPath;

        int _secondLayerId;
        readonly Dictionary<string, Model3D> _modelCache = new Dictionary<string, Model3D>(StringComparer.OrdinalIgnoreCase);

        Visual3D _lighting;
        BoundingBoxWireFrameVisual3D _selectionBox;
        LineVisual3D _trackedLine;
        ContentBase _selectedContent;
        ContentBase _trackedContent;

        FixedLineVisual3D _manipulatorX;
        FixedLineVisual3D _manipulatorY;
        FixedLineVisual3D _manipulatorZ;

        bool _manipulating;
        ManipulationMode _manipulationMode;
        ManipulationAxis _manipulationAxis;
        Point3D? _manipulationLastPosition;

        public delegate void SelectionChangedType(TableBlock block, bool toggle);

        public SelectionChangedType SelectionChanged;

        void OnSelectionChanged(TableBlock block, bool toggle)
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(block, toggle);
            }
        }

        public delegate void FileOpenType(string file);

        public FileOpenType FileOpen;

        void OnFileOpen(string file)
        {
            if (FileOpen != null)
            {
                FileOpen(file);
            }
        }

        public delegate void DataManipulatedType(TableBlock newBlock, TableBlock oldBlock);

        public DataManipulatedType DataManipulated;

        void OnDataManipulated(TableBlock newBlock, TableBlock oldBlock)
        {
            if (DataManipulated != null)
            {
                DataManipulated(newBlock, oldBlock);
            }
        }

        public Visual3D Lighting
        {
            get
            {
                return _lighting;
            }
            set
            {
                AddOrReplace(_lighting, value);
                _lighting = value;
            }
        }

        public BoundingBoxWireFrameVisual3D SelectionBox
        {
            get
            {
                return _selectionBox;
            }
            set
            {
                AddOrReplace(_selectionBox, value);
                _selectionBox = value;
            }
        }

        public LineVisual3D TrackedLine
        {
            get
            {
                return _trackedLine;
            }
            set
            {
                AddOrReplace(_trackedLine, value);
                _trackedLine = value;
            }
        }

        public FixedLineVisual3D ManipulatorLineX
        {
            get
            {
                return _manipulatorX;
            }
            set
            {
                AddOrReplace(_manipulatorX, value);
                _manipulatorX = value;
            }
        }

        public FixedLineVisual3D ManipulatorLineY
        {
            get
            {
                return _manipulatorY;
            }
            set
            {
                AddOrReplace(_manipulatorY, value);
                _manipulatorY = value;
            }
        }

        public FixedLineVisual3D ManipulatorLineZ
        {
            get
            {
                return _manipulatorZ;
            }
            set
            {
                AddOrReplace(_manipulatorZ, value);
                _manipulatorZ = value;
            }
        }

        public ContentBase SelectedContent
        {
            get
            {
                return _selectedContent;
            }
            set
            {
                if (_manipulating && _selectedContent != value)
                {
                    StopManipulating(true);
                }
                _selectedContent = value;

                //select content visually
                SetSelectionBox();
                SetTrackedLine(true);
                SetManipulatorLines();
                SetTitle();
            }
        }

        public ContentBase TrackedContent
        {
            get
            {
                return _trackedContent;
            }
            set
            {
                _trackedContent = value;

                //track content visually
                SetTrackedLine(false);
                SetTitle();
            }
        }

        public ManipulationMode ManipulationMode
        {
            get
            {
                return _manipulationMode;
            }
            set
            {
                if (_manipulating && value == ManipulationMode.None)
                {
                    StopManipulating(false);
                }

                _manipulationMode = value;

                SetManipulatorLines();
            }
        }

        public Presenter(HelixViewport3D viewport)
        {
            Viewport = viewport;
            Viewport.MouseDown += Viewport_MouseDown;
            Viewport.MouseUp += Viewport_MouseUp;
            Viewport.MouseMove += Viewport_MouseMove;

            Viewport.ViewCubeLeftBrush = new SolidColorBrush(SharedMaterials.ManipulatorZ);
            Viewport.ViewCubeRightBrush = Viewport.ViewCubeLeftBrush;
            Viewport.ViewCubeTopBrush = new SolidColorBrush(SharedMaterials.ManipulatorY);
            Viewport.ViewCubeBottomBrush = Viewport.ViewCubeTopBrush;
            Viewport.ViewCubeFrontBrush = new SolidColorBrush(SharedMaterials.ManipulatorX);
            Viewport.ViewCubeBackBrush = Viewport.ViewCubeFrontBrush;
            Viewport.ViewCubeLeftText = "F";
            Viewport.ViewCubeRightText = "B";
            Viewport.ViewCubeFrontText = "R";
            Viewport.ViewCubeBackText = "L";

            Lighting = new SystemLightsVisual3D();
        }

        public void LookAt(ContentBase content)
        {
            Viewport.LookAt(content.GetPositionPoint(), Animator.AnimationDuration.TimeSpan.TotalMilliseconds);
        }

        public void LookAt(Point3D point)
        {
            Viewport.LookAt(point, Animator.AnimationDuration.TimeSpan.TotalMilliseconds);
        }

        public void LookAtAndZoom(ContentBase content, double zoomFactor, bool animate)
        {
            Rect3D bounds = GetBounds(content);
            Matrix3D matrix = content.Transform.Value;

            // prepend translation to account for model scale
            matrix.TranslatePrepend(new Vector3D(bounds.X + bounds.SizeX * 0.5f, bounds.Y + bounds.SizeY * 0.5f, bounds.Z + bounds.SizeZ * 0.5f));
            Point3D point = new Point3D(matrix.OffsetX, matrix.OffsetY, matrix.OffsetZ);

            // get distance based on model transform
            matrix.TranslatePrepend(new Vector3D(bounds.SizeX, bounds.SizeY, bounds.SizeZ));
            double distance = Math.Max(Math.Max(Math.Abs(matrix.OffsetX - point.X), Math.Abs(matrix.OffsetY - point.Y)), Math.Abs(matrix.OffsetZ - point.Z));

            Viewport.ZoomExtents(point, distance * 0.5 * zoomFactor, animate ? Animator.AnimationDuration.TimeSpan.TotalMilliseconds : 0);
        }

        void AddContent(ContentBase content)
        {
            // load model if it was not loaded yet
            if (content.Content == null)
            {
                LoadModel(content);
            }

            AddModel(content);

            // reset reference of previously invisible selected content
            if (_selectedContent != null && content.Block == _selectedContent.Block)
            {
                SelectedContent = content;
            }
        }

        public void Add(List<TableBlock> blocks)
        {
            Animator.AnimationDuration = new Duration(TimeSpan.Zero);

            foreach (TableBlock block in blocks)
            {
                if (block.Visibility)
                {
                    AddBlock(block);
                }
            }

            Animator.AnimationDuration = new Duration(TimeSpan.FromMilliseconds(500));
        }

        public void Add(TableBlock block)
        {
            Animator.AnimationDuration = new Duration(TimeSpan.Zero);

            AddBlock(block);

            Animator.AnimationDuration = new Duration(TimeSpan.FromMilliseconds(500));
        }

        void AddBlock(TableBlock block)
        {
            ContentBase content = CreateContent(block);
            if (content != null)
            {
                AddContent(content);
            }
        }

        void AddModel(ContentBase content)
        {
            if (content.IsEmissive())
            {
                Viewport.Children.Add(content);
            }
            else
            {
                Viewport.Children.Insert(_secondLayerId, content);
                ++_secondLayerId;
            }
        }

        public void Delete(ContentBase content)
        {
            //if we delete a system also delete all universe connections to and from it
            if (ViewerType == ViewerType.Universe)
            {
                Content.System system = content as Content.System;
                if (system != null)
                {
                    DeleteConnections(system);
                }
            }

            RemoveModel(content);
        }

        void RemoveModel(ContentBase content)
        {
            Viewport.Children.Remove(content);

            if (!content.IsEmissive())
            {
                --_secondLayerId;
            }
            if (content == _trackedContent)
            {
                TrackedContent = null;
            }
        }

        static int GetSelectionPriority(ContentBase content)
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

        public ContentBase GetSelection(Point position, bool farthest, bool checkManipulators)
        {
            PointHitTestParameters hitParams = new PointHitTestParameters(position);

            ContentBase visual = null;
            double distance = 0.0;
            int selectionPriority = -1;

            VisualTreeHelper.HitTest(
                Viewport.Viewport,
                null,
                delegate(HitTestResult hit)
                {
                    RayMeshGeometry3DHitTestResult rayHit = hit as RayMeshGeometry3DHitTestResult;
                    if (rayHit != null)
                    {
                        if (checkManipulators)
                        {
                            // start manipulation on manipulator selection
                            if (rayHit.VisualHit == _manipulatorX)
                            {
                                StartManipulation(ManipulationAxis.X, _manipulatorX, position);
                                visual = null;
                                return HitTestResultBehavior.Stop;
                            }
                            if (rayHit.VisualHit == _manipulatorY)
                            {
                                StartManipulation(ManipulationAxis.Y, _manipulatorY, position);
                                visual = null;
                                return HitTestResultBehavior.Stop;
                            }
                            if (rayHit.VisualHit == _manipulatorZ)
                            {
                                StartManipulation(ManipulationAxis.Z, _manipulatorZ, position);
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
                            GeneralTransform3D t = Viewport3DHelper.GetTransform(Viewport.Viewport, rayHit.VisualHit);
                            if (t != null)
                            {
                                p = t.Transform(p);
                            }

                            double newDistance = (Viewport.Camera.Position - p).LengthSquared;
                            int newSelectionPriority = GetSelectionPriority(newHit);

                            if (newSelectionPriority > selectionPriority ||
                                (newSelectionPriority == selectionPriority &&
                                 (farthest ? newDistance > distance : newDistance < distance)))
                            {
                                visual = newHit;
                                distance = newDistance;
                                selectionPriority = newSelectionPriority;
                            }
                        }
                    }

                    return HitTestResultBehavior.Continue;
                },
                hitParams);

            return visual;
        }

        void Viewport_MouseDown(object sender, MouseButtonEventArgs e)
        {
            bool isLookAt = e.ChangedButton == MouseButton.Right && e.ClickCount == 2;

            if (e.ChangedButton == MouseButton.Left || isLookAt)
            {
                e.Handled = true;

                bool isShiftDown = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
                bool isCtrlDown = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

                ContentBase visual = GetSelection(e.GetPosition(Viewport.Viewport), isShiftDown, !isLookAt);
                if (visual != null)
                {
                    if (isLookAt)
                    {
                        // change the 'lookat' point
                        LookAt(visual);
                    }
                    else
                    {
                        Select(visual, isCtrlDown);
                    }
                }
            }
        }

        void Viewport_MouseUp(object sender, MouseButtonEventArgs e)
        {
            StopManipulating(false);
        }

        void StopManipulating(bool cancelled)
        {
            if (!_manipulating)
            {
                return;
            }

            switch (_manipulationAxis)
            {
                case ManipulationAxis.X:
                    ManipulatorLineX.Color = SharedMaterials.ManipulatorX;
                    break;
                case ManipulationAxis.Y:
                    ManipulatorLineY.Color = SharedMaterials.ManipulatorY;
                    break;
                case ManipulationAxis.Z:
                    ManipulatorLineZ.Color = SharedMaterials.ManipulatorZ;
                    break;
            }

            if (cancelled)
            {
                SystemParser.SetValues(_selectedContent, _selectedContent.Block, true);
            }
            else
            {
                // update data globally
                TableBlock oldBlock = _selectedContent.Block;
                TableBlock newBlock = ObjectClone.Clone(oldBlock);
                newBlock.SetModifiedChanged();

                _selectedContent.Block = newBlock;
                SystemParser.WriteBlock(_selectedContent);
                OnDataManipulated(newBlock, oldBlock);
            }

            // stop manipulating
            _manipulating = false;
            Viewport.Viewport.ReleaseMouseCapture();
        }

        Point3D? GetMousePoint(Point mousePosition)
        {
            return Viewport.CameraController.UnProject(mousePosition, Viewport.CameraController.CameraTarget(), Viewport.CameraController.Camera.LookDirection);
        }

        Vector3D GetMouseDelta(Point mousePosition)
        {
            // get mouse delta
            Point3D? thisPoint3D = GetMousePoint(mousePosition);
            Vector3D delta3D = thisPoint3D.Value - _manipulationLastPosition.Value;
            _manipulationLastPosition = thisPoint3D;

            // transform mouse delta using matrix
            Matrix3D matrix = ContentBase.RotationMatrix(_selectedContent.Rotation);
            matrix.Invert();
            delta3D = matrix.Transform(delta3D);

            if (_manipulationMode == ManipulationMode.Rotate)
            {
                double length = delta3D.Length;
                if (length > 0)
                {
                    delta3D /= length;
                    delta3D *= 2;
                }
            }
            else if (_manipulationMode == ManipulationMode.Scale)
            {
                if (ManipulatorLineY == null)
                {
                    if (ManipulatorLineZ == null)
                    {
                        return new Vector3D(delta3D.X, delta3D.X, delta3D.X);
                    }

                    if (_manipulationAxis == ManipulationAxis.X)
                    {
                        return new Vector3D(delta3D.X, 0, delta3D.X);
                    }
                }
            }

            switch (_manipulationAxis)
            {
                default:
                    return new Vector3D(delta3D.X, 0, 0);
                case ManipulationAxis.Y:
                    return new Vector3D(0, 0, delta3D.Z);
                case ManipulationAxis.Z:
                    return new Vector3D(0, delta3D.Y, 0);
            }
        }

        void Viewport_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_manipulating)
            {
                return;
            }

            switch (_manipulationMode)
            {
                case ManipulationMode.Translate:
                    ManipulateTranslate(GetMouseDelta(e.GetPosition(Viewport.Viewport)));
                    break;
                case ManipulationMode.Rotate:
                    ManipulateRotate(GetMouseDelta(e.GetPosition(Viewport.Viewport)));
                    break;
                case ManipulationMode.Scale:
                    ManipulateScale(GetMouseDelta(e.GetPosition(Viewport.Viewport)));
                    break;
            }

            // update selection box and tracked line
            SelectedContent = _selectedContent;
        }

        void ManipulateTranslate(Vector3D delta)
        {
            // calculate using matrix
            Matrix3D matrix = ContentBase.RotationMatrix(_selectedContent.Rotation);
            matrix.Translate(_selectedContent.Position);
            matrix.TranslatePrepend(delta);

            // update position
            _selectedContent.Position = new Vector3D(matrix.OffsetX, matrix.OffsetY, matrix.OffsetZ);

            // update transform
            _selectedContent.UpdateTransform(false);
        }

        void ManipulateRotate(Vector3D delta)
        {
            // calculate using matrix
            Matrix3D matrix = ContentBase.RotationMatrix(_selectedContent.Rotation);
            matrix.Prepend(ContentBase.RotationMatrix(delta));

            // update rotation
            _selectedContent.Rotation = ContentBase.GetRotation(matrix);

            // update transform
            _selectedContent.UpdateTransform(false);
        }

        void ManipulateScale(Vector3D delta)
        {
            // update position
            _selectedContent.Scale += delta;

            // prevent negative scaling
            const double minScale = 10 * SystemParser.SIZE_FACTOR;

            if (_selectedContent.Scale.X < minScale ||
                _selectedContent.Scale.Y < minScale ||
                _selectedContent.Scale.Z < minScale)
            {
                _selectedContent.Scale -= delta;
                return;
            }

            // update transform
            _selectedContent.UpdateTransform(false);
        }

        void StartManipulation(ManipulationAxis axis, ScreenSpaceVisual3D line, Point mousePosition)
        {
            _manipulating = true;
            _manipulationAxis = axis;
            _manipulationLastPosition = GetMousePoint(mousePosition);

            line.Color = SharedMaterials.Selection;

            Viewport.Viewport.CaptureMouse();
        }

        void Select(ContentBase content, bool toggle)
        {
            if (!toggle && _selectedContent == content)
            {
                if (ViewerType == ViewerType.Universe)
                {
                    Content.System system = content as Content.System;
                    if (system != null)
                    {
                        DisplayContextMenu(system.Path);
                    }
                }
            }

            OnSelectionChanged(content.Block, toggle);
        }

        void DisplayContextMenu(string path)
        {
            ContextMenu menu = new ContextMenu();
            MenuItem item = new MenuItem
                {
                    Header = string.Format(Strings.SystemPresenterOpen, Path.GetFileName(path)),
                    Tag = path
                };
            item.Click += item_Click;

            menu.Items.Add(item);
            menu.IsOpen = true;
        }

        void item_Click(object sender, RoutedEventArgs e)
        {
            OnFileOpen((string)((MenuItem)sender).Tag);
        }

        void SetSelectionBox()
        {
            if (_selectedContent == null)
            {
                SelectionBox = null;
                return;
            }

            Color color = SharedMaterials.Selection;
            if (_trackedContent == _selectedContent)
            {
                color = SharedMaterials.TrackedLine;
            }

            if (SelectionBox != null)
            {
                BoundingBoxWireFrameVisual3D selectionBox = SelectionBox;
                selectionBox.Color = color;
                selectionBox.BoundingBox = GetBounds(_selectedContent);
                selectionBox.Transform = _selectedContent.Transform;
            }
            else
            {
                SelectionBox = new BoundingBoxWireFrameVisual3D
                    {
                        Color = color,
                        BoundingBox = GetBounds(_selectedContent),
                        Transform = _selectedContent.Transform,
                    };
            }
        }

        void SetTrackedLine(bool update)
        {
            if (_selectedContent == null)
            {
                TrackedLine = null;
                return;
            }

            if (!update)
            {
                BoundingBoxWireFrameVisual3D selectionBox = SelectionBox;
                if (_trackedContent == _selectedContent)
                {
                    selectionBox.Color = SharedMaterials.TrackedLine;
                }
                else
                {
                    selectionBox.Color = SharedMaterials.Selection;
                }
            }

            if (_trackedContent == null)
            {
                TrackedLine = null;
                return;
            }

            if (TrackedLine != null)
            {
                LineVisual3D trackedLine = TrackedLine;
                trackedLine.Point1 = _selectedContent.GetPositionPoint();
                trackedLine.Point2 = _trackedContent.GetPositionPoint();
            }
            else
            {
                TrackedLine = new LineVisual3D
                    {
                        Point1 = _selectedContent.GetPositionPoint(),
                        Point2 = _trackedContent.GetPositionPoint(),
                        Color = SharedMaterials.TrackedLine,
                        DepthOffset = 1,
                    };
            }
        }

        public void SetManipulatorLines()
        {
            if (ViewerType != ViewerType.System && ViewerType != ViewerType.Universe)
            {
                return;
            }

            if (_selectedContent == null || _manipulationMode == ManipulationMode.None)
            {
                ManipulatorLineX = null;
                ManipulatorLineY = null;
                ManipulatorLineZ = null;
                return;
            }

            if (ViewerType == ViewerType.Universe && _manipulationMode != ManipulationMode.Translate)
            {
                return;
            }

            int axisCount = 3;
            if (_manipulationMode == ManipulationMode.Scale || ViewerType == ViewerType.Universe)
            {
                axisCount = GetAxisCount(_selectedContent.Block.ObjectType);
                if (axisCount == 0)
                {
                    ManipulatorLineX = null;
                    ManipulatorLineY = null;
                    ManipulatorLineZ = null;
                    return;
                }
            }

            Matrix3D matrix = new Matrix3D();
            // scale used as workaround for fixed screen space line visual glitches
            matrix.Scale(new Vector3D(0.0001, 0.0001, 0.0001));
            matrix *= ContentBase.RotationMatrix(_selectedContent.Rotation);
            matrix.Translate(_selectedContent.Position);

            Transform3D transform = new MatrixTransform3D(matrix);

            if (axisCount >= 3)
            {
                if (ManipulatorLineY != null)
                {
                    FixedLineVisual3D manipulatorLine = ManipulatorLineY;
                    manipulatorLine.Point2 = new Point3D(0, 0, 1);
                    manipulatorLine.Transform = transform;
                }
                else
                {
                    ManipulatorLineY = new FixedLineVisual3D
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
                ManipulatorLineY = null;
            }

            if (axisCount >= 2)
            {
                if (ManipulatorLineZ != null)
                {
                    FixedLineVisual3D manipulatorLine = ManipulatorLineZ;
                    manipulatorLine.Point2 = new Point3D(0, 1, 0);
                    manipulatorLine.Transform = transform;
                }
                else
                {
                    ManipulatorLineZ = new FixedLineVisual3D
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
                ManipulatorLineZ = null;
            }

            if (ManipulatorLineX != null)
            {
                FixedLineVisual3D manipulatorLine = ManipulatorLineX;
                manipulatorLine.Point2 = new Point3D(1, 0, 0);
                manipulatorLine.Transform = transform;
            }
            else
            {
                ManipulatorLineX = new FixedLineVisual3D
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

        static int GetAxisCount(ContentType type)
        {
            switch (type)
            {
                case ContentType.ZoneSphere:
                case ContentType.ZoneSphereExclusion:
                case ContentType.ZoneVignette:
                    return 1;
                case ContentType.ZonePath:
                case ContentType.ZonePathTrade:
                case ContentType.System:
                    return 2;
                case ContentType.ZoneEllipsoid:
                case ContentType.ZoneEllipsoidExclusion:
                case ContentType.ZoneCylinder:
                case ContentType.ZoneCylinderExclusion:
                case ContentType.ZoneRing:
                case ContentType.ZoneBox:
                case ContentType.ZoneBoxExclusion:
                case ContentType.ZonePathTradeLane:
                    return 3;
                default:
                    return 0;
            }
        }

        Rect3D GetBounds(ContentBase content)
        {
            if (IsModelMode && content.Block.IsRealModel())
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
                    if (_modelCache.TryGetValue(content.Block.Archetype.ModelPath, out contentModel))
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
            Viewport.Children.Clear();

            if (light || Lighting == null)
            {
                _secondLayerId = 0;
            }
            else
            {
                Viewport.Children.Add(Lighting);
                _secondLayerId = 1;
            }
        }

        public int GetContentStartId()
        {
            int index = 0;
            if (Lighting != null)
            {
                ++index;
            }
            if (SelectionBox != null)
            {
                ++index;
            }
            if (TrackedLine != null)
            {
                ++index;
            }
            if (ManipulatorLineX != null)
            {
                ++index;
            }
            if (ManipulatorLineY != null)
            {
                ++index;
            }
            if (ManipulatorLineZ != null)
            {
                ++index;
            }

            return index;
        }

        public void DisplayUniverse(string path, int systemTemplate, List<TableBlock> blocks, ArchetypeManager archetype)
        {
            //filter the systems to improve speed as we need to loop them often in the analyzer
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

            DisplayUniverseConnections(analyzer.Connections);
        }

        void DisplayUniverseConnections(Dictionary<int, UniverseConnection> connections)
        {
            Viewport.Dispatcher.Invoke((MethodInvoker)(delegate
                {
                    foreach (UniverseConnection connection in connections.Values)
                    {
                        Viewport.Children.Add(GetConnection(connection));
                    }
                }));
        }

        void DeleteConnections(Content.System system)
        {
            foreach (Connection connection in system.Connections)
            {
                Delete(connection);
            }
        }

        void UpdateConnections(Content.System system)
        {
            foreach (Connection connection in system.Connections)
            {
                SetConnection(connection);
            }
        }

        Connection GetConnection(UniverseConnection connection)
        {
            Connection line = new Connection();
            SetConnection(line, connection);

            line.LoadModel();
            return line;
        }

        void SetConnection(Connection line, UniverseConnection connection)
        {
            int count = 2;

            for (int i = GetContentStartId(); i < Viewport.Children.Count && count > 0; ++i)
            {
                ContentBase content = (ContentBase)Viewport.Children[i];
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

        static void SetConnection(Connection line)
        {
            Vector3D fromPosition = line.From.Position;
            Vector3D toPosition = line.To.Position;

            line.Position = (fromPosition + toPosition) / 2;
            line.Scale = new Vector3D(SystemParser.UNIVERSE_CONNECTION_SCALE, (fromPosition - toPosition).Length, 1);

            if (line.FromType == ConnectionType.JumpGateAndHole || line.ToType == ConnectionType.JumpGateAndHole)
            {
                line.Scale.X = SystemParser.UNIVERSE_DOUBLE_CONNECTION_SCALE;
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

            const double radToDeg = 180 / Math.PI;

            double c = Math.Sqrt(a * a + b * b);
            double angle = Math.Acos(a / c) * radToDeg;

            line.Rotation = new Vector3D(0, 0, (angle + angleOffset) * factor);
            line.UpdateTransform(false);
        }

        static ConnectionType GetConnectionType(bool jumpgate, bool jumphole)
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

        static double Difference(double x, double y)
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
                //delete content if it was changed back to an invalid type
                Delete(content);
                if (_selectedContent == content)
                {
                    SelectedContent = null;
                }
            }
            else
            {
                SetValues(content, block);
            }
        }

        void SetValues(ContentBase content, TableBlock block)
        {
            bool modelChanged;
            if (ViewerType == ViewerType.SolarArchetype || ViewerType == ViewerType.ModelPreview)
            {
                modelChanged = SystemParser.SetModelPreviewValues(content, block);
            }
            else
            {
                modelChanged = SystemParser.SetValues(content, block, true);
            }

            if (modelChanged && content.Content != null)
            {
                LoadModel(content);
            }

            if (ViewerType == ViewerType.Universe)
            {
                Content.System system = content as Content.System;
                if (system != null)
                {
                    UpdateConnections(system);
                }
            }

            if (_selectedContent == content)
            {
                //update selection if changed content is selected
                SelectedContent = content;

                // set title when block name was changed in properties window
                SetTitle();
            }
        }

        Model3D LoadModel(string modelPath)
        {
            string extension = Path.GetExtension(modelPath);

            if (extension != null &&
                (extension.Equals(".cmp", StringComparison.OrdinalIgnoreCase) ||
                 extension.Equals(".3db", StringComparison.OrdinalIgnoreCase)))
            {
                // try to get the cached model
                Model3D contentModel;
                if (ViewerType == ViewerType.System && _modelCache.TryGetValue(modelPath, out contentModel))
                {
                    return contentModel;
                }

                string file = Path.Combine(DataPath, modelPath);
                if (File.Exists(file))
                {
                    contentModel = UtfModel.LoadModel(file);

                    // cache model
                    if (ViewerType == ViewerType.System)
                    {
                        _modelCache[modelPath] = contentModel;
                    }

                    return contentModel;
                }
            }
            return null;
        }

        void LoadModel(ContentBase content)
        {
            if (IsModelMode && content.Block.IsRealModel())
            {
                if (content.Block.Archetype != null && content.Block.Archetype.ModelPath != null)
                {
                    content.Content = LoadModel(content.Block.Archetype.ModelPath);

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
            for (int i = GetContentStartId(); i < Viewport.Children.Count; ++i)
            {
                ContentBase content = (ContentBase)Viewport.Children[i];
                if (content.Block.IsRealModel())
                {
                    LoadModel(content);
                }
            }

            if (_selectedContent != null && _selectedContent.Block.IsRealModel())
            {
                // update selection box
                SelectedContent = _selectedContent;
            }
        }

        ContentBase CreateContent(TableBlock block)
        {
            ContentBase content = CreateContent(block.ObjectType);
            if (content == null)
            {
                return null;
            }

            SetValues(content, block);

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

        void AddOrReplace(Visual3D visual, Visual3D value)
        {
            int index = Viewport.Children.IndexOf(visual);
            if (index != -1)
            {
                if (value != null)
                {
                    Viewport.Children[index] = value;
                }
                else
                {
                    Viewport.Children.RemoveAt(index);
                    --_secondLayerId;
                }
            }
            else if (value != null)
            {
                Viewport.Children.Insert(0, value);
                ++_secondLayerId;
            }
        }

        void AddOrReplace(ScreenSpaceVisual3D visual, ScreenSpaceVisual3D value)
        {
            if (visual != null && value == null)
            {
                visual.StopRendering();
            }

            AddOrReplace(visual, (Visual3D)value);

            if (visual == null && value != null)
            {
                value.StartRendering();
            }
        }

        public void SetTitle()
        {
            if (_selectedContent == null)
            {
                Viewport.Title = null;
                return;
            }

            Helper.String.StringBuilder.Length = 0;
            Helper.String.StringBuilder.AppendLine(_selectedContent.Block.Name);

            if (_trackedContent != null && _trackedContent != _selectedContent)
            {
                AddTrackInfo();
            }

            Viewport.Title = Helper.String.StringBuilder.ToString();
        }

        void AddTrackInfo()
        {
            Helper.String.StringBuilder.AppendLine();
            Helper.String.StringBuilder.AppendLine(_trackedContent.Block.Name);

            Helper.String.StringBuilder.Append(Strings.SystemPresenterTrackedDistance);
            Vector3D a = _selectedContent.Position / SystemParser.SIZE_FACTOR;
            Vector3D b = _trackedContent.Position / SystemParser.SIZE_FACTOR;
            Helper.String.StringBuilder.Append(Math.Round((a - b).Length));

            Helper.String.StringBuilder.AppendLine();
            Helper.String.StringBuilder.Append(Strings.SystemPresenterTrackedAngles);

            const double radToDeg = 180 / Math.PI;

            double denominator = -b.Y - -a.Y;
            if (denominator == 0.0)
            {
                Helper.String.StringBuilder.Append("0, 0, ");
            }
            else
            {
                Helper.String.StringBuilder.Append(Math.Round(Math.Atan((b.Z - a.Z) / denominator) * radToDeg));
                Helper.String.StringBuilder.Append(", ");
                Helper.String.StringBuilder.Append(Math.Round(Math.Atan((b.X - a.X) / denominator) * radToDeg));
                Helper.String.StringBuilder.Append(", ");
            }

            denominator = b.X - a.X;
            if (denominator == 0.0)
            {
                Helper.String.StringBuilder.Append('0');
            }
            else
            {
                Helper.String.StringBuilder.Append(Math.Round(Math.Atan((b.Z - a.Z) / denominator) * radToDeg));
            }
        }

        public ContentBase FindContent(TableBlock block)
        {
            for (int i = GetContentStartId(); i < Viewport.Children.Count; ++i)
            {
                ContentBase content = (ContentBase)Viewport.Children[i];
                if (content.Block != null && content.Block.Id == block.Id)
                {
                    return content;
                }
            }
            return null;
        }
    }
}
