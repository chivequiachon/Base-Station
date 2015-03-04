using System;
using System.Windows.Forms;
using System.Drawing;
using System.Text;
using System.Data;

namespace BaseStationTest
{
    public class View : Form
    {
        // mvc attributes
        private Controller c;
        private Model m;

        private TextBox firstMultiplicand;
        private TextBox secondMultiplicand;
        private TextBox firstAdder;
        private TextBox thirdMultiplicand;
        private Label appServerIDLabel;
        private Label comPortLabel;
        private ComboBox comPortList;
        private Button startButton;
        private GroupBox group1;
        private RichTextBox rtxtbConStatus;
        private GroupBox group2;
        private GroupBox group4;
        private GroupBox group3;
        private RichTextBox rtxtbConsole;
        private DataGridView gridWCS;
       
        private DataTable wcsTable;

        private StringBuilder consoleTexts;
        private Button stopButton;
        private Label label1;
        private Button refreshButton;
        private TextBox tb_port;
        private Label label2;
        private TextBox tb_host;
        private StringBuilder connectionTexts;

        public View(Model m, Controller c)
        {
            // initialize mvc attributes
            this.m = m;
            this.c = c;

            // add view as observer to model
            m.AddObserver(this);

            // initialize wcs table
            wcsTable = new DataTable();
            wcsTable.Columns.Add("WCS_ID", typeof(String));
            wcsTable.Columns.Add("Time_Remaining", typeof(int));

            // initialize string builders
            consoleTexts = new StringBuilder();
            connectionTexts = new StringBuilder();

            InitializeComponent();

            gridWCS.DataSource = wcsTable;

            this.FormClosing += new FormClosingEventHandler(
                (object sender, FormClosingEventArgs e) =>
                {
                    this.c.HandleClosingEvent();
                }
            );

            this.startButton.Click += new EventHandler(
                (object sender, EventArgs e) =>
                {
                    if (this.startButton.Text == "Start")
                    {
                        String multiplier1 = firstMultiplicand.Text;
                        String multiplier2 = secondMultiplicand.Text;
                        String multiplier3 = thirdMultiplicand.Text;
                        String adder1 = firstAdder.Text;

                        String port = comPortList.Text;
                        String dbHost = tb_host.Text;
                        String dbPort = tb_port.Text;

                        this.c.HandleActivationEvent(multiplier1, multiplier2, multiplier3, adder1, port, dbHost, dbPort);
                    }
                    else if (this.startButton.Text == "Resume")
                    {
                        this.c.HandleResumeEvent();
                    }
                    else if (this.startButton.Text == "Pause")
                    {
                        this.c.HandlePauseEvent();
                    }
                }
            );

            this.stopButton.Click += new EventHandler(
                (object sender, EventArgs e) =>
                {
                    this.c.HandleDeactivationEvent();
                }
            );

            this.refreshButton.Click += new EventHandler(
                (object sender, EventArgs e) =>
                {
                    this.comPortList.DataSource = m.DetectComPorts();
                }
            );

            // set com port list
            comPortList.DataSource = m.DetectComPorts();
        }

        public void Run()
        {
            Application.Run(this);
        }

        // observer notifications
        public void RemoveFromTable(String id)
        {
            this.Invoke(
                new MethodInvoker(
                    () =>
                    {
                        for (int i = wcsTable.Rows.Count - 1; i >= 0; i--)
                        {
                            DataRow dr = wcsTable.Rows[i];
                            if (dr["WCS_ID"] == id)
                                dr.Delete();
                        }
                    }
                )
            );
        }

        public void AddToTable(String id, int time)
        {
            this.Invoke(
                new MethodInvoker(
                    () =>
                    {
                        DataRow newRow = wcsTable.NewRow();
                        newRow["WCS_ID"] = id;
                        newRow["Time_Remaining"] = time;
                        wcsTable.Rows.Add(newRow);
                    }
                )
            );
        }

        public void UpdateTable(String id, int time)
        {
            this.Invoke(
                new MethodInvoker(
                    () =>
                    {
                        try
                        {
                            DataRow[] toBeUpdatedRow = wcsTable.Select("WCS_ID = '" + id + "'");
                            toBeUpdatedRow[0]["Time_Remaining"] = time;
                        }
                        catch(Exception)
                        {
                        
                        }
                    }
                )
            );
        }

        public void AppendToConsole(String componentName, String text)
        {
            this.Invoke(
                new MethodInvoker(
                    ()=>
                    {
                        consoleTexts.Append("<" + componentName + "> " + text + Environment.NewLine);
                        rtxtbConsole.Text = consoleTexts.ToString();
                    }
                )
            );
        }

        public void AppendToConnectionStatus(String text)
        {
            this.Invoke(
                new MethodInvoker(
                    () =>
                    {
                        connectionTexts.Append("[" + DateTime.Now + "] " + text + Environment.NewLine);
                        rtxtbConStatus.Text = connectionTexts.ToString();
                    }
                )
            );
        }

