using System;
using System.Collections.Generic;
using System.IO;
using FreelancerModStudio.Data.INI;
using FreelancerModStudio.Data.IO;
using FreelancerModStudio.SystemPresenter;

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

        public static string GetRelativeArchetype(string file, int fileTemplate)
        {
            return GetRelativeArchetype(Helper.Template.Data.GetDataPath(file, fileTemplate));
        }

        public static string GetRelativeArchetype(string dataPath)
        {
            if (dataPath != null)
            {
                string archetypePath = Path.Combine(dataPath, Path.Combine("Solar", "SolarArch.ini"));
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
                KeyValuePair<string, ArchetypeInfo> info = SystemParser.GetArchetypeInfo(block);
                if (info.Key != null)
                {
                    _archetypes[info.Key] = info.Value;
                }
            }
        }
    }
}
