using System;
using System.Collections.Generic;
using FreelancerModStudio.Data;
using FreelancerModStudio.Data.IO;

namespace FreelancerModStudio.SystemPresenter
{
    public class Analyzer
    {
        public Table<UniverseConnectionID, UniverseConnection> Connections { get; set; }

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
            Connections = new Table<UniverseConnectionID, UniverseConnection>();

            foreach (TableBlock block in Universe)
            {
                foreach (EditorINIOption option in block.Block.Options)
                {
                    if (option.Name.ToLower() == "file" && option.Values.Count > 0)
                    {
                        // GetConnections throws an exception if the file cant be read
                        try
                        {
                            Table<int, ConnectionPart> systemConnections = GetConnections(block.UniqueID, global::System.IO.Path.Combine(UniversePath, option.Values[0].Value.ToString()));
                            if (systemConnections != null)
                                AddConnections(block.UniqueID, systemConnections);
                        }
                        catch { }

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
                                                        From = new ConnectionPart { ID = id, Jumpgate = connectionPart.Jumpgate, Jumphole = connectionPart.Jumphole },
                                                        To = new ConnectionPart { ID = connectionPart.ID }
                                                    };

                UniverseConnection existingConnection;
                if (Connections.TryGetValue(connection.ID, out existingConnection))
                {
                    existingConnection.To.Jumpgate = connection.From.Jumpgate;
                    existingConnection.To.Jumphole = connection.From.Jumphole;
                }
                else
                    Connections.Add(connection);
            }
        }

        Table<int, ConnectionPart> GetConnections(int id, string file)
        {
            if (!global::System.IO.File.Exists(file))
                return null;

            Table<int, ConnectionPart> connections = new Table<int, ConnectionPart>();

            FileManager fileManager = new FileManager(file);
            EditorINIData iniContent = fileManager.Read(FileEncoding.Automatic, SystemTemplate);

            foreach (EditorINIBlock block in iniContent.Blocks)
            {
                if (block.Name.ToLower() == "object")
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
                            ConnectionPart connection = new ConnectionPart();
                            connection.ID = GetConnectionID(BeforeSeperator(gotoString, ","));

                            if (archetypeInfo.Type == ContentType.JumpGate)
                                connection.Jumpgate = true;
                            else if (archetypeInfo.Type == ContentType.JumpHole)
                                connection.Jumphole = true;

                            ConnectionPart existingConnection;
                            if (connections.TryGetValue(connection.ID, out existingConnection))
                            {
                                if (connection.Jumpgate)
                                    existingConnection.Jumpgate = true;

                                if (connection.Jumphole)
                                    existingConnection.Jumphole = true;
                            }
                            else if (id != connection.ID && connection.ID != -1)
                                connections.Add(connection);
                        }
                    }
                }
            }

            return connections;
        }

        string BeforeSeperator(string value, string seperator)
        {
            int index = value.IndexOf(seperator);
            if (index == -1)
                return value;

            return value.Substring(0, index);
        }

        int GetConnectionID(string blockName)
        {
            blockName = blockName.ToLower();
            foreach (TableBlock block in Universe)
            {
                if (block.Name.ToLower() == blockName)
                    return block.UniqueID;
            }
            return -1;
        }
    }

    public class UniverseConnection : ITableRow<UniverseConnectionID>
    {
        public UniverseConnectionID ID
        {
            get
            {
                if (From != null && To != null)
                    return new UniverseConnectionID { From = From.ID, To = To.ID };

                return null;
            }
        }

        public ConnectionPart From { get; set; }
        public ConnectionPart To { get; set; }
    }

    public class UniverseConnectionID : IComparable<UniverseConnectionID>
    {
        public int From { get; set; }
        public int To { get; set; }

        public int CompareTo(UniverseConnectionID other)
        {
            int from = To.CompareTo(other.From);
            int from2 = To.CompareTo(other.To);

            if (from == 0)
                return From.CompareTo(other.To);
            if (from2 == 0)
                return From.CompareTo(other.From);

            return (From * To).CompareTo(other.From * other.To);
        }
    }

    public class ConnectionPart : ITableRow<int>
    {
        public int ID { get; set; }
        public bool Jumpgate { get; set; }
        public bool Jumphole { get; set; }
    }
}
