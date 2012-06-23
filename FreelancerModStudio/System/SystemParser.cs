using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using FreelancerModStudio.Data;
using FreelancerModStudio.Data.IO;
using FreelancerModStudio.SystemPresenter.Content;

namespace FreelancerModStudio.SystemPresenter
{
    public static class SystemParser
    {
        public const double SIZE_FACTOR = 0.005;

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
                        // ReSharper disable CompareOfFloatsByEqualityOperator
                        if (radius != 0d)
                        // ReSharper restore CompareOfFloatsByEqualityOperator
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

            Vector3D position = ParsePosition(positionString);
            Vector3D rotation;
            Vector3D scale;

            //set content values
            switch (block.ObjectType)
            {
                case ContentType.System:
                    position = ParseUniverseVector(positionString);
                    scale = new Vector3D(8, 8, 8);
                    rotation = ParseRotation(rotationString, false);

                    Content.System system = (Content.System)content;
                    system.Path = fileString;
                    break;
                case ContentType.LightSource:
                    scale = new Vector3D(1, 1, 1);
                    rotation = ParseRotation(rotationString, false);
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
                    scale = new Vector3D(1, 1, 1);
                    rotation = ParseRotation(rotationString, false);

                    if (block.Archetype != null)
                    {
                        if (block.Archetype.Radius != 0d)
                        {
                            scale = new Vector3D(block.Archetype.Radius, block.Archetype.Radius, block.Archetype.Radius)*SIZE_FACTOR;
                        }
                    }
                    break;
                default: // all zones
                    rotation = ParseRotation(rotationString,
                        block.ObjectType == ContentType.ZonePath ||
                        block.ObjectType == ContentType.ZonePathTrade ||
                        block.ObjectType == ContentType.ZoneCylinder ||
                        block.ObjectType == ContentType.ZoneCylinderExclusion ||
                        block.ObjectType == ContentType.ZoneRing);
                    scale = ParseScale(scaleString, block.ObjectType);
                    break;
            }

            // update the model if the object type was changed
            bool modelChanged = content.Block == null || content.Block.ObjectType != block.ObjectType;

            // set reference to block (this one is different than the one passed in the argument because a new copy was create in the undomanager)
            content.Block = block;

            content.SetTransform(position, rotation, scale, animate);

            return modelChanged;
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
                        return new Vector3D(tempScale, tempScale, tempScale)*SIZE_FACTOR;
                    }
                    break;
                case ContentType.ZonePath:
                case ContentType.ZonePathTrade:
                    if (values.Length > 1)
                    {
                        double tempScale1 = Parser.ParseDouble(values[0], 1);
                        double tempScale2 = Parser.ParseDouble(values[1], 1);
                        return new Vector3D(tempScale1, tempScale2, tempScale1)*SIZE_FACTOR;
                    }
                    break;
                default:
                    if (values.Length > 2)
                    {
                        return new Vector3D(Parser.ParseDouble(values[0], 1), Parser.ParseDouble(values[2], 1), Parser.ParseDouble(values[1], 1))*SIZE_FACTOR;
                    }
                    break;
            }

            return new Vector3D(1, 1, 1);
        }

        public static Vector3D ParsePosition(string vector)
        {
            return Parser.ParseVector(vector)*SIZE_FACTOR;
        }

        public static Vector3D ParseRotation(string vector, bool isCylinder)
        {
            Vector3D rotation = Parser.ParseVector(vector);

            // our cylinder meshes are by default not rotated like DirectX cylinders
            if (isCylinder)
            {
                rotation.X += 90;
            }

            return rotation;
        }

        public static Vector3D ParseUniverseVector(string vector)
        {
            const double axisCenter = 7.5;
            const double positionScale = 1/SIZE_FACTOR/4;

            //Use Point.Parse after implementation of type handling
            string[] values = vector.Split(new[] { ',' });
            if (values.Length > 1)
            {
                double tempScale1 = Parser.ParseDouble(values[0], 0);
                double tempScale2 = Parser.ParseDouble(values[1], 0);
                return new Vector3D(tempScale1 - axisCenter, -tempScale2 + axisCenter, 0)*positionScale;
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
    }
}
