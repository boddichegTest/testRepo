using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GEditor.Controllers
{
    class History
    {
        /// <summary>
        /// Container
        /// </summary>
        private List<string> history;

        /// <summary>
        /// Date pattern
        /// </summary>
        private const string datePatt = @"M/d/yyyy hh:mm:ss tt";

        /// <summary>
        /// Enviroment
        /// -----------
        /// </summary>
        private const string env = "-------------------------------------------------------------------";

        /// <summary>
        /// Name of log file
        /// </summary>
        private const string filename = "log.txt";


        public event StatusMessage OnMessage;

        public List<string> GetHistory
        {
            get { return history; }
            set { history = value; }
        }
        //---------------------------------------------------
        /// <summary>
        /// Constructor
        /// </summary>
        public History() 
        {
            this.history = new List<string>();
            try
            {
                Load();
                AddEvent("Запуск программы", true, true);
            }
            catch (Exception)
            {
                OnMessage("Ошибка при открытии файла с логами!");
            }
        }
        //---------------------------------------------------
        /// <summary>
        /// Add new event to History container
        /// </summary>
        /// <param name="message"></param>
        public void AddEvent(string message, bool status, bool isInit)
        {
            DateTime saveNow = DateTime.Now;
            string dtString = saveNow.ToString(datePatt);

            if (isInit)
            {
                history.Add(env);
            }
            if (status)
            {
                history.Add("[" + dtString + "] Event : " + message); 
            }
            else
            {
                history.Add("[" + dtString + "] Error : " + message);
            }
        }
        //---------------------------------------------------
        /// <summary>
        /// Save history to log file
        /// </summary>
        public void Save() 
        {
            StreamWriter sw = new StreamWriter(filename);
            try
            {
                for (int i = 0; i < history.Count; i++)
                {
                    sw.WriteLine(history[i]);
                }
            }
            catch (IOException)
            {
                if (OnMessage != null)
                {
                    OnMessage("Ошибка при сохранении файла с логами!");
                }
            }
            finally
            {
                sw.Close();
            }
        }
        public void Load()
        {

            try
            {
                StreamReader sr = new StreamReader(filename);
                while(!sr.EndOfStream)
                {
                    history.Add(sr.ReadLine());
                }
                sr.Close();
            }
            catch (IOException)
            {
                if (OnMessage != null)
                {
                    OnMessage("Ошибка при открытии файла с логами!");
                }
            }
        }
    }
}
