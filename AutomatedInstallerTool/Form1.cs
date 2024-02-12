using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Media.Ocr;
using Windows.Graphics.Imaging;
using System.Drawing.Imaging;
using System.Management.Automation;


namespace WinFormsTest
{
    public partial class Form1 : Form
    {
        private delegate void PassFiles(Dictionary<string, string> files);

        public Dictionary<string, string> exeMap;
        PassFiles passDelegate;
        public Form1()
        {
            InitializeComponent();
            exeMap = new Dictionary<string, string>();
            loadList();
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public class ImgData
        {
            public int xPos;
            public int yPos;
            public Bitmap bmap;

            public ImgData(int x, int y, Bitmap map)
            {
                xPos = x;
                yPos = y;
                bmap = map;
            }
        }

        public static ImgData CaptureWindow()
        {
            var rect = new RECT();
            GetWindowRect(new HandleRef(null, GetForegroundWindow()), ref rect);
            var bounds = new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
            if (bounds.Width == 0 || bounds.Height == 0) return null;
            var result = new Bitmap(bounds.Width, bounds.Height);
            Debug.WriteLine(bounds.Left + " " + bounds.Top);

            using (var graphics = Graphics.FromImage(result))
            {
                graphics.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
            }

            return new ImgData(bounds.Left, bounds.Top, result);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(HandleRef hwnd, ref RECT lpRect);

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;

        private void Form1_Load(object sender, EventArgs e)
        {
            loadList();
        }

        private void refreshList_Click(object sender, EventArgs e)
        {
            loadList();
        }

        private void loadList()
        {
            checkedListBox1.Items.Clear();
            Directory.CreateDirectory(".\\cfg");
            foreach (string path in Directory.GetFiles(Directory.GetCurrentDirectory()))
            {
                if(path.Contains(".lnk"))
                {
                    Debug.WriteLine(path);
                    string newKey = Path.GetFileNameWithoutExtension(path);
                    exeMap[newKey] = path;
                    checkedListBox1.Items.Add(newKey);
                    File.Create(Directory.GetCurrentDirectory() + "\\cfg\\" + Path.GetFileNameWithoutExtension(path) + ".cfg").Close();
                }
            }
        }

        async Task ParallelSearch(string[] paths)
        {
            Debug.WriteLine("Invoke thread");
            foreach (string path in paths)
                await Task.Run(() => recurseSearch(path));
        }

        private void recurseSearch(string path)
        {
            Dictionary<string, string> toAdd = new Dictionary<string, string>();
            string[] curr = Directory.GetFiles(path, "setup");
            if (curr.Length == 0)
            {
                curr = Directory.GetFiles(path, "*.exe");
                if (curr.Length == 0)
                {
                    curr = Directory.GetDirectories(path);
                    ParallelSearch(curr);
                    return;
                }
                foreach (string file in curr)
                    toAdd.Add(file, path);
                IAsyncResult exe = BeginInvoke(passDelegate, new object[] { toAdd });
                return;
            }
            foreach (string file in curr)
                toAdd.Add(file, path);
            IAsyncResult setup = BeginInvoke(passDelegate, new object[] { toAdd });
            return;
        }

        async private void installButton_Click(object sender, EventArgs e)
        {
            if (checkedListBox1.CheckedItems.Count != 0)
            {
                OcrEngine ocr = null;
                ocr = OcrEngine.TryCreateFromUserProfileLanguages();

                for (int x = 0; x < checkedListBox1.CheckedItems.Count; x++)
                {
                    string currItem = exeMap[checkedListBox1.CheckedItems[x].ToString()];
                    ProcessStartInfo info = new ProcessStartInfo(currItem);
                    info.UseShellExecute = true;
                    //info.Arguments = $"/k \"{exeMap[checkedListBox1.CheckedItems[x].ToString()]}\"";
                    Process curr = Process.Start(info);

                    Mutex mutex;
                    Mutex.TryOpenExisting(@"Global\_MSIExecute", out mutex);

                    int currPID = curr.Id;
                    String prevScan = "";
                    string dir = Path.GetDirectoryName(currItem) + "\\cfg\\" + checkedListBox1.CheckedItems[x].ToString() + ".cfg";
                    var sr = new StreamReader(File.Open(dir, FileMode.OpenOrCreate));
                    Debug.WriteLine(currPID);

                    Thread.Sleep(5000);

                    while (ProcessExists(currPID))
                    {
                        SoftwareBitmap currWindow;
                        ImgData imgData = CaptureWindow();
                        Bitmap bitmap = imgData.bmap;
                        if (bitmap == null) continue;

                        using (var stream = new Windows.Storage.Streams.InMemoryRandomAccessStream())
                        {
                            var fileStream = File.Create(".\\img.bmp");
                            bitmap.Save(fileStream, ImageFormat.Png);
                            fileStream.Close();
                            bitmap.Save(stream.AsStream(), ImageFormat.Bmp); //choose the specific image format by your own bitmap source
                            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                            currWindow = await decoder.GetSoftwareBitmapAsync();
                        }

                        var ocrResult = await ocr.RecognizeAsync(currWindow);
                        curr.Refresh();
                        string currCommand = sr.ReadLine();

                        if ((ocrResult.Text != prevScan && (!ocrResult.Text.ToLower().Contains("installing") || !ocrResult.Text.ToLower().Contains("being installed"))) || currCommand != null || currCommand != "")
                        {
                            Debug.WriteLine(ocrResult.Text);
                            Debug.WriteLine(currCommand);

                            if (currCommand != null && currCommand != "")
                            {
                                if(!ocrResult.Text.ToLower().Contains(currCommand))
                                {
                                    curr.Kill();
                                    // TODO: Figure out a good method for bringing this pop-up to focus
                                    var w = new Form() { Size = new Size(0, 0) };
                                    await Task.Delay(TimeSpan.FromSeconds(5))
                                    .ContinueWith((t) => w.Close(), TaskScheduler.FromCurrentSynchronizationContext());

                                    MessageBox.Show(w, "Invalid Command. Please double check the associated steps.cfg file.", "Cannot find next step.", MessageBoxButtons.OK);
                                    w.BringToFront();
                                    w.Activate();
                                    break;
                                }
                                Debug.WriteLine("First if");
                                foreach (OcrLine line in ocrResult.Lines.ToArray())
                                {
                                    string currLine = line.Text.ToLower();
                                    if (currLine.Contains(currCommand.ToLower()))
                                    {
                                        Debug.WriteLine("Found line");
                                        foreach (OcrWord word in line.Words)
                                        {
                                            if (word.Text.ToLower().Equals(currCommand.ToLower()))
                                            {
                                                Debug.WriteLine("Found word");
                                                GraphicsUnit unit = GraphicsUnit.Point;
                                                RectangleF bounds = bitmap.GetBounds(ref unit);
                                                Thread thread = new Thread(() => LeftMouseClick((int)word.BoundingRect.X + imgData.xPos, (int)word.BoundingRect.Y + imgData.yPos));
                                                thread.Start();
                                                goto PostSearch;
                                            }
                                        }
                                    }
                                }
                            PostSearch:
                                Debug.WriteLine("Out of loops");
                            }
                        }
                        currCommand = sr.ReadLine();
                        prevScan = ocrResult.Text;
                        Thread.Sleep(2000);
                    }
                    sr.Close();
                    Debug.WriteLine("done");
                }
                if(renamePC.Text != "" && renamePC.Text != null)
                {
                    PowerShell ps = PowerShell.Create().AddCommand("Rename-Computer").AddParameter("-NewName", $"\"{renamePC.Text}\"");
                    ps.BeginInvoke();
                }

                string regCommand = "Windows Registry Editor Version 5.00\r\n\r\n[HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System]\r\n\"ConsentPromptBehaviorAdmin\"=dword:00000005\r\n\"ConsentPromptBehaviorUser\"=dword:00000003\r\n\"EnableInstallerDetection\"=dword:00000001\r\n\"EnableLUA\"=dword:00000001\r\n\"EnableVirtualization\"=dword:00000001\r\n\"PromptOnSecureDesktop\"=dword:00000001\r\n\"ValidateAdminCodeSignatures\"=dword:00000000\r\n\"FilterAdministratorToken\"=dword:00000000";
                File.WriteAllText(Directory.GetCurrentDirectory() + "\\key.reg", regCommand);
                Debug.WriteLine(Directory.GetCurrentDirectory() + "\\key.reg");
                Process regeditProcess = Process.Start("regedit.exe", "/s key.reg");
                regeditProcess.WaitForExit();
                File.Delete(Directory.GetCurrentDirectory() + "\\key.reg");
            }
        }

        public static void LeftMouseClick(int xpos, int ypos)
        {
            Debug.WriteLine("Click");
            SetCursorPos(xpos, ypos);
            mouse_event(MOUSEEVENTF_LEFTDOWN, xpos, ypos, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, xpos, ypos, 0, 0);
            SetCursorPos(10, 10);
        }

        private static bool ProcessExists(int id)
        {
            return Process.GetProcesses().Any(x => x.Id == id);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "PowerShell Scripts (*.ps1)|*.ps1|Batch Scripts (*.bat)|*.bat";
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                if(openFile.FileName.Contains(".ps1"))
                {
                    string psArgs = $"-NoExit -ExecutionPolicy Unrestricted -File \"{openFile.FileName}\"";
                    Process pShell = new Process();
                    pShell.StartInfo.Verb = "runas";
                    pShell.StartInfo.Arguments = psArgs;
                    pShell.StartInfo.FileName = "C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe";
                    pShell.Start();
                }
                else
                {
                    string cmdArgs = $"/k \"{openFile.FileName}\"";
                    Process pShell = new Process();
                    pShell.StartInfo.Verb = "runas";
                    pShell.StartInfo.Arguments = cmdArgs;
                    pShell.StartInfo.FileName = "cmd.exe";
                    pShell.Start();
                }
            }
        }
    }
}
