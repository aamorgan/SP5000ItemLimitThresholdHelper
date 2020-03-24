using BandR;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint;
using SP5000ItemLimitThresholdHelper.classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.IO;

namespace SP5000ItemLimitThresholdHelper
{ 
    public partial class Form1 : System.Windows.Forms.Form
    {

        private AboutForm aboutForm = null;
        private BackgroundWorker bgw = null;
        private int statusWindowOutputBatchSize = GenUtil.SafeToInt(ConfigurationManager.AppSettings["statusWindowOutputBatchSize"]);
        private bool showFullErrMsgs = GenUtil.SafeToBool(ConfigurationManager.AppSettings["showFullErrMsgs"]);
        private bool ErrorOccurred = false;
        private string selAction;
        private ListCollection collLists;
        private List<string> ListNames;



        /// <summary>
        /// </summary>
        public Form1()
        {
            InitializeComponent();

            toolStripStatusLabel1.Text = "";

            this.FormClosed += Form1_FormClosed;

            ddlActions.SelectedItem = "Archive List";
            ddlActions_SelectedIndexChanged(null, null);

            LoadSettingsFromRegistry();
            tbDestList.Text = tbSourceList.Text + "_Archive";

            imageBandR.Visible = true;
            imageBandRwait.Visible = false;

            lblNoErrorFound.Visible = lblErrorFound.Visible = false;
        }



        /// <summary>
        /// </summary>
        private void btnSwapSourceDest_Click(object sender, EventArgs e)
        {
            var source = tbSourceList.Text;
            var dest = tbDestList.Text;

            tbSourceList.Text = dest;
            tbDestList.Text = source;
        }

        /// <summary>
        /// </summary>
        private ICredentials BuildCreds()
        {
            var userName = tbUsername.Text.Trim();
            var passWord = tbPassword.Text.Trim();
            var domain = tbDomain.Text.Trim();

            if (!cbIsSPOnline.Checked)
            {
                return new NetworkCredential(userName, passWord, domain);
            }
            else
            {
                return new SharePointOnlineCredentials(userName, GenUtil.BuildSecureString(passWord));
            }
        }

        /// <summary>
        /// </summary>
        private void ctx_ExecutingWebRequest_FixForMixedMode(object sender, WebRequestEventArgs e)
        {
            // to support mixed mode auth
            e.WebRequestExecutor.RequestHeaders.Add("X-FORMS_BASED_AUTH_ACCEPTED", "f");
        }

        /// <summary>
        /// </summary>
        private void FixCtxForMixedMode(ClientContext ctx)
        {
            if (!cbIsSPOnline.Checked)
            {
                ctx.ExecutingWebRequest += new EventHandler<WebRequestEventArgs>(ctx_ExecutingWebRequest_FixForMixedMode);
            }
        }

        /// <summary>
        /// </summary>
        private void LoadSettingsFromRegistry()
        {
            var msg = "";
            var json = "";

            if (RegistryHelper.GetRegStuff(out json, out msg) && !json.IsNull())
            {
                var obj = JsonExtensionMethod.FromJson<CustomRegistrySettings>(json);

                tbSiteUrl.Text = obj.siteUrl;
                tbUsername.Text = obj.userName;
                tbPassword.Text = obj.passWord;
                tbDomain.Text = obj.domain;
                cbIsSPOnline.Checked = GenUtil.SafeToBool(obj.isSPOnline);
                tbSourceList.Text = obj.sourceListName;
                tbDestList.Text = obj.destListName;
                tbItemsToProcess.Text = GenUtil.SafeToInt(obj.numItemsToProcess).ToString();
            }
        }

        /// <summary>
        /// </summary>
        private void SaveSettingsToRegistry()
        {
            var msg = "";

            var obj = new CustomRegistrySettings
            {
                siteUrl = tbSiteUrl.Text.Trim(),
                userName = tbUsername.Text.Trim(),
                passWord = tbPassword.Text.Trim(),
                domain = tbDomain.Text.Trim(),
                isSPOnline = cbIsSPOnline.Checked ? "1" : "0",
                sourceListName = tbSourceList.Text.Trim(),
                destListName = tbDestList.Text.Trim(),
                numItemsToProcess = tbItemsToProcess.Text.Trim()
            };
            var json = JsonExtensionMethod.ToJson(obj);

            RegistryHelper.SaveRegStuff(json, out msg);
        }

        /// <summary>
        /// </summary>
        private List<int> ConvertToListOfInts(string s)
        {
            var lst = new List<int>();

            if (!s.IsNull())
            {
                lst = GenUtil.NormalizeEol(s)
                    .Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .Distinct().ToList();
            }

            return lst;
        }

        /// <summary>
        /// </summary>
        private List<string> ConvertToListOfStrings(string s)
        {
            var lst = new List<string>();

            if (!s.IsNull())
            {
                lst = GenUtil.NormalizeEol(s)
                    .Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                    .Distinct().ToList();
            }

            return lst;
        }

        /// <summary>
        /// </summary>
        void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (aboutForm != null)
            {
                aboutForm.Dispose();
            }

            SaveSettingsToRegistry();
        }

        /// <summary>
        /// </summary>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// </summary>
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (aboutForm == null)
            {
                aboutForm = new AboutForm();
            }

