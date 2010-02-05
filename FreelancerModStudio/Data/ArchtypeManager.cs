using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreelancerModStudio.Data.IO;
using System.Globalization;
using FreelancerModStudio.SystemPresenter;

namespace FreelancerModStudio.Data
{
    public class ArchtypeManager
    {
        private SortedList<string, ArchtypeInfo> contentTable;

        public ArchtypeInfo TypeOf(string archtype)
        {
            ArchtypeInfo info;
            if (contentTable.TryGetValue(archtype, out info))
                return info;

            return null;
        }

        public void CreateContentTable(List<TableBlock> blocks)
        {
            CultureInfo usCulture = new CultureInfo("en-US", false);
            contentTable = new SortedList<string, ArchtypeInfo>();

            foreach (TableBlock block in blocks)
            {
                if ((SolarBlockType)block.Block.TemplateIndex == SolarBlockType.Solar)
                {
                    ContentType type = ContentType.None;
                    string name = null;
                    double radius = -1;

                    foreach (EditorINIOption option in block.Block.Options)
                    {
                        if ((SolarOptionType)option.TemplateIndex == SolarOptionType.Nickname)
                        {
                            if (option.Values.Count > 0)
                                name = option.Values[0].Value.ToString();
                        }
                        else if ((SolarOptionType)option.TemplateIndex == SolarOptionType.Type)
                        {
                            if (option.Values.Count > 0)
                                type = GetType(option.Values[0].Value.ToString());
                        }
                        else if ((SolarOptionType)option.TemplateIndex == SolarOptionType.Radius)
                        {
                            if (option.Values.Count > 0)
                                radius = double.Parse(option.Values[0].Value.ToString(), usCulture);
                        }
                    }

                    if (name != null && type != ContentType.None)
                        contentTable.Add(name, new ArchtypeInfo() { Type = type, Radius = radius });
                }
            }
        }

        private ContentType GetType(string type)
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
                return ContentType.JumpGate; //airlock_gate = jumpgate ??

            return ContentType.None;
        }

        public ArchtypeManager(string file, int templateIndex)
        {
            FileManager fileManager = new FileManager(file);
            EditorINIData iniContent = fileManager.Read(FileEncoding.Automatic, templateIndex);
            TableData solarArch = new TableData(iniContent);

            CreateContentTable(solarArch.Blocks);
        }

        enum SolarBlockType
        {
            Solar = 1,
            Other
        }

        enum SolarOptionType
        {
            Nickname = 18,
            Radius = 25,
            Type = 27,
            Other
        }
    }

    public class ArchtypeInfo
    {
        public ContentType Type { get; set; }
        public double Radius { get; set; }

        public override string ToString()
        {
            return Type.ToString() + ", " + Radius.ToString();
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
