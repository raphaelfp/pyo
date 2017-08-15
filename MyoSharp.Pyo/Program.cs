using MyoSharp.Communication;
using MyoSharp.Device;
using MyoSharp.Exceptions;
using System;
using System.Windows.Forms;

namespace MyoSharp.Pyo
{
    static class Program
    {
        #region Methods
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Pyo());
            //Application.Run(new Form1());
        }
        #endregion
    }
}
