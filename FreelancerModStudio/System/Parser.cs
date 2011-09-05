using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using FreelancerModStudio.Data;
using FreelancerModStudio.Data.IO;

namespace FreelancerModStudio.SystemPresenter
{
    public class SystemParser
    {
        public bool ModelChanged { get; set; }

        public void SetValues(ContentBase content, TableBlock block)
        {
            string positionString = "0,0,0";
            string rotationString = "0,0,0";
            string shapeString = "box";
            string scaleString = "1,1,1";
            string usageString = "";
            string vignetteString = "";
            string flagsString = "";
            string fileString = "";

            //get properties of content
            foreach (EditorINIOption option in block.Block.Options)
            {
                if (option.Values.Count > 0)
                {
                    string value = option.Values[0].Value.ToString();
                    switch (option.Name.ToLower())
                    {
                        case "pos":
                            positionString = value;
                            break;
                        case "rotate":
                            rotationString = value;
                            break;
                        case "shape":
                            shapeString = value;
                            break;
                        case "size":
                            scaleString = value;
                            break;
                        case "usage":
                            usageString = value.ToLower();
                            break;
                        case "vignette_type":
                            vignetteString = value.ToLower();
                            break;
                        case "property_flags":
                            flagsString = value;
                            break;
                        case "file":
                            fileString = value;
                            break;
                    }
                }
            }

            Vector3D position = Parser.ParsePosition(positionString);
            Vector3D rotation;
            Vector3D scale;

            //set content values
            if (block.ObjectType == ContentType.Zone)
            {
                Zone zone = (Zone)content;

                ZoneShape oldShape = zone.Shape;
                ZoneType oldType = zone.Type;

                zone.Shape = Parser.ParseShape(shapeString);

                if (usageString == "trade" || usageString == "patrol")
                    zone.Type = ZoneType.Path;
                else if (vignetteString == "open" || vignetteString == "field" || vignetteString == "exclusion")
                    zone.Type = ZoneType.Vignette;
                else if (flagsString == "131072")
                    zone.Type = ZoneType.Exclusion;
                else
                    zone.Type = ZoneType.Zone;

                rotation = Parser.ParseRotation(rotationString, zone.Type == ZoneType.Path);
                scale = Parser.ParseScale(scaleString, zone.Shape);

                if (zone.Shape != oldShape || zone.Type != oldType)
                    ModelChanged = true;
            }
            else if (block.ObjectType == ContentType.LightSource)
            {
                scale = new Vector3D(1, 1, 1);
                rotation = Parser.ParseRotation(rotationString, false);
            }
            else if (block.ObjectType == ContentType.System)
            {
                position = Parser.ParseUniverseVector(positionString);
                scale = new Vector3D(2, 2, 2);
                rotation = Parser.ParseRotation(rotationString, false);

                System system = (System)content;
                system.Path = fileString;
            }
            else
            {
                if (block.Archetype != null)
                {
                    scale = new Vector3D(block.Archetype.Radius, block.Archetype.Radius, block.Archetype.Radius) / 1000;
                    if (block.ObjectType != ContentType.Planet && block.ObjectType != ContentType.Sun)
                    {
                        //clamp scale of objects which size is actually not being defined by archetype radius
                        if (scale.X < 0.1)
                            scale = new Vector3D(0.1, 0.1, 0.1);
                        else if (scale.X > 1)
                            scale = new Vector3D(1, 1, 1);
                    }
                }
                else
                    scale = new Vector3D(1, 1, 1);

                rotation = Parser.ParseRotation(rotationString, false);
            }

            content.SetDisplay(position, rotation, scale);
        }
    }

    public static class Parser
    {
        public static double ParseDouble(string text, double defaultValue)
        {
            double value;
            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                return value;

            return defaultValue;
        }

