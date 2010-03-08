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

namespace FreelancerModStudio.SystemPresenter
{
    public class SystemPresenter
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

        void SetSelectedContent(ContentBase content)
        {
            selectedContent = content;

            if (content != null)
            {
                //goto content
                Viewport.LookAt(content.Position.ToPoint3D(), ContentAnimator.AnimationDuration.TimeSpan.TotalMilliseconds);

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

        public delegate void SelectionChangedType(ContentBase content);
        public SelectionChangedType SelectionChanged;

        void OnSelectionChanged(ContentBase content)
        {
            if (this.SelectionChanged != null)
                this.SelectionChanged(content);
        }

        public SystemPresenter(HelixView3D viewport)
        {
            Objects = new Table<int, ContentBase>();
            Viewport = viewport;
            Viewport.SelectionChanged += camera_SelectionChanged;
        }

        void AddContent(ContentBase content)
        {
            if (content.Model == null)
                content.LoadModel();

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
            ContentAnimator.AnimationDuration = new Duration(TimeSpan.Zero);

            foreach (TableBlock block in blocks)
            {
                ContentBase content = GetContent(block);

                if (content != null)
                    AddContent(content);
            }

            ContentAnimator.AnimationDuration = new Duration(TimeSpan.FromMilliseconds(500));
        }

        public void Delete(List<TableBlock> blocks)
        {
            foreach (TableBlock block in blocks)
            {
                ContentBase content;
                if (Objects.TryGetValue(block.ID, out content))
                    Delete(content);
            }
        }

        public void Delete(ContentBase content)
        {
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
                    SelectedContent = content;
                    OnSelectionChanged(content);
                    return;
                }
            }
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
                {
                    //show model
                    Viewport.Add(content.Model);
                }
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

        public void LoadUniverseConnections(string path, int systemTemplate)
        {
            UniverseAnalyzer analyzer = new UniverseAnalyzer()
            {
                Universe = Objects,
                UniversePath = path,
                SystemTemplate = systemTemplate
            };
            analyzer.Analyze();

            DisplayUniverseConnections(analyzer.Connections);
        }

        void DisplayUniverseConnections(List<GlobalConnection> connections)
        {
            double sphereLenght = 1 - 0.36;

            //Viewport.Dispatcher.Invoke(new Action(delegate
            //{
            foreach (GlobalConnection globalConnection in connections)
            {
                foreach (UniverseConnection connection in globalConnection.Universe)
                {
                    Connection line = new Connection();
                    if (connection.Jumpgate && !connection.Jumphole)
                        line.Type = ConnectionType.Jumpgate;
                    else if (!connection.Jumpgate && connection.Jumphole)
                        line.Type = ConnectionType.Jumphole;
                    else if (connection.Jumpgate && connection.Jumphole)
                        line.Type = ConnectionType.Both;


                    line.Position = (globalConnection.Content.Position + connection.ID.Position) / 2;
                    line.Scale = new Vector3D(1, Vector3D.Multiply((globalConnection.Content.Position - connection.ID.Position), sphereLenght).Length, 1);

                    Vector v2 = new Vector(connection.ID.Position.X, connection.ID.Position.Y);
                    double angleBetween = Vector.AngleBetween(new Vector(0, v2.Y), v2);

                    line.Rotation = new AxisAngleRotation3D(new Vector3D(0, 0, 1), angleBetween);

                    line.LoadModel();
                    Viewport.Add(line.Model);
                }
            }
            //}));
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
            SystemParser parser = new SystemParser();
            parser.SetValues(content, block);

            if (selectedContent == content)
            {
                //update selection if changed content is selected
                SetSelectedContent(content);
            }
        }

        ContentBase GetContent(TableBlock block)
        {
            ContentBase content = GetContentFromType(block.ObjectType);
            if (content == null)
                return null;

            content.Visibility = block.Visibility;
            content.ID = block.ID;
            SetValues(content, block);

            content.Block = block;
            return content;
        }

        ContentBase GetContentFromType(ContentType type)
        {
            if (type == ContentType.LightSource)
                return new LightSource();
            else if (type == ContentType.Sun || type == ContentType.System)
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

            return null;
        }
    }
}
