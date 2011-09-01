using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using FreelancerModStudio.Data;
using FreelancerModStudio.Data.IO;
using HelixEngine;
using HelixEngine.Meshes;
using HelixEngine.Wires;
using IO = System.IO;

namespace FreelancerModStudio.SystemPresenter
{
    public class Presenter
    {
        public Table<int, ContentBase> Objects { get; set; }
        public HelixView3D Viewport { get; set; }
        public bool IsUniverse { get; set; }

        ModelVisual3D lightning;
        public ModelVisual3D Lightning
        {
            get
            {
                return lightning;
            }
            set
            {
                int index = Viewport.Children.IndexOf(lightning);
                if (index != -1)
                {
                    if (value != null)
                        Viewport.Children[index] = value;
                    else
                        Viewport.Children.RemoveAt(index);
                }
                else if (value != null)
                    Viewport.Children.Insert(0, value);

                lightning = value;
            }
        }

        ModelVisual3D selection;
        public ModelVisual3D Selection
        {
            get
            {
                return selection;
            }
            set
            {
                int index = Viewport.Children.IndexOf(selection);
                if (index != -1)
                {
                    if (value != null)
                        Viewport.Children[index] = value;
                    else
                        Viewport.Children.RemoveAt(index);
                }
                else if (value != null)
                    Viewport.Children.Insert(0, value);

                selection = value;
            }
        }

        ContentBase selectedContent;
        public ContentBase SelectedContent
        {
            get
            {
                return selectedContent;
            }
            set
            {
                if (selectedContent != value)
                    SetSelectedContent(value, true);
            }
        }

        public delegate void SelectionChangedType(ContentBase content);
        public SelectionChangedType SelectionChanged;

        void OnSelectionChanged(ContentBase content)
        {
            if (this.SelectionChanged != null)
                this.SelectionChanged(content);
        }

        public delegate void FileOpenType(string file);
        public FileOpenType FileOpen;

        void OnFileOpen(string file)
        {
            if (this.FileOpen != null)
                this.FileOpen(file);
        }

        public Presenter(HelixView3D viewport)
        {
            Objects = new Table<int, ContentBase>();
            Viewport = viewport;
            Viewport.SelectionChanged += camera_SelectionChanged;
            Lightning = new SystemLightsVisual3D();
        }

        void SetSelectedContent(ContentBase content, bool lookAt)
        {
            selectedContent = content;

            if (content != null)
            {
                //goto content
                if (lookAt)
                    LookAt(content);

                //select content visually
                Selection = GetSelectionBox(content);

                Viewport.Title = content.Title;
            }
            else
            {
                Selection = null;
                Viewport.Title = string.Empty;
            }
        }

        public void LookAt(ContentBase content)
        {
            Viewport.LookAt(content.GetPositionPoint(), Animator.AnimationDuration.TimeSpan.TotalMilliseconds);
        }

        void AddContent(ContentBase content)
        {
            //load model it was is not loaded yet
            if (content.Model == null)
                content.LoadModel();

            //only add it to viewpoint if its actually visible
            if (content.Visibility)
            {
                Viewport.Add(content.Model);

                if (content == SelectedContent)
                    Selection = GetSelectionBox(content);
            }

            Objects.Add(content);
        }

        public void Add(List<TableBlock> blocks)
        {
            Animator.AnimationDuration = new Duration(TimeSpan.Zero);

            foreach (TableBlock block in blocks)
            {
                ContentBase content = GetContent(block);

                if (content != null)
                    AddContent(content);
            }

            Animator.AnimationDuration = new Duration(TimeSpan.FromMilliseconds(500));
        }

        public void Delete(List<TableBlock> blocks)
        {
            foreach (TableBlock block in blocks)
            {
                ContentBase content;
                if (Objects.TryGetValue(block.UniqueID, out content))
                    Delete(content);
            }
        }

        public void Delete(ContentBase content)
        {
            //if we delete a system also delete all universe connections to and from it
            if (IsUniverse && content is System)
                DeleteConnections(content as System);

            Objects.Remove(content);
            Viewport.Remove(content.Model);
        }

        void camera_SelectionChanged(DependencyObject visual)
        {
            ModelVisual3D model = (ModelVisual3D)visual;
            foreach (ContentBase content in Objects)
            {
                if (content.Model == model)
                {
                    if (SelectedContent == content)
                    {
                        if (IsUniverse && content is System)
                            DisplayContextMenu(((System)content).Path);
                    }
                    else
                        SetSelectedContent(content, false);

                    OnSelectionChanged(content);
                    return;
                }
            }
        }