        public void ShowErrorDialog(String title, String message)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void UpdateButtons()
        {
            if (m.State == 0) // stopped
            {
                startButton.Text = "Start";
                stopButton.Enabled = false;
                refreshButton.Enabled = true;
            }
            else if (m.State == 1) // started
            {
                startButton.Text = "Pause";
                stopButton.Enabled = true;
                refreshButton.Enabled = false;
            }
            else if (m.State == 2) // paused
            {
                startButton.Text = "Resume";
                stopButton.Enabled = true;
                refreshButton.Enabled = false;
            }

            startButton.Enabled = true;
        }

        public void DisableButtons()
        {
            stopButton.Enabled = false;
            startButton.Enabled = false;
            refreshButton.Enabled = false;
        }

        public void EnableButtons()
        {
            stopButton.Enabled = true;
            startButton.Enabled = true;
            refreshButton.Enabled = true;
        }

        private void InitializeComponent()
        {
            this.firstMultiplicand = new System.Windows.Forms.TextBox();
            this.secondMultiplicand = new System.Windows.Forms.TextBox();
            this.firstAdder = new System.Windows.Forms.TextBox();
            this.thirdMultiplicand = new System.Windows.Forms.TextBox();
            this.appServerIDLabel = new System.Windows.Forms.Label();
            this.comPortLabel = new System.Windows.Forms.Label();
            this.comPortList = new System.Windows.Forms.ComboBox();
            this.startButton = new System.Windows.Forms.Button();
            this.group1 = new System.Windows.Forms.GroupBox();
            this.stopButton = new System.Windows.Forms.Button();
            this.rtxtbConStatus = new System.Windows.Forms.RichTextBox();
            this.group2 = new System.Windows.Forms.GroupBox();
            this.group4 = new System.Windows.Forms.GroupBox();
            this.gridWCS = new System.Windows.Forms.DataGridView();
            this.group3 = new System.Windows.Forms.GroupBox();
            this.rtxtbConsole = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tb_host = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tb_port = new System.Windows.Forms.TextBox();
            this.refreshButton = new System.Windows.Forms.Button();
            this.group1.SuspendLayout();
            this.group2.SuspendLayout();
            this.group4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridWCS)).BeginInit();
            this.group3.SuspendLayout();
            this.SuspendLayout();
            // 
            // firstMultiplicand
            // 
            this.firstMultiplicand.Location = new System.Drawing.Point(86, 23);
            this.firstMultiplicand.MaxLength = 3;
            this.firstMultiplicand.Name = "firstMultiplicand";
            this.firstMultiplicand.Size = new System.Drawing.Size(37, 20);
            this.firstMultiplicand.TabIndex = 0;
            // 
            // secondMultiplicand
            // 
            this.secondMultiplicand.Location = new System.Drawing.Point(129, 23);
            this.secondMultiplicand.MaxLength = 3;
            this.secondMultiplicand.Name = "secondMultiplicand";
            this.secondMultiplicand.Size = new System.Drawing.Size(37, 20);
            this.secondMultiplicand.TabIndex = 1;
            // 
            // firstAdder
            // 
            this.firstAdder.Location = new System.Drawing.Point(215, 23);
            this.firstAdder.MaxLength = 3;
            this.firstAdder.Name = "firstAdder";
            this.firstAdder.Size = new System.Drawing.Size(37, 20);
            this.firstAdder.TabIndex = 2;
            // 
            // thirdMultiplicand
            // 
            this.thirdMultiplicand.Location = new System.Drawing.Point(172, 23);
            this.thirdMultiplicand.MaxLength = 3;
            this.thirdMultiplicand.Name = "thirdMultiplicand";
            this.thirdMultiplicand.Size = new System.Drawing.Size(37, 20);
            this.thirdMultiplicand.TabIndex = 3;
            // 
            // appServerIDLabel
            // 
            this.appServerIDLabel.AutoSize = true;
            this.appServerIDLabel.Location = new System.Drawing.Point(6, 26);
            this.appServerIDLabel.Name = "appServerIDLabel";
            this.appServerIDLabel.Size = new System.Drawing.Size(77, 13);
            this.appServerIDLabel.TabIndex = 4;
            this.appServerIDLabel.Text = "App Server ID:";
            // 
            // comPortLabel
            // 
            this.comPortLabel.AutoSize = true;
            this.comPortLabel.Location = new System.Drawing.Point(6, 133);
            this.comPortLabel.Name = "comPortLabel";
            this.comPortLabel.Size = new System.Drawing.Size(56, 13);
            this.comPortLabel.TabIndex = 5;
            this.comPortLabel.Text = "COM Port:";
            // 
            // comPortList
            // 
            this.comPortList.FormattingEnabled = true;
            this.comPortList.Location = new System.Drawing.Point(86, 130);
            this.comPortList.Name = "comPortList";
            this.comPortList.Size = new System.Drawing.Size(166, 21);
            this.comPortList.TabIndex = 6;
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(265, 23);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(68, 59);
            this.startButton.TabIndex = 7;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            // 
            // group1
            // 
            this.group1.Controls.Add(this.refreshButton);
            this.group1.Controls.Add(this.tb_port);
            this.group1.Controls.Add(this.label2);
            this.group1.Controls.Add(this.tb_host);
            this.group1.Controls.Add(this.label1);
            this.group1.Controls.Add(this.stopButton);
            this.group1.Controls.Add(this.appServerIDLabel);
            this.group1.Controls.Add(this.startButton);
            this.group1.Controls.Add(this.firstMultiplicand);
            this.group1.Controls.Add(this.comPortList);
            this.group1.Controls.Add(this.secondMultiplicand);
            this.group1.Controls.Add(this.comPortLabel);
            this.group1.Controls.Add(this.firstAdder);
            this.group1.Controls.Add(this.thirdMultiplicand);
            this.group1.Location = new System.Drawing.Point(38, 26);
            this.group1.Name = "group1";
            this.group1.Size = new System.Drawing.Size(413, 163);
            this.group1.TabIndex = 8;
            this.group1.TabStop = false;
            this.group1.Text = "Control";
            // 
            // stopButton
            // 
            this.stopButton.Enabled = false;
            this.stopButton.Location = new System.Drawing.Point(339, 23);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(68, 59);
            this.stopButton.TabIndex = 8;
            this.stopButton.Text = "Stop";
            this.stopButton.UseVisualStyleBackColor = true;
            // 
            // rtxtbConStatus
            // 
            this.rtxtbConStatus.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.rtxtbConStatus.Location = new System.Drawing.Point(6, 19);
            this.rtxtbConStatus.Name = "rtxtbConStatus";
            this.rtxtbConStatus.ReadOnly = true;
            this.rtxtbConStatus.Size = new System.Drawing.Size(523, 138);
            this.rtxtbConStatus.TabIndex = 9;
            this.rtxtbConStatus.Text = "";
            // 
            // group2
            // 
            this.group2.Controls.Add(this.rtxtbConStatus);
            this.group2.Location = new System.Drawing.Point(457, 26);
            this.group2.Name = "group2";
            this.group2.Size = new System.Drawing.Size(535, 163);
            this.group2.TabIndex = 10;
            this.group2.TabStop = false;
            this.group2.Text = "Connection Status";
            // 
            // group4
            // 
            this.group4.Controls.Add(this.gridWCS);
            this.group4.Location = new System.Drawing.Point(38, 195);
            this.group4.Name = "group4";
            this.group4.Size = new System.Drawing.Size(230, 314);
            this.group4.TabIndex = 12;
            this.group4.TabStop = false;
            this.group4.Text = "WCS Status";
            // 
            // gridWCS
            // 
            this.gridWCS.BackgroundColor = System.Drawing.SystemColors.Window;
            this.gridWCS.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridWCS.Location = new System.Drawing.Point(9, 19);
            this.gridWCS.Name = "gridWCS";
            this.gridWCS.RowHeadersVisible = false;
            this.gridWCS.Size = new System.Drawing.Size(212, 289);
            this.gridWCS.TabIndex = 0;
            // 
            // group3
            // 
            this.group3.Controls.Add(this.rtxtbConsole);
            this.group3.Location = new System.Drawing.Point(274, 195);
            this.group3.Name = "group3";
            this.group3.Size = new System.Drawing.Size(718, 314);
            this.group3.TabIndex = 11;
            this.group3.TabStop = false;
            this.group3.Text = "Console";
            // 
            // rtxtbConsole
            // 
            this.rtxtbConsole.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.rtxtbConsole.Location = new System.Drawing.Point(6, 19);
            this.rtxtbConsole.Name = "rtxtbConsole";
            this.rtxtbConsole.ReadOnly = true;
            this.rtxtbConsole.Size = new System.Drawing.Size(706, 289);
            this.rtxtbConsole.TabIndex = 9;
            this.rtxtbConsole.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 65);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "DB Server host:";
            // 
            // tb_host
            // 
            this.tb_host.Location = new System.Drawing.Point(86, 62);
            this.tb_host.MaxLength = 15;
            this.tb_host.Name = "tb_host";
            this.tb_host.Size = new System.Drawing.Size(166, 20);
            this.tb_host.TabIndex = 10;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 99);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "DB Server port:";
            // 
            // tb_port
            // 
            this.tb_port.Location = new System.Drawing.Point(86, 96);
            this.tb_port.MaxLength = 315;
            this.tb_port.Name = "tb_port";
            this.tb_port.Size = new System.Drawing.Size(166, 20);
            this.tb_port.TabIndex = 12;
            // 
            // refreshButton
            // 
            this.refreshButton.Location = new System.Drawing.Point(265, 88);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(142, 63);
            this.refreshButton.TabIndex = 13;
            this.refreshButton.Text = "Refresh";
            this.refreshButton.UseVisualStyleBackColor = true;
            // 
            // View
            // 
            this.ClientSize = new System.Drawing.Size(1038, 557);
            this.Controls.Add(this.group3);
            this.Controls.Add(this.group4);
            this.Controls.Add(this.group2);
            this.Controls.Add(this.group1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "View";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PWSS Application Server";
            this.group1.ResumeLayout(false);
            this.group1.PerformLayout();
            this.group2.ResumeLayout(false);
            this.group4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridWCS)).EndInit();
            this.group3.ResumeLayout(false);
            this.ResumeLayout(false);

        }
    }
}
