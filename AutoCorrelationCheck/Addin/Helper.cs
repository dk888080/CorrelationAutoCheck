using Box.V2;
using Box.V2.Auth;
using Box.V2.Config;
using Box.V2.Exceptions;
using Box.V2.JWTAuth;
using Box.V2.Models;
using Box.V2.Utility;
using Microsoft.Win32;
using MPAD_TestTimer;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Diagnostics.Process;

namespace ClothoSharedItems
{
    public static class Helper
    {
        public static string GetDescription(Enum en)
        {
            Type type = en.GetType();
            MemberInfo[] memInfo = type.GetMember(en.ToString());
            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs != null && attrs.Length > 0)
                {
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }
            return en.ToString();
        }

        public class AutoClosingMessageBox
        {
            private System.Threading.Timer _timeoutTimer;
            private string _caption;
            private DialogResult _result;
            private DialogResult _timerResult;
            private bool timedOut = false;
            private Form _message = new Form() { Size = new Size(0, 0), TopMost = true };

            private AutoClosingMessageBox(string text, string caption, int timeout, MessageBoxButtons buttons = MessageBoxButtons.OK, DialogResult timerResult = DialogResult.None, MessageBoxIcon icon = MessageBoxIcon.None)
            {
                _caption = caption;
                _timeoutTimer = new System.Threading.Timer(OnTimerElapsed, null, timeout, System.Threading.Timeout.Infinite);
                _timerResult = timerResult;

                using (_timeoutTimer) _result = MessageBox.Show(_message, text, caption, buttons, icon);
                if (timedOut) _result = _timerResult;
            }

            public static DialogResult Show(string text, string caption, int timeout = 1000, MessageBoxButtons buttons = MessageBoxButtons.OK, DialogResult timerResult = DialogResult.None, MessageBoxIcon icon = MessageBoxIcon.None)
            {
                return new AutoClosingMessageBox(text, caption, timeout, buttons, timerResult, icon)._result;
            }

            private void FormDispose()
            {
                _message.InvokeIfRequired(() => _message.Dispose());
            }

            private void OnTimerElapsed(object state)
            {
                timedOut = true;
                _timeoutTimer.Dispose();
                FormDispose();
            }
        }

        public class MakeNotifyForm
        {
            private Thread m_thread;

            private MakeNotifyForm(int timeout, string message)
            {
                m_thread = new Thread(() =>
                {
                    NotifyIcon notifyIcon1 = new NotifyIcon();

                    try
                    {
                        notifyIcon1.Visible = true;
                        notifyIcon1.Text = "Notify";
                        notifyIcon1.ShowBalloonTip(timeout, "HI, THERE", message, ToolTipIcon.Info);
                    }
                    catch
                    {
                    }
                    finally
                    {
                        Thread.Sleep(timeout + 500);
                        notifyIcon1.Visible = false;
                    }
                });
                m_thread.Start();
            }

            public static void Show(string msg = "YOUR TEST is FINISHED!", int timeout = 8000)
            {
                new MakeNotifyForm(timeout, msg);
            }
        }

        public class NotifyTelegram
        {
            public static string SendMessage(string apilToken, string destID, string text)
            {
                string urlString = $"https://api.telegram.org/bot{apilToken}/sendMessage?chat_id={destID}&text={text}";
                WebClient webclient = new WebClient();

                return webclient.DownloadString(urlString);
            }
        }

        #region System_Function

        public static int GetLastProcID(string ProcName)
        {
            Process[] ProcAry = Process.GetProcessesByName(ProcName);
            int LastProcID = 0;
            DateTime LastTime = new DateTime(2000, 1, 1);

            foreach (Process Proc in ProcAry)
            {
                if (Proc.StartTime.CompareTo(LastTime) > 0)
                {
                    LastTime = Proc.StartTime;
                    LastProcID = Proc.Id;
                }
            }
            return LastProcID;
        }

        public static void KillProcByID(int ProcID)
        {
            Process Proc = Process.GetProcessById(ProcID);
            Proc.Kill();
        }

        #endregion System_Function

        public static IEnumerable<string> GetFileList(string fileSearchPattern, string rootFolderPath)
        {
            Queue<string> pending = new Queue<string>();
            pending.Enqueue(rootFolderPath);
            string[] tmp;
            while (pending.Count > 0)
            {
                rootFolderPath = pending.Dequeue();
                try
                {
                    tmp = Directory.GetFiles(rootFolderPath, fileSearchPattern);
                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }
                for (int i = 0; i < tmp.Length; i++)
                {
                    yield return tmp[i];
                }
                tmp = Directory.GetDirectories(rootFolderPath);
                for (int i = 0; i < tmp.Length; i++)
                {
                    pending.Enqueue(tmp[i]);
                }
            }
        }

