using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Hosting;

namespace StocksDecision.Models
{
    public class StockBackgroundWorker : IRegisteredObject
    {
        private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(60000);

        private readonly Timer _timer;

        private readonly object _updateLock = new object();
        private volatile bool _updating = false;

        public StockBackgroundWorker()
        {
            _timer = new Timer(UpdateNewlyAddedStocks, null, _updateInterval, _updateInterval);
        }

        private void UpdateNewlyAddedStocks(object state)
        {
            DateTime datenow = DateTime.UtcNow;
            StockRealTime sr = new StockRealTime();
            StocksData sd = new StocksData();

            lock (_updateLock)
            {
                if (!_updating)
                {
                    _updating = true;

                    //Do Task
                    try
                    {
                        if ((datenow.Hour == 21 && datenow.Minute == 30) && (datenow.DayOfWeek != DayOfWeek.Saturday && datenow.DayOfWeek != DayOfWeek.Sunday))
                        {
                            sr.NotifyUser();
                        }


                        if (datenow.Hour == 3 && datenow.Minute == 30)
                        {
                            sd.GetDataAll();
                        }



                    }
                    catch
                    {

                    }
                    finally
                    {

                    }

                    _updating = false;
                }
            }
        }

        //public void WriteString()
        //{
        //    string path = @"C:\Users\romeltined\Documents\Visual Studio 2017\Projects\StocksDecision\StocksDecision\File\WriteText.txt";
        //    // This text is added only once to the file.
        //    string text = DateTime.Now.ToLongDateString() + DateTime.Now.ToLongTimeString();
        //    if (!File.Exists(path))
        //    {
        //        // Create a file to write to.
        //        using (StreamWriter sw = File.CreateText(path))
        //        {
        //            sw.WriteLine(text);
        //        }
        //    }

        //    // This text is always added, making the file longer over time
        //    // if it is not deleted.
        //    using (StreamWriter sw = File.AppendText(path))
        //    {
        //        sw.WriteLine(text);
        //    }

        //}

        public void Stop(bool immediate)
        {
            _timer.Dispose();

            HostingEnvironment.UnregisterObject(this);
        }
    }
}