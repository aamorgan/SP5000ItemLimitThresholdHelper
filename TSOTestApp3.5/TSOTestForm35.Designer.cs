namespace TSOTestApp
{
    partial class TSOTester
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TSOTester));
            this.btnGetData = new System.Windows.Forms.Button();
            this.helloWorldLabel = new System.Windows.Forms.Label();
            this.txtURL = new System.Windows.Forms.TextBox();
            this.cbAlternateURL = new System.Windows.Forms.ComboBox();
            this.btnCheckCerts = new System.Windows.Forms.Button();
            this.txtCertCriteria1 = new System.Windows.Forms.TextBox();
            this.txtCertCriteria2 = new System.Windows.Forms.TextBox();
            this.txtRequest = new System.Windows.Forms.TextBox();
            this.txtResponse = new System.Windows.Forms.TextBox();
            this.btnQuit = new System.Windows.Forms.Button();
            this.chkUseCAC = new System.Windows.Forms.CheckBox();
            this.lbCerts = new System.Windows.Forms.ListBox();
            this.cbJustOne = new System.Windows.Forms.CheckBox();
            this.cbTLS12 = new System.Windows.Forms.CheckBox();
            this.rbXml = new System.Windows.Forms.RadioButton();
            this.rbJSON = new System.Windows.Forms.RadioButton();
            this.rbExcel = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rbLocal = new System.Windows.Forms.RadioButton();
            this.cbStore = new System.Windows.Forms.ComboBox();
            this.rbUser = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnGetData
            // 
            this.btnGetData.Enabled = false;
            this.btnGetData.Location = new System.Drawing.Point(14, 315);
            this.btnGetData.Margin = new System.Windows.Forms.Padding(2);
            this.btnGetData.Name = "btnGetData";
            this.btnGetData.Size = new System.Drawing.Size(130, 27);
            this.btnGetData.TabIndex = 2;
            this.btnGetData.Text = "2. Make Request";
            this.btnGetData.UseVisualStyleBackColor = true;
            this.btnGetData.Click += new System.EventHandler(this.btnGetData_Click);
            // 
            // helloWorldLabel
            // 
            this.helloWorldLabel.AutoSize = true;
            this.helloWorldLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.helloWorldLabel.ForeColor = System.Drawing.Color.Navy;
            this.helloWorldLabel.Location = new System.Drawing.Point(11, 9);
            this.helloWorldLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.helloWorldLabel.Name = "helloWorldLabel";
            this.helloWorldLabel.Size = new System.Drawing.Size(155, 20);
            this.helloWorldLabel.TabIndex = 3;
            this.helloWorldLabel.Text = "Web Service URL:";
            // 
            // txtURL
            // 
            this.txtURL.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtURL.Location = new System.Drawing.Point(171, 7);
            this.txtURL.Name = "txtURL";
            this.txtURL.Size = new System.Drawing.Size(492, 22);
            this.txtURL.TabIndex = 4;
            this.txtURL.Text = "https://interfacestest.atsc.army.mil/transcript-ws/api/";
            // 
            // cbAlternateURL
            // 
            this.cbAlternateURL.ForeColor = System.Drawing.Color.Navy;
            this.cbAlternateURL.FormattingEnabled = true;
            this.cbAlternateURL.Items.AddRange(new object[] {
            "Select an alternate test endpoint ...",
            "https://interfacestest.atsc.army.mil/transcript-ws/api/",
            "https://interfacestest.atsc.army.mil/transcript-ws/api",
            "https://atiasoatest.train.army.mil/profile-ws/api/ping?format=xml",
            "https://carsis.train.army.mil/B2Bcatalog/users/currentuser?format=xml",
            "https://tsmats.atsc.army.mil/TSMATS_Tools/api/tsims"});
            this.cbAlternateURL.Location = new System.Drawing.Point(171, 36);
            this.cbAlternateURL.Name = "cbAlternateURL";
            this.cbAlternateURL.Size = new System.Drawing.Size(370, 21);
            this.cbAlternateURL.TabIndex = 5;
            this.cbAlternateURL.Text = "Select an alternate test endpoint below ...";
            this.cbAlternateURL.SelectedIndexChanged += new System.EventHandler(this.cbAlternateURL_SelectedIndexChanged);
            // 
            // btnCheckCerts
            // 
            this.btnCheckCerts.Location = new System.Drawing.Point(15, 70);
            this.btnCheckCerts.Name = "btnCheckCerts";
            this.btnCheckCerts.Size = new System.Drawing.Size(130, 27);
            this.btnCheckCerts.TabIndex = 6;
            this.btnCheckCerts.Text = "1. Check for Certs";
            this.btnCheckCerts.UseVisualStyleBackColor = true;
            this.btnCheckCerts.Click += new System.EventHandler(this.btnCheckCerts_Click);
            // 
            // txtCertCriteria1
            // 
            this.txtCertCriteria1.Location = new System.Drawing.Point(171, 74);
            this.txtCertCriteria1.Name = "txtCertCriteria1";
            this.txtCertCriteria1.Size = new System.Drawing.Size(325, 20);
            this.txtCertCriteria1.TabIndex = 7;
            this.txtCertCriteria1.Text = "CN=portal2.tradoc.army.mil, OU=USA, OU=PKI, OU=DoD, O=U.S. Government, C=US";
            // 
            // txtCertCriteria2
            // 
            this.txtCertCriteria2.Location = new System.Drawing.Point(503, 74);
            this.txtCertCriteria2.Name = "txtCertCriteria2";
            this.txtCertCriteria2.Size = new System.Drawing.Size(160, 20);
            this.txtCertCriteria2.TabIndex = 8;
            this.txtCertCriteria2.Text = "DoD Root CA 3";
            // 
            // txtRequest
            // 
            this.txtRequest.Location = new System.Drawing.Point(171, 322);
            this.txtRequest.Name = "txtRequest";
            this.txtRequest.Size = new System.Drawing.Size(502, 20);
            this.txtRequest.TabIndex = 10;
            this.txtRequest.Text = "classes?format=xml&fromdate=2020-04-08&todate=2020-06-07";
            // 
            // txtResponse
            // 
            this.txtResponse.Location = new System.Drawing.Point(15, 348);
            this.txtResponse.Multiline = true;
            this.txtResponse.Name = "txtResponse";
            this.txtResponse.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtResponse.Size = new System.Drawing.Size(658, 256);
            this.txtResponse.TabIndex = 11;
            this.txtResponse.Text = "response ...";
            // 
            // btnQuit
            // 
            this.btnQuit.Location = new System.Drawing.Point(598, 610);
            this.btnQuit.Name = "btnQuit";
            this.btnQuit.Size = new System.Drawing.Size(75, 23);
            this.btnQuit.TabIndex = 12;
            this.btnQuit.Text = "Exit";
            this.btnQuit.UseVisualStyleBackColor = true;
            this.btnQuit.Click += new System.EventHandler(this.btnQuit_Click);
            // 
            // chkUseCAC
            // 
            this.chkUseCAC.AutoSize = true;
            this.chkUseCAC.Location = new System.Drawing.Point(44, 103);
            this.chkUseCAC.Name = "chkUseCAC";
            this.chkUseCAC.Size = new System.Drawing.Size(69, 17);
            this.chkUseCAC.TabIndex = 13;
            this.chkUseCAC.Text = "Use CAC";
            this.chkUseCAC.UseVisualStyleBackColor = true;
            // 
            // lbCerts
            // 
            this.lbCerts.FormattingEnabled = true;
            this.lbCerts.Location = new System.Drawing.Point(16, 172);
            this.lbCerts.Name = "lbCerts";
            this.lbCerts.Size = new System.Drawing.Size(658, 108);
            this.lbCerts.TabIndex = 14;
            this.lbCerts.SelectedIndexChanged += new System.EventHandler(this.lbCerts_SelectedIndexChanged);
            // 
            // cbJustOne
            // 
            this.cbJustOne.AutoSize = true;
            this.cbJustOne.Enabled = false;
            this.cbJustOne.Location = new System.Drawing.Point(16, 149);
            this.cbJustOne.Name = "cbJustOne";
            this.cbJustOne.Size = new System.Drawing.Size(242, 17);
            this.cbJustOne.TabIndex = 15;
            this.cbJustOne.Text = "Just use selected to Auth (uncheck to use All)";
            this.cbJustOne.UseVisualStyleBackColor = true;
            this.cbJustOne.CheckedChanged += new System.EventHandler(this.cbJustOne_CheckedChanged);
            // 
            // cbTLS12
            // 
            this.cbTLS12.AutoSize = true;
            this.cbTLS12.Location = new System.Drawing.Point(577, 40);
            this.cbTLS12.Name = "cbTLS12";
            this.cbTLS12.Size = new System.Drawing.Size(86, 17);
            this.cbTLS12.TabIndex = 16;
            this.cbTLS12.Text = "TLS 1.2 only";
            this.cbTLS12.UseVisualStyleBackColor = true;
            // 
            // rbXml
            // 
            this.rbXml.AutoSize = true;
            this.rbXml.Checked = true;
            this.rbXml.Location = new System.Drawing.Point(26, 610);
            this.rbXml.Name = "rbXml";
            this.rbXml.Size = new System.Drawing.Size(87, 17);
            this.rbXml.TabIndex = 17;
            this.rbXml.TabStop = true;
            this.rbXml.Text = "XML Content";
            this.rbXml.UseVisualStyleBackColor = true;
            this.rbXml.CheckedChanged += new System.EventHandler(this.rbXml_CheckedChanged);
            // 
            // rbJSON
            // 
            this.rbJSON.AutoSize = true;
            this.rbJSON.Location = new System.Drawing.Point(132, 610);
            this.rbJSON.Name = "rbJSON";
            this.rbJSON.Size = new System.Drawing.Size(93, 17);
            this.rbJSON.TabIndex = 18;
            this.rbJSON.Text = "JSON Content";
            this.rbJSON.UseVisualStyleBackColor = true;
            this.rbJSON.CheckedChanged += new System.EventHandler(this.rbXml_CheckedChanged);
            // 
            // rbExcel
            // 
            this.rbExcel.AutoSize = true;
            this.rbExcel.Location = new System.Drawing.Point(231, 610);
            this.rbExcel.Name = "rbExcel";
            this.rbExcel.Size = new System.Drawing.Size(91, 17);
            this.rbExcel.TabIndex = 19;
            this.rbExcel.Text = "Excel Content";
            this.rbExcel.CheckedChanged += new System.EventHandler(this.rbXml_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rbLocal);
            this.groupBox1.Controls.Add(this.cbStore);
            this.groupBox1.Controls.Add(this.rbUser);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(171, 103);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(492, 43);
            this.groupBox1.TabIndex = 26;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Certificate Store Location";
            // 
            // rbLocal
            // 
            this.rbLocal.AutoSize = true;
            this.rbLocal.Checked = true;
            this.rbLocal.Location = new System.Drawing.Point(296, 14);
            this.rbLocal.Name = "rbLocal";
            this.rbLocal.Size = new System.Drawing.Size(95, 17);
            this.rbLocal.TabIndex = 23;
            this.rbLocal.TabStop = true;
            this.rbLocal.Text = "Local Machine";
            this.rbLocal.UseVisualStyleBackColor = true;
            // 
            // cbStore
            // 
            this.cbStore.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.cbStore.FormattingEnabled = true;
            this.cbStore.Items.AddRange(new object[] {
            "WebHosting",
            "My",
            "TrustedPeople",
            "SharePoint"});
            this.cbStore.Location = new System.Drawing.Point(139, 13);
            this.cbStore.Name = "cbStore";
            this.cbStore.Size = new System.Drawing.Size(105, 21);
            this.cbStore.TabIndex = 22;
            this.cbStore.Text = "WebHosting";
            // 
            // rbUser
            // 
            this.rbUser.AutoSize = true;
            this.rbUser.Location = new System.Drawing.Point(406, 14);
            this.rbUser.Name = "rbUser";
            this.rbUser.Size = new System.Drawing.Size(84, 17);
            this.rbUser.TabIndex = 24;
            this.rbUser.Text = "Current User";
            this.rbUser.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(111, 13);
            this.label1.TabIndex = 21;
            this.label1.Text = "Look in this Cert Store";
            // 
            // TSOTester
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(686, 646);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.rbExcel);
            this.Controls.Add(this.rbJSON);
            this.Controls.Add(this.rbXml);
            this.Controls.Add(this.cbTLS12);
            this.Controls.Add(this.cbJustOne);
            this.Controls.Add(this.lbCerts);
            this.Controls.Add(this.chkUseCAC);
            this.Controls.Add(this.btnQuit);
            this.Controls.Add(this.txtResponse);
            this.Controls.Add(this.txtRequest);
            this.Controls.Add(this.txtCertCriteria2);
            this.Controls.Add(this.txtCertCriteria1);
            this.Controls.Add(this.btnCheckCerts);
            this.Controls.Add(this.cbAlternateURL);
            this.Controls.Add(this.txtURL);
            this.Controls.Add(this.helloWorldLabel);
            this.Controls.Add(this.btnGetData);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TSOTester";
            this.Text = "TSO Test Application (.Net 3.5 compatible w/ SP2010)";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnGetData;
        private System.Windows.Forms.Label helloWorldLabel;
        private System.Windows.Forms.TextBox txtURL;
        private System.Windows.Forms.ComboBox cbAlternateURL;
        private System.Windows.Forms.Button btnCheckCerts;
        private System.Windows.Forms.TextBox txtCertCriteria1;
        private System.Windows.Forms.TextBox txtCertCriteria2;
        private System.Windows.Forms.TextBox txtRequest;
        private System.Windows.Forms.TextBox txtResponse;
        private System.Windows.Forms.Button btnQuit;
        private System.Windows.Forms.CheckBox chkUseCAC;
        private System.Windows.Forms.ListBox lbCerts;
        private System.Windows.Forms.CheckBox cbJustOne;
        private System.Windows.Forms.CheckBox cbTLS12;
        private System.Windows.Forms.RadioButton rbXml;
        private System.Windows.Forms.RadioButton rbJSON;
        private System.Windows.Forms.RadioButton rbExcel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rbLocal;
        private System.Windows.Forms.ComboBox cbStore;
        private System.Windows.Forms.RadioButton rbUser;
        private System.Windows.Forms.Label label1;
    }
}

