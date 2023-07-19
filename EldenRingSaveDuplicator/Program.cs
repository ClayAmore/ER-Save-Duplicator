﻿using System;
using System.Diagnostics;
using DirectN;
using Wice.Utilities;
using Wice;


namespace EldenwarfareHelper
{
    static class Program
    {
        private static readonly string _storageDirectoryPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), typeof(Program).Namespace);
        public static string StorageDirectoryPath => _storageDirectoryPath;

        static void Main()
        {

#if DEBUG
            Application.Logger = UILogger.Instance;
            MFMediaTypeWrapper.Logger = DirectNLogger.Instance;
#else
            Audit.StartAuditing(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), typeof(Program).Namespace, "logs"));
            Audit.FlushRecords();
#endif

            try
            {
                if (Debugger.IsAttached)
                {
                    newWindow();
                }
                else
                {
                    try
                    {
                        newWindow();
                    }
                    catch (Exception e)
                    {
                        Application.AddError(e);
                        Application.ShowFatalError(IntPtr.Zero);
                    }
                }
            }
            finally
            {
#if DEBUG
                ComObjectLogger.Instance.Dispose();
                UILogger.Instance.Dispose();
                DirectNLogger.Instance.Dispose();
#else
                Audit.StopAuditing();
#endif
            }

            void newWindow()
            {
                using (var dw = new Application())
                {
                    var win = new MainWindow();
                    win.Center();
                    win.Show();
                    dw.Run();
                }
            }
        }
    }
}
