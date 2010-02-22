using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FreelancerModStudio.Data.IO;
using FreelancerModStudio.Data;
using FreelancerModStudio;
using FreelancerModStudio.SystemPresenter;
using System.Threading;
using System.IO;

namespace fms_test
{
    /// <summary>
    /// Interaktionslogik für Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        SystemPresenter systemPresenter = null;
        public TableData Data { get; set; }
        public string File { get; set; }
        ArchtypeManager archtype = null;
        int templateIndex;

        Thread loadingThread;
        FileSystemWatcher monitor = new FileSystemWatcher();

        public Window1()
        {
            InitializeComponent();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (Helper.Thread.IsRunning(ref loadingThread))
                loadingThread.Join();

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Title = "Open INI file";
            dlg.Filter = "INI files (.ini)|*.ini"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
                OpenFile(dlg.FileName);
        }

        void SetFileMonitor()
        {
            monitor.EnableRaisingEvents = false;
            monitor.Path = System.IO.Path.GetDirectoryName(File);
            monitor.Filter = System.IO.Path.GetFileName(File);
            monitor.EnableRaisingEvents = true;
        }

        void monitor_Changed(object sender, FileSystemEventArgs e)
        {
            FileManager fileManager = new FileManager(File);
            EditorINIData iniContent = fileManager.Read(FileEncoding.Automatic, templateIndex);

            Data = new TableData(iniContent);

            view.Dispatcher.Invoke(new Action(delegate()
            {
                ShowData();
            }));
        }

        void OpenFile(string file)
        {
            DisplayFile(file, FileManager.GetTemplateIndex(file));
            SetFileMonitor();
        }

        void DisplayFile(string file, int templateIndex)
        {
            if (templateIndex == -1)
                templateIndex = Helper.Template.Data.Files.IndexOf("System");

            FileManager fileManager = new FileManager(file);
            EditorINIData iniContent = fileManager.Read(FileEncoding.Automatic, templateIndex);

            Data = new TableData(iniContent);
            File = file;
            this.templateIndex = templateIndex;

            ShowData();
        }

        void ShowData()
        {
            bool isSystem = Data.TemplateIndex == Helper.Template.Data.Files.IndexOf("System");
            if (isSystem)
                LoadArchtypes();

            //sort by type and name
            //Data.Blocks.Sort();
            ShowData(Data);
        }

        public void LoadArchtypes()
        {
            int archtypeTemplate = Helper.Template.Data.Files.IndexOf("Solar Arch");
            string archtypeFile = ArchtypeManager.GetRelativeArchtype(File, Data.TemplateIndex, archtypeTemplate);

            //user interaction required to get the path of the archtype file
            if (archtypeFile == null)
                archtypeFile = ShowSolarArchtypeSelector();

            archtype = new ArchtypeManager(archtypeFile, archtypeTemplate);

            foreach (TableBlock block in Data.Blocks)
                SetArchtype(block, archtype);
        }

        string ShowSolarArchtypeSelector()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Title = "Open Solar Archtype INI";
            dlg.Filter = "Solar Archtype INI (.ini)|*.ini"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
                return dlg.FileName;

            return null;
        }

        void SetArchtype(TableBlock block, ArchtypeManager archtypeManager)
        {
            switch (block.Block.Name.ToLower())
            {
                case "lightsource":
                    block.ObjectType = FreelancerModStudio.SystemPresenter.ContentType.LightSource;
                    break;
                case "zone":
                    block.ObjectType = FreelancerModStudio.SystemPresenter.ContentType.Zone;
                    break;
                case "object":

                    bool hasArchtype = false;

                    //get type of object based on archtype
                    foreach (EditorINIOption option in block.Block.Options)
                    {
                        if (option.Name.ToLower() == "archetype")
                        {
                            if (option.Values.Count > 0)
                            {
                                block.Archtype = archtypeManager.TypeOf(option.Values[0].Value.ToString());
                                if (block.Archtype != null)
                                {
                                    block.ObjectType = block.Archtype.Type;
                                    hasArchtype = true;
                                }

                                break;
                            }
                        }
                    }

                    if (!hasArchtype)
                        block.ObjectType = FreelancerModStudio.SystemPresenter.ContentType.None;

                    break;
            }

            if (block.ObjectType != FreelancerModStudio.SystemPresenter.ContentType.None)
                block.Visibility = true;
        }

        public void Dispose()
        {
            systemPresenter.ClearDisplay(true);
            systemPresenter.Objects.Clear();
        }

        public void ShowData(TableData data)
        {
            Clear();
            systemPresenter.Add(data.Blocks);
        }

        public void Clear()
        {
            systemPresenter.ClearDisplay(false);
            systemPresenter.Objects.Clear();
        }

        public void Select(int id)
        {
            ContentBase content;
            if (systemPresenter.Objects.TryGetValue(id, out content))
                systemPresenter.SelectedContent = content;
        }

        public void Deselect()
        {
            systemPresenter.SelectedContent = null;
        }

        public void SetVisibility(int id, bool value)
        {
            ContentBase content;
            if (systemPresenter.Objects.TryGetValue(id, out content))
                systemPresenter.SetVisibility(content, value);
        }

        public void SetValues(List<TableBlock> blocks)
        {
            List<TableBlock> newBlocks = new List<TableBlock>();

            foreach (TableBlock block in blocks)
            {
                ContentBase content;
                if (systemPresenter.Objects.TryGetValue(block.ID, out content))
                    systemPresenter.ChangeValues(content, block);
                else
                    newBlocks.Add(block);
            }

            if (newBlocks.Count > 0)
                Add(newBlocks);
        }

        public void Add(List<TableBlock> blocks)
        {
            systemPresenter.Add(blocks);
            systemPresenter.RefreshDisplay();
        }

        public void Delete(List<TableBlock> blocks)
        {
            systemPresenter.Delete(blocks);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ThreadStart ts = new ThreadStart(delegate
            {
                Helper.Template.Load();

            });
            loadingThread = new Thread(ts);
            Helper.Thread.Start(ref loadingThread, ts, ThreadPriority.Highest, true);

            systemPresenter = new SystemPresenter(view);
            monitor.Changed += new FileSystemEventHandler(monitor_Changed);

            InputBindings.Add(new InputBinding(ApplicationCommands.Open, new KeyGesture(Key.O, ModifierKeys.Control)));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, MenuItem_Click));
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space || e.Key == Key.H)
            {
                if (systemPresenter.SelectedContent != null)
                    systemPresenter.SetVisibility(systemPresenter.SelectedContent, false);
            }
        }
    }
}
