namespace FreelancerModStudio.SystemDesigner
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows.Media.Media3D;

    using FreelancerModStudio.Data;
    using FreelancerModStudio.Data.INI;
    using FreelancerModStudio.SystemDesigner.Content;

    using System = FreelancerModStudio.SystemDesigner.Content.System;

    public static class SystemParser
    {
        public const double SystemScale = 0.005;
        public const double ModelPreviewScale = 1000;
        public const double UniverseScale = 1;
        public const double UniverseSystemScale = 0.2 * UniverseScale;
        public const double UniverseConnectionScale = 0.04 * UniverseScale;
        public const double UniverseDoubleConnectionScale = 2.5 * UniverseConnectionScale;
        public const double UniverseAxisCenter = 7.5;

        public static KeyValuePair<string, ArchetypeInfo> GetArchetypeInfo(EditorIniBlock block)
        {
            if (block.Name.Equals("solar", StringComparison.OrdinalIgnoreCase))
            {
                ContentType type = ContentType.None;
                string name = null;
                double radius = 0d;
                string cmpFile = null;

                foreach (EditorIniOption option in block.Options)
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
                        // save radius only for planets and suns
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
                        // save model path only for supported objects (not planets and suns)
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

        public static ArchetypeInfo GetModelPreviewInfo(EditorIniBlock block)
        {
            foreach (EditorIniOption option in block.Options)
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
                            // get type of object based on archetype
                            foreach (EditorIniOption option in block.Block.Options)
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

                        const int ExclusionFlag = 0x10000 | 0x20000; // exclusion type 1 + exclusion type 2

                        foreach (EditorIniOption option in block.Block.Options)
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

                        bool isExclusion = (flags & ExclusionFlag) != 0;

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

        private static bool SetBlock(ContentBase content, TableBlock block, bool animate)
        {
            // update the model if the object type or the archetype was changed
            bool modelChanged =
                content.Block == null ||
                content.Block.ObjectType != block.ObjectType ||
                content.Block.Archetype != block.Archetype;

            // set reference to block (this one is different than the one passed in the argument because a new copy was create in the undomanager)
            content.Block = block;

            // update transform after block was set
            content.UpdateTransform(animate);

            return modelChanged;
        }

        public static bool SetModelPreviewValues(ContentBase content, TableBlock block)
        {
            content.Scale = new Vector3D(ModelPreviewScale, ModelPreviewScale, ModelPreviewScale);

            return SetBlock(content, block, false);
        }

        public static bool SetValues(ContentBase content, TableBlock block, bool animate)
        {
            string positionString = "0,0,0";
            string rotationString = "0,0,0";
            string scaleString = "1,1,1";
            string fileString = string.Empty;

            // get properties of content
            foreach (EditorIniOption option in block.Block.Options)
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

            // set content values
            switch (block.ObjectType)
            {
                case ContentType.System:
                    content.Position = ParseUniverseVector(positionString);
                    content.Scale = new Vector3D(UniverseSystemScale, UniverseSystemScale, UniverseSystemScale);

                    System system = (Content.System)content;
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
                            content.Scale = new Vector3D(block.Archetype.Radius, block.Archetype.Radius, block.Archetype.Radius) * SystemScale;
                        }
                    }

                    break;
                default: // all zones
                    content.Scale = ParseScale(scaleString, block.ObjectType);
                    content.Rotation = ParseRotation(rotationString, IsCylinder(block.ObjectType));
                    break;
            }

            return SetBlock(content, block, animate);
        }

        public static Vector3D ParseVector(string vector)
        {
            // Use Vector3D.Parse after implementation of type handling
            string[] values = vector.Split(new[] { ',' });
            if (values.Length > 2)
                return new Vector3D(Parser.ParseDouble(values[0], 0), -Parser.ParseDouble(values[2], 0), Parser.ParseDouble(values[1], 0));

            return new Vector3D(0d, 0d, 0d);
        }

        public static Vector3D ParsePosition(string vector)
        {
            return ParseVector(vector) * SystemScale;
        }

        public static Vector3D ParseRotation(string vector, bool isCylinder)
        {
            Vector3D rotation = ParseVector(vector);

            // our cylinder meshes are by default not rotated like DirectX cylinders
            if (isCylinder) 
                rotation.X += 90d;

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
                        return new Vector3D(tempScale, tempScale, tempScale) * SystemScale;
                    }

                    break;
                case ContentType.ZonePath:
                case ContentType.ZonePathTrade:
                    if (values.Length > 1)
                    {
                        double tempScale1 = Parser.ParseDouble(values[0], 1);
                        double tempScale2 = Parser.ParseDouble(values[1], 1);
                        return new Vector3D(tempScale1, tempScale2, tempScale1) * SystemScale;
                    }

                    break;
                default:
                    if (values.Length > 2)
                    {
                        return new Vector3D(Parser.ParseDouble(values[0], 1), Parser.ParseDouble(values[2], 1), Parser.ParseDouble(values[1], 1)) * SystemScale;
                    }

                    break;
            }

            return new Vector3D(1, 1, 1);
        }

        public static Vector3D ParseUniverseVector(string vector)
        {
            // Use Point.Parse after implementation of type handling
            string[] values = vector.Split(new[] { ',' });
            if (values.Length > 1)
            {
                double tempScale1 = Parser.ParseDouble(values[0], 0);
                double tempScale2 = Parser.ParseDouble(values[1], 0);
                return new Vector3D(tempScale1 - UniverseAxisCenter, -tempScale2 + UniverseAxisCenter, 0) * UniverseScale;
            }

            return new Vector3D(0, 0, 0);
        }

        public static ContentType ParseContentType(string type)
        {
            switch (type?.ToLowerInvariant())
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

        private static bool IsCylinder(ContentType type)
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

            // get properties of content
            foreach (EditorIniOption option in content.Block.Block.Options)
            {
                switch (option.Name.ToLowerInvariant())
                {
                    case "pos":
                        if (option.Values.Count > 0)
                            option.Values[0].Value = WritePosition(content.Position, isUniverse);
                        else
                            option.Values.Add(new EditorIniEntry(WritePosition(content.Position, isUniverse)));

                        break;
                    case "rotate":
                        if (option.Values.Count > 0)
                        {
                            if (!IsZeroRounded(content.Rotation))
                                option.Values[0].Value = WriteRotation(content.Rotation, IsCylinder(content.Block.ObjectType));
                            else
                                option.Values.Clear();
                        }
                        else if (!IsZeroRounded(content.Rotation))
                            option.Values.Add(new EditorIniEntry(WriteRotation(content.Rotation, IsCylinder(content.Block.ObjectType))));

                        break;
                    case "size":
                        if (option.Values.Count > 0)
                            option.Values[0].Value = WriteScale(content.Scale, content.Block.ObjectType);
                        else
                            option.Values.Add(new EditorIniEntry(WriteScale(content.Scale, content.Block.ObjectType)));

                        break;
                }
            }
        }

        public static string WriteUniverseVector(Vector3D value)
        {
            Helper.String.StringBuilder.Length = 0;

            WriteDouble((value.X + UniverseAxisCenter) / UniverseScale);
            Helper.String.StringBuilder.Append(", ");
            WriteDouble((-value.Y + UniverseAxisCenter) / UniverseScale);

            return Helper.String.StringBuilder.ToString();
        }

        public static string WriteVector(Vector3D value)
        {
            Helper.String.StringBuilder.Length = 0;

            WriteDouble(value.X);
            Helper.String.StringBuilder.Append(", ");
            WriteDouble(value.Z);
            Helper.String.StringBuilder.Append(", ");
            WriteDouble(-value.Y);

            return Helper.String.StringBuilder.ToString();
        }

        public static string WritePosition(Vector3D value, bool isUniverse) => isUniverse ? WriteUniverseVector(value) : WriteVector(value / SystemScale);

        public static string WriteRotation(Vector3D value, bool isCylinder)
        {
            // our cylinder meshes are by default not rotated like DirectX cylinders
            if (isCylinder)
                value.X -= 90;

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
                    WriteDouble(value.X / SystemScale);
                    break;
                case ContentType.ZonePath:
                case ContentType.ZonePathTrade:
                    WriteDouble(value.X / SystemScale);
                    Helper.String.StringBuilder.Append(", ");
                    WriteDouble(value.Y / SystemScale);
                    break;
                default:
                    WriteDouble(value.X / SystemScale);
                    Helper.String.StringBuilder.Append(", ");
                    WriteDouble(value.Z / SystemScale);
                    Helper.String.StringBuilder.Append(", ");
                    WriteDouble(value.Y / SystemScale);
                    break;
            }

            return Helper.String.StringBuilder.ToString();
        }

        private static void WriteDouble(double value)
        {
            value = Helper.Settings.Data.Data.General.RoundFloatingPointValues ? Math.Round(value, 1) : value;
            Helper.String.StringBuilder.Append(value.ToString(CultureInfo.InvariantCulture));
        }

        private static bool IsZeroRounded(Vector3D value) => IsZeroRounded(value.X) && IsZeroRounded(value.Y) && IsZeroRounded(value.Z);

        private static bool IsZeroRounded(double value) => value > -0.5 && value < 0.5;
    }
}
