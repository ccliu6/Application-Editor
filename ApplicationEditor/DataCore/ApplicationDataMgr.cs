using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace DataCore
{
 
    /// <summary>
    /// command wrapper
    /// </summary>
    public class RelayCommand : ICommand
    {
        #region Fields

        readonly Action<object> _execute;
        readonly Predicate<object> _canExecute;

        #endregion // Fields

        #region Constructors

        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }
        #endregion // Constructors

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged = delegate { };

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        #endregion // ICommand Members
    }


    /// <summary>
    /// AppMgr 
    /// </summary>
    public class ApplicationDataMgr
    {
        #region Constructor
        public ApplicationDataMgr()
        {
            apps_BuildIn = new Dictionary<string, Application>();
            apps_Custom = new Dictionary<string, Application>();

            CommandSave = new RelayCommand(WriteFiles);
            CommandClone = new RelayCommand(CloneSelected);
        }
        #endregion


        #region Interface
        private Dictionary<string, Application> apps_BuildIn;

        public Dictionary<string, Application> Apps_BuildIn
        {
            get { return apps_BuildIn; }
            set { apps_BuildIn = value; }
        }

        private Dictionary<string, Application> apps_Custom;

        public Dictionary<string, Application> Apps_Custom
        {
            get { return apps_Custom; }
            set { apps_Custom = value; }
        }

        public void ReadFiles(string path)
        {
            ReadFile(path, ref apps_BuildIn);
            ReadFile(path.Replace(".", "C."), ref apps_Custom);

            filePath = path;
        }

        public ObservableCollection<ParmAttribute> GetEditList(string appName, bool bBuildIn)
        {
            currentAppName = appName;
            bIsBuildIn = bBuildIn;

            var apps = bIsBuildIn ? Apps_BuildIn : Apps_Custom;
            if (IsAdvanced)
                return apps[appName].ParmList;
            else
                return new ObservableCollection<ParmAttribute>(apps[appName].ParmList.Where(p => p.Visibility != "IP").ToList());
        }

        public RelayCommand CommandSave { get; set; }

        public RelayCommand CommandClone { get; set; }

        public bool IsAdvanced { get; set; } = true; 
        #endregion

        #region SupportMembers

        const string strVersion = "@Version 1.0.0";
        const string strAppTag = "\\*Application: ";
        const string strAppEnd = "\\*Application End";

        string filePath;
        string currentAppName;
        bool bIsBuildIn;

        void WriteFiles(object p)
        {
            WriteFile(filePath, ref apps_BuildIn);
            WriteFile(filePath.Replace(".", "C."), ref apps_Custom);
        }

        void ReadFile(string path, ref Dictionary<string, Application> appDic)
        {
            using (System.IO.StreamReader file =
                new System.IO.StreamReader(path))
            {
                string line;
                line = file.ReadLine();
                if (line == strVersion)
                {
                    while ((line = file.ReadLine()) != null)
                    {
                        if (line.Contains(strAppTag))
                        {
                            var app = new Application(line.Substring(strAppTag.Length));
                            if (app.Read(file, strAppEnd) > 4)
                                appDic.Add(app.Name, app);
                        }

                    }
                }
            }
        }

        void WriteFile(string path, ref Dictionary<string, Application> appDic)
        {
            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(path))
            {
                file.WriteLine(strVersion);
                foreach (var app in appDic.Values)
                {
                    file.WriteLine(strAppTag + app.Name);
                    app.Write(file);
                    file.WriteLine(strAppEnd);
                }
            }
        }

        void CloneSelected(object p)
        {
            string newName = p as string;
            var apps = bIsBuildIn ? Apps_BuildIn : Apps_Custom;

            var newApp = new Application(newName, apps[currentAppName]);
            Apps_Custom.Add(newName, newApp);
        }

        #endregion
    }
}