        public static List<string> Client_IP
        {
            get
            {
                List<string> cip = new List<string>();
                IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
                string ClientIP = string.Empty;
                for (int i = 0; i < host.AddressList.Length; i++)
                {
                    if (host.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                    {
                        cip.Add(host.AddressList[i].ToString());
                    }
                }
                return cip;
            }
        }

        public class FTPClient : IDisposable
        {
            private string id = "waveform";
            private string pwd = "npi";
            private string url = "ftp://10.100.34.134:721/";
            public bool VAILDFTP { get; private set; }

            public FTPClient(string url = "ftp://10.100.34.134:721/", string id = "waveform", string pwd = "npi")
            {
                VAILDFTP = true;

                if (!url.CIvStartsWith("ftp")) url = "ftp://" + url;
                if (!url.CIvEndsWith("/")) url = url + "/";

                this.url = url;
                this.id = id;
                this.pwd = pwd;
            }

            public bool isValidConnection()
            {
                try
                {
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);
                    request.Method = WebRequestMethods.Ftp.ListDirectory;
                    request.Credentials = new NetworkCredential(id, pwd);
                    request.Timeout = 1000;
                    request.GetResponse().Close();
                }
                catch (WebException ex)
                {
                    return VAILDFTP = false;
                }
                return VAILDFTP = true;
            }

            public FtpWebResponse Connect(string remoteSource, string method, Action<FtpWebRequest> action = null)
            {
                if (!VAILDFTP) return null;

                var request = WebRequest.Create(url + remoteSource) as FtpWebRequest;
                request.UseBinary = true;
                request.Method = method;
                request.Credentials = new NetworkCredential(id, pwd);
                request.Timeout = 1000;

                FtpWebResponse ftpWebResponse = null;

                try
                {
                    action?.Invoke(request);
                    ftpWebResponse = request.GetResponse() as FtpWebResponse;
                }
                catch (Exception ex)
                {
                }
                return ftpWebResponse;
            }

            public void UploadFileList(String remoteSource, string source)
            {
                if (!VAILDFTP) return;

                var attr = File.GetAttributes(source);
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    DirectoryInfo dir = new DirectoryInfo(source);
                    foreach (var item in dir.GetFiles())
                    {
                        UploadFileList(url + remoteSource + "/" + item.Name, item.FullName);
                    }

                    foreach (var item in dir.GetDirectories())
                    {
                        try
                        {
                            Connect(url + remoteSource + "/" + item.Name, WebRequestMethods.Ftp.MakeDirectory).Close();
                        }
                        catch (WebException)
                        {
                        }

                        UploadFileList(url + remoteSource + "/" + item.Name, item.FullName);
                    }
                }
                else
                {
                    using (var fs = File.OpenRead(source))
                    {
                        Connect(remoteSource, WebRequestMethods.Ftp.UploadFile, (req) =>
                        {
                            req.ContentLength = fs.Length;
                            using (var stream = req.GetRequestStream())
                            {
                                fs.CopyTo(stream);
                            }
                        }).Close();
                    }
                }
            }

            public void DownloadFileList(string remoteSource, string target)
            {
                if (!VAILDFTP) return;

                var list = new List<String>();

                using (var res = Connect(remoteSource, WebRequestMethods.Ftp.ListDirectory))
                {
                    if (res == null) return;

                    using (var stream = res.GetResponseStream())
                    {
                        using (var rd = new StreamReader(stream))
                        {
                            while (true)
                            {
                                string buf = rd.ReadLine();
                                if (string.IsNullOrWhiteSpace(buf))
                                {
                                    break;
                                }
                                list.Add(buf);
                            }
                        }
                    }
                }

                foreach (var item in list)
                {
                    string filename = "";
                    try
                    {
                        var remotetarget = remoteSource + "/" + item;
                        if (item.StartsWith("/"))
                        {
                            remotetarget = remoteSource;
                            filename = Path.GetFileName(item);
                        }
                        else
                            filename = item;

                        using (var res = Connect(remotetarget, WebRequestMethods.Ftp.DownloadFile))
                        {
                            if (res == null) return;

                            using (var stream = res.GetResponseStream())
                            {
                                using (var fs = File.Create(target + "\\" + filename))
                                {
                                    stream.CopyTo(fs);
                                }
                            }
                        }
                    }
                    catch (WebException)
                    {
                        Directory.CreateDirectory(target + "\\" + item);
                        DownloadFileList(remoteSource + "/" + item, target + "\\" + item);
                    }
                }
            }

