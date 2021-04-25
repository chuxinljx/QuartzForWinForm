using QuartzForWinForm.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuartzForWinForm
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }
         
        /// <summary>
        /// 刷新DataGridView控件
        /// </summary>
        public void LoadDataGridView()
        {
           
           DataTable dt = new DataTable();
            DataColumn dc1 = new DataColumn("Dll名称");
            DataColumn dc2 = new DataColumn("操作栏");
            dt.Columns.Add(dc1);
            dt.Columns.Add(dc2);

            foreach (var item in QuartzHelper._AppDomainKeys)
            {
                DataRow dr = dt.NewRow();
                dr["Dll名称"] = item.Key;
                dr["操作栏"] = PublicMethod.ToDescription<Status>(item.Value.status.ToString());
                dt.Rows.Add(dr);

            }
            BindingSource bs = new BindingSource();
            bs.DataSource = dt;
            this.dataGridView1.DataSource = bs;
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            PublicMethod.AddJobScheduler();
            LoadDataGridView();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Multiselect = false;
            openFile.RestoreDirectory = true;
            openFile.FilterIndex = 1;
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                var path = openFile.FileName;
                if (!path.EndsWith(".dll"))
                {
                    MessageBox.Show("只能导入后缀为dll的动态链接库");
                    return;
                }
                label3.Text = path;
            }

        }

        private void inputButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(this.label3.Text))
                {
                    MessageBox.Show("请导入Dll文件");
                    return;
                }
                var name = PublicMethod.GetDllName(label3.Text);
                if (PublicMethod.QueryNode(name))
                {
                    MessageBox.Show("当前Dll已存在配置文件，不能再次添加");
                    return;
                }
                if (string.IsNullOrEmpty(this.job.Text))
                {
                    throw new ArgumentNullException(nameof(this.job));

                }
                if (string.IsNullOrEmpty(this.group.Text))
                {
                    throw new ArgumentNullException(nameof(this.group));
                }
                if (string.IsNullOrEmpty(this.jobtype.Text))
                {
                    throw new ArgumentNullException(nameof(this.jobtype));
                }
                if (string.IsNullOrEmpty(this.intervaltime.Value.ToString()))
                {
                    throw new ArgumentNullException(nameof(this.intervaltime));
                }
                if (string.IsNullOrEmpty(this.begintime.Value.ToString()))
                {
                    throw new  ArgumentNullException(nameof(this.begintime));
                }
                if (string.IsNullOrEmpty(this.endtime.Value.ToString()))
                {
                    throw new ArgumentNullException(nameof(this.endtime));
                }
                NodeDetail nodeDetail = new NodeDetail()
                {
                    Begintime = this.begintime.Value.ToString(),
                    Endtime = this.endtime.Value.ToString(),
                    IntervalTime = this.intervaltime.Value.ToString(),
                    Group = this.group.Text,
                    Job = this.job.Text,
                    Jobtype = this.jobtype.Text,
                    Name = name
                };
                PublicMethod.AddNode(nodeDetail);
                QuartzHelper.Import(this.label3.Text, "Add");
                LoadDataGridView();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
   

        }

        private void button2_Click(object sender, EventArgs e)
        {
            LoadDataGridView();
        }

        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int rowIndex = dataGridView1.CurrentRow.Index;
            string dllName = dataGridView1.Rows[rowIndex].Cells[0].Value.ToString();
            var result = MessageBox.Show($"将卸载{dllName}和删除相关配置文件", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
            if (result == DialogResult.OK)
            {
                QuartzHelper.Unload(dllName);
                PublicMethod.RemoveNode(dllName);
                LoadDataGridView();
            }


        }

        private void 更新ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int rowIndex = dataGridView1.CurrentRow.Index;
            string dllName = dataGridView1.Rows[rowIndex].Cells[0].Value.ToString();
            var result = MessageBox.Show($"将重新导入Dll替换旧Dll", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
            if (result == DialogResult.OK)
            {
                QuartzHelper.Unload(dllName);
                OpenFileDialog openFile = new OpenFileDialog();
                openFile.Multiselect = false;
                openFile.RestoreDirectory = true;
                openFile.FilterIndex = 1;
                if (openFile.ShowDialog() == DialogResult.OK)
                {
                    var path = openFile.FileName;
                    if (!path.EndsWith(".dll"))
                    {
                        MessageBox.Show("只能导入后缀为dll的动态链接库");
                        return;
                    }
                    QuartzHelper.Import(path);
                    MessageBox.Show("更新成功");
                    LoadDataGridView();
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
           var result= MessageBox.Show("关闭窗体将会卸载Dll并且删除配置文件","提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
            if (result == DialogResult.OK)
            {
                PublicMethod.CloseRemove();
            }
            else {
                e.Cancel = true;
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.ColumnIndex == 1 && e.RowIndex != -1)
                {
                   
                    var dllName = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
                    if (!QuartzHelper._AppDomainKeys.ContainsKey(dllName))
                    {
                        throw new Exception($"当前{dllName}不存在");
                    }
                    var status = QuartzHelper._AppDomainKeys[dllName].status == Status.off ? "启用" : "停用";
                    var result = MessageBox.Show($"是否{status}Dll?", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
                    if (!(result == DialogResult.OK)) { return; }
                    if (QuartzHelper._AppDomainKeys[dllName].status == Status.on)
                    {
                        AppDomain.Unload(QuartzHelper._AppDomainKeys[dllName].appDomain);
                        QuartzHelper._AppDomainKeys[dllName].status = Status.off;
                    }
                    else {
                        OpenFileDialog openFile = new OpenFileDialog();
                        openFile.Multiselect = false;
                        openFile.RestoreDirectory = true;
                        openFile.FilterIndex = 1;
                        if (openFile.ShowDialog() == DialogResult.OK)
                        {
                            var path = openFile.FileName;
                            if (!path.EndsWith(".dll"))
                            {
                                MessageBox.Show("只能导入后缀为dll的动态链接库");
                                return;
                            }
                           var dllName1= PublicMethod.GetDllName(path);
                            if (dllName!=dllName1) {
                                MessageBox.Show("只能导入与停用Dll一致的动态库");
                                return;
                            }
                            QuartzHelper.CheckOn(path);
                        }
                        QuartzHelper._AppDomainKeys[dllName].status = Status.on;
                    }
                    LoadDataGridView();
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
           
           
        }

    }
}
