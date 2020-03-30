namespace FreelancerModStudio.Data
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using FreelancerModStudio.Data.INI;
    using FreelancerModStudio.Data.IO;
    using FreelancerModStudio.SystemDesigner;

    public class ArchetypeManager
    {
        private Dictionary<string, ArchetypeInfo> archetypes;

        public ArchetypeManager(string file, int templateIndex)
        {
            if (file == null)
            {
                return;
            }

            FileManager fileManager = new FileManager(file);
            EditorIniData iniContent = fileManager.Read(FileEncoding.Automatic, templateIndex);

            this.CreateContentTable(iniContent.Blocks);
        }

        public ArchetypeInfo TypeOf(string archetype)
        {
            ArchetypeInfo info;
            if (this.archetypes != null && this.archetypes.TryGetValue(archetype, out info))
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

        private void CreateContentTable(List<EditorIniBlock> blocks)
        {
            this.archetypes = new Dictionary<string, ArchetypeInfo>(StringComparer.OrdinalIgnoreCase);

            foreach (EditorIniBlock block in blocks)
            {
                KeyValuePair<string, ArchetypeInfo> info = SystemParser.GetArchetypeInfo(block);
                if (info.Key != null)
                {
                    this.archetypes[info.Key] = info.Value;
                }
            }
        }
    }
}
