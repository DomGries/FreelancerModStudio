using System;
using System.Collections.Generic;
using System.IO;
using FreelancerModStudio.Data;
using FreelancerModStudio.Data.IO;
using FreelancerModStudio.SystemPresenter.Content;

namespace FreelancerModStudio.SystemPresenter
{
    public class Analyzer
    {
        public Dictionary<int, UniverseConnection> Connections { get; set; }

        public List<TableBlock> Universe { get; set; }
        public ArchetypeManager Archetype { get; set; }
        public string UniversePath { get; set; }
        public int SystemTemplate { get; set; }

        public void Analyze()
        {
            LoadUniverseConnections();
        }

        public void LoadUniverseConnections()
        {
            Connections = new Dictionary<int, UniverseConnection>();

            foreach (TableBlock block in Universe)
            {
                foreach (EditorINIOption option in block.Block.Options)
                {
                    if (option.Name.Equals("file", StringComparison.OrdinalIgnoreCase) && option.Values.Count > 0)
                    {
                        // GetConnections could throw an exception if the file can't be opened
                        try
                        {
                            Table<int, ConnectionPart> systemConnections = GetConnections(block.Id, Path.Combine(UniversePath, option.Values[0].Value.ToString()));
                            if (systemConnections != null)
                            {
                                AddConnections(block.Id, systemConnections);
                            }
                        }
                            // ReSharper disable EmptyGeneralCatchClause
                        catch
                            // ReSharper restore EmptyGeneralCatchClause
                        {
                        }

                        break;
                    }
                }
            }
        }

        void AddConnections(int id, Table<int, ConnectionPart> connections)
        {
            foreach (ConnectionPart connectionPart in connections)
            {
                UniverseConnection connection = new UniverseConnection
                    {
                        From = new ConnectionPart
                            {
                                Id = id,
                                Jumpgate = connectionPart.Jumpgate,
                                Jumphole = connectionPart.Jumphole
                            },
                        To = new ConnectionPart
                            {
                                Id = connectionPart.Id
                            }
                    };

                int connectionHash = connection.GetHashCode();
                UniverseConnection existingConnection;
                if (Connections.TryGetValue(connectionHash, out existingConnection))
                {
                    existingConnection.To.Jumpgate = connection.From.Jumpgate;
                    existingConnection.To.Jumphole = connection.From.Jumphole;
                }
                else
                {
                    Connections[connectionHash] = connection;
                }
            }
        }

        Table<int, ConnectionPart> GetConnections(int id, string file)
        {
            if (!File.Exists(file))
            {
                return null;
            }

            Table<int, ConnectionPart> connections = new Table<int, ConnectionPart>();

            FileManager fileManager = new FileManager(file);
            EditorINIData iniContent = fileManager.Read(FileEncoding.Automatic, SystemTemplate);

            foreach (EditorINIBlock block in iniContent.Blocks)
            {
                if (block.Name.Equals("object", StringComparison.OrdinalIgnoreCase))
                {
                    string archetypeString = null;
                    string gotoString = null;

                    foreach (EditorINIOption option in block.Options)
                    {
                        if (option.Values.Count > 0)
                        {
                            string value = option.Values[0].Value.ToString();
                            switch (option.Name.ToLower())
                            {
                                case "archetype":
                                    archetypeString = value;
                                    break;
                                case "goto":
                                    gotoString = value;
                                    break;
                            }
                        }
                    }

                    if (archetypeString != null && gotoString != null)
                    {
                        ArchetypeInfo archetypeInfo = Archetype.TypeOf(archetypeString);
                        if (archetypeInfo != null)
                        {
                            ConnectionPart connection = new ConnectionPart
                                {
                                    Id = GetConnectionId(BeforeSeperator(gotoString, ","))
                                };

                            switch (archetypeInfo.Type)
                            {
                                case ContentType.JumpGate:
                                    connection.Jumpgate = true;
                                    break;
                                case ContentType.JumpHole:
                                    connection.Jumphole = true;
                                    break;
                            }

                            ConnectionPart existingConnection;
                            if (connections.TryGetValue(connection.Id, out existingConnection))
                            {
                                if (connection.Jumpgate)
                                {
                                    existingConnection.Jumpgate = true;
                                }

                                if (connection.Jumphole)
                                {
                                    existingConnection.Jumphole = true;
                                }
                            }
                            else if (id != connection.Id && connection.Id != -1)
                            {
                                connections.Add(connection);
                            }
                        }
                    }
                }
            }

            return connections;
        }

        static string BeforeSeperator(string value, string seperator)
        {
            int index = value.IndexOf(seperator, StringComparison.Ordinal);
            if (index != -1)
            {
                return value.Substring(0, index);
            }

            return value;
        }

        int GetConnectionId(string blockName)
        {
            foreach (TableBlock block in Universe)
            {
                if (block.Name.Equals(blockName, StringComparison.OrdinalIgnoreCase))
                {
                    return block.Id;
                }
            }
            return -1;
        }
    }

    public class UniverseConnection
    {
        public override int GetHashCode()
        {
            const int idOffset = 0x1000;

            if (From != null && To != null)
            {
                return (From.Id + idOffset)*(To.Id + idOffset);
            }

            return -1;
        }

        public ConnectionPart From { get; set; }
        public ConnectionPart To { get; set; }
    }

    public class ConnectionPart : ITableRow<int>
    {
        public int Id { get; set; }
        public bool Jumpgate { get; set; }
        public bool Jumphole { get; set; }
    }
}
