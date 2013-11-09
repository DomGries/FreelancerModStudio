using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Media.Media3D;
using FreelancerModStudio.Data;
using FreelancerModStudio.Data.INI;
using FreelancerModStudio.SystemPresenter.Content;

namespace FreelancerModStudio.SystemPresenter
{
    public static class SystemParser
    {
        public const double SIZE_FACTOR = 0.005;
        public const double MODEL_PREVIEW_SCALE = 1000;
        public const double UNIVERSE_SCALE = 1;
        public const double UNIVERSE_SYSTEM_SCALE = 0.2 * UNIVERSE_SCALE;
        public const double UNIVERSE_CONNECTION_SCALE = 0.04 * UNIVERSE_SCALE;
        public const double UNIVERSE_DOUBLE_CONNECTION_SCALE = 2.5 * UNIVERSE_CONNECTION_SCALE;
        public const double UNIVERSE_AXIS_CENTER = 7.5;

        public static KeyValuePair<string, ArchetypeInfo> GetArchetypeInfo(EditorINIBlock block)
        {
            if (block.Name.Equals("solar", StringComparison.OrdinalIgnoreCase))
            {
                ContentType type = ContentType.None;
                string name = null;
                double radius = 0d;
                string cmpFile = null;

                foreach (EditorINIOption option in block.Options)
                {
                    if (option.Values.Count > 0)
                    {
                        switch (option.Name.ToLowerInvariant())
                        {
                            case "nickname":
                                name = option.Values[0].Value.ToString();
                                break;
                            case "solar_radius":
                                radius = Parser.ParseDouble(option.Values[0].Value.ToString(), 1);
                                break;
                            case "type":
                                type = ParseContentType(option.Values[0].Value.ToString());
                                break;
                            case "da_archetype":
                                cmpFile = option.Values[0].Value.ToString();
                                break;
                        }
                    }
                }

                if (name != null)
                {
                    if (type == ContentType.Planet || type == ContentType.Sun)
                    {
                        //save radius only for planets and suns
                        if (radius != 0.0)
                        {
                            return new KeyValuePair<string, ArchetypeInfo>(name, new ArchetypeInfo
                                {
                                    Type = type,
                                    Radius = radius
                                });
                        }
                    }
                    else if (type != ContentType.None && cmpFile != null)
                    {
                        //save model path only for supported objects (not planets and suns)
                        return new KeyValuePair<string, ArchetypeInfo>(name, new ArchetypeInfo
                            {
                                Type = type,
                                ModelPath = cmpFile
                            });
                    }
                }
            }
            return new KeyValuePair<string, ArchetypeInfo>(null, null);
        }

        public static ArchetypeInfo GetModelPreviewInfo(EditorINIBlock block)
        {
            foreach (EditorINIOption option in block.Options)
            {
                if (option.Values.Count > 0)
                {
                    switch (option.Name.ToLowerInvariant())
                    {
                        case "da_archetype":
                            return new ArchetypeInfo
                                {
                                    Type = ContentType.ModelPreview,
                                    ModelPath = option.Values[0].Value.ToString()
                                };
                    }
                }
            }
            return null;
        }

        public static void SetUniverseObjectType(TableBlock block)
        {
            if (block.Block.Name.Equals("system", StringComparison.OrdinalIgnoreCase))
            {
                block.ObjectType = ContentType.System;
            }
        }

        public static void SetSolarArchetypeObjectType(TableBlock block)
        {
            KeyValuePair<string, ArchetypeInfo> info = GetArchetypeInfo(block.Block);
            if (info.Key != null)
            {
                block.Archetype = info.Value;
                block.ObjectType = info.Value.Type;
            }
        }

        public static void SetModelPreviewObjectType(TableBlock block)
        {
            ArchetypeInfo info = GetModelPreviewInfo(block.Block);
            if (info != null)
            {
                block.Archetype = info;
                block.ObjectType = info.Type;
            }
        }