        public static Vector3D ParseScale(string scale, ZoneShape shape)
        {
            string[] values = scale.Split(new char[] { ',' });

            if (shape == ZoneShape.Sphere && values.Length > 0)
            {
                double tempScale = ParseDouble(values[0], 1);
                return new Vector3D(tempScale, tempScale, tempScale) / 1000;
            }
            else if (shape == ZoneShape.Cylinder && values.Length > 1)
            {
                double tempScale1 = ParseDouble(values[0], 1);
                double tempScale2 = ParseDouble(values[1], 1);
                return new Vector3D(tempScale1, tempScale2, tempScale1) / 1000;
            }
            else if (values.Length > 2)
            {
                double tempScale1 = ParseDouble(values[0], 1);
                double tempScale2 = ParseDouble(values[1], 1);
                double tempScale3 = ParseDouble(values[2], 1);
                return new Vector3D(tempScale1, tempScale3, tempScale2) / 1000;
            }

            return new Vector3D(1, 1, 1);
        }

        public static Vector3D ParseRotation(string vector, bool pathRotation)
        {
            Vector3D tempRotation = ParseVector(vector);

            if (pathRotation)
            {
                tempRotation.X += 90;
                tempRotation.Z *= 2;
            }

            return new Vector3D(tempRotation.X, tempRotation.Z, tempRotation.Y);
        }

        public static ZoneShape ParseShape(string shape)
        {
            shape = shape.ToLower();
            if (shape == "box")
                return ZoneShape.Box;
            else if (shape == "sphere")
                return ZoneShape.Sphere;
            else if (shape == "cylinder")
                return ZoneShape.Cylinder;
            else if (shape == "ring")
                return ZoneShape.Ring;
            else
                return ZoneShape.Ellipsoid;
        }

        public static Vector3D ParseUniverseVector(string vector)
        {
            //Use Point.Parse after implementation of type handling
            string[] values = vector.Split(new char[] { ',' });
            if (values.Length > 1)
            {
                double tempScale1 = ParseDouble(values[0], 0);
                double tempScale2 = ParseDouble(values[1], 0);
                return new Vector3D(tempScale1 - 7, -tempScale2 + 7, 0) / 0.09;
            }
            return new Vector3D(0, 0, 0);
        }

        public static Vector3D ParseVector(string vector)
        {
            //Use Vector3D.Parse after implementation of type handling
            string[] values = vector.Split(new char[] { ',' });
            if (values.Length > 2)
            {
                double tempScale1 = ParseDouble(values[0], 0);
                double tempScale2 = ParseDouble(values[1], 0);
                double tempScale3 = ParseDouble(values[2], 0);
                return new Vector3D(tempScale1, tempScale2, tempScale3);
            }
            return new Vector3D(0, 0, 0);
        }

        public static Vector3D ParsePosition(string vector)
        {
            Vector3D tempVector = ParseVector(vector);
            return new Vector3D(tempVector.X, -tempVector.Z, tempVector.Y) / 1000;
        }

        public static double GetFactor(double number)
        {
            if (number < 0)
                return -1;
            return 1;
        }

        public static double GetPositive(double number)
        {
            if (number < 0)
                return number * -1;
            return number;
        }

        public static ContentBase ParseContentBase(ContentType type)
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

        public static ContentType ParseContentType(string type)
        {
            type = type.ToLower();

            if (type == "jump_hole")
                return ContentType.JumpHole;
            else if (type == "jump_gate")
                return ContentType.JumpGate;
            else if (type == "sun")
                return ContentType.Sun;
            else if (type == "planet")
                return ContentType.Planet;
            else if (type == "station")
                return ContentType.Station;
            else if (type == "destroyable_depot")
                return ContentType.Depot;
            else if (type == "satellite")
                return ContentType.Satellite;
            else if (type == "mission_satellite")
                return ContentType.Ship;
            else if (type == "weapons_platform")
                return ContentType.WeaponsPlatform;
            else if (type == "docking_ring")
                return ContentType.DockingRing;
            else if (type == "tradelane_ring")
                return ContentType.TradeLane;
            else if (type == "non_targetable")
                return ContentType.Construct;
            else if (type == "airlock_gate")
                return ContentType.JumpGate;

            return ContentType.None;
        }
    }
}
