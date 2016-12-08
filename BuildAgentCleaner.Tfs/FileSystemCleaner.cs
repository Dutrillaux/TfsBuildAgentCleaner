using System;
using System.Configuration;
using System.IO;
using System.Linq;
using log4net;

namespace BuildAgentCleaner.Tfs
{
    public class FileSystemCleaner
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private bool _isInitialized;

        private string _fullPathNameToClean;
        private int _deleteFilesOlderThanThisNumberOfDays = -5;
        private bool _isDeleteActive = false;
        private int _level = 3;

        public void Initialize()
        {
            try
            {
                _fullPathNameToClean = ConfigurationManager.AppSettings.Get("FullPathNameToClean");
                
                int.TryParse(ConfigurationManager.AppSettings.Get("DeleteFilesOlderThanThisNumberOfDays"), out _deleteFilesOlderThanThisNumberOfDays);
                _deleteFilesOlderThanThisNumberOfDays = - Math.Abs(_deleteFilesOlderThanThisNumberOfDays);
                
                bool.TryParse(ConfigurationManager.AppSettings.Get("IsDeleteActive"), out _isDeleteActive);
                int.TryParse(ConfigurationManager.AppSettings.Get("level"), out _level);

                Log.Info("----------------------------------------------");
                Log.InfoFormat($"Clean of {DateTime.Now} - RootFullPath: {_fullPathNameToClean} - Level: {_level}");
                
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                Log.ErrorFormat($"ERROR {ex.Message + Environment.NewLine + ex.StackTrace}");
            }
        }

        public void Clean()
        {
            var startTime = DateTime.Now.Millisecond;
            
            if (!_isInitialized) Initialize();

            Log.Info("Start cleaning");

            if (!Directory.Exists(_fullPathNameToClean))
            {
                Log.ErrorFormat($"ERROR : {_fullPathNameToClean} doesn't exists.");
                return;
            }

            GetFilesToDelete(_isDeleteActive, _level, 1, new DirectoryInfo(_fullPathNameToClean), _deleteFilesOlderThanThisNumberOfDays);

            Log.Info($"End cleaning [duration: {DateTime.Now.Millisecond - startTime} millisecond(s)]");
        }

        private void GetFilesToDelete(bool isDelete, int level, int currentLevel, DirectoryInfo di, int nbDays)
        {
            var directories = di.GetDirectories();

            if (!directories.Any()) return;

            foreach (var directory in directories)
            {
                try
                {
                    if (currentLevel.Equals(level))
                    {
                        if (!isDelete) continue;
                        if (directory.CreationTime >= DateTime.Now.AddDays(-Math.Abs(nbDays)) ||
                            directory.LastAccessTime >= DateTime.Now.AddDays(-Math.Abs(nbDays))) continue;

                        Console.WriteLine(directory.FullName);

                        Log.InfoFormat($"trying to delete {directory.FullName}");
                        DeleteDirectory(directory);
                        Log.InfoFormat($"{directory.FullName} deleted");
                    }
                    else
                    {
                        GetFilesToDelete(isDelete, level, currentLevel + 1, directory, nbDays);
                    }
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat($"ERROR {ex.Message + Environment.NewLine + ex.StackTrace}");
                }
            }
        }

        private void DeleteDirectory(DirectoryInfo directory)
        {
            try
            {
                foreach (var fileInfo in directory.GetFiles("*", SearchOption.AllDirectories))
                {
                    DeleteFileEvenReadOnly(fileInfo);
                }

                directory.Delete(true);

                Console.WriteLine("deleted :" + directory.FullName);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat($"ERROR {ex.Message + Environment.NewLine + ex.StackTrace}");
            }
        }

        private static void DeleteFileEvenReadOnly(FileInfo fileInfo)
        {
            try
            {
                if (fileInfo.IsReadOnly)
                {
                    fileInfo.IsReadOnly = false;
                }

                fileInfo.Delete();

                Console.WriteLine("deleted :" + fileInfo.FullName);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat($"ERROR {ex.Message + Environment.NewLine + ex.StackTrace}");
            }
        }
    }
}
