using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Address.UnusedImages.Cleaner
{
    public class Cleaner
    {
        private readonly string _rootFolder = @"E:\";
        private readonly string searchPattern = @"*.jpg";
        private int _count = 0;
        private readonly IConfigurationRoot _configuration;

        private List<string> _pathList;

        public Cleaner()
        {
        }
        
        public Cleaner(IConfigurationRoot configuration)
        {
            _configuration = configuration;
            searchPattern = configuration.GetSection("SearchPattern").Value;
            _rootFolder = configuration.GetSection("RootFolder").Value;

            _count = 0;
            
            var start = DateTime.Now;
            
            _pathList = GetImagesPaths();
            
            Console.WriteLine($"\n\rDatabase extract speed: {DateTime.Now - start}");
        }

        private List<string> GetImagesPaths()
        {
            try
            {
                var paths = new List<string>();
                var database = new DatabaseFactory(_configuration);

                var data = database.GetListingPhotos();

                foreach (DataTable table in data.Tables)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        var a = row["PhysicalPath"].ToString();
                        paths.Add(a);
                    }
                }

                return paths;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
        
        public bool CleanImages()
        {
            try
            {
                var files = GetFiles(_rootFolder, searchPattern);

                foreach (var file in files)
                {
                    Console.WriteLine(file);
                    TryToDelete(file);
                }
                
                Console.WriteLine($"\r\n\r\ncount: {_count}");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private IEnumerable<string> GetFiles(string root, string searchPattern)
        {
            var pending = new Stack<string>();
            pending.Push(root);
            while (pending.Count != 0)
            {
                var path = pending.Pop();
                string[] next = null;

                try
                {
                    next = Directory.GetFiles(path, searchPattern);
                }
                catch (UnauthorizedAccessException ex)
                {
                    Console.WriteLine("UnauthorizedAccessException - " + ex.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }

                if (next != null && next.Length != 0)
                {
                    foreach (var file in next)
                    {
                        _count++;
                        yield return file;
                    }
                }

                try
                {
                    next = Directory.GetDirectories(path);
                    foreach (var subdir in next)
                    {
                        pending.Push(subdir);
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    Console.WriteLine("UnauthorizedAccessException - " + ex.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }
            }
        }

        private void TryToDelete(string path)
        {
            if (!_pathList.Any(x => path.Contains(x)))
            {
                DeleteImages(path);
            }
        }

        private void DeleteImages(string file)
        {
            DeleteReadonlyAttribute(file);
            File.Delete(file);
            
            Console.WriteLine($"\n\r File deleted: {file}");
        }

        private bool CheckDate(string path)
        {
            DateTime creationDate = File.GetCreationTime(path);

            if (creationDate < DateTime.Now - TimeSpan.FromDays(180))
            {
                return true;
            }

            return false;
        }
        
        private void DeleteReadonlyAttribute(string fileName)
        {
            var attr = File.GetAttributes(fileName);

            if ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            {
                File.SetAttributes(fileName, attr ^ FileAttributes.ReadOnly);
            }
        }
    }
}