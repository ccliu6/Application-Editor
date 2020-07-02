using System;
using System.Collections.ObjectModel;

namespace DataCore
{
    public class Application
    {
        /// <summary>
        /// For Empty New
        /// </summary>
        /// <param name="name"></param>
        public Application(string name)
        {
            Name = name;
            ParmList = new ObservableCollection<ParmAttribute>();
        }

        /// <summary>
        /// For Clone
        /// </summary>
        /// <param name="name"></param>
        /// <param name="app"></param>
        public Application(string name, Application app)
        {
            Name = name;
            ParmList = new ObservableCollection<ParmAttribute>(app.ParmList);
        }

        public string Name { get; set; }

        public ObservableCollection<ParmAttribute> ParmList { get; set; }

        #region Read/Write
        public void Write(System.IO.StreamWriter file)
        {
            string line;
            foreach (var p in ParmList)
            {
                line = $"{p.Source};{p.Tag};{p.Name};{p.Visibility};" +
                    $"{p.StrValue};{p.NumValue};{p.NumMin};{p.NumMax};{p.ExtZRatio}";
                line = line.Replace('\xB5', '~');
                //line = line.Replace('\xB5', '~').Replace('\xB2', '^').Replace('\xB3', '`');
                file.WriteLine(line);
            }
        }

        public int Read(System.IO.StreamReader file, string endTag)
        {
            int nCount = 0;

            Char[] sepKey = { ';' };
            string line;
            while ((line = file.ReadLine()) != null && (endTag != line))
            {
                line = line.Replace('~', '\xB5');
                //line = line.Replace('~', '\xB5').Replace('^', '\xB2').Replace('`', '\xB3');
                string[] cells = line.Split(sepKey);
                if (cells.Length == 9)
                {
                    var pa = new ParmAttribute()
                    {
                        Source = cells[0],
                        Tag = cells[1],
                        Name = cells[2],
                        Visibility = cells[3],
                        StrValue = cells[4],
                        NumValue = cells[5],
                        NumMin = cells[6],
                        NumMax = cells[7],
                        ExtZRatio = cells[8]
                    };

                    ParmList.Add(pa);

                    nCount++;
                }
            }

            return nCount;
        } 
        #endregion
    }
}
