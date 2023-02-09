using System.IO;
using System.Runtime;
using System.Windows.Forms;
using System.Collections.Specialized;

using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace FileManager
{
    /*1. ����������� ��������� "�������� ��������", ������� ����� ��������� �������:
    - ���������� ����� � ����� � treeView � listView 
    - ���������� ������������ ��� �������� � ��������� ������
    - ����������� �������� ��������� ������
    - ����������� ������ � �����
    - �������� ������ � �����
    - �������������� / ����������� ������ � �����
    - ������ ��������� �����
    - ������� / ����������� ����
   */
    

    
    //���������� ������ ������� ������
    public partial class FormMain : Form
    {
        private ImageList imgList;
        int newFileCount = 1;

        //��������� ���������� ��� �������� ����������� (�����, ���� ����� ���� ������� �����������)
        Bitmap imagePreview;
        FileOperations fileOperations = FileOperations.CreateFileOperations();

        //����� � ������� �����
        FileInfo[] files;

        //������� �����
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
                MessageBox.Show(ex.Message, "������ ��� ������ �� ������� �����������", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        void CreateImageList()
        {
            // �������� ������ ����������� 
            imgList = new ImageList();

            // ���������� ������ � ������ �����������
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
                //�������� ���� � ������
                TreeNode node = new TreeNode(drive, 4, 4);

                // �������� ������� ���� � ������
                dirTreeView.Nodes.Add(node);

                // ���������� ����� � �������
                FillByDirectories(node);
            }
        }

        private void FillByDirectories(TreeNode node)
        {
            try
            {
                // � node.FullPath - ��������� ������ ���� � �����
                DirectoryInfo dirInfo = new DirectoryInfo(node.FullPath);

                // ��������� ���������� � ���������
                DirectoryInfo[] dirs = dirInfo.GetDirectories();

                foreach (DirectoryInfo dir in dirs)
                {
                    TreeNode tree = new TreeNode(dir.Name, 0, 1);
                    node.Nodes.Add(tree);
                }
            }
            // ���������� (�������� ��� �����)
            catch { }
        }

        // ����� ����������� �� �������� ������� ����� ������
        // sender - ������ �� ������. e - ��������� ������
        private void dirTreeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            // ������ ���������� ����������� ������ �� ����� ���������� ���������
            dirTreeView.BeginUpdate();
            
            try
            {
                //������� ���� �������� ����� ��� ����(��������������� �� +)
                foreach (TreeNode node in e.Node.Nodes)
                {
                    FillByDirectories(node);
                }
            }
            catch { }
            // ������� ������ �������� ���������� ������ (����� �������� ����������� ������)
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
            // ��������� ����������
            fileListView.LargeImageList.Images.Clear();
            fileListView.SmallImageList.Images.Clear();
            int iconIndex = 0;
            fileListView.LargeImageList.Images.Add(Bitmap.FromFile("../../../resources/note11.ico"));
            fileListView.SmallImageList.Images.Add(Bitmap.FromFile("../../../resources/note11.ico"));

            foreach (FileInfo file in files)
            {
                ListViewItem item = new ListViewItem(file.Name);

                // �������� ������ ��� �������� �����
                Icon icon = Icon.ExtractAssociatedIcon(file.FullName);
                fileListView.LargeImageList.Images.Add(icon);
                fileListView.SmallImageList.Images.Add(icon);
                iconIndex++;

                //������� ����� ������ ��� listView
                item.ImageIndex = iconIndex;

                //�������� ����� � listView
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

        //�������� ���������� �����
        private void buttonCreate_Click(object sender, EventArgs e)
        {
            if (fileListView.Focus())
            {
                File.WriteAllText(currentDirInfo.FullName + $"\\New text file {newFileCount++}.txt", "");
            }
            FillByFiles(currentDirInfo.FullName);
        }

        //�������������� �����
        
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
        }        //�������������� �����
        
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {            

            // ������� ������ ������ �����
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
            // ��������� ���� ������ ��������� ������������� ��� ������
            fileOperations.copyFlag = true;                // ��������� ������ ��� ������ � ����� ������
            Clipboard.SetFileDropList(str);
             
        }
        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // ������� ������ ������ �����
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
                // ��������� ���� ������ ��������� ������������� ��� ������
                str.Add(fileOperations.dirInfo.FullName);
                // ��������� ������ ��� ������ � ����� ������
                Clipboard.SetFileDropList(str);
            }
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {             
            // �������� �� ������ ������ ��������� � �������
            IDataObject obj = Clipboard.GetDataObject();
             // ��������� ������� � ���������� ������ ������
            if (obj.GetDataPresent(DataFormats.FileDrop))
            {
                // �������� ������ ������ ��    
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
            // ���� ��������������� ������ ������
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // �������� � ���������� ������ ������
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
            // ���� ������������ �������� ������ ��������������� � ��� ������ ������ � ��� �� �������������� �� listBox � ���� ��
            if (e.Data.GetDataPresent(DataFormats.FileDrop) && (e.AllowedEffect & DragDropEffects.Copy) != 0 && !e.Data.GetDataPresent("Myappformat"))
            {
                // ��������� �����������
                e.Effect = DragDropEffects.Copy;
            }
        }
        private void fileListView_MouseDown(object sender, MouseEventArgs e)
        {
           if (fileListView.Focus() && fileListView.SelectedItems.Count > 0)
            {
                 // ������� ��������� ��� �������� ������
                DataObject data1 = new DataObject();

                // �������� ���������� ���������� � ������ ������
                StringCollection col = new StringCollection();
                foreach (ListViewItem file in fileListView.SelectedItems)
                {
                    col.Add(fileOperations.dirInfo.FullName + "\\" + file.Text);
                }                 
  
                data1.SetFileDropList(col);
                 

                // �������� ������� ����������������� ������� � ���������
                data1.SetData("Myappformat", 0);

                // ������ �������������� ����������
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
        // ��������� ����� ����� �� �� ��������� �� �������
        private void dirTreeView_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                Point p = dirTreeView.PointToClient(new Point(e.X, e.Y));
                TreeNode node = dirTreeView.GetNodeAt(p.X, p.Y);//��� ����� ����� ������ ��� �����
                if (node != null)
                {
                    fileOperations.dirInfo= new DirectoryInfo(node.FullPath);
                    // ���� ��������������� ������ ������
                    if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    {
                        // �������� � ���������� ������ ������
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