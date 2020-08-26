using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Data.SQLite;
using System.Data;
using System.Diagnostics;

namespace MotorController.ViewModels
{
    public enum eSTATE
    {
        START,
        STOP
    };
    enum eType
    {
        _float = 0,
        _int = 1
    };
    public class Operations : ViewModelBase//, IDisposable
    {
        private static readonly object Synlock = new object();
        private static Operations _instance;
        public static Operations GetInstance
        {
            get
            {
                lock(Synlock)
                {
                    if(_instance != null)
                        return _instance;
                    _instance = new Operations();
                    return _instance;
                }
            }
        }
        public Dictionary<Tuple<Int32>, Operations> _all_Operation = new Dictionary<Tuple<Int32>, Operations>();

        private Operations()
        {
        }
        public Int32 Gui_ID { get; set; }
        public string OperationName { get; set; }
        public int Operation_ID { get; set; }
        public int Operation_Index { get; set; }
        public string Operation_Type { get; set; }
        public string Operation_Data { get; set; }

        public void readDataBase()
        {
            SQLiteDataReader reader;
            Operations _op;

            using(var connection = new SQLiteConnection("data source = " + "C:\\Projects\\Motor Driver C# GUI\\DataBase\\RayonOperations.db"))
            {
                if(connection.State.ToString() == "Closed")
                    connection.Open();

                // Build your query
                var query = "SELECT * FROM dbOperation";// WHERE Property = @_dbVersion";

                // Scope your command to execute
                using(var command = new SQLiteCommand(query, connection))
                {
                    using(reader = command.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);
                        string[] colHeader = new string[dataTable.Columns.Count];

                        int i = 0;
                        foreach(DataColumn col in dataTable.Columns)
                        {
                            colHeader[i++] = col.ToString();
                        }
                        //var commande = new SQLiteCommand(query, connection);
                        reader = command.ExecuteReader();
                        while(reader.Read())
                        {
                            _op = new Operations
                            {
                                Gui_ID = Convert.ToInt32(reader[colHeader[0]]),
                                OperationName = reader[colHeader[2]].ToString(),
                                Operation_ID = Convert.ToInt32(reader[colHeader[3]]),
                                Operation_Index = Convert.ToInt32(reader[colHeader[4]]),
                                Operation_Type = reader[colHeader[5]].ToString()
                            };
                            _all_Operation.Add(new Tuple<Int32>(_op.Gui_ID), _op);
                        }
                    }
                }
            }
        }
    }
}
