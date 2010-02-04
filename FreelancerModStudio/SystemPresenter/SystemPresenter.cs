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
        public List<ContentBase> Objects { get; set; }
        public HelixView3D Viewport { get; set; }
        public ArchtypeManager Archtype { get; set; }

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
                    Viewport.Add(value);

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
                selectedContent = value;

                //goto content
                Viewport.LookAt(value.Position.ToPoint3D(), ContentAnimator.AnimationDuration.TimeSpan.TotalMilliseconds);

                //select content visually
                Point3DCollection points = new Point3DCollection();
                Point3DCollection positions = SelectedContent.GetMesh().Positions;
                for (int i = 0; i < positions.Count - 1; i++)
                {
                    points.Add(positions[i]);
                    points.Add(positions[i + 1]);
                }
                points.Add(positions[positions.Count - 1]);
                points.Add(positions[0]);

                WireLines lines = GetWireBox(SelectedContent.GetMesh().Bounds);

                ContentAnimator.AddTransformation(lines, new TranslateTransform3D(value.Position));
                ContentAnimator.AddTransformation(lines, new RotateTransform3D(value.Rotation, value.Position.ToPoint3D()));
                ContentAnimator.AddTransformation(lines, new ScaleTransform3D(value.Scale, value.Position.ToPoint3D()));

                Selection = lines;
                Viewport.Title = GetTitle(value.Block);
            }
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

        public SystemPresenter(HelixView3D viewport)
        {
            Objects = new List<ContentBase>();
            Viewport = viewport;
        }

        public void Show(TableData data)
        {
            LoadObjects(data);
            //ContentAnimator.AnimationDuration = new Duration(TimeSpan.FromMilliseconds(2000));

            ClearDisplay(false);

            foreach (ContentBase content in Objects)
            {
                content.LoadModel();
                Viewport.Add(content.Model);
            }

            ContentAnimator.AnimationDuration = new Duration(TimeSpan.FromMilliseconds(500));
        }

        public void ClearDisplay(bool light)
        {
            for (int i = Viewport.Children.Count - 1; i >= 0; i--)
            {
                ModelVisual3D model = (ModelVisual3D)Viewport.Children[i];
                if (light || model != Lightning)
                    Viewport.Remove(model);
            }
        }

        public void LoadArchtypes(string file, int templateIndex)
        {
            Archtype = new ArchtypeManager(file, templateIndex);
        }

        private void LoadObjects(TableData data)
        {
            foreach (TableBlock block in data.Blocks)
            {
                ContentBase content = null;
                if ((BlockType)block.Block.TemplateIndex == BlockType.Object)
                    content = GetContent(block.Block, BlockType.Object);
                else if ((BlockType)block.Block.TemplateIndex == BlockType.LightSource)
                    content = GetContent(block.Block, BlockType.LightSource);
                else if ((BlockType)block.Block.TemplateIndex == BlockType.Zone)
                    content = GetContent(block.Block, BlockType.Zone);

                if (content != null)
                    Objects.Add(content);
            }
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

        private ContentBase GetContent(EditorINIBlock block, BlockType blockType)
        {
            int positionIndex = -1;
            int rotationIndex = -1;
            int scaleIndex = -1;

            //get type of content
            ContentType type = ContentType.None;
            if (blockType == BlockType.LightSource)
            {
                type = ContentType.LightSource;

                positionIndex = (int)LightSourceOptionType.Position;
                rotationIndex = (int)LightSourceOptionType.Rotation;
            }
            else if (blockType == BlockType.Zone)
            {
                type = ContentType.Zone;

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
            Rotation3D rotation = null;
            Vector3D scale = new Vector3D(1, 1, 1);
            ZoneShape shape = ZoneShape.Box;

            //get transformation of content
            foreach (EditorINIOption option in block.Options)
            {
                if (option.Values.Count > 0)
                {
                    if (option.TemplateIndex == positionIndex)
                    {
                        Vector3D tempPosition = ParseVector3D(option.Values[0].Value.ToString()) / 1000;
                        position = new Vector3D(tempPosition.X, -tempPosition.Z, tempPosition.Y);
                    }
                    else if (option.TemplateIndex == rotationIndex)
                    {
                        Vector3D tempRotation = ParseVector3D(option.Values[0].Value.ToString());
                        double max = tempRotation.Max();
                        if (max != 0)
                            rotation = new AxisAngleRotation3D(new Vector3D(tempRotation.X, tempRotation.Z, tempRotation.Y) / max, max);
                    }
                    else if (option.TemplateIndex == scaleIndex)
                        scale = ParseSize(option.Values[0].Value.ToString());
                    else if (option.TemplateIndex == (int)ZoneOptionType.Shape)
                        shape = ParseShape(option.Values[0].Value.ToString());
                }

                //get type of object based on archtype
                if (blockType == BlockType.Object)
                {
                    if ((ObjectOptionType)option.TemplateIndex == ObjectOptionType.Archtype)
                    {
                        if (option.Values.Count > 0)
                        {
                            ArchtypeInfo info = Archtype.TypeOf(option.Values[0].Value.ToString());
                            if (info != null)
                            {
                                type = info.Type;
                                scale = new Vector3D(info.Radius, info.Radius, info.Radius) / 1000;
                            }
                        }
                    }
                }
            }

            //set content
            ContentBase content = GetContentFromType(type);

            if (content == null)
                return null;

            if (type == ContentType.Zone)
                ((Zone)content).Shape = shape;

            if (position != content.Position)
                content.Position = position;

            if (rotation != null && rotation != content.Rotation)
                content.Rotation = rotation;

            if (scale != content.Scale)
                content.Scale = scale;

            content.Block = block;
            return content;
        }

        private int OccuranceCount(string text, string value)
        {
            int count = 0;
            int index;
            while ((index = text.IndexOf(value)) != -1)
            {
                text = text.Substring(index + 1);
                count++;
            }

            return count;
        }

        private Vector3D ParseSize(string scale)
        {
            CultureInfo usCulture = new CultureInfo("en-US", false);
            int occurances = OccuranceCount(scale, ",");

            if (occurances == 0)
            {
                double tempScale = double.Parse(scale, usCulture);
                return new Vector3D(tempScale, tempScale, tempScale) / 1000;
            }
            else if (occurances == 1)
            {
                Vector tempScale = ParseVector(scale) / 1000;
                return new Vector3D(tempScale.X, tempScale.Y, tempScale.Y);
            }
            else if (occurances == 2)
            {
                Vector3D tempScale = ParseVector3D(scale) / 1000;
                return new Vector3D(tempScale.X, tempScale.Z, tempScale.Y);
            }

            return new Vector3D(1, 1, 1);
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

    enum BlockType
    {
        LightSource = 10,
        Object = 11,
        Zone = 12,
        Other
    }

    enum LightSourceOptionType
    {
        Color = 2,
        Position = 6,
        Rotation = 8,
        Other
    }

    enum ObjectOptionType
    {
        Archtype = 2,
        Position = 20,
        Rotation = 24,
        Other
    }

    enum ZoneOptionType
    {
        Position = 22,
        Rotation = 28,
        Shape = 29,
        Size = 30,
        Spin = 34,
        Other
    }
}
