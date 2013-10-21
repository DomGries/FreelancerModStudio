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

        public Visual3D Lighting
        {
            get
            {
                return _lighting;
            }
            set
            {
                AddOrReplace(_lighting, value, true);
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
                AddOrReplace(_selectionBox, value, false);
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
                AddOrReplace(_trackedLine, value, false);
                _trackedLine = value;
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
                _selectedContent = value;

                //select content visually
                SetSelectionBox();
                SetTrackedLine(true);
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

        public delegate void SelectionChangedType(ContentBase content, bool toggle);

        public SelectionChangedType SelectionChanged;

        void OnSelectionChanged(ContentBase content, bool toggle)
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(content, toggle);
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

        public Presenter(HelixViewport3D viewport)
        {
            Viewport = viewport;
            Viewport.MouseDown += Viewport_MouseDown;
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
                _secondLayerId--;
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

        public ContentBase GetSelection(Point position, bool farthest)
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
                        //if (rayHit.VisualHit is Manipulator)
                        //{
                        //    return HitTestResultBehavior.Stop;
                        //}

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

                ContentBase visual = GetSelection(e.GetPosition((IInputElement)sender), isShiftDown);
                if (visual != null)
                {
                    if (isLookAt)
                    {
                        // change the 'lookat' point
                        Viewport.LookAt(visual.GetPositionPoint());
                    }
                    else
                    {
                        Select(visual, isCtrlDown);
                    }
                }
            }
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

            OnSelectionChanged(content, toggle);
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

            Color color = Colors.Yellow;
            if (_trackedContent == _selectedContent)
            {
                color = Colors.Red;
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
                    selectionBox.Color = Colors.Red;
                }
                else
                {
                    selectionBox.Color = Colors.Yellow;
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
                        Color = Colors.Red,
                    };
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

            Vector3D position = (fromPosition + toPosition) / 2;
            Vector3D scale = new Vector3D(SystemParser.UNIVERSE_CONNECTION_SCALE, (fromPosition - toPosition).Length, 1);

            if (line.FromType == ConnectionType.JumpGateAndHole || line.ToType == ConnectionType.JumpGateAndHole)
            {
                scale.X = SystemParser.UNIVERSE_DOUBLE_CONNECTION_SCALE;
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

            double c = Math.Sqrt(a * a + b * b);
            double angle = Math.Acos(a / c) * 180 / Math.PI;

            Vector3D rotation = new Vector3D(0, 0, (angle + angleOffset) * factor);

            line.SetTransform(position, rotation, scale, false);
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

        void AddOrReplace(Visual3D visual, Visual3D value, bool secondLayer)
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
                }
            }
            else if (value != null)
            {
                Viewport.Children.Insert(0, value);

                if (secondLayer)
                {
                    ++_secondLayerId;
                }
            }
        }

        void AddOrReplace(ScreenSpaceVisual3D visual, ScreenSpaceVisual3D value, bool secondLayer)
        {
            if (visual != null && value == null)
            {
                visual.StopRendering();
            }

            AddOrReplace(visual, (Visual3D)value, secondLayer);

            if (visual == null && value != null)
            {
                value.StartRendering();
            }
        }

        void SetTitle()
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

            Helper.String.StringBuilder.Append("Distance: ");
            Vector3D a = _selectedContent.Position / SystemParser.SIZE_FACTOR;
            Vector3D b = _trackedContent.Position / SystemParser.SIZE_FACTOR;
            Helper.String.StringBuilder.Append(Math.Round((a - b).Length));

            Helper.String.StringBuilder.AppendLine();
            Helper.String.StringBuilder.Append("Angle: ");

            double denominator = -b.Y - -a.Y;
            if (denominator == 0.0)
            {
                Helper.String.StringBuilder.Append("0, 0, ");
            }
            else
            {
                Helper.String.StringBuilder.Append(Math.Round(Math.Atan((b.Z - a.Z) / denominator) * 180.0 / Math.PI));
                Helper.String.StringBuilder.Append(", ");
                Helper.String.StringBuilder.Append(Math.Round(Math.Atan((b.X - a.X) / denominator) * 180.0 / Math.PI));
                Helper.String.StringBuilder.Append(", ");
            }

            denominator = b.X - a.X;
            if (denominator == 0.0)
            {
                Helper.String.StringBuilder.Append('0');
            }
            else
            {
                Helper.String.StringBuilder.Append(Math.Round(Math.Atan((b.Z - a.Z) / denominator) * 180.0 / Math.PI));
            }
        }
    }
}
