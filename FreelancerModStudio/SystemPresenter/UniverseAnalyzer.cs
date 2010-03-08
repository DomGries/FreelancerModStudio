using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreelancerModStudio.Data;
using FreelancerModStudio.Data.IO;

namespace FreelancerModStudio.SystemPresenter
{
    public class UniverseAnalyzer
    {
        public List<GlobalConnection> Connections { get; set; }
        public Table<int, ContentBase> Universe { get; set; }
        public string UniversePath { get; set; }
        public int SystemTemplate { get; set; }

        public void Analyze()
        {
            LoadUniverseConnections();
        }

        public void LoadUniverseConnections()
        {
            Connections = new List<GlobalConnection>();
            foreach (ContentBase content in Universe)
            {
                foreach (EditorINIOption option in content.Block.Block.Options)
                {
                    if (option.Name.ToLower() == "file" && option.Values.Count > 0)
                    {
                        Connections.Add(new GlobalConnection()
                        {
                            Content = content,
                            Universe = GetConnections(content, System.IO.Path.Combine(UniversePath, option.Values[0].Value.ToString()))
                        });
                        break;
                    }
                }
            }
        }

        Table<ContentBase, UniverseConnection> GetConnections(ContentBase content, string file)
        {
            FileManager fileManager = new FileManager(file);
            EditorINIData iniContent = fileManager.Read(FileEncoding.Automatic, SystemTemplate);

            Table<ContentBase, UniverseConnection> connections = new Table<ContentBase, UniverseConnection>();

            foreach (EditorINIBlock block in iniContent.Blocks)
            {
                if (block.Name.ToLower() == "object")
                {
                    string archtypeString = null;
                    string gotoString = null;

                    foreach (EditorINIOption option in block.Options)
                    {
                        if (option.Values.Count > 0)
                        {
                            string value = option.Values[0].Value.ToString();
                            switch (option.Name.ToLower())
                            {
                                case "archetype":
                                    archtypeString = value;
                                    break;
                                case "goto":
                                    gotoString = value;
                                    break;
                            }
                        }
                    }

                    if (archtypeString != null && gotoString != null)
                    {
                        UniverseConnection connection = null;
                        if (archtypeString == "jumphole" && gotoString.Contains(','))
                        {
                            connection = new UniverseConnection()
                            {
                                ID = GetContent(gotoString.Substring(0, gotoString.IndexOf(','))),
                                Jumphole = true
                            };
                        }
                        else if (archtypeString == "jumpgate" && gotoString.Contains(','))
                        {
                            connection = new UniverseConnection()
                            {
                                ID = GetContent(gotoString.Substring(0, gotoString.IndexOf(','))),
                                Jumpgate = true
                            };
                        }

                        if (connection != null && connection.ID != null)
                        {
                            UniverseConnection existingConnection;
                            if (connections.TryGetValue(connection.ID, out existingConnection))
                            {
                                if (!existingConnection.Jumpgate)
                                    existingConnection.Jumpgate = connection.Jumpgate;

                                if (!existingConnection.Jumphole)
                                    existingConnection.Jumphole = connection.Jumphole;
                            }
                            else
                                connections.Add(connection);
                        }
                    }
                }
            }
            return connections;
        }

        ContentBase GetContent(string name)
        {
            foreach (ContentBase content in Universe)
            {
                if (content.Block.Name.ToLower() == name.ToLower())
                    return content;
            }
            return null;
        }
    }

    public class GlobalConnection
    {
        public ContentBase Content { get; set; }
        public Table<ContentBase, UniverseConnection> Universe { get; set; }
    }

    public class UniverseConnection : ITableRow<ContentBase>
    {
        public ContentBase ID { get; set; }
        public bool Jumpgate { get; set; }
        public bool Jumphole { get; set; }
    }
}
