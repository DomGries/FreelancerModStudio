using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreelancerModStudio.Data;
using FreelancerModStudio.Data.IO;
using IO = System.IO;

namespace FreelancerModStudio.SystemPresenter
{
    public class Analyzer
    {
        public Table<UniverseConnectionID, UniverseConnection> Connections { get; set; }

        public Table<int, ContentBase> Universe { get; set; }
        public ArchtypeManager Archtype { get; set; }
        public string UniversePath { get; set; }
        public int SystemTemplate { get; set; }

        public void Analyze()
        {
            LoadUniverseConnections();
        }

        public void LoadUniverseConnections()
        {
            Connections = new Table<UniverseConnectionID, UniverseConnection>();

            foreach (ContentBase content in Universe)
            {
                foreach (EditorINIOption option in content.Block.Block.Options)
                {
                    if (option.Name.ToLower() == "file" && option.Values.Count > 0)
                    {
                        Table<int, ConnectionPart> systemConnections = GetConnections(content, IO.Path.Combine(UniversePath, option.Values[0].Value.ToString()));
                        if (systemConnections != null)
                            AddConnections(content, systemConnections);

                        break;
                    }
                }
            }
        }

        void AddConnections(ContentBase content, Table<int, ConnectionPart> connections)
        {
            foreach (ConnectionPart connectionPart in connections)
            {
                UniverseConnection connection = new UniverseConnection()
                {
                    From = new ConnectionPart() { Content = content, Jumpgate = connectionPart.Jumpgate, Jumphole = connectionPart.Jumphole },
                    To = new ConnectionPart() { Content = connectionPart.Content }
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

        Table<int, ConnectionPart> GetConnections(ContentBase content, string file)
        {
            if (!IO.File.Exists(file))
                return null;

            FileManager fileManager = new FileManager(file);
            EditorINIData iniContent = fileManager.Read(FileEncoding.Automatic, SystemTemplate);

            Table<int, ConnectionPart> connections = new Table<int, ConnectionPart>();

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
                        ArchtypeInfo archtypeInfo = Archtype.TypeOf(archtypeString);
                        if (archtypeInfo != null)
                        {
                            ConnectionPart connection = new ConnectionPart();
                            connection.Content = GetContent(BeforeSeperator(gotoString, ","));

                            if (archtypeInfo.Type == ContentType.JumpGate)
                                connection.Jumpgate = true;
                            else if (archtypeInfo.Type == ContentType.JumpHole)
                                connection.Jumphole = true;

                            ConnectionPart existingConnection;
                            if (connections.TryGetValue(connection.ID, out existingConnection))
                            {
                                if (connection.Jumpgate)
                                    existingConnection.Jumpgate = true;

                                if (connection.Jumphole)
                                    existingConnection.Jumphole = true;
                            }
                            else if (content.ID != connection.ID && connection.ID != -1)
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

    public class UniverseConnection : ITableRow<UniverseConnectionID>
    {
        public UniverseConnectionID ID
        {
            get
            {
                if (From != null && To != null)
                    return new UniverseConnectionID() { From = From.Content.ID, To = To.Content.ID };
                else
                    return null;
            }
        }

        public ConnectionPart From { get; set; }
        public ConnectionPart To { get; set; }

        public override string ToString()
        {
            return From.Content.Block.Name + " - " + To.Content.Block.Name;
        }
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
            else if (from2 == 0)
                return From.CompareTo(other.From);

            return (From * To).CompareTo(other.From * other.To);
        }
    }

    public class ConnectionPart : ITableRow<int>
    {
        public int ID
        {
            get
            {
                if (Content != null)
                    return Content.ID;
                else
                    return -1;
            }
        }

        public ContentBase Content { get; set; }
        public bool Jumpgate { get; set; }
        public bool Jumphole { get; set; }

        public override string ToString()
        {
            return Content.Block.Name;
        }
    }
}
