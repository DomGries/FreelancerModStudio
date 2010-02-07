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

namespace FreelancerModStudio.SystemPresenter
{
    public class SystemPresenter
    {
        public Table<int, ContentBase> Objects { get; set; }
        public HelixView3D Viewport { get; set; }

        private ModelVisual3D lightning;
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

        private ModelVisual3D selection;
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
                else
                    Viewport.Children.Insert(0, value);

                selection = value;
            }
        }

        private ContentBase selectedContent;
        public ContentBase SelectedContent
        {
            get
            {
                return selectedContent;
            }
            set
            {
                if (selectedContent == value)
                    return;

                selectedContent = value;

                if (value != null)
                {
                    //goto content
                    Viewport.LookAt(value.Position.ToPoint3D(), ContentAnimator.AnimationDuration.TimeSpan.TotalMilliseconds);

                    //select content visually
                    Selection = GetSelectionBox(value);

                    Viewport.Title = GetTitle(value.Block.Block);
                }
                else
                {
                    Selection = null;
                    Viewport.Title = string.Empty;
                }
            }
        }

        public delegate void SelectionChangedType(ContentBase content);
        public SelectionChangedType SelectionChanged;

        private void OnSelectionChanged(ContentBase content)
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

        public void Show(TableData data)
        {
            Add(data.Blocks);
        }

        public void RefreshDisplay()
        {
            ContentAnimator.AnimationDuration = new Duration(TimeSpan.Zero);

            ClearDisplay(false);

            List<ContentBase> objects = Objects.Values.ToList<ContentBase>();
            objects.Sort();

            ShowObjects(objects);

            ContentAnimator.AnimationDuration = new Duration(TimeSpan.FromMilliseconds(500));
        }

        private void ShowObjects(List<ContentBase> objects)
        {
            foreach (ContentBase content in objects)
            {
                if (content.Model == null)
                    content.LoadModel();

                if (content.Visibility)
                    Viewport.Add(content.Model);
            }
        }

        public void Add(List<TableBlock> blocks)
        {
            foreach (TableBlock block in blocks)
            {
                ContentBase content = null;
                if ((BlockType)block.Block.TemplateIndex == BlockType.Object)
                    content = GetContent(block);
                else if ((BlockType)block.Block.TemplateIndex == BlockType.LightSource)
                    content = GetContent(block);
                else if ((BlockType)block.Block.TemplateIndex == BlockType.Zone)
                    content = GetContent(block);

                if (content != null)
                    Objects.Add(content);
            }

            //reload models and resort everything
            RefreshDisplay();
        }

        public void Delete(List<TableBlock> blocks)
        {
            foreach (TableBlock block in blocks)
            {
                ContentBase content;
                if (Objects.TryGetValue(block.ID, out content))
                {
                    Objects.Remove(content);
                    Viewport.Remove(content.Model);
                }
            }
        }

        private void camera_SelectionChanged(DependencyObject visual)
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

        private ModelVisual3D GetSelectionBox(ContentBase content)
        {
            WireLines lines = GetWireBox(content.GetMesh().Bounds);
            lines.Transform = content.Model.Transform;

            return lines;
        }

        private WireLines GetWireBox(Rect3D bounds)
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

        private string GetTitle(EditorINIBlock block)
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
                    //display model
                    if (content is Zone)
                    {
                        //zone should be behind all
                        Viewport.Add(content.Model);
                    }
                    else
                    {
                        //new visible content should be in front of all after selection
                        if (Selection == null)
                            Viewport.Children.Insert(0, content.Model);
                        else
                            Viewport.Children.Insert(1, content.Model);
                    }
                }
                else
                    //hide model
                    Viewport.Remove(content.Model);
            }
        }

        public void ClearDisplay(bool light)
        {
            if (light)
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

        public void SetValues(ContentBase content, TableBlock block)
        {
            int positionIndex = -1;
            int rotationIndex = -1;
            int scaleIndex = -1;

            if (block.ObjectType == ContentType.LightSource)
            {
                positionIndex = (int)LightSourceOptionType.Position;
                rotationIndex = (int)LightSourceOptionType.Rotation;
            }
            else if (block.ObjectType == ContentType.Zone)
            {
                positionIndex = (int)ZoneOptionType.Position;
                rotationIndex = (int)ZoneOptionType.Rotation;
                scaleIndex = (int)ZoneOptionType.Size;
            }
            else
            {
                positionIndex = (int)ObjectOptionType.Position;
                rotationIndex = (int)ObjectOptionType.Rotation;
            }

            Vector3D position = new Vector3D(0, 0, 0);
            Rotation3D rotation = new AxisAngleRotation3D(position, 0);
            Vector3D scale = new Vector3D(1, 1, 1);
            ZoneShape shape = ZoneShape.Box;

            //get transformation of content
            foreach (EditorINIOption option in block.Block.Options)
            {
                if (option.Values.Count > 0)
                {
                    if (option.TemplateIndex == positionIndex)
                    {
                        Vector3D tempPosition = ParseVector3D(option.Values[0].Value.ToString()) / 1000;
                        position = new Vector3D(tempPosition.X, -tempPosition.Z, tempPosition.Y);
                    }
                    else if (option.TemplateIndex == rotationIndex)
                        rotation = ParseRotation(option.Values[0].Value.ToString());
                    else if (block.ObjectType == ContentType.Zone && option.TemplateIndex == (int)ZoneOptionType.Shape)
                        shape = ParseShape(option.Values[0].Value.ToString());
                    else if (option.TemplateIndex == scaleIndex)
                        scale = ParseSize(option.Values[0].Value.ToString());
                }
            }

            //set content values
            if (block.ObjectType == ContentType.Zone)
                ((Zone)content).Shape = shape;
            else if (block.ObjectType != ContentType.LightSource)
                scale = new Vector3D(block.Archtype.Radius, block.Archtype.Radius, block.Archtype.Radius) / 1000;

            content.SetDisplay(position, rotation, scale);

            //also update selection wire box
            if (selectedContent == content)
                Selection = GetSelectionBox(content);
        }

        private ContentBase GetContentFromType(ContentType type)
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

            return null;
        }

        private ContentBase GetContent(TableBlock block)
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

        private Vector3D ParseSize(string scale)
        {
            CultureInfo usCulture = new CultureInfo("en-US", false);
            string[] values = scale.Split(new char[] { ',' });

            if (values.Length == 1)
            {
                double tempScale = double.Parse(values[0], usCulture);
                return new Vector3D(tempScale, tempScale, tempScale) / 1000;
            }
            else if (values.Length == 2)
            {
                double tempScale1 = double.Parse(values[0], usCulture);
                double tempScale2 = double.Parse(values[1], usCulture);
                return new Vector3D(tempScale1, tempScale2, tempScale1) / 1000;
            }
            else if (values.Length == 3)
            {
                double tempScale1 = double.Parse(values[0], usCulture);
                double tempScale2 = double.Parse(values[1], usCulture);
                double tempScale3 = double.Parse(values[2], usCulture);
                return new Vector3D(tempScale1, tempScale3, tempScale2) / 1000;
            }

            return new Vector3D(1, 1, 1);
        }

        private Vector3D ParseSize2(string scale, ZoneShape shape)
        {
            //check for correct shape! after PERFORMANCE Table Data
            CultureInfo usCulture = new CultureInfo("en-US", false);
            string[] values = scale.Split(new char[] { ',' });

            if (shape == ZoneShape.Box && values.Length > 0)
            {
                double tempScale = double.Parse(values[0], usCulture);
                return new Vector3D(tempScale, tempScale, tempScale) / 1000;
            }
            else if (shape == ZoneShape.Cylinder && values.Length > 1)
            {
                double tempScale1 = double.Parse(values[0], usCulture);
                double tempScale2 = double.Parse(values[1], usCulture);
                return new Vector3D(tempScale1, tempScale2, tempScale1) / 1000;
            }
            else if (values.Length > 2)
            {
                double tempScale1 = double.Parse(values[0], usCulture);
                double tempScale2 = double.Parse(values[1], usCulture);
                double tempScale3 = double.Parse(values[2], usCulture);
                return new Vector3D(tempScale1, tempScale3, tempScale2) / 1000;
            }

            return new Vector3D(1, 1, 1);
        }

        private Rotation3D ParseRotation(string vector)
        {
            Vector3D tempRotation = ParseVector3D(vector);
            Vector3D factor = new Vector3D(GetFactor(tempRotation.X), GetFactor(tempRotation.Y), GetFactor(tempRotation.Z));

            tempRotation.X = GetPositive(tempRotation.X);
            tempRotation.Y = GetPositive(tempRotation.Y);
            tempRotation.Z = GetPositive(tempRotation.Z);

            double max = Math.Max(Math.Max(tempRotation.X, tempRotation.Y), tempRotation.Z);
            if (max != 0)
            {
                tempRotation = tempRotation / max;
                return new AxisAngleRotation3D(new Vector3D(tempRotation.X * factor.X, tempRotation.Z * factor.Z, tempRotation.Y * factor.Y), max);
            }

            return null;
        }

        private double GetFactor(double number)
        {
            if (number < 0)
                return -1;
            return 1;
        }

        private double GetPositive(double number)
        {
            if (number < 0)
                return number * -1;
            return number;
        }

        private ZoneShape ParseShape(string shape)
        {
            shape = shape.ToLower();
            if (shape == "box")
                return ZoneShape.Box;
            else if (shape == "sphere")
                return ZoneShape.Sphere;
            else if (shape == "cylinder")
                return ZoneShape.Cylinder;
            else
                return ZoneShape.Ellipsoid;
        }

        private Vector3D ParseVector3D(string vector)
        {
            return Vector3D.Parse(vector);
        }

        private Vector ParseVector(string vector)
        {
            return Vector.Parse(vector);
        }
    }
}
