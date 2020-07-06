using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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

    public enum EditModes
    {
        ReadOnly,
        ValueEdit,
        FullyEdit
    }


    /// <summary>
    /// AppMgr 
    /// </summary>
    public class ApplicationDataMgr : ViewModelBase
    {
        #region Constructor
        public ApplicationDataMgr()
        {
            apps_Dic = new Dictionary<string, Application>();

            CommandSave = new RelayCommand(WriteFiles);
            CommandClone = new RelayCommand(CloneSelected);
        }
        #endregion


        #region Interface
        private Dictionary<string, Application> apps_Dic;

        public Dictionary<string, Application> AppsDic
        {
            get { return apps_Dic; }
            set { apps_Dic = value; }
        }

        public void ReadFiles(string path)
        {
            ReadFile(path);
            nBuildInAppCount = apps_Dic.Count;
            ReadFile(path.Replace(".", "C."));

            filePath = path;
        }

        public ObservableCollection<ParmAttribute> GetEditList(string appName)
        {
            currentAppName = appName;

            if (IsAdvanced)
                return AppsDic[appName].ParmList;
            else
                return new ObservableCollection<ParmAttribute>(AppsDic[appName].ParmList.Where(p => p.Visibility != "IP").ToList());
        }

        public void DeleteSelctedApp()
        {
            AppsDic.Remove(currentAppName);   
        }

        public RelayCommand CommandSave { get; set; }

        public RelayCommand CommandClone { get; set; }

        public bool IsAdvanced { get; set; } = true;

        public int nBuildInAppCount;

        public Action EnableCells;

        /// <summary>
        /// Edit Modes
        /// </summary>
        public IEnumerable<EditModes> ModeValues
        {
            get
            {
                return Enum.GetValues(typeof(EditModes))
                    .Cast<EditModes>();
            }
        }
        EditModes eMode = EditModes.ReadOnly;
        public EditModes EditMode
        {
            get { return eMode; }
            set
            {
                SetField(ref eMode, value, nameof(EditModes));
                EnableCells?.Invoke();
            }
        }

        #endregion

        #region SupportMembers

        const string strVersion = "@Version 1.0.0";
        const string strAppTag = "\\*Application: ";
        const string strAppEnd = "\\*Application End";

        string filePath;
        string currentAppName;
        

        void ReadFile(string path)
        {
            if (File.Exists(path))
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
                                if (app.Read(file, strAppEnd) > 10)
                                    apps_Dic.Add(app.Name, app);
                            }

                        }
                    }
                }
            }
        }

        void WriteFiles(object p)
        {
            using (System.IO.StreamWriter filec =
                new System.IO.StreamWriter(filePath.Replace(".", "C.")))
            {
                using (System.IO.StreamWriter file =
                     new System.IO.StreamWriter(filePath))
                {
                    file.WriteLine(strVersion);
                    filec.WriteLine(strVersion);

                    int i = 0;
                    foreach (var app in apps_Dic.Values)
                    {
                        if (i < nBuildInAppCount) //BuildIn
                        {
                            file.WriteLine(strAppTag + app.Name);
                            app.Write(file);
                            file.WriteLine(strAppEnd);
                            i++;
                        }
                        else  //Custom
                        {
                            filec.WriteLine(strAppTag + app.Name);
                            app.Write(filec);
                            filec.WriteLine(strAppEnd);
                        }
                    }
                }
            }
        }

        void CloneSelected(object p)
        {
            string newName = p as string;

            var newApp = new Application(newName, AppsDic[currentAppName]);
            AppsDic.Add(newName, newApp);
        }

        #endregion
    }
}
