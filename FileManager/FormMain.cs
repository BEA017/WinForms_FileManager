using System.IO;
using System.Runtime;
using System.Windows.Forms;
using System.Collections.Specialized;

using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace FileManager
{
    /*1. Разработать программу "Файловый менеджер", которая имеет следующие функции:
    - показывать папки и файлы в treeView и listView 
    - показывать предпросмотр для картинок и текстовых файлов
    - реализовать создание текстовых файлов
    - копирование файлов и папок
    - удаление файлов и папок
    - переименование / перемещение файлов и папок
    - строка состояние внизу
    - верхнее / контекстное меню
   */
    

    
    //подключить кнопки верхней панели
    public partial class FormMain : Form
    {
        private ImageList imgList;
        int newFileCount = 1;

        //Временная переменная для хранения изображения (нужна, чтоб можно было удалить изображение)
        Bitmap imagePreview;
        FileOperations fileOperations = FileOperations.CreateFileOperations();

        //файлы в текущей папке
        FileInfo[] files;

        //текущая папка
        DirectoryInfo currentDirInfo;
         
        public FormMain()
        {
            InitializeComponent();       
            InitFileView();
            CreateImageList();
            InitTreeView();
        }
       
        void InitFileView()
        {
            try
            {
                fileListView.SmallImageList = new ImageList();
                fileListView.LargeImageList = new ImageList();
                fileListView.LargeImageList.ImageSize = new Size(48, 48);
                fileListView.LargeImageList.Images.Add(Bitmap.FromFile("../../../resources/note11.ico"));
                fileListView.SmallImageList.Images.Add(Bitmap.FromFile("../../../resources/note11.ico"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка при работе со списком изображений", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        void CreateImageList()
        {
            // Создание списка изображений 
            imgList = new ImageList();

            // Добавление иконок в список изображений
            imgList.Images.Add(Bitmap.FromFile("../../../resources/CLSDFOLD.ICO"));
            imgList.Images.Add(Bitmap.FromFile("../../../resources/OPENFOLD.ICO"));
            imgList.Images.Add(Bitmap.FromFile("../../../resources/NOTE11.ICO"));
            imgList.Images.Add(Bitmap.FromFile("../../../resources/NOTE12.ICO"));
            imgList.Images.Add(Bitmap.FromFile("../../../resources/Drive01.ico"));
        }

        void InitTreeView()
        {
            dirTreeView.ImageList = imgList;

            foreach (var drive in Directory.GetLogicalDrives())
            {
                //Создание узла и иконок
                TreeNode node = new TreeNode(drive, 4, 4);

                // Добавили готовый узел к дереву
                dirTreeView.Nodes.Add(node);

                // Заполнение узлов с дисками
                FillByDirectories(node);
            }
        }

        private void FillByDirectories(TreeNode node)
        {
            try
            {
                // В node.FullPath - находится полный путь к ветке
                DirectoryInfo dirInfo = new DirectoryInfo(node.FullPath);

                // Получение информации о каталогах
                DirectoryInfo[] dirs = dirInfo.GetDirectories();

                foreach (DirectoryInfo dir in dirs)
                {
                    TreeNode tree = new TreeNode(dir.Name, 0, 1);
                    node.Nodes.Add(tree);
                }
            }
            // Исключение (дисковод без диска)
            catch { }
        }

        // Метод запускается ДО открытия узловой ветки дерева
        // sender - ссылка на дерево. e - параметры метода
        private void dirTreeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            // Запрет постоянной перерисовки дерева во время добавления элементов
            dirTreeView.BeginUpdate();
            
            try
            {
                //Перебор всех дочерних узлов для узла(разворачивается по +)
                foreach (TreeNode node in e.Node.Nodes)
                {
                    FillByDirectories(node);
                }
            }
            catch { }
            // возврат режима обычного обновления дерева (сразу вызывает перерисовку дерева)
            dirTreeView.EndUpdate();
        }

        private void dirTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                FillByFiles(e.Node.FullPath);
            }
            catch { }
        }

        private void FillByFiles(string path)
        {
            fileListView.BeginUpdate();
            fileListView.Items.Clear();

            textBoxStatusBar.Text = path;
            currentDirInfo = new DirectoryInfo(path);

            files = currentDirInfo.GetFiles();
            fileOperations.dirInfo = currentDirInfo;
            // Обработка информации
            fileListView.LargeImageList.Images.Clear();
            fileListView.SmallImageList.Images.Clear();
            int iconIndex = 0;
            fileListView.LargeImageList.Images.Add(Bitmap.FromFile("../../../resources/note11.ico"));
            fileListView.SmallImageList.Images.Add(Bitmap.FromFile("../../../resources/note11.ico"));

            foreach (FileInfo file in files)
            {
                ListViewItem item = new ListViewItem(file.Name);

                // Получить иконку для текущего файла
                Icon icon = Icon.ExtractAssociatedIcon(file.FullName);
                fileListView.LargeImageList.Images.Add(icon);
                fileListView.SmallImageList.Images.Add(icon);
                iconIndex++;

                //Указать номер иконки для listView
                item.ImageIndex = iconIndex;

                //добавить пункт в listView
                item.SubItems.Add(file.LastWriteTime.ToString());
                item.SubItems.Add(file.Length.ToString());
                fileListView.Items.Add(item);
            }
            fileListView.EndUpdate();
        }

        private void fileListView_MouseClick(object sender, MouseEventArgs e)
        {
            previewPanel.Controls.Clear();
            foreach (ListViewItem file in fileListView.SelectedItems)
            {
                if (file.Text.Contains(".txt"))
                {
                    TextBox tbPreview = new TextBox();
                    tbPreview.Multiline = true;
                    tbPreview.Parent = previewPanel;
                    tbPreview.Size = previewPanel.Size;
                    tbPreview.Location = previewPanel.Location;
                    tbPreview.Dock = DockStyle.Fill;
                    tbPreview.ScrollBars = ScrollBars.Both;
                    tbPreview.Text = File.ReadAllText(files[file.Index].FullName);
                }
                if (file.Text.Contains(".jpg") || file.Text.Contains(".png") || file.Text.Contains(".bmp"))
                {
                    imagePreview = new Bitmap(files[file.Index].FullName);

                    PictureBox picturePreview = new PictureBox();
                    picturePreview.Parent = previewPanel;
                    picturePreview.BackgroundImage = imagePreview;
                    picturePreview.BackgroundImageLayout = ImageLayout.Zoom;
                    picturePreview.Size = previewPanel.Size;
                    picturePreview.Location = previewPanel.Location;
                    picturePreview.Dock = DockStyle.Fill;
                }
            }
        }

        //создание текстового файла
        private void buttonCreate_Click(object sender, EventArgs e)
        {
            if (fileListView.Focus())
            {
                File.WriteAllText(currentDirInfo.FullName + $"\\New text file {newFileCount++}.txt", "");
            }
            FillByFiles(currentDirInfo.FullName);
        }

        //Переименование файла
        
        private void fileListView_BeforeLabelEdit(object sender, LabelEditEventArgs e)
        {
            fileOperations.curFileName = fileListView.SelectedItems[0].Text;
        }

        private void fileListView_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
           // fileOperations.curFileName = fileListView.SelectedItems[0].Text;

            if (fileOperations.curFileName!= null)
            {
                if (fileOperations.FileRename(fileListView.SelectedItems[0].Text, e.Label))
                {
                    FillByFiles(fileOperations.dirInfo.FullName);
                }
            }             
        }       

        private void dirTreeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Label==null)
            {
                e.CancelEdit = true;
            }
            else
            {
                try
                {
                    string result = fileOperations.FolderRename(e.Label);
                    dirTreeView.SelectedNode.Text = e.Label;
                    FillByFiles(result);
                }
                catch
                {
                    e.CancelEdit = true;
                }
            }                
        }        //Переименование папки
        
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {            

            // Создать пустой список строк
            StringCollection str = new StringCollection();
            if (fileListView.Focus() && fileListView.SelectedItems.Count > 0)
            {
                foreach (ListViewItem file in fileListView.SelectedItems)
                {
                    str.Add(fileOperations.dirInfo.FullName+"\\"+file.Text);
                }                 
            }
            else
            if (dirTreeView.Focus())
            {
                str.Add(fileOperations.dirInfo.FullName);
            }
            // Поместить туда список выбранных пользователем имён файлов
            fileOperations.copyFlag = true;                // Поместить список имён файлов в буфер обмена
            Clipboard.SetFileDropList(str);
             
        }
        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Создать пустой список строк
            StringCollection str = new StringCollection();
            if (fileListView.Focus() && fileListView.SelectedItems.Count > 0)
            {
                foreach (ListViewItem file in fileListView.SelectedItems)
                {
                    str.Add(fileOperations.dirInfo.FullName + "\\" + file.Text);
                }
                Clipboard.SetFileDropList(str);
                fileOperations.copyFlag = false;
                return;
            }
            else
            if (dirTreeView.Focus())
            {
                // Поместить туда список выбранных пользователем имён файлов
                str.Add(fileOperations.dirInfo.FullName);
                // Поместить список имён файлов в буфер обмена
                Clipboard.SetFileDropList(str);
            }
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {             
            // Получить из буфера обмена контейнер с данными
            IDataObject obj = Clipboard.GetDataObject();
             // Проверить наличие в контейнере списка файлов
            if (obj.GetDataPresent(DataFormats.FileDrop))
            {
                // Получить список файлов из    
                StringCollection files = Clipboard.GetFileDropList();
                if (fileOperations.copyFlag)
                {
                    foreach (var item in files)
                    {
                        fileOperations.CopyPaste(item);
                    }
                }
                else
                {
                    foreach (var item in files)
                    {
                        fileOperations.CutPaste(item);
                    }
                }
            }
                FillByDirectories(dirTreeView.SelectedNode);
                FillByFiles(currentDirInfo.FullName);
        } 
            
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (imagePreview != null)
                imagePreview.Dispose();
            if (fileListView.Focus() && fileListView.SelectedItems.Count > 0)
            {
                foreach (ListViewItem file in fileListView.SelectedItems)
                {
                    fileOperations.Delete(fileOperations.dirInfo.FullName + "\\" + file.Text);
                }
            }
            else if (dirTreeView.Focus() && dirTreeView.SelectedNode.Nodes.Count > 0)
            {
                fileOperations.Delete(fileOperations.dirInfo.FullName);
            }
            FillByDirectories(dirTreeView.SelectedNode);
            FillByFiles(currentDirInfo.FullName);
        }              

        private void createTextFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fileListView.Focus())
            {
                File.WriteAllText(currentDirInfo.FullName + $"\\New text file {newFileCount++}.txt", "");
            }
            FillByFiles(currentDirInfo.FullName);
        }

        private void textBoxStatusBar_TextChanged(object sender, EventArgs e)
        {

        }

        //Drags

        private void fileListView_DragDrop(object sender, DragEventArgs e)
        {
            // Если перетаскивается список файлов
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Получить и напечатать список файлов
                string[] str = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (var item in str)
                {
                    fileOperations.CopyPaste(item);
                }
                FillByDirectories(dirTreeView.SelectedNode);
                FillByFiles(currentDirInfo.FullName);
            }
        }

        private void fileListView_DragEnter(object sender, DragEventArgs e)
        {
            // Если пользователь копирует объект перетаскиванием и это список файлов и это не перетаскивание из listBox в него же
            if (e.Data.GetDataPresent(DataFormats.FileDrop) && (e.AllowedEffect & DragDropEffects.Copy) != 0 && !e.Data.GetDataPresent("Myappformat"))
            {
                // Разрешить копирование
                e.Effect = DragDropEffects.Copy;
            }
        }
        private void fileListView_MouseDown(object sender, MouseEventArgs e)
        {
           if (fileListView.Focus() && fileListView.SelectedItems.Count > 0)
            {
                 // Создать контейнер для хранения данных
                DataObject data1 = new DataObject();

                // Положить содержимое выделенной в списке строки
                StringCollection col = new StringCollection();
                foreach (ListViewItem file in fileListView.SelectedItems)
                {
                    col.Add(fileOperations.dirInfo.FullName + "\\" + file.Text);
                }                 
  
                data1.SetFileDropList(col);
                 

                // Добавить признак пользовательского формата в контейнер
                data1.SetData("Myappformat", 0);

                // НАЧАТЬ перетаскивание программно
                DragDropEffects dde = DoDragDrop(data1, DragDropEffects.Copy);
            }
        }
         
        private void dirTree_MouseClick(object sender, MouseEventArgs e)
        {
            TreeNode node = dirTreeView.GetNodeAt(e.X, e.Y);
            if (node != null)
            {
                this.Text = node.Text;
            }
        }
      

        private void dirTreeView_DragEnter(object sender, DragEventArgs e)
        {
            if ((e.AllowedEffect & DragDropEffects.Copy) != 0 && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }
        // принимает файлы извне но не принимает из листвью
        private void dirTreeView_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                Point p = dirTreeView.PointToClient(new Point(e.X, e.Y));
                TreeNode node = dirTreeView.GetNodeAt(p.X, p.Y);//как здесь взять полное имя папки
                if (node != null)
                {
                    fileOperations.dirInfo= new DirectoryInfo(node.FullPath);
                    // Если перетаскивается список файлов
                    if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    {
                        // Получить и напечатать список файлов
                        string[] str = (string[])e.Data.GetData(DataFormats.FileDrop);

                        foreach (var item in str)
                        {
                            fileOperations.CopyPaste(item);
                        }
                        FillByDirectories(dirTreeView.SelectedNode);
                        FillByFiles(currentDirInfo.FullName);
                    }
                }
            }
        }
    }
}