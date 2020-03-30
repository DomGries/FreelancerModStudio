namespace FreelancerModStudio.SystemDesigner
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using FreelancerModStudio.Data;
    using FreelancerModStudio.Data.INI;
    using FreelancerModStudio.Data.IO;
    using FreelancerModStudio.SystemDesigner.Content;

    public class Analyzer
    {
        public Dictionary<int, UniverseConnection> Connections { get; set; }

        public List<TableBlock> Universe { get; set; }
        public ArchetypeManager Archetype { get; set; }
        public string UniversePath { get; set; }
        public int SystemTemplate { get; set; }

        public void Analyze()
        {
            this.LoadUniverseConnections();
        }

        public void LoadUniverseConnections()
        {
            this.Connections = new Dictionary<int, UniverseConnection>();

            foreach (TableBlock block in this.Universe)
            {
                foreach (EditorIniOption option in block.Block.Options)
                {
                    if (option.Name.Equals("file", StringComparison.OrdinalIgnoreCase) && option.Values.Count > 0)
                    {
                        Table<int, ConnectionPart> systemConnections = this.GetConnections(block.Index, Path.Combine(this.UniversePath, option.Values[0].Value.ToString()));
                        if (systemConnections != null)
                        {
                            this.AddConnections(block.Index, systemConnections);
                        }

                        break;
                    }
                }
            }
        }

        private void AddConnections(int id, Table<int, ConnectionPart> connections)
        {
            foreach (ConnectionPart connectionPart in connections)
            {
                UniverseConnection connection = new UniverseConnection
                    {
                        From = new ConnectionPart
                            {
                                Id = id,
                                JumpGate = connectionPart.JumpGate,
                                JumpHole = connectionPart.JumpHole
                            },
                        To = new ConnectionPart
                            {
                                Id = connectionPart.Id
                            }
                    };

                int connectionHash = connection.GetHashCode();
                UniverseConnection existingConnection;
                if (this.Connections.TryGetValue(connectionHash, out existingConnection))
                {
                    existingConnection.To.JumpGate = connection.From.JumpGate;
                    existingConnection.To.JumpHole = connection.From.JumpHole;
                }
                else
                {
                    this.Connections[connectionHash] = connection;
                }
            }
        }

        private Table<int, ConnectionPart> GetConnections(int id, string file)
        {
            if (!File.Exists(file))
            {
                return null;
            }

            FileManager fileManager = new FileManager(file);
            EditorIniData iniContent;

            try
            {
                iniContent = fileManager.Read(FileEncoding.Automatic, this.SystemTemplate);
            }
            catch
            {
                // FileManager could throw an exception if the file can't be opened
                return null;
            }

            Table<int, ConnectionPart> connections = new Table<int, ConnectionPart>();

            foreach (EditorIniBlock block in iniContent.Blocks)
            {
                if (block.Name.Equals("object", StringComparison.OrdinalIgnoreCase))
                {
                    string archetypeString = null;
                    string gotoString = null;

                    foreach (EditorIniOption option in block.Options)
                    {
                        if (option.Values.Count > 0)
                        {
                            string value = option.Values[0].Value.ToString();
                            switch (option.Name.ToLowerInvariant())
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
                        ArchetypeInfo archetypeInfo = this.Archetype.TypeOf(archetypeString);
                        if (archetypeInfo != null)
                        {
                            ConnectionPart connection = new ConnectionPart
                                {
                                    Id = this.GetConnectionId(BeforeSeperator(gotoString, ","))
                                };

                            switch (archetypeInfo.Type)
                            {
                                case ContentType.JumpGate:
                                    connection.JumpGate = true;
                                    break;
                                case ContentType.JumpHole:
                                    connection.JumpHole = true;
                                    break;
                            }

                            ConnectionPart existingConnection;
                            if (connections.TryGetValue(connection.Id, out existingConnection))
                            {
                                if (connection.JumpGate)
                                {
                                    existingConnection.JumpGate = true;
                                }

                                if (connection.JumpHole)
                                {
                                    existingConnection.JumpHole = true;
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

        private static string BeforeSeperator(string value, string seperator)
        {
            int index = value.IndexOf(seperator, StringComparison.Ordinal);
            if (index != -1)
            {
                return value.Substring(0, index);
            }

            return value;
        }

        private int GetConnectionId(string blockName)
        {
            foreach (TableBlock block in this.Universe)
            {
                if (block.Name.Equals(blockName, StringComparison.OrdinalIgnoreCase))
                {
                    return block.Index;
                }
            }

            return -1;
        }
    }

    public class UniverseConnection
    {
        public override int GetHashCode()
        {
            const int IdOffset = 0x1000;

            if (this.From != null && this.To != null)
            {
                return (this.From.Id + IdOffset)*(this.To.Id + IdOffset);
            }

            return -1;
        }

        public override bool Equals(object obj)
        {
            return this == obj;
        }

        public ConnectionPart From { get; set; }
        public ConnectionPart To { get; set; }
    }

    public class ConnectionPart : ITableRow<int>
    {
        public int Id { get; set; }
        public bool JumpGate { get; set; }
        public bool JumpHole { get; set; }
    }
}
