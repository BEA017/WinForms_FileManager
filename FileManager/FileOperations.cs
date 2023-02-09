using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager
{
    sealed partial class FileOperations
    {
        //инфо текущей папки
        public DirectoryInfo dirInfo;
        //файлы в текущей папке
        public FileInfo[] filesInfo;
        //текущее имя файлф
        public string curFileName;
        //флаг операции copy/cut
        public bool copyFlag;
        public int newFileCnt = 1;

         private static readonly FileOperations fop = new FileOperations();
        public FileOperations() { }

        public static FileOperations CreateFileOperations()
        {
             return fop;
        }
        public bool FileRename(string fileSelectedName, string fileNewName)//переименование файла
        {
            try
            {
                string oldName = dirInfo.FullName + "\\" + fileSelectedName;
                string newName = dirInfo.FullName + "\\" + fileNewName;
                if (File.Exists(oldName))
                {
                    File.Move(oldName, newName, true);
                }
                return true;
            }
            catch (Exception)
            {
                return false;                 
            }
        }

        public string FolderRename( string folderNewName)//переименование папки
        {            
            string oldName = dirInfo.FullName;
            string newName = dirInfo.FullName.Replace(dirInfo.Name, "")+folderNewName;
            if (Directory.Exists(oldName))
            {
                Directory.Move(oldName, newName);
            }
            return newName;            
        }

        public void CopyPaste(string fileName)
        {
            FileAttributes fAtr = File.GetAttributes(fileName);//получить атрибуты по имени
            if ((fAtr & FileAttributes.Directory) == FileAttributes.Directory)//если это папка
            {
                DirectoryInfo source = new DirectoryInfo(fileName);   //создать папку с таким же именем
                if (!fileName.Equals(dirInfo.FullName))//если имя не занято
                {
                    CopyDir(fileName, dirInfo.FullName + "\\" + source.Name);//рекурс. функция копирования
                }
            }
            else
            {
                FileInfo file = new FileInfo(fileName);
                File.Copy(fileName, dirInfo.FullName + "\\" + file.Name);
            }
             
        }

        void CopyDir(string source, string dest)
        {
            DirectoryInfo newDir = new DirectoryInfo(dest);
            if (newDir.Exists==false)
            {
                newDir.Create();
            }
            foreach (string files in Directory.GetFiles(source))
            {
                string newFiles = dest + "\\" + Path.GetFileName(files);
                File.Copy(files, newFiles);
            }
            foreach (string dirs in Directory.GetDirectories(source))
            {
                CopyDir(dirs, dest + "\\" + Path.GetFileName(dirs));
            }
        }

        public void CutPaste(string fileName)
        {
            FileAttributes fAtr = File.GetAttributes(fileName);//получить атрибуты по имени
            if ((fAtr & FileAttributes.Directory) == FileAttributes.Directory)//если это папка
            {
                if (!fileName.Replace(fileName, "").Equals(dirInfo.FullName))
                {
                    DirectoryInfo source = new DirectoryInfo(fileName);
                    DirectoryInfo dest = new DirectoryInfo(dirInfo.FullName + "\\" + source.Name);
                    if (dest.Exists)
                    {
                        dest.Delete(true);
                    }
                    new DirectoryInfo(fileName).MoveTo(dest.FullName);
                }
                else // иначе если файл а не папка
                {
                    FileInfo file = new FileInfo(fileName);
                    if (fileName.Equals(dirInfo.FullName))
                    {
                        File.Move(fileName, dirInfo.FullName + "\\" + file.Name);
                    }
                }
            }
        }

        public void Delete(string fileName)
        {
            FileAttributes fAtr = File.GetAttributes(fileName);//получить атрибуты по имени
            if ((fAtr & FileAttributes.Directory) == FileAttributes.Directory)//если это папка
            {
                Directory.Delete(fileName, true);
            }
            else
            {
                File.Delete(fileName);
            }
        }

    }
}