        void DisplayContextMenu(string path)
        {
            ContextMenu menu = new ContextMenu();
            MenuItem item = new MenuItem();
            item.Header = string.Format(Properties.Strings.SystemPresenterOpen, IO.Path.GetFileName(path));
            item.Tag = path;
            item.Click += new RoutedEventHandler(item_Click);

            menu.Items.Add(item);
            menu.IsOpen = true;
        }

        void item_Click(object sender, RoutedEventArgs e)
        {
            OnFileOpen((string)((MenuItem)sender).Tag);
        }

        ModelVisual3D GetSelectionBox(ContentBase content)
        {
            WireLines lines = GetWireBox(content.GetMesh().Bounds);
            lines.Transform = content.Model.Transform;

            return lines;
        }

        WireLines GetWireBox(Rect3D bounds)
        {
            Point3DCollection points = new Point3DCollection();
            points.Add(new Point3D(bounds.X, bounds.Y, bounds.Z));
            points.Add(new Point3D(-bounds.X, bounds.Y, bounds.Z));

            points.Add(new Point3D(-bounds.X, bounds.Y, bounds.Z));
            points.Add(new Point3D(-bounds.X, bounds.Y, -bounds.Z));

            points.Add(new Point3D(-bounds.X, bounds.Y, -bounds.Z));
            points.Add(new Point3D(bounds.X, bounds.Y, -bounds.Z));

            points.Add(new Point3D(bounds.X, bounds.Y, -bounds.Z));
            points.Add(new Point3D(bounds.X, bounds.Y, bounds.Z));

            points.Add(new Point3D(bounds.X, -bounds.Y, bounds.Z));
            points.Add(new Point3D(-bounds.X, -bounds.Y, bounds.Z));

            points.Add(new Point3D(-bounds.X, -bounds.Y, bounds.Z));
            points.Add(new Point3D(-bounds.X, -bounds.Y, -bounds.Z));

            points.Add(new Point3D(-bounds.X, -bounds.Y, -bounds.Z));
            points.Add(new Point3D(bounds.X, -bounds.Y, -bounds.Z));

            points.Add(new Point3D(bounds.X, -bounds.Y, -bounds.Z));
            points.Add(new Point3D(bounds.X, -bounds.Y, bounds.Z));

            points.Add(new Point3D(bounds.X, bounds.Y, bounds.Z));
            points.Add(new Point3D(bounds.X, -bounds.Y, bounds.Z));

            points.Add(new Point3D(-bounds.X, bounds.Y, bounds.Z));
            points.Add(new Point3D(-bounds.X, -bounds.Y, bounds.Z));

            points.Add(new Point3D(bounds.X, bounds.Y, -bounds.Z));
            points.Add(new Point3D(bounds.X, -bounds.Y, -bounds.Z));

            points.Add(new Point3D(-bounds.X, bounds.Y, -bounds.Z));
            points.Add(new Point3D(-bounds.X, -bounds.Y, -bounds.Z));

            return new WireLines() { Lines = points, Color = Colors.Yellow, Thickness = 2 };
        }

        public void SetVisibility(ContentBase content, bool visibility)
        {
            if (content.Visibility != visibility)
            {
                content.Visibility = visibility;

                if (visibility)
                    //show model
                    Viewport.Add(content.Model);
                else
                    //hide model
                    Viewport.Remove(content.Model);
            }
        }

        public void ClearDisplay(bool light)
        {
            if (light || Lightning == null)
                Viewport.Children.Clear();
            else
            {
                for (int i = Viewport.Children.Count - 1; i >= 0; i--)
                {
                    ModelVisual3D model = (ModelVisual3D)Viewport.Children[i];
                    if (model != Lightning)
                        Viewport.Remove(model);
                }
            }
        }

        public void DisplayUniverse(string path, int systemTemplate, List<TableBlock> blocks, ArchetypeManager archetype)
        {
            //filter the systems to improve speed as we need to loop them often in the analyzer
            List<TableBlock> systems = new List<TableBlock>();
            foreach (TableBlock block in blocks)
            {
                if (block.ObjectType == ContentType.System)
                    systems.Add(block);
            }

            Analyzer analyzer = new Analyzer()
            {
                Universe = systems,
                UniversePath = path,
                SystemTemplate = systemTemplate,
                Archetype = archetype
            };
            analyzer.Analyze();

            DisplayUniverseConnections(analyzer.Connections);
        }

        void DisplayUniverseConnections(Table<UniverseConnectionID, UniverseConnection> connections)
        {
            Viewport.Dispatcher.Invoke(new Action(delegate
            {
                foreach (UniverseConnection connection in connections)
                    Viewport.Add(GetConnection(connection).Model);
            }));
        }

