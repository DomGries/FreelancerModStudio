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
                        AddConnection(new GlobalConnection()
                        {
                            Content = content,
                            Universe = GetConnections(content, IO.Path.Combine(UniversePath, option.Values[0].Value.ToString()))
                        });


                        break;
                    }
                }
            }
        }

        void AddConnection(GlobalConnection connection)
        {
            //bool contains = false;
            for (int i = 0; i < Connections.Count; i++)
            {
                if (Connections[i].Universe.Contains(connection.ID))
                {
                    int index = connection.Universe.IndexOf(Connections[i].ID);
                    if (index != -1)
                        connection.Universe.RemoveAt(index);
                    //integrate connection
                    //foreach (UniverseConnection universeConnection in connection.Universe)
                    //{
                    //    if (!Connections[i].Universe.Contains(universeConnection.ID))
                    //        Connections[i].Universe.Add(universeConnection);
                    //}

                    //contains = true;
                    //break;
                }
            }

            //if (!contains)
            if (connection.Universe.Count > 0)
                Connections.Add(connection);
        }

        UniverseConnections GetConnections(ContentBase content, string file)
        {
            FileManager fileManager = new FileManager(file);
            EditorINIData iniContent = fileManager.Read(FileEncoding.Automatic, SystemTemplate);

            UniverseConnections connections = new UniverseConnections();

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
                                Connection = GetContent(gotoString.Substring(0, gotoString.IndexOf(','))),
                                Jumphole = true
                            };
                        }
                        else if (archtypeString == "jumpgate" && gotoString.Contains(','))
                        {
                            connection = new UniverseConnection()
                            {
                                Connection = GetContent(gotoString.Substring(0, gotoString.IndexOf(','))),
                                Jumpgate = true
                            };
                        }

                        if (connection != null && connection.ID != -1)
                        {
                            UniverseConnection existingConnection;
                            if (connections.TryGetValue(connection.ID, out existingConnection))
                            {
                                if (!existingConnection.Jumpgate)
                                    existingConnection.Jumpgate = connection.Jumpgate;

                                if (!existingConnection.Jumphole)
                                    existingConnection.Jumphole = connection.Jumphole;
                            }
                            else if (connection.ID != content.ID)
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
        public Table<int, UniverseConnection> Universe { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (UniverseConnection connection in Universe)
                sb.Append(connection.ToString() + " ");

            return Content.Block.Name + " - " + sb.ToString();
        }
    }

    public class UniverseConnections : Table<int, UniverseConnection>
    {
        public override string ToString()
        {
            string[] sb = new string[this.Count];
            for (int i = 0; i < this.Count; i++)
                sb[i] = this.Values[i].ToString();

            return string.Join(", ", sb);
        }
    }

    public class UniverseConnection : ITableRow<int>
    {
        public int ID
        {
            get
            {
                if (Connection != null)
                    return Connection.ID;
                else
                    return -1;
            }
        }

        public ContentBase Connection { get; set; }
        public bool Jumpgate { get; set; }
        public bool Jumphole { get; set; }

        public override string ToString()
        {
            return Connection.Block.Name;
        }
    }
}