            aboutForm.Show();
            aboutForm.Focus();
        }

        /// <summary>
        /// </summary>
        private void DisableFormControls()
        {
            toolStripStatusLabel1.Text = "Running...";

            imageBandR.Visible = false;
            imageBandRwait.Visible = true;

            btnTestConnection.Enabled = false;

            lnkClear.Enabled = false;
            lnkExport.Enabled = false;
        }

        /// <summary>
        /// </summary>
        private void EnableFormControls()
        {
            toolStripStatusLabel1.Text = "";

            imageBandR.Visible = true;
            imageBandRwait.Visible = false;

            btnTestConnection.Enabled = true;

            lnkClear.Enabled = true;
            lnkExport.Enabled = true;

            btnAbort.Enabled = true;
        }





        /// <summary>
        /// Combine function params as strings with separator, no line breaks.
        /// </summary>
        public string CombineFnParmsToString(params object[] objs)
        {
            string output = "";
            string delim = ": ";

            for (int i = 0; i < objs.Length; i++)
            {
                if (objs[i] == null) objs[i] = "";
                if (i == objs.Length - 1) delim = "";
                output += string.Concat(objs[i], delim);
            }

            return output;
        }

        /// <summary>
        /// Build message for status window, prepend datetime, append message (already combined with separator), append newline chars.
        /// </summary>
        public string BuildCoutMessage(string msg)
        {
            return string.Format("{0}: {1}\r\n", DateTime.Now.ToLongTimeString(), msg);
        }

        /// <summary>
        /// Standard status dumping function, immediately dumps to status window.
        /// </summary>
        public void cout(params object[] objs)
        {
            tbStatus.AppendText(BuildCoutMessage(CombineFnParmsToString(objs)));
        }

        string tcout_buffer;
        int tcout_counter;

        /// <summary>
        /// Threaded status dumping function, uses buffer to only dump to status window peridocially, batch size configured in app.config.
        /// </summary>
        public void tcout(params object[] objs)
        {
            tcout_counter++;
            tcout_buffer += BuildCoutMessage(CombineFnParmsToString(objs));

            var batchSize = statusWindowOutputBatchSize == 0 ? 1 : statusWindowOutputBatchSize;

            if (tcout_counter % batchSize == 0)
            {
                bgw.ReportProgress(0, tcout_buffer);
                InitCoutBuffer();
            }
        }

        /// <summary>
        /// Reset status buffer.
        /// </summary>
        private void InitCoutBuffer()
        {
            tcout_counter = 0;
            tcout_buffer = "";
        }

        /// <summary>
        /// Flush status buffer to status window (since using mod operator).
        /// </summary>
        private void FlushCoutBuffer()
        {
            if (!tcout_buffer.IsNull())
            {
                tbStatus.AppendText(tcout_buffer);
                InitCoutBuffer();
            }
        }

        /// <summary>
        /// Threaded callback function, dump input to status window, already formatted with datetime, combined params, and linebreaks.
        /// </summary>
        private void BgwReportProgress(object sender, ProgressChangedEventArgs e)
        {
            tbStatus.AppendText(e.UserState.ToString());
        }

        /// <summary>
        /// </summary>
        private void lnkClear_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            tbStatus.ResetText();
        }

        /// <summary>
        /// </summary>
        private void lnkExport_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SaveLogToFile(null);
            MessageBox.Show("Log saved to EXE folder.");
        }

        /// <summary>
        /// </summary>
        void SaveLogToFile(string action)
        {
            if (!action.IsNull())
            {
                action = action.Trim().ToUpper() + "_";
            }

            var exportFilePath = AppDomain.CurrentDomain.BaseDirectory;
            if (!System.IO.Directory.Exists(exportFilePath.CombineFS("data")))
                System.IO.Directory.CreateDirectory(exportFilePath.CombineFS("data"));
            exportFilePath = exportFilePath.CombineFS("data\\log" + "_" + action.SafeTrim() + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".txt");

            System.IO.File.WriteAllText(exportFilePath, tbStatus.Text + "\r\n[EOF]");

            cout("Log saved to EXE folder.");
        }





        /// <summary>
        /// </summary>
        private string GetExcMsg(Exception ex)
        {
            if (showFullErrMsgs)
                return ex.ToString();
            else
                return ex.Message;
        }





        /// <summary>
        /// </summary>
        private void btnTestConnection_Click(object sender, EventArgs e)
        {
            DisableFormControls();
            InitCoutBuffer();
            tbStatus.Text = "";
            lblNoErrorFound.Visible = lblErrorFound.Visible = ErrorOccurred = false;

            bgw = new BackgroundWorker();
            bgw.DoWork += new DoWorkEventHandler(bgw_TestConnection);
            bgw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgw_TestConnection_End);
            bgw.ProgressChanged += new ProgressChangedEventHandler(BgwReportProgress);
            bgw.WorkerReportsProgress = true;
            bgw.WorkerSupportsCancellation = true;
            bgw.RunWorkerAsync();
        }

        /// <summary>
        /// </summary>
        private void bgw_TestConnection(object sender, DoWorkEventArgs e)
        {
            try
            {
                var targetSite = new Uri(tbSiteUrl.Text.Trim());

                using (ClientContext ctx = new ClientContext(targetSite))
                {
                    ctx.Credentials = BuildCreds();
                    FixCtxForMixedMode(ctx);

                    Web web = ctx.Web;
                    collLists = web.Lists;
                    ctx.Load(collLists);
                    ctx.Load(web, w => w.Title);
                    ctx.ExecuteQuery();
                    ListNames = new List<string>();
                    foreach (Microsoft.SharePoint.Client.List oList in collLists.Where(cl => !cl.Title.Contains("_Archive")))
                    {
                        ListNames.Add(oList.Title);
                    }
                    tbSourceList.Items.AddRange(ListNames.ToArray());
                    tbDestList.Items.AddRange(ListNames.ToArray());
                    tbSourceList.Text = ListNames.Any(ln => ln == tbSourceList.Text) ? tbSourceList.Text : "";
                    tbDestList.Text = ListNames.Any(ln => ln.Contains(tbSourceList.Text)) ? tbSourceList.Text : "";

                    tcout("Site loaded", web.Title);
                    tcout($"{ListNames.Count} Lists, Apps and Folders found.");
                    tcout("Switch to 'Actions' tab to manage your lists");
                }
            }
            catch (Exception ex)
            {
                tcout(" *** ERROR", GetExcMsg(ex));
                ErrorOccurred = true;
            }
        }

        /// <summary>
        /// </summary>
        private void bgw_TestConnection_End(object sender, RunWorkerCompletedEventArgs e)
        {
            FlushCoutBuffer();
            lblErrorFound.Visible = ErrorOccurred; lblNoErrorFound.Visible = !ErrorOccurred;
            EnableFormControls();
        }





        /// <summary>
        /// </summary>
        private void btnStartMain_Click(object sender, EventArgs e)
        {
            DisableFormControls();
            InitCoutBuffer();
            tbStatus.Text = "";
            lblNoErrorFound.Visible = lblErrorFound.Visible = ErrorOccurred = false;

            selAction = ddlActions.SelectedItem == null ? "" : ddlActions.SelectedItem.ToString();
            if (selAction.IsEqual("Archive List"))
            {
                tbDestList.Text = tbSourceList.Text + "_Archive";
            }
            bgw = new BackgroundWorker();
            var args = new UISettings {
                SiteURL = tbSiteUrl.Text.Trim(),
                Source = tbSourceList.Text.Trim(),
                Dest = tbDestList.Text.Trim(),
                Simulate = cbSimulate.Checked,
                MCOverwrite = cbMoveCopyOverwrite.Checked,
                IdsIncl = ConvertToListOfInts(tbItemIDsInclude.Text.Trim()),
                IdsExcl = ConvertToListOfInts(tbItemIDsExclude.Text.Trim()),
                UrlsIncl = tbFilterServerRelPathInc.Text.Trim().TrimEnd("/".ToCharArray()),
                UrlsExcl = tbFilterServerRelPathExc.Text.Trim().TrimEnd("/".ToCharArray())
            };
            bgw.DoWork += new DoWorkEventHandler(bgw_StartMain);
            bgw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgw_StartMain_End);
            bgw.ProgressChanged += new ProgressChangedEventHandler(BgwReportProgress);
            bgw.WorkerReportsProgress = true;
            bgw.WorkerSupportsCancellation = true;
            bgw.RunWorkerAsync(args);
        }

        /// <summary>
        /// </summary>
        private void bgw_StartMain(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bwAsync = sender as BackgroundWorker;

            var rowLimit = Convert.ToInt32(ConfigurationManager.AppSettings["rowLimit"]);
            var numItemsToProc = GenUtil.SafeToInt(tbItemsToProcess.Text);

            // update rowlimit if # of items to process is less, no reason to get more items than needed
            if (numItemsToProc < rowLimit && numItemsToProc > 0) rowLimit = numItemsToProc;

            var args = (UISettings)e.Argument;
            var siteUrl = args.SiteURL;
            var sourceListName = args.Source;
            var destListName = args.Dest;
            var simulate = args.Simulate;
            var overwrite = args.MCOverwrite;

            var fileIdsInclusive = args.IdsIncl;
            var fileIdsExclusive =args.IdsExcl;

            var folderUrlIncl = args.UrlsIncl;
            var folderUrlExcl = args.UrlsExcl;

            if (folderUrlIncl.Contains("%"))
                folderUrlIncl = HttpUtility.UrlDecode(folderUrlIncl);
            if (folderUrlExcl.Contains("%"))
                folderUrlExcl = HttpUtility.UrlDecode(folderUrlExcl);
           
            tcout("Site URL", siteUrl);
            tcout("Username", tbUsername.Text.Trim());
            tcout("Action", selAction);
            tcout("Source List Name", sourceListName);
            if (!selAction.IsEqual("Delete Files"))
                tcout("Destination List Name", destListName);
            if (!selAction.IsEqual("Delete Files"))
                tcout("Overwrite", overwrite);
            if (selAction.IsEqual("Archive List"))
            {
                destListName = sourceListName + "_Archive";
                tcout("Destination List Name", destListName);
            }
            tcout("Number of items to process", numItemsToProc);
            tcout("Query row limit batch size", rowLimit);
            tcout("Simulate", simulate.ToString().ToUpper());
            tcout("Filter Item IDs Inclusive", fileIdsInclusive.Any() ? "Yes" : "No");
            tcout("Filter Item IDs Exclusive", fileIdsExclusive.Any() ? "Yes" : "No");
            tcout("Server Relative Folder Path (Include)", folderUrlIncl);
            tcout("Server Relative Folder Path (Exclude)", folderUrlExcl);

            if (siteUrl.IsNull())
            {
                tcout("Site URL required.");
                return;
            }
            else if (sourceListName.IsNull())
            {
                tcout("Source List Name required.");
                return;
            }
            else if (!selAction.IsEqual("Delete Files") && destListName.IsNull())
            {
                tcout("Destination List Name required.");
                return;
            }
            else if (selAction.IsNull())
            {
                tcout("Action is required.");
                return;
            }

            try
            {
                var targetSite = new Uri(siteUrl);
                //SPSecurity.RunWithElevatedPrivileges((SPSecurity.CodeToRunElevated)(() =>
                //{
                    using (ClientContext ctx = new ClientContext(targetSite))
                    {
                        ctx.Credentials = BuildCreds();
                        FixCtxForMixedMode(ctx);

                        Web web = ctx.Web;
                        ctx.Load(web, w => w.Title);
                        ctx.ExecuteQuery();
                        tcout("Site loaded", web.Title);

                        if (selAction.IsEqual("Move Files") || selAction.IsEqual("Copy Files") || selAction.IsEqual("Archive List"))
                        {
                            CopyMoveArchiveListFiles(bwAsync, rowLimit, numItemsToProc, sourceListName, destListName, simulate, overwrite, fileIdsInclusive, fileIdsExclusive, folderUrlIncl, folderUrlExcl, ctx);
                        }
                        else if (selAction.IsEqual("Delete Files"))
                        {
                            DeleteFiles(bwAsync, rowLimit, numItemsToProc, sourceListName, destListName, simulate, overwrite, fileIdsInclusive, fileIdsExclusive, folderUrlIncl, folderUrlExcl, ctx);
                        }
                        else
                        {
                            tcout("Invalid Action Selected.");
                        }
                    }
                //}));
            }
            catch (Exception ex)
            {
                tcout(" *** ERROR", GetExcMsg(ex));
                ErrorOccurred = true;
            }
        }

        /// <summary>
        /// </summary>
        private void DeleteFiles(BackgroundWorker bwAsync, int rowLimit, int numItemsToProc, string sourceListName, string destListName, bool simulate, bool overwrite, List<int> fileIdsInclusive, List<int> fileIdsExclusive, string folderUrlIncl, string folderUrlExcl, ClientContext ctx)
        {
            int i = 0;

            // list source
            tcout("Loading Source List...");

            var list = ctx.Web.Lists.GetByTitle(sourceListName);

            var listRootFolder = list.RootFolder;
            ctx.Load(list, x => x.RootFolder, x => x.ItemCount);
            ctx.ExecuteQuery();

            var listServerRelUrl = listRootFolder.ServerRelativeUrl;
            ctx.Load(listRootFolder, y => y.ServerRelativeUrl);
            ctx.ExecuteQuery();

            listServerRelUrl = GenUtil.EnsureStartsWithForwardSlash(listServerRelUrl).TrimEnd("/".ToCharArray());

            tcout("Source List found", listServerRelUrl, "Item Count", list.ItemCount);

            if (list.ItemCount == 0)
            {
                tcout("No items found in Source List, quitting.");
                return;
            }

            // begin search
            tcout("Begin finding folders/files...");
            ListItemCollectionPosition pos = null;

            var lstFileObjs = new List<CustFileObj>();

            while (true)
            {
                var prog = (Convert.ToDouble(i) / Convert.ToDouble(list.ItemCount)) * 100;
                tcout(string.Format("Execute paged query, pagesize={0}, progress={1}%", rowLimit, prog.ToString("##0.##")));

                CamlQuery cq = new CamlQuery();

                cq.ListItemCollectionPosition = pos;

                // get items in root of library only, delete folders individually, delete files individually
                cq.ViewXml = string.Format("<View Scope=\"RecursiveAll\"><ViewFields><FieldRef Name=\"ID\" /><FieldRef Name=\"FileLeafRef\" /><FieldRef Name=\"FileDirRef\" /><FieldRef Name=\"FileRef\" /></ViewFields><RowLimit>{0}</RowLimit></View>", rowLimit);

                ListItemCollection lic = list.GetItems(cq);

                ctx.Load(lic,
                    itms => itms.ListItemCollectionPosition,
                    itms => itms.Include(
                        itm => itm["ID"],
                        itm => itm["FileLeafRef"],
                        itm => itm["FileDirRef"],
                        itm => itm["FileRef"],
                        itm => itm.FileSystemObjectType));

                ctx.ExecuteQuery();

                pos = lic.ListItemCollectionPosition;

                foreach (ListItem l in lic)
                {
                    if (bwAsync.CancellationPending)
                    {
                        tcout("Operation Aborted!");
                        return;
                    }

                    i++;

                    var fileId = Convert.ToInt32(l["ID"]);
                    var fileName = l["FileLeafRef"].SafeTrim();
                    var folderPath = GenUtil.EnsureStartsWithForwardSlash(GenUtil.SafeTrimLookupFieldValue(l["FileDirRef"])).TrimEnd("/".ToCharArray());
                    var fullPath = GenUtil.EnsureStartsWithForwardSlash(GenUtil.SafeTrimLookupFieldValue(l["FileRef"]));
                    var fso = l.FileSystemObjectType;

                    if (fileIdsInclusive.Any() && !fileIdsInclusive.Contains(fileId))
                    {
                        //tcout(fileId, fullPath, "Skipping file, Id not found in inclusive list.");
                        continue;
                    }
                    else if (fileIdsExclusive.Any() && fileIdsExclusive.Contains(fileId))
                    {
                        //tcout(fileId, fullPath, "Skipping file, Id found in exclusive list.");
                        continue;
                    }

                    if (fso.ToString().IsEqual("FILE"))
                    {
                        if (!folderUrlIncl.IsNull())
                        {
                            if (!folderPath.StartsWith(folderUrlIncl, StringComparison.CurrentCultureIgnoreCase))
                            {
                                //tcout(fileId, fullPath, string.Format("Skipping {0}, not in folder path", fso.ToString()));
                                continue;
                            }
                        }
                        else if (!folderUrlExcl.IsNull())
                        {
                            if (folderPath.StartsWith(folderUrlExcl, StringComparison.CurrentCultureIgnoreCase))
                            {
                                //tcout(fileId, fullPath, string.Format("Skipping {0}, excluded folder path", fso.ToString()));
                                continue;
                            }
                        }
                    }
                    else if (fso.ToString().IsEqual("FOLDER"))
                    {
                        if (!folderUrlIncl.IsNull())
                        {
                            if (!fullPath.StartsWith(folderUrlIncl, StringComparison.CurrentCultureIgnoreCase))
                            {
                                //tcout(fileId, fullPath, string.Format("Skipping {0}, not in folder path", fso.ToString()));
                                continue;
                            }
                        }
                        else if (!folderUrlExcl.IsNull())
                        {
                            if (fullPath.StartsWith(folderUrlExcl, StringComparison.CurrentCultureIgnoreCase))
                            {
                                //tcout(fileId, fullPath, string.Format("Skipping {0}, excluded folder path", fso.ToString()));
                                continue;
                            }
                        }
                    }

                    lstFileObjs.Add(new CustFileObj
                    {
                        fileId = fileId,
                        fileName = fileName,
                        folderPath = folderPath,
                        fullPath = fullPath,
                        relFolderPath = folderPath.Replace(listServerRelUrl, "").Trim("/".ToCharArray()),
                        relFullPath = fullPath.Replace(listServerRelUrl, "").Trim("/".ToCharArray()),
                        fileType = fso.ToString()
                    });

                    if (numItemsToProc > 0 && i >= numItemsToProc)
                    {
                        tcout("Search aborted, reached number of items found limit.");
                        break;
                    }
                }

                if (pos == null || (numItemsToProc > 0 && i >= numItemsToProc))
                    break;
                else
                    tcout(string.Format("Objects found: {0}/{1}", lstFileObjs.Count, list.ItemCount));
            }

            var folderCount = lstFileObjs.Where(x => x.fileType == "Folder").Count();
            var fileCount = lstFileObjs.Where(x => x.fileType == "File").Count();

            tcout("Finished finding folders/files.");

            tcout("Total item count", lstFileObjs.Count);
            tcout("Folder count", folderCount);
            tcout("File count", fileCount);

            if (lstFileObjs.Count == 0)
            {
                tcout("No files/folders found, quitting.");
            }
            else
            {
                // delete files first, any order (everywhere)
                if (fileCount > 0)
                {
                    tcout(string.Format("Begin deleting files..."));
                    i = 0;

                    foreach (var curFile in lstFileObjs.Where(x => x.fileType == "File"))
                    {
                        if (bwAsync.CancellationPending)
                        {
                            tcout("Operation Aborted!");
                            return;
                        }

                        i++;

                        var file = ctx.Web.GetFileByServerRelativeUrl(curFile.fullPath);

                        try
                        {
                            if (!simulate)
                            {
                                file.DeleteObject();
                                ctx.ExecuteQuery();
                            }
                            else
                            {
                                Thread.Sleep(300);
                            }

                            var prog = (Convert.ToDouble(i) / Convert.ToDouble(fileCount)) * 100;
                            tcout(string.Format("{0}/{1} - {2}%", i, fileCount, prog.ToString("##0.##")), "File deleted", curFile.fullPath);
                        }
                        catch (Exception ex)
                        {
                            tcout(string.Format("{0}/{1}", i, fileCount), "*** ERROR deleting file", curFile.fullPath, ex.Message);
                        }
                    }

                    tcout(string.Format("Finished deleting files."));
                }

                // delete folders, starting at lowest level, moving up (everywhere)
                if (folderCount > 0)
                {
                    tcout(string.Format("Begin deleting folders..."));
                    i = 0;

                    foreach (var curFolder in lstFileObjs.Where(x => x.fileType == "Folder").OrderByDescending(x => x.GetLevel()))
                    {
                        if (bwAsync.CancellationPending)
                        {
                            tcout("Operation Aborted!");
                            return;
                        }

                        i++;

                        var folder = ctx.Web.GetFolderByServerRelativeUrl(curFolder.fullPath);

                        var skipDelete = false;

                        var localFolders = folder.Folders;
                        var localFiles = folder.Files;
                        ctx.Load(localFolders, f1 => f1.Include(f2 => f2.Name));
                        ctx.Load(localFiles, f1 => f1.Include(f2 => f2.Name));
                        ctx.ExecuteQuery();

                        var localFolderCount = localFolders.Count();
                        var localFileCount = localFiles.Count();

                        if (localFolderCount + localFileCount > 0)
                        {
                            skipDelete = true;
                            tcout(string.Format("{0}/{1}", i, folderCount), "Skipping delete, folder not empty", curFolder.fullPath, "foldercount", localFolderCount, "filecount", localFileCount);
                        }

                        if (!skipDelete)
                        {
                            try
                            {
                                if (!simulate)
                                {
                                    folder.DeleteObject();
                                    ctx.ExecuteQuery();
                                }
                                else
                                {
                                    Thread.Sleep(300);
                                }

                                var prog = (Convert.ToDouble(i) / Convert.ToDouble(folderCount)) * 100;
                                tcout(string.Format("{0}/{1} - {2}%", i, folderCount, prog.ToString("##0.##")), "Folder deleted", curFolder.fullPath);
                            }
                            catch (Exception ex)
                            {
                                tcout(string.Format("{0}/{1}", i, folderCount), "*** ERROR deleting folder", curFolder.fullPath, ex.Message);
                            }
                        }
                    }

                    tcout(string.Format("Finished deleting folders."));
                }
            }
        }


        private void CopyMoveArchiveListFiles(BackgroundWorker bwAsync, int rowLimit, int numItemsToProc, string sourceListName, string destListName, bool simulate, bool overwrite, List<int> fileIdsInclusive, List<int> fileIdsExclusive, string folderUrlIncl, string folderUrlExcl, ClientContext ctx)
        {
            int i = 0;
            var isMove = selAction.IsEqual("Move Files");
            var isArchive = selAction.IsEqual("Archive List");
            var isOverwrite = cbMoveCopyOverwrite.Checked;

            // list source
            tcout("Loading Source List...");
            var listSource = ctx.Web.Lists.GetByTitle(sourceListName);

            var listRootFolderSource = listSource.RootFolder;
            ctx.Load(listSource, x => x.RootFolder, x => x.ItemCount, x => x.SchemaXml);
            ctx.ExecuteQuery();

            var listServerRelUrlSource = listRootFolderSource.ServerRelativeUrl;
            ctx.Load(listRootFolderSource, y => y.ServerRelativeUrl);
            ctx.ExecuteQuery();

            listServerRelUrlSource = GenUtil.EnsureStartsWithForwardSlash(listServerRelUrlSource).TrimEnd("/".ToCharArray());

            tcout("Source List found", listServerRelUrlSource, "Item Count", listSource.ItemCount);

            // list destination
            tcout("Loading Destination List...");
            var listDest = ctx.Web.Lists.GetByTitle(destListName);
            var listRootFolderDest = listDest.RootFolder;
            bool itsNotNew = true;
            if (isArchive)
            {
                try
                {
                    ctx.Load(listDest, x => x.RootFolder, x => x.ItemCount);
                    ctx.ExecuteQuery();
                    itsNotNew = false;
                }
                catch
                {
                    tcout("Archive Destination List does not exist .. Creating it now ..");
                    // Attempt to create the list
                    ListCreationInformation listCreationInfo = new ListCreationInformation();
                    listCreationInfo.Title = destListName;
                    listCreationInfo.Description = $"Archive of the {sourceListName} list";
                    listCreationInfo.TemplateType = (int)ListTemplateType.GenericList;
                    List oList = ctx.Web.Lists.Add(listCreationInfo);
                    ctx.ExecuteQuery();


                    XElement fieldsXML = XElement.Parse(ExtractFields(listSource.SchemaXml));
                    // First do non-calculated fields
                    List<XElement> fieldsNonCalc = fieldsXML.Elements("Field").ToList().Where(f => ((f.Attribute("Type") != null) ? (f.Attribute("Type").Value != "Calculated") : true)).ToList();
                    if (!AddFieldsToList(fieldsNonCalc, sourceListName, destListName, ctx, false))
                    {
                        tcout("Error creating Non-Calc fields, probably gonna have to bail .. trying the Calc fields");
                    }

                    // Skip Calculated field.  They are hard!
                    //List<XElement> fieldsCalc = fieldsXML.Elements("Field").ToList().Where(f => ((f.Attribute("Type") != null) ? (f.Attribute("Type").Value == "Calculated") : true)).ToList();
                    //if(!AddFieldsToList(fieldsCalc, sourceListName, destListName, ctx, false))
                    //{
                    //    tcout("Error creating Calc fields, Gonna have to bail!");
                    //    //return;
                    //}
                }
            }

            var destList = ctx.Web.Lists.GetByTitle(destListName);
            var destListFields = destList.Fields;
            ctx.Load(destList);
            ctx.Load(destListFields);
            ctx.ExecuteQuery();
            //foreach (var destfield in destListFields.Where(d => !d.ReadOnlyField))
            //{
            //    tcout($"--->   RO: {destfield.ReadOnlyField}    Display: {destfield.Title}   Internal: {destfield.InternalName}  Fieldtype : {destfield.TypeAsString}   hidden: {destfield.Hidden}");
            //}
            //foreach (var destfield in destListFields.Where(d => d.ReadOnlyField))
            //{
            //    tcout($"--->   RO: {destfield.ReadOnlyField}    Display: {destfield.Title}   Internal: {destfield.InternalName}  Fieldtype : {destfield.TypeAsString}   hidden: {destfield.Hidden}");
            //}

            #region The real Business
            if (itsNotNew)
            {
                ctx.Load(listDest, x => x.RootFolder, x => x.ItemCount);
                ctx.ExecuteQuery();
            }

            var listServerRelUrlDest = listRootFolderDest.ServerRelativeUrl;
            ctx.Load(listRootFolderDest, y => y.ServerRelativeUrl);
            ctx.ExecuteQuery();

            listServerRelUrlDest = GenUtil.EnsureStartsWithForwardSlash(listServerRelUrlDest).TrimEnd("/".ToCharArray());

            tcout("Destination List found", listServerRelUrlDest, "Item Count", listDest.ItemCount);

            if (listSource.ItemCount == 0)
            {
                tcout("No items found in Source List, quitting.");
                return;
            }
            if(isArchive && (listSource.ItemCount == listDest.ItemCount))
            {
                tcout("List appears to have already been archived. Quitting!");
                return;
            }
            // begin search
            tcout("Begin finding folders/files...");
            ListItemCollectionPosition pos = null;

            var lstFileObjs = new List<CustFileObj>();

            while (cbIncudeContents.Checked)
            {
                var prog = (Convert.ToDouble(i) / Convert.ToDouble(listSource.ItemCount)) * 100;
                tcout(string.Format("Execute paged query, pagesize={0}, progress={1}%", rowLimit, prog.ToString("##0.##")));

                CamlQuery cq = new CamlQuery();

                cq.ListItemCollectionPosition = pos;
                cq.ViewXml = string.Format("<View Scope=\"RecursiveAll\"><ViewFields><FieldRef Name=\"ID\" /><FieldRef Name=\"FileLeafRef\" /><FieldRef Name=\"FileDirRef\" /><FieldRef Name=\"FileRef\" /></ViewFields><RowLimit>{0}</RowLimit></View>", rowLimit);

                ListItemCollection lic = listSource.GetItems(cq);

                ctx.Load(lic,
                    itms => itms.ListItemCollectionPosition,
                    itms => itms.Include(
                        itm => itm["ID"],
                        itm => itm["FileLeafRef"],
                        itm => itm["FileDirRef"],
                        itm => itm["FileRef"],
                        itm => itm.FileSystemObjectType));

                ctx.ExecuteQuery();

                pos = lic.ListItemCollectionPosition;

                foreach (ListItem l in lic)
                {
                    if (bwAsync.CancellationPending)
                    {
                        tcout("Operation Aborted!");
                        return;
                    }

                    i++;

                    var fileId = Convert.ToInt32(l["ID"]);
                    var fileName = l["FileLeafRef"].SafeTrim();
                    var folderPath = GenUtil.EnsureStartsWithForwardSlash(GenUtil.SafeTrimLookupFieldValue(l["FileDirRef"])).TrimEnd("/".ToCharArray());
                    var fullPath = GenUtil.EnsureStartsWithForwardSlash(GenUtil.SafeTrimLookupFieldValue(l["FileRef"]));
                    var fso = l.FileSystemObjectType;

                    if (fileIdsInclusive.Any() && !fileIdsInclusive.Contains(fileId))
                    {
                        //tcout(fileId, fullPath, "Skipping file, Id not found in inclusive list.");
                        continue;
                    }
                    else if (fileIdsExclusive.Any() && fileIdsExclusive.Contains(fileId))
                    {
                        //tcout(fileId, fullPath, "Skipping file, Id found in exclusive list.");
                        continue;
                    }

                    if (fso.ToString().IsEqual("FILE"))
                    {
                        if (!folderUrlIncl.IsNull())
                        {
                            if (!folderPath.StartsWith(folderUrlIncl, StringComparison.CurrentCultureIgnoreCase))
                            {
                                //tcout(fileId, fullPath, string.Format("Skipping {0}, not in folder path", fso.ToString()));
                                continue;
                            }
                        }
                        else if (!folderUrlExcl.IsNull())
                        {
                            if (folderPath.StartsWith(folderUrlExcl, StringComparison.CurrentCultureIgnoreCase))
                            {
                                //tcout(fileId, fullPath, string.Format("Skipping {0}, excluded folder path", fso.ToString()));
                                continue;
                            }
                        }
                    }
                    else if (fso.ToString().IsEqual("FOLDER"))
                    {
                        if (!folderUrlIncl.IsNull())
                        {
                            if (!fullPath.StartsWith(folderUrlIncl, StringComparison.CurrentCultureIgnoreCase))
                            {
                                //tcout(fileId, fullPath, string.Format("Skipping {0}, not in folder path", fso.ToString()));
                                continue;
                            }
                        }
                        else if (!folderUrlExcl.IsNull())
                        {
                            if (fullPath.StartsWith(folderUrlExcl, StringComparison.CurrentCultureIgnoreCase))
                            {
                                //tcout(fileId, fullPath, string.Format("Skipping {0}, excluded folder path", fso.ToString()));
                                continue;
                            }
                        }
                    }

                    lstFileObjs.Add(new CustFileObj
                    {
                        fileId = fileId,
                        fileName = fileName,
                        folderPath = folderPath,
                        fullPath = fullPath,
                        relFolderPath = folderPath.Replace(listServerRelUrlSource, "").Trim("/".ToCharArray()),
                        relFullPath = fullPath.Replace(listServerRelUrlSource, "").Trim("/".ToCharArray()),
                        fileType = fso.ToString()
                    });

                    if (numItemsToProc > 0 && i >= numItemsToProc)
                    {
                        tcout("Search aborted, reached number of items found limit.");
                        break;
                    }
                }

                if (pos == null || (numItemsToProc > 0 && i >= numItemsToProc))
                    break;
                else
                    tcout(string.Format("Objects found: {0}/{1}", lstFileObjs.Count, listSource.ItemCount));
            }

            var folderCount = lstFileObjs.Where(x => x.fileType == "Folder").Count();
            var fileCount = lstFileObjs.Where(x => x.fileType == "File").Count();

            tcout("Finished finding folders/files.");

            tcout("Total item count", lstFileObjs.Count);
            tcout("Folder count", folderCount);
            tcout("File count", fileCount);

            if (lstFileObjs.Count == 0)
            {
                tcout("No files/folders found, quitting.");
            }
            else
            {
                // create folders in destination
                if (folderCount > 0)
                {
                    tcout("Begin creating folders in destination...");

                    if (simulate)
                    {
                        tcout("Simulation, skipping.");
                    }
                    else
                    {
                        i = 0;

                        foreach (var curFolder in lstFileObjs.Where(x => x.fileType == "Folder").Distinct().OrderBy(x => x.GetLevel()))
                        {
                            if (bwAsync.CancellationPending)
                            {
                                tcout("Operation Aborted!");
                                return;
                            }

                            i++;

                            var newFolderUrl = curFolder.fullPath.Replace(listServerRelUrlSource, listServerRelUrlDest);
                            var newParentFolderUrl = newFolderUrl.Substring(0, newFolderUrl.LastIndexOf('/'));
                            var newFolderName = newFolderUrl.Substring(newFolderUrl.LastIndexOf('/') + 1);

                            tcout(string.Format("{0}/{1}", i, folderCount), "Checking Folder", newFolderUrl);

                            try
                            {
                                var newFolder = ctx.Web.GetFolderByServerRelativeUrl(newFolderUrl);
                                ctx.Load(newFolder, f => f.Name);
                                ctx.ExecuteQuery();

                                tcout(" -- Folder Exists");

                            }
                            catch (Exception ex)
                            {
                                if (ex.Message.Contains("File Not Found"))
                                {
                                    tcout(" -- Folder Not Found");

                                    try
                                    {
                                        var folder = ctx.Web.GetFolderByServerRelativeUrl(newParentFolderUrl);
                                        var newFolder = folder.Folders.Add(newFolderName);
                                        ctx.ExecuteQuery();
                                        tcout(" -- Folder Created!");

                                    }
                                    catch (Exception ex2)
                                    {
                                        tcout("*** ERROR creating new folder", GetExcMsg(ex2));
                                    }

                                }
                                else
                                {
                                    tcout("*** ERROR checking if folder exists", GetExcMsg(ex));
                                }
                            }
                        }
                    }

                    tcout("Finished creating folders in destination.");
                }

                // move/copy files to destination
                if (fileCount > 0)
                {
                    tcout(string.Format("Begin {0} files to destination...", isMove ? "move" : "copy"));
                    i = 0;

                    foreach (var curFile in lstFileObjs.Where(x => x.fileType == "File"))
                    {
                        if (bwAsync.CancellationPending)
                        {
                            tcout("Operation Aborted!");
                            return;
                        }

                        i++;

                        var oldFileServerRelUrl = curFile.fullPath;
                        var newFileServerRelUrl = curFile.fullPath.Replace(listServerRelUrlSource, listServerRelUrlDest);
                        
                        var prog = (Convert.ToDouble(i) / Convert.ToDouble(fileCount)) * 100;
                        tcout(string.Format("{0}/{1} - {2}%", i, fileCount, prog.ToString("##0.##")), isMove ? "Move" : "Copy" + " File", oldFileServerRelUrl);

                        CopyItem(simulate, isMove, isOverwrite, sourceListName, destListName, curFile.fileId.ToString(), ctx);
                        #region hideme
                        //    if (!overwrite)
                        //    {
                        //        var newFile = ctx.Web.GetFileByServerRelativeUrl(newFileServerRelUrl);
                        //        bool fileExist = false;
                        //        try
                        //        {
                        //            // before action check if file exists at destination
                        //            ctx.Load(newFile, f => f.Exists);
                        //            ctx.ExecuteQuery();
                        //            fileExist = newFile.Exists;
                        //        }
                        //        catch (Exception ex)
                        //        {
                        //            tcout(" *** ERROR checking if file exists in destination", GetExcMsg(ex));
                        //        }

                        //        if (fileExist)
                        //        {
                        //            tcout(" -- File already exists, skipped.");
                        //        }
                        //        else
                        //        {
                        //            try
                        //            {
                        //                var sw = new Stopwatch();
                        //                sw.Start();

                        //                if (!simulate)
                        //                {
                        //                    var oldFile = ctx.Web.GetFileByServerRelativeUrl(oldFileServerRelUrl);

                        //                    if (isMove)
                        //                    {
                        //                        oldFile.MoveTo(newFileServerRelUrl, MoveOperations.None);
                        //                    }
                        //                    else
                        //                    {
                        //                        oldFile.CopyTo(newFileServerRelUrl, false);
                        //                    }

                        //                    ctx.ExecuteQuery();
                        //                }
                        //                else
                        //                {
                        //                    Thread.Sleep(300);
                        //                }

                        //                sw.Stop();

                        //                tcout(" -- File " + (isMove ? "moved" : "copied") + "!" + string.Format(" ({0}s)", sw.Elapsed.TotalSeconds.ToString("##0.##")));
                        //            }
                        //            catch (Exception ex)
                        //            {
                        //                tcout(" *** ERROR " + (isMove ? "moving" : "copying") + " file to destination", GetExcMsg(ex));
                        //            }
                        //        }
                        //    }
                        //    else
                        //    {
                        //        // always copy/move file, overwrite on
                        //        try
                        //        {
                        //            var sw = new Stopwatch();
                        //            sw.Start();

                        //            if (!simulate)
                        //            {
                        //                var oldFile = ctx.Web.GetFileByServerRelativeUrl(oldFileServerRelUrl);

                        //                if (isMove)
                        //                {
                        //                    oldFile.MoveTo(newFileServerRelUrl, MoveOperations.Overwrite);
                        //                }
                        //                else
                        //                {
                        //                    oldFile.CopyTo(newFileServerRelUrl, true);
                        //                }

                        //                ctx.ExecuteQuery();
                        //            }
                        //            else
                        //            {
                        //                Thread.Sleep(300);
                        //            }

                        //            sw.Stop();

                        //            tcout(" -- File " + (isMove ? "moved" : "copied") + "!" + string.Format(" ({0}s)", sw.Elapsed.TotalSeconds.ToString("##0.##")));
                        //        }
                        //        catch (Exception ex)
                        //        {
                        //            tcout(" *** ERROR " + (isMove ? "moving" : "copying") + " file to destination", GetExcMsg(ex));
                        //        }
                        //    }
                        #endregion
                    }

                    tcout(string.Format("Finished {0} files to destination.", isMove ? "move" : "copy"));
                }
            }
            #endregion
        }
        private bool CopyItem(bool simulate, bool isMove, bool isOverwrite, string sourceListName, string destListName, string ID, ClientContext ctx)
        {
            if (!simulate)
            {
                var sourceList = ctx.Web.Lists.GetByTitle(sourceListName);
                var sourceListFields = sourceList.Fields;
                ctx.Load(sourceList);
                ctx.Load(sourceListFields);
                ctx.ExecuteQuery();

                var destList = ctx.Web.Lists.GetByTitle(destListName);
                ctx.Load(destList);

                ctx.ExecuteQuery();

                Dictionary<string, ListDataField> sourceFields = new Dictionary<string, ListDataField>();
                var sourceQueryXML = new StringBuilder();
                sourceQueryXML.Append("<View><Query><Where><Eq><FieldRef Name='ID' /><Value Type='Counter'>" + ID + "</Value></Eq></Where></Query>");//<ViewFields>");
                
                foreach (var sourceListField in sourceListFields)
                {
                    if (sourceListField.ReadOnlyField == false)
                    {
                        sourceFields.Add(sourceListField.InternalName, new ListDataField { DisplayName = sourceListField.Title,
                            InternalName = sourceListField.InternalName,
                            FieldType = sourceListField.TypeAsString,
                            Hidden = sourceListField.Hidden
                        });
                        sourceQueryXML.Append("<FieldRef Name='" + sourceListField.InternalName + "' />");
                    }
                }
                
                 sourceQueryXML.Append("< FieldRef Name='AttachmentFiles' /></ViewFields></View>");
                var sourceQuery = new CamlQuery() { ViewXml = sourceQueryXML.ToString() };

                ListItemCollection sourceItems = sourceList.GetItems(sourceQuery);

                try
                {
                    ctx.Load(sourceItems);
                    ctx.ExecuteQuery();
                }
                catch(Exception ex)
                {
                    tcout($"Failed to copy! Details: {ex.Message}");
                    return false;
                }

                var sourceItem = sourceItems.FirstOrDefault();

                var destListCreationInfo = new ListItemCreationInformation();
                var destItem = destList.AddItem(destListCreationInfo);
                foreach (var sourceField in sourceFields)
                {
                    try
                    {
                        string sharepointified_Key = sourceField.Key;
                        ListDataField sharepointified_Value = sourceField.Value;

                        // internal field
                        if (sharepointified_Key == "ContentType" || sharepointified_Key == "Attachments" || sharepointified_Key == "MetaInfo" || sharepointified_Key == "FileLeafRef" || sharepointified_Key == "Order") continue;
                        // 'Don't touch'-type status
                        if (sharepointified_Value.Hidden) continue;
                                               
                        var dest_key = sharepointified_Key.Length > 32 ? sharepointified_Key.Substring(0, 32) : sharepointified_Key;
                        //tcout($"Writing to {dest_key} <- {sharepointified_Key}[{sourceItem[sharepointified_Key]}]");
                        destItem[dest_key] = sourceItem[sharepointified_Key];
                        destItem.Update();
                    }
                    catch (Exception ex)
                    {
                        tcout($"Failed to copy Field '{sourceField.Key}'! Details: {ex.Message}");
                        continue;
                    }
                    //if (sourceItem.AttachmentFiles.AreItemsAvailable)
                    //{
                    //    try
                    //    {
                    //        var attachments = sourceItem.AttachmentFiles;
                    //        ctx.Load(attachments);
                    //        ctx.ExecuteQuery();

                    //        //Copy attachments
                    //        foreach (Attachment attach in sourceItem.AttachmentFiles) // TODO: Not loaded, don't know why!?!?
                    //        {
                    //            var client = new WebClient();
                    //            client.Credentials = ctx.Credentials;
                    //            client.DownloadFile(attach.ServerRelativeUrl, attach.FileName);

                    //            byte[] imageData = client.DownloadData(attach.ServerRelativeUrl + attach.FileName);
                    //            AttachmentCreationInformation aci = new AttachmentCreationInformation() { FileName = attach.FileName, ContentStream = new MemoryStream(imageData) };
                    //            destItem.AttachmentFiles.Add(aci);
                    //        }
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        tcout($"Failed to copy Attachments '{sourceField.Key}'!");
                    //    }
                    //}
                }

                try
                {
                    ctx.ExecuteQuery();
                }
                catch (Exception ex)
                {
                    tcout($"Oh no'{ex.Message}'!");
                }
                return true;
            }
            else
            {
                Thread.Sleep(300);
                return true;
            }

            return false;
        }

        private string spify(string key)
        {
           return key.Replace(" ", "_x0020_").Replace("(", "_x0028_").Replace(")", "_x0029_");
        }
        private bool AddFieldsToList(List<XElement> fields, string sourceName,string listName, ClientContext ctx, bool isCalculated = false)
        {
            bool result = true;
            var listSource = ctx.Web.Lists.GetByTitle(sourceName);
            ctx.Load(listSource, x => x.Fields);
            ctx.ExecuteQuery();

            var listDest = ctx.Web.Lists.GetByTitle(listName);
            ctx.Load(listDest, x => x.Fields);
            ctx.ExecuteQuery();

            string displayName = "", internalName = "";
            foreach (XElement field in fields)
            {
                displayName = field.Attribute("DisplayName").Value;
                internalName = field.Attribute("Name").Value;
                field.Attribute("DisplayName").Value = internalName;
                //if (isCalculated)
                //{
                //    Field newField = listSource.Fields.GetByInternalNameOrTitle(internalName);
                //    ctx.Load(newField, nf => nf.InternalName, nf => nf.Description, nf => nf.SchemaXml, nf => nf.StaticName, nf => nf.Title, nf => nf.Id);
                //    ctx.ExecuteQuery();

                //    //newField.SchemaXml = field.ToString();
                //    listDest.Fields.Add(newField);

                //    listDest.Update();
                //}
                //else
                //{
                //    Field spField = listDest.Fields.AddFieldAsXml(field.ToString(), true, AddFieldOptions.DefaultValue);
                //    spField.Title = displayName;
                //    spField.StaticName = internalName;
                //    spField.Update();
                //}
                try
                {
                    Field spField = listDest.Fields.AddFieldAsXml(field.ToString(), true, AddFieldOptions.DefaultValue);
                    spField.Title = displayName;
                    spField.StaticName = internalName;
                    spField.Update();
                    ctx.ExecuteQuery();
                }
                catch (Exception ex)
                {
                    tcout($"Trouble adding {displayName}. Details {ex.Message}");
                    result = false;
                    //throw ex;
                }
            }

            return result;
        }
        private string ExtractFields(string schemaXml)
        {
            string GUIDPattern = "\"[{|(]?[0-9a-fA-F]{8}[-]?([0-9a-fA-F]{4}[-]?){3}[0-9a-fA-F]{12}[)|}]?\"";
            schemaXml = Regex.Replace(schemaXml, $"SourceID={GUIDPattern}", "");
            XElement schema = XElement.Parse(schemaXml);
            XElement fieldsNode = schema.Element("Fields");
            List<XElement> fields = fieldsNode.Elements("Field").ToList();
            string fieldDef = "";
            fields.Where(f => f.Attribute("SourceID") == null).ToList().ForEach(f => fieldDef += f.ToString());

            Regex rgxId = new Regex($"ID={GUIDPattern} ");
            foreach (Match match in rgxId.Matches(fieldDef))
            {
                fieldDef = fieldDef.Replace(match.Value, "");
            }

            //fieldDef = Regex.Replace(fieldDef, " Name=\"[a-zA-Z0-9_%%]*\" ", " ");
            fieldDef = Regex.Replace(fieldDef, " StaticName=\"[a-zA-Z0-9_%%]*\" ", " ");
            fieldDef = Regex.Replace(fieldDef, " ColName=\"[a-zA-Z0-9_%%]*\" ", " ");
            fieldDef = Regex.Replace(fieldDef, " RowOrdinal=\"[0-9]*\" ", " ");
            return $"<Fields>{fieldDef}</Fields>";
        }

        /// <summary>
        /// </summary>
        private void bgw_StartMain_End(object sender, RunWorkerCompletedEventArgs e)
        {
            FlushCoutBuffer();
            lblErrorFound.Visible = ErrorOccurred; lblNoErrorFound.Visible = !ErrorOccurred;

            SaveLogToFile(selAction.ToUpper());

            EnableFormControls();
        }

        /// <summary>
        /// </summary>
        private void ddlActions_SelectedIndexChanged(object sender, EventArgs e)
        {
            selAction = ddlActions.SelectedItem == null ? "" : ddlActions.SelectedItem.ToString();
            cbIncudeContents.Enabled = false;
            cbIncudeContents.Checked = true;
            if (selAction.IsEqual("Delete Files"))
            {
                cbMoveCopyOverwrite.Visible = false;
                tbDestList.Enabled = false;
                //tbItemIDsInclude.Enabled = false;
                //tbItemIDsExclude.Enabled = false;
            }
            else if (selAction.IsEqual("Archive List"))
            {
                cbMoveCopyOverwrite.Visible = false;
                tbDestList.Enabled = false;
                tbDestList.Text = tbSourceList.Text + "_Archive";
                cbIncudeContents.Enabled = true;
            }
            else
            {
                cbMoveCopyOverwrite.Visible = true;
                tbDestList.Enabled = true;
                //tbItemIDsInclude.Enabled = true;
                //tbItemIDsExclude.Enabled = true;
            }
        }

        private void tbItemsToProcess_MouseEnter(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "Enter number of items to process, 0 to process all.";
        }
        #region Tooltips
        private void tbItemsToProcess_MouseLeave(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "";
        }

        private void tbItemIDsInclude_MouseEnter(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "Enter listitem IDs, one per line, to include. All other items will be skipped. Entering IDs here overrides the Exclude box below.";
        }

        private void tbItemIDsInclude_MouseLeave(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "";
        }

        private void tbItemIDsExclude_MouseEnter(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "Enter listitem IDs, one per line, to exclude. These items will be skipped, all others will be processed.";
        }

        private void tbItemIDsExclude_MouseLeave(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "";
        }

        private void cbMoveCopyOverwrite_MouseEnter(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "When overwrite is unchecked an additional query is run to check if file exists in destination.";
        }

        private void cbMoveCopyOverwrite_MouseLeave(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "";
        }

        private void tbFilterServerRelPathInc_MouseEnter(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "";
        }

        private void tbFilterServerRelPathInc_MouseLeave(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "";
        }

        private void tbFilterServerRelPathExc_MouseEnter(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "Enter a Server Relative Folder Path here OR above, not in both locations, above field has higher priority.";
        }

        private void tbFilterServerRelPathExc_MouseLeave(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "";
        }

        #endregion

        private void btnAbort_Click(object sender, EventArgs e)
        {
            if (bgw != null && bgw.IsBusy && !bgw.CancellationPending)
            {
                bgw.CancelAsync();
                btnAbort.Enabled = false;
            }
        }

        private void tbSourceList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(ddlActions.SelectedItem as string == "Archive List")
            {
                tbDestList.Text = tbSourceList.Text + "_Archive";
            }
        }

        private void btnDelDest_Click(object sender, EventArgs e)
        {
            if (tbDestList.Text.Contains("_Archive"))
            {
                using (ClientContext ctx = new ClientContext(new Uri(tbSiteUrl.Text.Trim())))
                {
                    try
                    {
                        ctx.Credentials = BuildCreds();
                        FixCtxForMixedMode(ctx);

                        Web web = ctx.Web;
                        ctx.Load(web, w => w.Title);
                        ctx.ExecuteQuery();
                        var destList = ctx.Web.Lists.GetByTitle(tbDestList.Text);

                        destList.DeleteObject();
                        ctx.ExecuteQuery();

                        cout($"{tbDestList.Text} has been deleted!!");
                    }
                    catch(Exception ex)
                    {
                        cout($"{tbDestList.Text} has not been deleted!! Error details: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Destination List is not an Archive!", "Are you sure? Cause ...");
                cout($"{tbDestList.Text} is not an Archive and has not been deleted!! You may want to Delete items via the 'Actions' dropdown.");
            }
        }
    }
}