        void DeleteConnections(System system)
        {
            foreach (Connection connection in system.Connections)
                Delete(connection);
        }

        void UpdateConnections(System system)
        {
            foreach (Connection connection in system.Connections)
                SetConnection(connection);
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
            line.From = Objects[connection.From.ID];
            line.To = Objects[connection.To.ID];
            line.FromType = GetConnectionType(connection.From.Jumpgate, connection.From.Jumphole);
            line.ToType = GetConnectionType(connection.To.Jumpgate, connection.To.Jumphole);

            ((System)line.From).Connections.Add(line);
            ((System)line.To).Connections.Add(line);

            SetConnection(line);
        }

        void SetConnection(Connection line)
        {
            Vector3D fromPosition = line.From.GetPosition();
            Vector3D toPosition = line.To.GetPosition();

            Vector3D position = (fromPosition + toPosition) / 2;
            Vector3D scale = new Vector3D(1, (fromPosition - toPosition).Length, 1);

            if (line.FromType == ConnectionType.Jumphole || line.ToType == ConnectionType.Jumphole)
                scale.X = 0.5;

            Vector v1 = new Vector(fromPosition.X, fromPosition.Y);
            Vector v2 = new Vector(toPosition.X, toPosition.Y);

            double a = Difference(v2.X, v1.X);
            double b = Difference(v2.Y, v1.Y);
            double factor = 1;
            double angleOffset = 90;

            if (v2.X < v1.X)
                factor = -1;

            if (v2.Y < v1.Y)
            {
                angleOffset = -90;
                factor *= -1;
            }

            double c = Math.Sqrt(a * a + b * b);
            double angle = Math.Acos(a / c) * 180 / Math.PI;

            Vector3D rotation = new Vector3D(0, 0, (angle + angleOffset) * factor);

            line.SetDisplay(position, rotation, scale);
        }

        ConnectionType GetConnectionType(bool jumpgate, bool jumphole)
        {
            if (jumpgate && !jumphole)
                return ConnectionType.Jumpgate;
            else if (!jumpgate && jumphole)
                return ConnectionType.Jumphole;
            else if (jumpgate && jumphole)
                return ConnectionType.Both;

            return ConnectionType.None;
        }

        double Difference(double x, double y)
        {
            if (x > y)
                return x - y;
            else
                return y - x;
        }

        public void ChangeValues(ContentBase content, TableBlock block)
        {
            if (block.ObjectType == ContentType.None)
            {
                //delete content if it was changed back to an invalid type
                Delete(content);
                if (selectedContent == content)
                    SelectedContent = null;
            }
            else
                SetValues(content, block);
        }

        void SetValues(ContentBase content, TableBlock block)
        {
            Parser parser = new Parser();
            parser.SetValues(content, block);
            content.Title = block.Name;

            if (parser.ModelChanged && content.Model != null)
                ReloadModel(content);

            if (IsUniverse && content is System)
                UpdateConnections(content as System);

            if (selectedContent == content)
            {
                //update selection if changed content is selected
                SetSelectedContent(content, true);
            }
        }

        void ReloadModel(ContentBase content)
        {
            int index = Viewport.Children.IndexOf(content.Model);
            if (index != -1)
            {
                Viewport.Children.RemoveAt(index);
                content.LoadModel();
                Viewport.Children.Insert(index, content.Model);
            }
        }

        ContentBase GetContent(TableBlock block)
        {
            ContentBase content = GetContentFromType(block.ObjectType);
            if (content == null)
                return null;

            content.Visibility = block.Visibility;
            SetValues(content, block);

            content.ID = block.UniqueID;
            return content;
        }

        ContentBase GetContentFromType(ContentType type)
        {
            if (type == ContentType.LightSource)
                return new LightSource();
            else if (type == ContentType.Sun)
                return new Sun();
            else if (type == ContentType.Planet)
                return new Planet();
            else if (type == ContentType.Station)
                return new Station();
            else if (type == ContentType.Satellite)
                return new Satellite();
            else if (type == ContentType.Construct)
                return new Construct();
            else if (type == ContentType.Depot)
                return new Depot();
            else if (type == ContentType.Ship)
                return new Ship();
            else if (type == ContentType.WeaponsPlatform)
                return new WeaponsPlatform();
            else if (type == ContentType.DockingRing)
                return new DockingRing();
            else if (type == ContentType.JumpHole)
                return new JumpHole();
            else if (type == ContentType.JumpGate)
                return new JumpGate();
            else if (type == ContentType.TradeLane)
                return new TradeLane();
            else if (type == ContentType.Zone)
                return new Zone();
            else if (type == ContentType.System)
                return new System();

            return null;
        }
    }
}
