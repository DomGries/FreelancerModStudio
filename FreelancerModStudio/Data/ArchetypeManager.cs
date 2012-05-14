using System;
using System.Collections.Generic;
using System.IO;
using FreelancerModStudio.Data.IO;
using FreelancerModStudio.SystemPresenter;
using FreelancerModStudio.SystemPresenter.Content;

namespace FreelancerModStudio.Data
{
    public class ArchetypeManager
    {
        Dictionary<string, ArchetypeInfo> _archetypes;

        public ArchetypeManager(string file, int templateIndex)
        {
            if (file == null)
            {
                return;
            }

            FileManager fileManager = new FileManager(file);
            EditorINIData iniContent = fileManager.Read(FileEncoding.Automatic, templateIndex);

            CreateContentTable(iniContent.Blocks);
        }

        public ArchetypeInfo TypeOf(string archetype)
        {
            ArchetypeInfo info;
            if (_archetypes != null && _archetypes.TryGetValue(archetype, out info))
            {
                return info;
            }

            return null;
        }

        public static string GetRelativeArchetype(string file, int fileTemplate, int archetypeTemplate)
        {
            string dataPath = Helper.Template.Data.GetDataPath(file, fileTemplate);
            if (dataPath != null)
            {
                string archetypePath = dataPath + @"\Solar\SolarArch.ini";
                if (File.Exists(archetypePath))
                {
                    return archetypePath;
                }
            }

            return null;
        }

        void CreateContentTable(List<EditorINIBlock> blocks)
        {
            _archetypes = new Dictionary<string, ArchetypeInfo>(StringComparer.OrdinalIgnoreCase);

            foreach (EditorINIBlock block in blocks)
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
                            switch (option.Name.ToLower())
                            {
                                case "nickname":
                                    name = option.Values[0].Value.ToString();
                                    break;
                                case "solar_radius":
                                    radius = Parser.ParseDouble(option.Values[0].Value.ToString(), 1);
                                    break;
                                case "type":
                                    type = SystemParser.ParseContentType(option.Values[0].Value.ToString());
                                    break;
                                case "da_archetype":
                                    cmpFile = option.Values[0].Value.ToString();
                                    break;
                            }
                        }
                    }

                    if (name != null)
                    {
                        if ((type == ContentType.Planet || type == ContentType.Sun) && radius != 0d)
                        {
                            //save radius only for planets and suns
                            _archetypes[name] = new ArchetypeInfo
                                {
                                    Type = type,
                                    Radius = radius
                                };
                        }
                        else if (type != ContentType.None && cmpFile != null)
                        {
                            //save model path only for supported objects (not planets and suns)
                            _archetypes[name] = new ArchetypeInfo
                                {
                                    Type = type,
                                    ModelPath = cmpFile
                                };
                        }
                    }
                }
            }
        }
    }
}
