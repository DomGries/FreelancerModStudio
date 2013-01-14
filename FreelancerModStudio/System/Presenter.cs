using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using FreelancerModStudio.Data;
using FreelancerModStudio.Properties;
using FreelancerModStudio.SystemPresenter.Content;
using HelixEngine;
using HelixEngine.Wires;
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

        ModelVisual3D _lightning;
        ModelVisual3D _selection;
        ContentBase _selectedContent;

        public ModelVisual3D Lightning
        {
            get
            {
                return _lightning;
            }
            set
            {
                int index = Viewport.Children.IndexOf(_lightning);
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
                    ++_secondLayerId;
                }

                _lightning = value;
            }
        }

        public ModelVisual3D Selection
        {
            get
            {
                return _selection;
            }
            set
            {
                int index = Viewport.Children.IndexOf(_selection);
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
                }

                _selection = value;
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

                if (value != null)
                {
                    //select content visually
                    Selection = GetSelectionBox(value);
                    Viewport.Title = value.Block.Name;
                }
                else
                {
                    Selection = null;
                    Viewport.Title = null;
                }
            }
        }

        public delegate void SelectionChangedType(ContentBase content);

        public SelectionChangedType SelectionChanged;

        void OnSelectionChanged(ContentBase content)
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(content);
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
            Viewport.SelectionChanged += camera_SelectionChanged;
            Lightning = new SystemLightsVisual3D();
        }

        public void LookAt(ContentBase content)
        {
            Viewport.LookAt(content.GetPositionPoint(), Animator.AnimationDuration.TimeSpan.TotalMilliseconds);
        }

        public void LookAt(Point3D point)
        {
            Viewport.LookAt(point, Animator.AnimationDuration.TimeSpan.TotalMilliseconds);
        }

        void AddContent(ContentBase content)
        {
            //load model it was is not loaded yet
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
        }

        void camera_SelectionChanged(DependencyObject visual)
        {
            // prevent selecting objects in solararchetype file
            if (ViewerType == ViewerType.SolarArchetype)
            {
                return;
            }

            ContentBase content = visual as ContentBase;

            // prevent selecting wirebox + universe connection
            if (content == null || content.Block == null)
            {
                return;
            }

            if (_selectedContent == content)
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
            else
            {
                SelectedContent = content;
            }

            OnSelectionChanged(content);
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

        ModelVisual3D GetSelectionBox(ContentBase content)
        {
            WireLines lines = GetWireBox(GetBounds(content));
            lines.Transform = content.Transform;

            return lines;
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

        static WireLines GetWireBox(Rect3D bounds)
        {
            Point3DCollection points = new Point3DCollection
                {
                    new Point3D(bounds.X + bounds.SizeX, bounds.Y + bounds.SizeY, bounds.Z + bounds.SizeZ),
                    new Point3D(bounds.X, bounds.Y + bounds.SizeY, bounds.Z + bounds.SizeZ),
                    new Point3D(bounds.X, bounds.Y + bounds.SizeY, bounds.Z + bounds.SizeZ),
                    new Point3D(bounds.X, bounds.Y + bounds.SizeY, bounds.Z),
                    new Point3D(bounds.X, bounds.Y + bounds.SizeY, bounds.Z),
                    new Point3D(bounds.X + bounds.SizeX, bounds.Y + bounds.SizeY, bounds.Z),
                    new Point3D(bounds.X + bounds.SizeX, bounds.Y + bounds.SizeY, bounds.Z),
                    new Point3D(bounds.X + bounds.SizeX, bounds.Y + bounds.SizeY, bounds.Z + bounds.SizeZ),
                    new Point3D(bounds.X + bounds.SizeX, bounds.Y, bounds.Z + bounds.SizeZ),
                    new Point3D(bounds.X, bounds.Y, bounds.Z + bounds.SizeZ),
                    new Point3D(bounds.X, bounds.Y, bounds.Z + bounds.SizeZ),
                    new Point3D(bounds.X, bounds.Y, bounds.Z),
                    new Point3D(bounds.X, bounds.Y, bounds.Z),
                    new Point3D(bounds.X + bounds.SizeX, bounds.Y, bounds.Z),
                    new Point3D(bounds.X + bounds.SizeX, bounds.Y, bounds.Z),
                    new Point3D(bounds.X + bounds.SizeX, bounds.Y, bounds.Z + bounds.SizeZ),
                    new Point3D(bounds.X + bounds.SizeX, bounds.Y + bounds.SizeY, bounds.Z + bounds.SizeZ),
                    new Point3D(bounds.X + bounds.SizeX, bounds.Y, bounds.Z + bounds.SizeZ),
                    new Point3D(bounds.X, bounds.Y + bounds.SizeY, bounds.Z + bounds.SizeZ),
                    new Point3D(bounds.X, bounds.Y, bounds.Z + bounds.SizeZ),
                    new Point3D(bounds.X + bounds.SizeX, bounds.Y + bounds.SizeY, bounds.Z),
                    new Point3D(bounds.X + bounds.SizeX, bounds.Y, bounds.Z),
                    new Point3D(bounds.X, bounds.Y + bounds.SizeY, bounds.Z),
                    new Point3D(bounds.X, bounds.Y, bounds.Z)
                };

            return new WireLines
                {
                    Lines = points,
                    Color = Colors.Yellow
                };
        }

        public void ClearDisplay(bool light)
        {
            Viewport.Children.Clear();

            if (light || Lightning == null)
            {
                _secondLayerId = 0;
            }
            else
            {
                Viewport.Children.Add(Lightning);
                _secondLayerId = 1;
            }
        }

        public int GetContentStartId()
        {
            int index = 0;
            if (Selection != null)
            {
                ++index;
            }
            if (Lightning != null)
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
            Vector3D fromPosition = line.From.GetPosition();
            Vector3D toPosition = line.To.GetPosition();

            Vector3D position = (fromPosition + toPosition)/2;
            Vector3D scale = new Vector3D(2.5, (fromPosition - toPosition).Length, 1);

            if (line.FromType == ConnectionType.JumpGateAndHole || line.ToType == ConnectionType.JumpGateAndHole)
            {
                scale.X = 4.5;
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

            double c = Math.Sqrt(a*a + b*b);
            double angle = Math.Acos(a/c)*180/Math.PI;

            Vector3D rotation = new Vector3D(0, 0, (angle + angleOffset)*factor);

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
            if (SystemParser.SetValues(content, block, ViewerType == ViewerType.System) && content.Content != null)
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
                Viewport.Title = content.Block.Name;
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
                Selection = GetSelectionBox(_selectedContent);
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
                    return new SystemObject();
                default: // zone
                    return new Zone();
            }
        }
    }
}