            public void Dispose()
            {
            }
        }

        public static void CheckSystemEventsHandlersForFreeze()
        {
            var handlers = typeof(SystemEvents).GetField("_handlers", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            var handlersValues = handlers.GetType().GetProperty("Values").GetValue(handlers);
            foreach (var invokeInfos in (handlersValues as IEnumerable).OfType<object>().ToArray())
            {
                foreach (var invokeInfo in (invokeInfos as IEnumerable).OfType<object>().ToArray())
                {
                    var syncContext = invokeInfo.GetType().GetField("_syncContext", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(invokeInfo);
                    if (syncContext == null) throw new Exception("syncContext missing");
                    if (!(syncContext is WindowsFormsSynchronizationContext)) continue;
                    var threadRef = (WeakReference)syncContext.GetType().GetField("destinationThreadRef", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(syncContext);
                    if (!threadRef.IsAlive) continue;
                    var thread = (Thread)threadRef.Target;
                    if (thread.ManagedThreadId == 1) continue;  // Change here if you have more valid UI threads to ignore
                    var dlg = (Delegate)invokeInfo.GetType().GetField("_delegate", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(invokeInfo);
                    MessageBox.Show($"SystemEvents handler '{dlg.Method.DeclaringType}.{dlg.Method.Name}' could freeze app due to wrong thread: "
                                    + $"{thread.ManagedThreadId},{thread.IsThreadPoolThread},{thread.IsAlive},{thread.Name}");
                }
            }
        }

        public static void UnsubscribeuserPreferenceChanged()
        {
            MethodInfo handler = typeof(RichTextBox).GetMethod("UserPreferenceChangedHandler", BindingFlags.Instance | BindingFlags.NonPublic);

            EventInfo evt = typeof(SystemEvents).GetEvent("UserPreferenceChanged", BindingFlags.Static | BindingFlags.Public);
            MethodInfo remove = evt.GetRemoveMethod(true);

            remove.Invoke(null, new object[]
            {
                Delegate.CreateDelegate(evt.EventHandlerType, null, handler)
            });
        }

        public static void UnsubscribeSystemEvents()
        {
            try
            {
                var handlers = typeof(SystemEvents).GetField("_handlers", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
                var handlersValues = handlers.GetType().GetProperty("Values").GetValue(handlers);
                foreach (var invokeInfos in (handlersValues as IEnumerable).OfType<object>().ToArray())
                    foreach (var invokeInfo in (invokeInfos as IEnumerable).OfType<object>().ToArray())
                    {
                        var syncContext = invokeInfo.GetType().GetField("_syncContext", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(invokeInfo);
                        if (syncContext == null)
                            throw new Exception("syncContext missing");
                        if (!(syncContext is WindowsFormsSynchronizationContext))
                            continue;
                        var threadRef = (WeakReference)syncContext.GetType().GetField("destinationThreadRef", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(syncContext);
                        if (!threadRef.IsAlive)
                            continue;
                        var thread = (System.Threading.Thread)threadRef.Target;
                        if (thread.ManagedThreadId == 1)
                            continue;  // Change here if you have more valid UI threads to ignore
                        var dlg = (Delegate)invokeInfo.GetType().GetField("_delegate", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(invokeInfo);
                        var handler = (UserPreferenceChangedEventHandler)Delegate.CreateDelegate(typeof(UserPreferenceChangedEventHandler), dlg.Target, dlg.Method.Name);
                        SystemEvents.UserPreferenceChanged -= handler;
                    }
            }
            catch
            {
            }
        }

        public static void WinformAutoInstalled(bool setting = false)
        {
            var cAutoInstall = WindowsFormsSynchronizationContext.AutoInstall;
            bool msgsent = false;

            if (setting != cAutoInstall)
            {
                msgsent = true;
                MPAD_TestTimer.LoggingManager.Instance.LogInfo(string.Format("WindowsForms.AutoInstall: {0}", cAutoInstall));
                WindowsFormsSynchronizationContext.AutoInstall = setting;
                cAutoInstall = WindowsFormsSynchronizationContext.AutoInstall;
            }

            if (!msgsent)
                MPAD_TestTimer.LoggingManager.Instance.LogInfo(string.Format("WindowsForms.AutoInstall: {0}", cAutoInstall));
        }

        public class MailServer
        {
            private MailAddress sender = null;
            private string receiver = null;
            private string password = "";
            private ClothoNotificationServiceConfig MailServiceConfig = null;

            public MailServer(string jsonString, string receiver)
            {
                MailServiceConfig = JsonConvert.DeserializeObject<ClothoNotificationServiceConfig>(jsonString);
                sender = new MailAddress(MailServiceConfig.Sender);
                this.receiver = receiver;
            }

            public async Task SendAsync(string subject, string body)
            {
                using (MailMessage message = new MailMessage()
                {
                    From = sender,
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                })
                {
                    message.To.Add(receiver);

                    using (var client = new SmtpClient
                    {
                        Host = MailServiceConfig.SmtpServer,
                        Port = MailServiceConfig.SmtpPort,
                        EnableSsl = MailServiceConfig.EnableSsl,
                        Credentials = new NetworkCredential(),
                        Timeout = 20000,
                    })
                    {
                        await client.SendMailAsync(message);
                    }
                }
            }

            public class ClothoNotificationServiceConfig
            {
                public string SmtpServer { get; set; }
                public int SmtpPort { get; set; }
                public bool EnableSsl { get; set; }
                public string Sender { get; set; }
            }
        }

        /// <summary>
        /// A helper class that invokes an action when the Click method is triggered enough times within a certain time window
        /// </summary>
        public class ClickStreakMachine
        {
            private readonly int _requiredClicks;
            private readonly TimeSpan _maxClickSpacing;
            private readonly Action _action;

            private DateTime _lastClickedAt = DateTime.MinValue;
            private int _clickStreak = 0;

            /// <summary>
            /// A helper class that invokes an action when the Click method is triggered enough times within a certain time window
            /// </summary>
            /// <param name="requiredClicks">Clicks required to trigger the action</param>
            /// <param name="maxClickSpacing">Max distance between clicks</param>
            /// <param name="action">Action to trigger when conditions are met</param>
            public ClickStreakMachine(int requiredClicks, TimeSpan maxClickSpacing, Action action)
            {
                _requiredClicks = requiredClicks;
                _maxClickSpacing = maxClickSpacing;
                _action = action;
            }

            public void Click()
            {
                var now = DateTime.Now;

                if (_clickStreak == 0)
                {
                    //first click
                    _clickStreak++;
                    _lastClickedAt = now;
                    return;
                }

                //reset if clicked too late
                if (now - _lastClickedAt > _maxClickSpacing)
                {
                    _clickStreak = 1;
                    _lastClickedAt = now;
                    return;
                }

                //add
                _clickStreak++;
                _lastClickedAt = now;

                //invoke action and reset streak if enough clicks
                if (_clickStreak >= _requiredClicks)
                {
                    _action?.Invoke();
                    _clickStreak = 0;
                }
            }
        }

        public class BoxUserClient
        {
            public async Task<BoxClient> GetUserBoxClient(string appUserId)
            {
                return await this.GetAppUserTokenInfo(appUserId);
            }

            private async Task<BoxClient> GetAppUserTokenInfo(string appUserId)
            {
                IBoxConfig config = this.ConfigureBoxApi();
                BoxJWTAuth session = new BoxJWTAuth(config);
                string adminToken = await session.AdminTokenAsync();
                BoxClient client = session.AdminClient(adminToken, null, null);
                BoxCollection<BoxUser> boxCollection = await client.UsersManager.GetEnterpriseUsersAsync(null, 0U, 100U, null, null, null, false);
                BoxCollection<BoxUser> users = boxCollection;
                boxCollection = null;
                BoxUser found = (from x in users.Entries
                                 where x.Id == appUserId
                                 select x).First<BoxUser>();
                IBoxConfig userConfig = this.ConfigureBoxApi();
                BoxJWTAuth userSession = new BoxJWTAuth(userConfig);
                string userToken = await userSession.UserTokenAsync(found.Id);
                BoxClient userClient = userSession.UserClient(userToken, found.Id);
                BoxConfig bc = new BoxConfig(userConfig.ClientId, userConfig.ClientSecret, new Uri("http://localhost"));
                OAuthSession uauth = new OAuthSession(userClient.Auth.Session.AccessToken, "NA", 3000, "bearer");
                return new BoxClient(bc, uauth, null, null);
            }

            private IBoxConfig ConfigureBoxApi()
            {
                return BoxConfig.CreateFromJsonString(Properties.Resources.BoxConfig);
            }
        }

        public class AlgoImpl_BoxAPI
        {
            private const long CHUNKED_UPLOAD_MINIMUM = 100000000; //Allowable filze size 100MB

            public BoxClient client;

            public const string ID_ROOT_Test_Vector = "123252238813";
            public const string ID_ROOT_Test_Vector_INDEXFILE = "1356510892788";

            private const string ID_ROOT_SEOUL_DEV_ARCHIVE = "230329158358";
            private const string ID_ROOT_SEOUL_DEV_ARCHIVE_INDEXFILE = "1338659517292";
            private const string ID_RF2_TRACE_FOLDER = "230522704221";
            private const string SECRET_KEY_BOX_WSD_USER1 = "8826064036";

            public ConcurrentQueue<BoxItemData> BoxItems = new ConcurrentQueue<BoxItemData>();
            private Dictionary<string, string> dicBackup = new Dictionary<string, string>();
            public string ExeCutionPath { get; set; }

            public async Task<string> DownloadReferenceFileAsString(string Id = "1338659517292")
            {
                if (client == null) return string.Empty;

                var input = await client.FilesManager.DownloadAsync(Id);
                using (var streamReader = new StreamReader(input))
                {
                    return streamReader.ReadToEnd();
                }
            }

            public async Task DownloadFileToTempFolder(string tempLocalFolderPath)
            {
                if (client == null) return;

                Console.WriteLine($"Starting to Download file from Box to Temp Folder");

                BoxCollection<BoxItem> folderItems = await client.FoldersManager.GetFolderItemsAsync(ID_ROOT_SEOUL_DEV_ARCHIVE, 1000, autoPaginate: true); // Top Layer Folder
                var fileDownloadTasks = new List<Task>();
                var files = folderItems.Entries.Where(i => i.Type == "file");

                Parallel.ForEach(folderItems.Entries, item =>
                {
                    if (item.Type == "file")
                    {
                        BoxCollection<BoxFileVersion> previousVersions = client.FilesManager.ViewVersionsAsync(item.Id).GetAwaiter().GetResult();
                        BoxFile fileInformation;
                        string output;

                        using (Stream fileContents = client.FilesManager.DownloadAsync(item.Id).GetAwaiter().GetResult())
                        {
                            fileInformation = client.FilesManager.GetInformationAsync(item.Id).GetAwaiter().GetResult();
                            output = $"{item.Name}";

                            using (FileStream outputFileStream = new FileStream(Path.Combine(tempLocalFolderPath, output), FileMode.Create))
                                fileContents.CopyTo(outputFileStream);
                        }

                        Console.WriteLine($"File Copy To Local Done: {Path.Combine(tempLocalFolderPath, output)}");
                        Console.WriteLine($"File Last Modified Date: {fileInformation.ContentModifiedAt}, ModifiedBy: {fileInformation.ModifiedBy.Name}, FileVersion:{previousVersions.TotalCount + 1}");
                    }
                });
            }

            public async Task DownloadSelectedFiles(List<BoxItemData> Ids, string tempLocalFolderPath)
            {
                if (client == null || (Ids?.Count ?? 0) == 0) return;

                //if (Ids.Count > 1)
                //{
                //    BoxZipRequest request = new BoxZipRequest();
                //    request.Name = "test";
                //    request.Items = new List<BoxZipRequestItem>();

                //    foreach (var t in Ids)
                //    {
                //        var file = new BoxZipRequestItem()
                //        {
                //            Id = t.Id,
                //            Type = BoxZipItemType.file
                //        };
                //        request.Items.Add(file);
                //    }
                //    Stream fs = new FileStream(@"c:\temp\MyTest.zip", FileMode.Create);

                //    BoxZipDownloadStatus status = await client.FilesManager.DownloadZip(request, fs);
                //}
                //else
                {
                    //await Ids.ParallelForEachAsync(async file =>
                    //{
                    //    Console.WriteLine($"File Copy To Local start: {Path.Combine(tempLocalFolderPath, file.File_Name)}");

                    //    var fileContents = await client.FilesManager.DownloadAsync(file.Id);

                    //    using (FileStream outputFileStream = new FileStream(Path.Combine(tempLocalFolderPath, file.File_Name), FileMode.Create))
                    //    {
                    //        fileContents.CopyTo(outputFileStream);
                    //    }
                    //}, maxDegreeOfParallelism: 0);
                }
                Console.WriteLine($"File Copy To Local Done");
            }

            public class BoxItemData
            {
                public string Id { get; set; }
                public string File_Name { get; set; }
            }

            public async Task MakeListJson()
            {
                await GetFileTags(ID_ROOT_SEOUL_DEV_ARCHIVE);

                //File.WriteAllText(@"c:\TEST.JSON", updatedJson);
                using (MemoryStream stringInMemoryStream = new MemoryStream(Encoding.Default.GetBytes(JsonConvert.SerializeObject(BoxItems, Formatting.Indented))))
                {
                    var bri = await UploadFileVersion(ID_ROOT_SEOUL_DEV_ARCHIVE, "ALL_FILE_LIST.json", stringInMemoryStream);
                }

                while (!BoxItems.IsEmpty) { BoxItems.TryDequeue(out _); }
            }

            public async Task GetFileTags(string foid = ID_RF2_TRACE_FOLDER, int currentDepth = 0, int maxDepth = 10)
            {
                if (client == null) return;
                Console.WriteLine(string.Format("foid:{0} currentDepth:{1}/{2}", foid, currentDepth, maxDepth));

                if (currentDepth >= maxDepth)
                {
                    Console.WriteLine("Reach out max depth");
                    return;
                }

                BoxCollection<BoxItem> folderItems = await client.FoldersManager.GetFolderItemsAsync(foid, 1000, autoPaginate: false); // Top Layer Folder

                var files = folderItems.Entries.Where(i => i.Type == "file");
                var folders = folderItems.Entries.Where(i => i.Type == "folder");

                Parallel.ForEach(files, fl =>
                {
                    BoxItemData bi = new BoxItemData() { Id = fl.Id, File_Name = fl.Name };
                    BoxItems.Enqueue(bi);
                });

                foreach (var fo in folders)
                    await GetFileTags(fo.Id, currentDepth + 1, maxDepth);
            }

            public class UploadContentInformation
            {
                public bool? IsUploadSuccess = false;
                public string Id = null;
                public string folderId = null;
                public string UploadedName = null;
                public long? Size = null;
            }

            public async Task<UploadContentInformation> UploadFileVersion(string foid, string filename, Stream fs)
            {
                UploadContentInformation ri = new UploadContentInformation();

                if (client == null) return ri;

                BoxFile bf;

                try
                {
                    try
                    {
                        var preflightRequest = new BoxPreflightCheckRequest
                        {
                            Name = filename,
                            Parent = new BoxRequestEntity
                            {
                                Id = foid
                            }
                        };

                        var preflightCheck = await client.FilesManager.PreflightCheck(preflightRequest);

                        using (SHA1 sha1 = SHA1.Create())
                        {
                            var fileUploadRequest = new BoxFileRequest
                            {
                                Name = filename,
                                Parent = new BoxRequestEntity
                                {
                                    Id = foid
                                }
                            };

                            var fileSHA = sha1.ComputeHash(fs);

                            if (fs.Length > CHUNKED_UPLOAD_MINIMUM)
                            {
                                var progress = new Progress<BoxProgress>(val =>
                                {
                                    //DumpLog(string.Format("Uploaded {0}%", val.progress));
                                });

                                bf = await client.FilesManager.UploadUsingSessionAsync(stream: fs, fileName: fileUploadRequest.Name, folderId: fileUploadRequest.Parent.Id, progress: progress);
                            }
                            else
                            {
                                bf = await client.FilesManager.UploadAsync(fileRequest: fileUploadRequest, stream: fs, contentMD5: fileSHA);
                            }

                            ri.IsUploadSuccess = true;
                            ri.folderId = fileUploadRequest.Parent.Id;
                            ri.Id = bf.Id;
                            ri.Size = bf.Size;
                            ri.UploadedName = bf.Name;

                            return ri;
                        }
                    }
                    catch (BoxPreflightCheckConflictException<BoxFile> e)
                    {
                        using (SHA1 sha1 = SHA1.Create())
                        {
                            var fileUploadRequest = new BoxFileRequest
                            {
                                Name = e.ConflictingItem.Name,
                                Id = e.ConflictingItem.Id,
                                Parent = new BoxRequestEntity
                                {
                                    Id = foid
                                }
                            };

                            var fileSHA = sha1.ComputeHash(fs);

                            if (fs.Length > CHUNKED_UPLOAD_MINIMUM)
                            {
                                bf = await client.FilesManager.UploadNewVersionUsingSessionAsync(fileId: e.ConflictingItem.Id, stream: fs);
                            }
                            else
                            {
                                bf = await client.FilesManager.UploadNewVersionAsync(fileName: fileUploadRequest.Name, fileId: e.ConflictingItem.Id, stream: fs, contentMD5: fileSHA);
                            }

                            ri.IsUploadSuccess = true;
                            ri.folderId = fileUploadRequest.Parent.Id;
                            ri.Id = bf.Id;
                            ri.Size = bf.Size;
                            ri.UploadedName = bf.Name;

                            return ri;
                        }
                    }
                }
                catch (Box.V2.Exceptions.BoxException ex)
                {
                    //DumpLog($"Other Exception\n[{ex.StatusCode}] {ex.Message}");

                    ri.IsUploadSuccess = false;
                    return ri;
                }
            }

            public async Task<UploadContentInformation> UploadFile(string targetfolderName, FileInfo fileInfo, FileStream toUpload)
            {
                UploadContentInformation ri = new UploadContentInformation();
                if (client == null) return ri;

                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        var folderId = "";

                        if (dicBackup.ContainsKey(targetfolderName)) folderId = dicBackup[targetfolderName];
                        else
                        {
                            try
                            {
                                var createdFolder = await client.FoldersManager.CreateAsync(
                                    new BoxFolderRequest
                                    {
                                        Parent = new BoxRequestEntity
                                        {
                                            Id = ID_RF2_TRACE_FOLDER
                                        },
                                        Name = targetfolderName
                                    });

                                folderId = createdFolder.Id;
                            }
                            catch (BoxConflictException<BoxFolder> e)
                            {
                                folderId = e.ConflictingItems.FirstOrDefault().Id;
                                Console.WriteLine($"Found existing folder: {folderId}");
                            }

                            dicBackup[targetfolderName] = folderId;
                        }

                        Console.WriteLine(string.Format("- '{0}' to {1}({2})", fileInfo.Name, targetfolderName, folderId));

                        ri.folderId = folderId;

                        var preflightRequest = new BoxPreflightCheckRequest
                        {
                            Name = fileInfo.Name,
                            Size = fileInfo.Length,
                            Parent = new BoxRequestEntity
                            {
                                Id = folderId
                            }
                        };

                        BoxFile bf;
                        try
                        {
                            var preflightCheck = await client.FilesManager.PreflightCheck(preflightRequest);

                            using (SHA1 sha1 = SHA1.Create())
                            {
                                var fileUploadRequest = new BoxFileRequest
                                {
                                    Name = fileInfo.Name,
                                    Parent = new BoxRequestEntity
                                    {
                                        Id = folderId
                                    }
                                };

                                var fileSHA = sha1.ComputeHash(toUpload);

                                if (toUpload.Length > CHUNKED_UPLOAD_MINIMUM)
                                {
                                    var progress = new Progress<BoxProgress>(val =>
                                    {
                                        //DumpLog(string.Format("Uploaded {0}%", val.progress));
                                    });

                                    bf = await client.FilesManager.UploadUsingSessionAsync(stream: toUpload, fileName: fileUploadRequest.Name, folderId: fileUploadRequest.Parent.Id, progress: progress);
                                }
                                else
                                {
                                    bf = await client.FilesManager.UploadAsync(fileRequest: fileUploadRequest, stream: toUpload, contentMD5: fileSHA);
                                }

                                ri.IsUploadSuccess = true;
                                ri.Id = bf.Id;
                                ri.Size = bf.Size;
                                ri.UploadedName = bf.Name;

                                break;
                            }
                        }
                        catch (BoxPreflightCheckConflictException<BoxFile> e)
                        {
                            ri.IsUploadSuccess = false;
                            //DumpLog($"-Upload exception (Existed) {e.ConflictingItem.Name} auto change the name...");

                            using (SHA1 sha1 = SHA1.Create())
                            {
                                var fileUploadRequest = new BoxFileRequest
                                {
                                    Name = string.Format("CONFLICTED_{0:yyyyMMddHHmmss}_{1}", DateTime.Now, fileInfo.Name),
                                    Parent = new BoxRequestEntity
                                    {
                                        Id = folderId
                                    }
                                };

                                var fileSHA = sha1.ComputeHash(toUpload);

                                if (toUpload.Length > CHUNKED_UPLOAD_MINIMUM)
                                {
                                    var progress = new Progress<BoxProgress>(val =>
                                    {
                                    });

                                    bf = await client.FilesManager.UploadUsingSessionAsync(stream: toUpload, fileName: fileUploadRequest.Name, folderId: fileUploadRequest.Parent.Id, progress: progress);
                                }
                                else
                                {
                                    bf = await client.FilesManager.UploadAsync(fileRequest: fileUploadRequest, stream: toUpload, contentMD5: fileSHA);
                                }

                                ri.IsUploadSuccess = true;
                                ri.Id = bf.Id;
                                ri.Size = bf.Size;
                                ri.UploadedName = bf.Name;

                                break;
                            }
                        }
                    }
                    catch (Box.V2.Exceptions.BoxException ex)
                    {
                        ri.IsUploadSuccess = false;
                        //DumpLog($"Other Exception, try {i + 1}/3 times\n[{ex.StatusCode}] {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        ri.IsUploadSuccess = false;
                        //DumpLog($"Other Exception, try {i + 1}/3 times\n" + ex.Message);
                    }
                }
                return ri;
            }

            public async Task UploadBulkFiles(string directoryName, string targetfolderName)
            {
                var files = Directory.EnumerateFiles(directoryName);

                if (files.Count() > 0)
                {
                    var folderId = "";
                    try
                    {
                        var createdFolder = await client.FoldersManager.CreateAsync(
                          new BoxFolderRequest
                          {
                              Parent = new BoxRequestEntity
                              {
                                  Id = ID_RF2_TRACE_FOLDER
                              },
                              Name = targetfolderName
                          });
                        folderId = createdFolder.Id;
                    }
                    catch (BoxConflictException<BoxFolder> e)
                    {
                        folderId = e.ConflictingItems.FirstOrDefault().Id;
                        //DumpLog($"Found existing folder: {folderId}");
                    }

                    var fileUploadTasks = new List<Task<BoxFile>>();
                    foreach (var file in files)
                    {
                        var fileInfo = new FileInfo(file);

                        fileUploadTasks.Add(Task.Run(
                          async () =>
                          {
                              //DumpLog(fileInfo.Name);

                              var preflightRequest = new BoxPreflightCheckRequest
                              {
                                  Name = fileInfo.Name,
                                  Size = fileInfo.Length,
                                  Parent = new BoxRequestEntity
                                  {
                                      Id = folderId
                                  }
                              };

                              using (FileStream toUpload = new FileStream(file, FileMode.Open))
                              {
                                  try
                                  {
                                      var preflightCheck = await client.FilesManager.PreflightCheck(preflightRequest);
                                      if (toUpload.Length < CHUNKED_UPLOAD_MINIMUM)
                                      {
                                          using (SHA1 sha1 = SHA1.Create())
                                          {
                                              var fileUploadRequest = new BoxFileRequest
                                              {
                                                  Name = fileInfo.Name,
                                                  Parent = new BoxRequestEntity
                                                  {
                                                      Id = folderId
                                                  }
                                              };
                                              var fileSHA = sha1.ComputeHash(toUpload);
                                              //DumpLog(string.Join(" ", fileSHA));
                                              return await client.FilesManager.UploadAsync(fileRequest: fileUploadRequest, stream: toUpload, contentMD5: fileSHA);
                                          }
                                      }
                                      else
                                      {
                                          return await client.FilesManager.UploadUsingSessionAsync(stream: toUpload, fileName: fileInfo.Name, folderId: folderId);
                                      }
                                  }
                                  catch (BoxPreflightCheckConflictException<BoxFile> e)
                                  {
                                      if (toUpload.Length < CHUNKED_UPLOAD_MINIMUM)
                                      {
                                          using (SHA1 sha1 = SHA1.Create())
                                          {
                                              var fileSHA = sha1.ComputeHash(toUpload);
                                              return await client.FilesManager.UploadNewVersionAsync(fileName: e.ConflictingItem.Name, fileId: e.ConflictingItem.Id, stream: toUpload, contentMD5: fileSHA);
                                          }
                                      }
                                      else
                                      {
                                          await client.FilesManager.UploadNewVersionUsingSessionAsync(fileId: e.ConflictingItem.Id, stream: toUpload);
                                          return await client.FilesManager.GetInformationAsync(e.ConflictingItem.Id);
                                      }
                                  }
                              }
                          }));
                    }

                    var uploaded = await Task.WhenAll(fileUploadTasks);
                    foreach (var file in uploaded)
                    {
                        Console.WriteLine(string.Format("Uploaded, {0} Id:{1}", file.Name, file.Id));
                    }
                }
            }

            private async Task<BoxClient> GetBoxClient()
            {
                BoxUserClient buc = new BoxUserClient();
                client = await buc.GetUserBoxClient(SECRET_KEY_BOX_WSD_USER1);
                client.Auth.SessionAuthenticated += Auth_SessionAuthenticated;
                client.Auth.SessionInvalidated += Auth_SessionInvalidated;
                return client;
            }

            public async Task RenewOrCreateClientSession()
            {
                Console.WriteLine(string.Format("{0} Client Session", client == null ? "Create New" : "Renew"));

                await GetBoxClient();

                if (client != null) Console.WriteLine($"- Token: {client.Auth.Session.AccessToken}");
            }

            private async void Auth_SessionInvalidated(object sender, EventArgs e)
            {
                await RenewOrCreateClientSession();
            }

            private void Auth_SessionAuthenticated(object sender, Box.V2.Auth.SessionAuthenticatedEventArgs e)
            {
                Console.WriteLine(string.Format("AccessToken:{0}\nAuthVersion:{1}\nExpiresIn:{2}\nRefreshToken:{3}\nTokenType:{4}", e.Session.AccessToken, string.Empty, e.Session.ExpiresIn, e.Session.RefreshToken, e.Session.TokenType));
            }
        }

        public static string GetGUIDBase64()
        {
            string shortGuid = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            return shortGuid = shortGuid.Replace("/", "_").Replace("+", "-");
        }

        public static string GetGUIDBase36()
        {
            var base36Chars = "0123456789abcdefghijklmnopqrstuvwxyz".ToCharArray();
            var bytes = Guid.NewGuid().ToByteArray();
            var value = BitConverter.ToUInt64(bytes, 0);
            var result = new StringBuilder();

            while (value > 0)
            {
                result.Insert(0, base36Chars[value % 36]);
                value /= 36;
            }

            return result.ToString();
        }

        public static void Checkprogram(string programName, string programDelimiter, string programPath, string[] delimiterArrray)
        {
            // check path
            if (!File.Exists(programPath.Replace("\"", ""))) throw new Exception("Please check program path!");

            ProcessStartInfo pri = new ProcessStartInfo();
            Process pro = new Process();

            // string findcmd = string.Format("tasklist /v |findstr 835x.exe|findstr VNA");
            string findcmd;
            if (programDelimiter == "")
                findcmd = string.Format("tasklist /v |findstr {0}", programName);
            else
                findcmd = string.Format("tasklist /v |findstr {0}|findstr {1}", programName, programDelimiter);

            pri.FileName = @"cmd.exe";
            pri.CreateNoWindow = false;
            pri.UseShellExecute = false;

            pri.RedirectStandardInput = true;
            pri.RedirectStandardOutput = true;
            pri.RedirectStandardError = true;

            pro.StartInfo = pri;
            pro.Start();
            int _pid = pro.Id;

            while (true)
            {
                string str = WriteAndReadCMDstring(findcmd, ref pro);

                if (str == "") // if vna program is closed, excute the program and check the status.
                {
                    str = WriteAndReadCMDstring(string.Format("{0}", programPath), ref pro);

                    pro.StandardInput.WriteLine(findcmd);
                    str = WriteAndReadCMDstring(findcmd, ref pro);
                }
                else if (str.CIvContains("running")) break;

                Stopwatch sw = new Stopwatch();
                sw.Start();

                while (!str.CIvContainsAllOf(delimiterArrray))
                {
                    if (sw.Elapsed.TotalMilliseconds < 600000) // 10 min
                    {
                        str = WriteAndReadCMDstring(findcmd, ref pro);

                        Thread.Sleep(500);
                    }
                    else
                        throw new Exception(string.Format("[Error : overtime 10 min] fail to check status of {0}", programName));
                }

                break;
            }

            pro.StandardInput.Close();
            pro.Close();

            KillProcess(_pid);
            Thread.Sleep(1000);
        }

        private static string WriteAndReadCMDstring(string cmd, ref Process pro)
        {
            string str = "";
            pro.StandardInput.WriteLine(cmd);
            while (true)
            {
                str = pro.StandardOutput.ReadLine();
                if (ClothoDataObject.Instance.EnableSeoulSpecific)
                {
                    ClothoDataObject.Instance.SeoulHelper.LogMessage("[CMD] " + str);
                    LoggingManager.Instance.LogInfo("[CMD] " + str);
                }

                if (str.CIvEndsWith(cmd))
                {
                    str = pro.StandardOutput.ReadLine();
                    if (ClothoDataObject.Instance.EnableSeoulSpecific)
                    {
                        ClothoDataObject.Instance.SeoulHelper.LogMessage("[CMD] " + str);
                        LoggingManager.Instance.LogInfo("[CMD] " + str);
                    }
                    break;
                }
            }

            return str;
        }

        private static void KillProcess(int pid)
        {
            try
            {
                if (Process.GetProcesses().Any(x => x.Id == pid))
                {
                    Process proc = Process.GetProcessById(pid);
                    if (!proc.HasExited) proc.Kill();
                }
            }
            catch (ArgumentException ex)
            {
            }
        }
    }

    /// <summary>
    /// Common Encrypt
    /// </summary>
    public static class Encrypt
    {
#if false
        // This size of the IV (in bytes) must = (keysize / 8).  Default keysize is 256, so the IV must be
        // 32 bytes long.  Using a 16 character string here gives us 32 bytes when converted to a byte array.
        private const string initVector = "smpadmagicbox!@#";
#endif

        // This constant is used to determine the keysize of the encryption algorithm
        private const int keysize = 256;

        //Encrypt
        public static string EncryptString(this string initVector, string plainText, string passPhrase)
        {
            byte[] initVectorBytes = Encoding.UTF8.GetBytes(initVector);
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null);
            byte[] keyBytes = password.GetBytes(keysize / 8);
            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;
            ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();
            byte[] cipherTextBytes = memoryStream.ToArray();
            memoryStream.Close();
            cryptoStream.Close();
            return Convert.ToBase64String(cipherTextBytes);
        }

        //Decrypt
        public static string DecryptString(this string initVector, string cipherText, string passPhrase)
        {
            byte[] initVectorBytes = Encoding.UTF8.GetBytes(initVector);
            byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
            PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null);
            byte[] keyBytes = password.GetBytes(keysize / 8);
            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;
            ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);
            MemoryStream memoryStream = new MemoryStream(cipherTextBytes);
            CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            byte[] plainTextBytes = new byte[cipherTextBytes.Length];
            int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            memoryStream.Close();
            cryptoStream.Close();
            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
        }
    }

    public class PerformanceRecorder
    {
        private static Stopwatch timer = new Stopwatch();
        private static long bytesPhysicalBefore = 0;
        private static long bytesVirtualBefore = 0;

        public static void Start()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            bytesPhysicalBefore = GetCurrentProcess().WorkingSet64;
            bytesVirtualBefore = GetCurrentProcess().VirtualMemorySize64;

            timer.Restart();
        }

        public static void Stop()
        {
            timer.Stop();

            long bytesPhysicalAfter = GetCurrentProcess().WorkingSet64;
            long bytesVirtualAfter = GetCurrentProcess().VirtualMemorySize64;

            Debug.WriteLine("Stopped recording.");

            Debug.WriteLine($"{bytesPhysicalAfter - bytesPhysicalBefore:N0} physical bytes used.");
            Debug.WriteLine($"{bytesVirtualAfter - bytesVirtualBefore:N0} virtual bytes used.");

            Debug.WriteLine($"{timer.Elapsed} time span ellapsed");
            Debug.WriteLine($"{timer.Elapsed.TotalMilliseconds:N0} total milliseconds ellapsed.");
        }
    }

    public class CustomManualResetEvent
    {
        public ManualResetEvent ResetEvent;
        public string type;

        public CustomManualResetEvent()
        {
            ResetEvent = new ManualResetEvent(true);
        }

        public void WaitforTaskFinished(bool bTaskEnabled = true, int iTimeout = 1000)
        {
            if (bTaskEnabled)
                ResetEvent.WaitOne(iTimeout);
        }

        public void Reset()
        {
            ResetEvent.Reset();
        }

        public void Set()
        {
            ResetEvent.Set();
        }

        public static void WaitAll(IEnumerable<CustomManualResetEvent> events, int timeout = 1000)
        {
            var waitHandles = events.Select(e => e.ResetEvent).ToArray();
            WaitHandle.WaitAll(waitHandles, timeout);
        }

        public static void WaitAll(int timeout, params CustomManualResetEvent[] events)
        {
            var waitHandles = events.Select(e => e.ResetEvent).ToArray();
            WaitHandle.WaitAll(waitHandles, timeout);
        }
    }
}