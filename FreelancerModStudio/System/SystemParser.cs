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
            var positionString = "0,0,0";
            var rotationString = "0,0,0";
            var shapeString = "box";
            var scaleString = "1,1,1";
            var usageString = string.Empty;
            var vignetteString = string.Empty;
            var flagsString = string.Empty;
            var fileString = string.Empty;

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

            Vector3D position = ParsePosition(positionString);
            Vector3D rotation;
            Vector3D scale;

            //set content values
            if (block.ObjectType == ContentType.Zone)
            {
                Zone zone = (Zone)content;

                ZoneShape oldShape = zone.Shape;
                ZoneType oldType = zone.Type;

                zone.Shape = ParseShape(shapeString);

                if (usageString.Contains("trade"))
                    zone.Type = ZoneType.PathTrade;
                else if (usageString.Contains("patrol"))
                    zone.Type = ZoneType.PathPatrol;
                else if (vignetteString == "open" || vignetteString == "field" || vignetteString == "exclusion")
                    zone.Type = ZoneType.Vignette;
                else if (flagsString == "131072")
                    zone.Type = ZoneType.Exclusion;
                else
                    zone.Type = ZoneType.Zone;

                rotation = ParseRotation(rotationString, zone.Type == ZoneType.PathPatrol || zone.Type == ZoneType.PathTrade);
                scale = ParseScale(scaleString, zone.Shape);

                if (zone.Shape != oldShape || zone.Type != oldType)
                    ModelChanged = true;
            }
            else if (block.ObjectType == ContentType.LightSource)
            {
                scale = new Vector3D(1, 1, 1);
                rotation = ParseRotation(rotationString, false);
            }
            else if (block.ObjectType == ContentType.System)
            {
                position = ParseUniverseVector(positionString);
                scale = new Vector3D(2, 2, 2);
                rotation = ParseRotation(rotationString, false);

                System system = (System)content;
                system.Path = fileString;
            }
            else
            {
                if (block.Archetype != null)
                {
                    scale = new Vector3D(block.Archetype.Radius, block.Archetype.Radius, block.Archetype.Radius) / 1000;

                    //clamp scale of objects which size is actually not being defined by archetype radius
                    if (block.ObjectType != ContentType.Planet && block.ObjectType != ContentType.Sun)
                    {
                        if (scale.X < 0.1)
                            scale = new Vector3D(0.1, 0.1, 0.1);
                        else if (scale.X > 1)
                            scale = new Vector3D(1, 1, 1);
                    }

                    //update system object type
                    SystemObject contentObject = content as SystemObject;
                    if (contentObject != null && block.ObjectType != contentObject.Type)
                    {
                        contentObject.Type = block.ObjectType;
                        ModelChanged = true;
                    }

                    //string ext = Path.GetExtension(block.Archetype.ModelPath).ToLower();
                    //if (ext == ".cmp" || ext == ".3db")
                    //{
                    //    var path = Path.Combine(@"E:\Games\FL\DATA", block.Archetype.ModelPath);
                    //    if (File.Exists(path))
                    //    {
                    //        var cmpModel = new CmpModelContent();
                    //        content.Model = cmpModel.LoadModel(path);
                    //    }
                    //}
                }
                else
                    scale = new Vector3D(1, 1, 1);

                rotation = ParseRotation(rotationString, false);
            }

            content.SetDisplay(position, rotation, scale);
        }

        public static Vector3D ParseScale(string scale, ZoneShape shape)
        {
            string[] values = scale.Split(new[] { ',' });

            if (shape == ZoneShape.Sphere && values.Length > 0)
            {
                var tempScale = Parser.ParseDouble(values[0], 1);
                return new Vector3D(tempScale, tempScale, tempScale) / 1000;
            }
            if (shape == ZoneShape.Cylinder && values.Length > 1)
            {
                var tempScale1 = Parser.ParseDouble(values[0], 1);
                var tempScale2 = Parser.ParseDouble(values[1], 1);
                return new Vector3D(tempScale1, tempScale2, tempScale1) / 1000;
            }
            if (values.Length > 2)
                return new Vector3D(Parser.ParseDouble(values[0], 1), Parser.ParseDouble(values[2], 1), Parser.ParseDouble(values[1], 1)) / 1000;

            return new Vector3D(1, 1, 1);
        }

        public static Vector3D ParsePosition(string vector)
        {
            Vector3D tempVector = Parser.ParseVector(vector);
            return new Vector3D(tempVector.X, -tempVector.Z, tempVector.Y) / 1000;
        }

        public static Vector3D ParseRotation(string vector, bool pathRotation)
        {
            Vector3D tempRotation = Parser.ParseVector(vector);

            if (pathRotation)
            {
                tempRotation.X += 90;
                tempRotation.Z *= 2;
            }

            return new Vector3D(tempRotation.X, -tempRotation.Z, tempRotation.Y);
        }

        public static ZoneShape ParseShape(string shape)
        {
            switch (shape.ToLower())
            {
                case "box":
                    return ZoneShape.Box;
                case "sphere":
                    return ZoneShape.Sphere;
                case "cylinder":
                    return ZoneShape.Cylinder;
                case "ring":
                    return ZoneShape.Ring;
                default:
                    return ZoneShape.Ellipsoid;
            }
        }

        public Vector3D ParseUniverseVector(string vector)
        {
            //Use Point.Parse after implementation of type handling
            string[] values = vector.Split(new[] { ',' });
            if (values.Length > 1)
            {
                var tempScale1 = Parser.ParseDouble(values[0], 0);
                var tempScale2 = Parser.ParseDouble(values[1], 0);
                return new Vector3D(tempScale1 - 7, -tempScale2 + 7, 0) / 0.09;
            }
            return new Vector3D(0, 0, 0);
        }

        public double GetFactor(double number)
        {
            if (number < 0)
                return -1;
            return 1;
        }

        public static ContentType ParseContentType(string type)
        {
            switch (type.ToLower())
            {
                case "jump_hole":
                    return ContentType.JumpHole;
                case "jump_gate":
                case "airlock_gate":
                    return ContentType.JumpGate;
                case "sun":
                    return ContentType.Sun;
                case "planet":
                    return ContentType.Planet;
                case "station":
                    return ContentType.Station;
                case "destroyable_depot":
                    return ContentType.Depot;
                case "satellite":
                    return ContentType.Satellite;
                case "mission_satellite":
                    return ContentType.Ship;
                case "weapons_platform":
                    return ContentType.WeaponsPlatform;
                case "docking_ring":
                    return ContentType.DockingRing;
                case "tradelane_ring":
                    return ContentType.TradeLane;
                case "non_targetable":
                    return ContentType.Construct;
            }

            return ContentType.None;
        }
    }
}