        public static void SetObjectType(TableBlock block, ArchetypeManager archetypeManager)
        {
            switch (block.Block.Name.ToLowerInvariant())
            {
                case "lightsource":
                    block.ObjectType = ContentType.LightSource;
                    return;
                case "object":
                    {
                        if (archetypeManager != null)
                        {
                            //get type of object based on archetype
                            foreach (EditorINIOption option in block.Block.Options)
                            {
                                if (option.Name.Equals("archetype", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (option.Values.Count > 0)
                                    {
                                        block.Archetype = archetypeManager.TypeOf(option.Values[0].Value.ToString());
                                        if (block.Archetype != null)
                                        {
                                            block.ObjectType = block.Archetype.Type;
                                            return;
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                        break;
                    }
                case "zone":
                    {
                        string shape = "box";
                        int flags = 0;

                        const int exclusionFlag = 0x10000 | 0x20000; // exclusion type 1 + exclusion type 2

                        foreach (EditorINIOption option in block.Block.Options)
                        {
                            if (option.Values.Count > 0)
                            {
                                string value = option.Values[0].Value.ToString();
                                switch (option.Name.ToLowerInvariant())
                                {
                                    case "lane_id":
                                        // overrides exclusion zones as those are set after the loop
                                        block.ObjectType = ContentType.ZonePathTradeLane;
                                        return;
                                    case "usage":
                                        string[] values = value.Split(new[] { ',' });
                                        foreach (string valueEntry in values)
                                        {
                                            if (valueEntry.Equals("trade", StringComparison.OrdinalIgnoreCase))
                                            {
                                                block.ObjectType = ContentType.ZonePathTrade;
                                                return;
                                            }
                                        }
                                        block.ObjectType = ContentType.ZonePath;
                                        return;
                                    case "vignette_type":
                                        switch (value.ToLowerInvariant())
                                        {
                                            case "open":
                                            case "field":
                                            case "exclusion":
                                                block.ObjectType = ContentType.ZoneVignette;
                                                return;
                                        }
                                        break;
                                    case "shape":
                                        shape = value;
                                        break;
                                    case "property_flags":
                                        flags = Parser.ParseInt(value, 0);
                                        break;
                                }
                            }
                        }

                        bool isExclusion = (flags & exclusionFlag) != 0;

                        // set type based on shape and flags
                        switch (shape.ToLowerInvariant())
                        {
                            case "sphere":
                                block.ObjectType = isExclusion ? ContentType.ZoneSphereExclusion : ContentType.ZoneSphere;
                                return;
                            default: // ellipsoid
                                block.ObjectType = isExclusion ? ContentType.ZoneEllipsoidExclusion : ContentType.ZoneEllipsoid;
                                return;
                            case "cylinder":
                                block.ObjectType = isExclusion ? ContentType.ZoneCylinderExclusion : ContentType.ZoneCylinder;
                                return;
                            case "ring":
                                block.ObjectType = ContentType.ZoneRing; // rings can't be used as exclusions
                                return;
                            case "box":
                                block.ObjectType = isExclusion ? ContentType.ZoneBoxExclusion : ContentType.ZoneBox;
                                return;
                        }
                    }
            }
            block.ObjectType = ContentType.None;
        }

        public static bool SetBlock(ContentBase content, TableBlock block)
        {
            // update the model if the object type or the archetype was changed
            bool modelChanged =
                content.Block == null ||
                content.Block.ObjectType != block.ObjectType ||
                content.Block.Archetype != block.Archetype;

            // set reference to block (this one is different than the one passed in the argument because a new copy was create in the undomanager)
            content.Block = block;

            return modelChanged;
        }

        public static bool SetModelPreviewValues(ContentBase content, TableBlock block)
        {
            content.Scale = new Vector3D(MODEL_PREVIEW_SCALE, MODEL_PREVIEW_SCALE, MODEL_PREVIEW_SCALE);
            content.UpdateTransform(false);
            return SetBlock(content, block);
        }

        public static bool SetValues(ContentBase content, TableBlock block, bool animate)
        {
            string positionString = "0,0,0";
            string rotationString = "0,0,0";
            string scaleString = "1,1,1";
            string fileString = string.Empty;

            //get properties of content
            foreach (EditorINIOption option in block.Block.Options)
            {
                if (option.Values.Count > 0)
                {
                    string value = option.Values[0].Value.ToString();
                    switch (option.Name.ToLowerInvariant())
                    {
                        case "pos":
                            positionString = value;
                            break;
                        case "rotate":
                            rotationString = value;
                            break;
                        case "size":
                            scaleString = value;
                            break;
                        case "file":
                            fileString = value;
                            break;
                    }
                }
            }

            content.Position = ParsePosition(positionString);

            //set content values
            switch (block.ObjectType)
            {
                case ContentType.System:
                    content.Position = ParseUniverseVector(positionString);
                    content.Scale = new Vector3D(UNIVERSE_SYSTEM_SCALE, UNIVERSE_SYSTEM_SCALE, UNIVERSE_SYSTEM_SCALE);

                    Content.System system = (Content.System)content;
                    system.Path = fileString;
                    break;
                case ContentType.LightSource:
                    content.Scale = new Vector3D(1, 1, 1);
                    content.Rotation = ParseRotation(rotationString, false);
                    break;
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
                    content.Scale = new Vector3D(1, 1, 1);
                    content.Rotation = ParseRotation(rotationString, false);

                    if (block.Archetype != null)
                    {
                        if (block.Archetype.Radius != 0.0)
                        {
                            content.Scale = new Vector3D(block.Archetype.Radius, block.Archetype.Radius, block.Archetype.Radius) * SIZE_FACTOR;
                        }
                    }
                    break;
                default: // all zones
                    content.Scale = ParseScale(scaleString, block.ObjectType);
                    content.Rotation = ParseRotation(rotationString, IsCylinder(block.ObjectType));
                    break;
            }

            content.UpdateTransform(animate);
            return SetBlock(content, block);
        }

        public static Vector3D ParseVector(string vector)
        {
            //Use Vector3D.Parse after implementation of type handling
            string[] values = vector.Split(new[] { ',' });
            if (values.Length > 2)
            {
                return new Vector3D(Parser.ParseDouble(values[0], 0), -Parser.ParseDouble(values[2], 0), Parser.ParseDouble(values[1], 0));
            }

            return new Vector3D(0, 0, 0);
        }

        public static Vector3D ParsePosition(string vector)
        {
            return ParseVector(vector) * SIZE_FACTOR;
        }

        public static Vector3D ParseRotation(string vector, bool isCylinder)
        {
            Vector3D rotation = ParseVector(vector);

            // our cylinder meshes are by default not rotated like DirectX cylinders
            if (isCylinder)
            {
                rotation.X += 90;
            }

            return rotation;
        }

        public static Vector3D ParseScale(string scale, ContentType type)
        {
            string[] values = scale.Split(new[] { ',' });

            switch (type)
            {
                case ContentType.ZoneSphere:
                case ContentType.ZoneSphereExclusion:
                case ContentType.ZoneVignette:
                    if (values.Length > 0)
                    {
                        double tempScale = Parser.ParseDouble(values[0], 1);
                        return new Vector3D(tempScale, tempScale, tempScale) * SIZE_FACTOR;
                    }
                    break;
                case ContentType.ZonePath:
                case ContentType.ZonePathTrade:
                    if (values.Length > 1)
                    {
                        double tempScale1 = Parser.ParseDouble(values[0], 1);
                        double tempScale2 = Parser.ParseDouble(values[1], 1);
                        return new Vector3D(tempScale1, tempScale2, tempScale1) * SIZE_FACTOR;
                    }
                    break;
                case ContentType.ZoneRing:
                    if (values.Length > 2)
                    {
                        double outerRadius = Parser.ParseDouble(values[0], 1);
                        double innerRadius = Parser.ParseDouble(values[1], 1);
                        double length = Parser.ParseDouble(values[2], 1);

                        if (innerRadius > outerRadius)
                        {
                            return new Vector3D(innerRadius, length, innerRadius) * SIZE_FACTOR;
                        }

                        return new Vector3D(outerRadius, length, outerRadius) * SIZE_FACTOR;
                    }
                    break;
                default:
                    if (values.Length > 2)
                    {
                        return new Vector3D(Parser.ParseDouble(values[0], 1), Parser.ParseDouble(values[2], 1), Parser.ParseDouble(values[1], 1)) * SIZE_FACTOR;
                    }
                    break;
            }

            return new Vector3D(1, 1, 1);
        }

        public static Vector3D ParseUniverseVector(string vector)
        {
            //Use Point.Parse after implementation of type handling
            string[] values = vector.Split(new[] { ',' });
            if (values.Length > 1)
            {
                double tempScale1 = Parser.ParseDouble(values[0], 0);
                double tempScale2 = Parser.ParseDouble(values[1], 0);
                return new Vector3D(tempScale1 - UNIVERSE_AXIS_CENTER, -tempScale2 + UNIVERSE_AXIS_CENTER, 0) * UNIVERSE_SCALE;
            }
            return new Vector3D(0, 0, 0);
        }

        public static ContentType ParseContentType(string type)
        {
            switch (type.ToLowerInvariant())
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

        static bool IsCylinder(ContentType type)
        {
            switch (type)
            {
                case ContentType.ZonePath:
                case ContentType.ZonePathTrade:
                case ContentType.ZoneCylinder:
                case ContentType.ZoneCylinderExclusion:
                case ContentType.ZoneRing:
                    return true;
            }

            return false;
        }

        public static void WriteBlock(ContentBase content)
        {
            bool isUniverse = content.Block.ObjectType == ContentType.System;

            //get properties of content
            foreach (EditorINIOption option in content.Block.Block.Options)
            {
                switch (option.Name.ToLowerInvariant())
                {
                    case "pos":
                        if (option.Values.Count > 0)
                        {
                            option.Values[0].Value = WritePosition(content.Position, isUniverse);
                        }
                        else
                        {
                            option.Values.Add(new EditorINIEntry
                                (
                                    WritePosition(content.Position, isUniverse)
                                ));
                        }
                        break;
                    case "rotate":
                        if (option.Values.Count > 0)
                        {
                            if (!IsZeroRounded(content.Rotation))
                            {
                                option.Values[0].Value = WriteRotation(content.Rotation, IsCylinder(content.Block.ObjectType));
                            }
                            else
                            {
                                option.Values.Clear();
                            }
                        }
                        else if (!IsZeroRounded(content.Rotation))
                        {
                            option.Values.Add(new EditorINIEntry
                                (
                                    WriteRotation(content.Rotation, IsCylinder(content.Block.ObjectType))
                                ));
                        }
                        break;
                    case "size":
                        if (option.Values.Count > 0)
                        {
                            option.Values[0].Value = WriteScale(content.Scale, content.Block.ObjectType);
                        }
                        else
                        {
                            option.Values.Add(new EditorINIEntry
                                (
                                    WriteScale(content.Scale, content.Block.ObjectType)
                                ));
                        }
                        break;
                }
            }
        }

        public static string WriteUniverseVector(Vector3D value)
        {
            Helper.String.StringBuilder.Length = 0;

            WriteDouble(Math.Round((value.X + UNIVERSE_AXIS_CENTER) / UNIVERSE_SCALE, 1));
            Helper.String.StringBuilder.Append(", ");
            WriteDouble(Math.Round((-value.Y + UNIVERSE_AXIS_CENTER) / UNIVERSE_SCALE, 1));

            return Helper.String.StringBuilder.ToString();
        }

        public static string WriteVector(Vector3D value)
        {
            Helper.String.StringBuilder.Length = 0;

            WriteDouble(Math.Round(value.X));
            Helper.String.StringBuilder.Append(", ");
            WriteDouble(Math.Round(value.Z));
            Helper.String.StringBuilder.Append(", ");
            WriteDouble(Math.Round(-value.Y));

            return Helper.String.StringBuilder.ToString();
        }

        public static string WritePosition(Vector3D value, bool isUniverse)
        {
            if (isUniverse)
            {
                return WriteUniverseVector(value);
            }

            return WriteVector(value / SIZE_FACTOR);
        }

        public static string WriteRotation(Vector3D value, bool isCylinder)
        {
            // our cylinder meshes are by default not rotated like DirectX cylinders
            if (isCylinder)
            {
                value.X -= 90;
            }

            return WriteVector(value);
        }

        public static string WriteScale(Vector3D value, ContentType type)
        {
            Helper.String.StringBuilder.Length = 0;

            switch (type)
            {
                case ContentType.ZoneSphere:
                case ContentType.ZoneSphereExclusion:
                case ContentType.ZoneVignette:
                    WriteDouble(Math.Round(value.X / SIZE_FACTOR));
                    break;
                case ContentType.ZonePath:
                case ContentType.ZonePathTrade:
                    WriteDouble(Math.Round(value.X / SIZE_FACTOR));
                    Helper.String.StringBuilder.Append(", ");
                    WriteDouble(Math.Round(value.Y / SIZE_FACTOR));
                    break;
                default:
                    WriteDouble(Math.Round(value.X / SIZE_FACTOR));
                    Helper.String.StringBuilder.Append(", ");
                    WriteDouble(Math.Round(value.Z / SIZE_FACTOR));
                    Helper.String.StringBuilder.Append(", ");
                    WriteDouble(Math.Round(value.Y / SIZE_FACTOR));
                    break;
            }

            return Helper.String.StringBuilder.ToString();
        }

        static void WriteDouble(double value)
        {
            Helper.String.StringBuilder.Append(value.ToString(CultureInfo.InvariantCulture));
        }

        static bool IsZeroRounded(Vector3D value)
        {
            return IsZeroRounded(value.X) && IsZeroRounded(value.Y) && IsZeroRounded(value.Z);
        }

        static bool IsZeroRounded(double value)
        {
            return value > -0.5 && value < 0.5;
        }
    }
}
