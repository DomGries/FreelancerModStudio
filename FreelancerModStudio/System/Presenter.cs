using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Controls;
using HelixEngine;
using HelixEngine.Meshes;
using FreelancerModStudio.Data;
using System.Globalization;
using System.Windows;
using FreelancerModStudio.Data.IO;
using HelixEngine.Wires;
using System.Windows.Threading;
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
                else
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
                    SetSelectedContent(value);
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
        }

        void SetSelectedContent(ContentBase content)
        {
            selectedContent = content;

            if (content != null)
            {
                //goto content
                Viewport.LookAt(content.Position.ToPoint3D(), Animator.AnimationDuration.TimeSpan.TotalMilliseconds);

                //select content visually
                Selection = GetSelectionBox(content);

                if (content.Block != null)
                    Viewport.Title = GetTitle(content.Block.Block);
            }
            else
            {
                Selection = null;
                Viewport.Title = string.Empty;
            }
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
                Delete(Objects[block.ID]);
        }

        public void Delete(ContentBase content)
        {
            //if we delete a system also delete all universe connections to and from it
            if (IsUniverse && content is System)
                DeleteConnections(content as System);

            Objects.Remove(content);
            Viewport.Remove(content.Model);
        }

        public void Move(List<TableBlock> oldBlocks)
        {
            List<ContentBase> contents = new List<ContentBase>();
            foreach (TableBlock block in oldBlocks)
            {
                //only remove the content from the list because we have to change the ID
                //as it wont automatically be changed due to being used in a dictionary
                ContentBase content = Objects[block.ID];
                contents.Add(content);
                Objects.Remove(content);
            }

            //we can simply add them again because the contents block ID was already changed due to being a reference
            Objects.AddRange(contents);
        }

        void camera_SelectionChanged(DependencyObject visual)
        {
            ModelVisual3D model = (ModelVisual3D)visual;
            foreach (ContentBase content in Objects)
            {
                if (content.Model == model)
                {
                    if (SelectedContent == content && IsUniverse && content is System)
                        DisplayContextMenu(((System)content).Path);

                    SelectedContent = content;
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

        string GetTitle(EditorINIBlock block)
        {
            if (block.Options.Count > block.MainOptionIndex)
            {
                if (block.Options[block.MainOptionIndex].Values.Count > 0)
                    return block.Options[block.MainOptionIndex].Values[0].Value.ToString();
            }
            return block.Name;
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

        public void DisplayUniverse(string path, int systemTemplate, ArchetypeManager archetype)
        {
            Analyzer analyzer = new Analyzer()
            {
                Universe = Objects,
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
            line.From = connection.From.Content;
            line.To = connection.To.Content;
            line.FromType = GetConnectionType(connection.From.Jumpgate, connection.From.Jumphole);
            line.ToType = GetConnectionType(connection.To.Jumpgate, connection.To.Jumphole);

            ((System)connection.From.Content).Connections.Add(line);
            ((System)connection.To.Content).Connections.Add(line);

            SetConnection(line);
        }

        void SetConnection(Connection line)
        {
            Vector3D position = (line.From.Position + line.To.Position) / 2;
            //line.Position = newPos - (globalConnection.Content.Position - newPos) / 2;
            //line.Scale = new Vector3D(1, (globalConnection.Content.Position - connection.Connection.Position).Length / 2, 1);
            Vector3D scale = new Vector3D(1, (line.From.Position - line.To.Position).Length, 1);

            Vector v1 = new Vector(line.From.Position.X, line.From.Position.Y);
            Vector v2 = new Vector(line.To.Position.X, line.To.Position.Y);

            double a = Difference(v2.X, v1.X);
            double b = Difference(v2.Y, v1.Y);
            double factor = 1;
            if (v2.X < v1.X)
                factor = -1;

            if (v2.Y < v1.Y)
                factor *= -1;

            double c = Math.Sqrt(a * a + b * b);
            double angle = Math.Acos(a / c) * 180 / Math.PI;

            Rotation3D rotation = new AxisAngleRotation3D(new Vector3D(0, 0, factor), angle + 90);

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
            {
                content.Block = block;
                SetValues(content, block);
            }
        }

        void SetValues(ContentBase content, TableBlock block)
        {
            Parser parser = new Parser();
            parser.SetValues(content, block);

            if (parser.ModelChanged && content.Model != null)
                ReloadModel(content);

            if (IsUniverse && content is System)
                UpdateConnections(content as System);

            if (selectedContent == content)
            {
                //update selection if changed content is selected
                SetSelectedContent(content);
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

            content.Block = block;
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
