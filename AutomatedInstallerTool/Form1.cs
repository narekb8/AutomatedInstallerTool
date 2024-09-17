using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Media.Ocr;
using Windows.Graphics.Imaging;
using System.Drawing.Imaging;
using System.Management.Automation;
using Windows.UI.UIAutomation;


namespace WinFormsTest
{
    public partial class Form1 : Form
    {
        public Dictionary<string, string> exeMap;
        public Form1()
        {
            InitializeComponent();
            exeMap = [];
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

        public class ImgData(int x, int y, Bitmap map)
        {
            public int xPos = x;
            public int yPos = y;
            public Bitmap bmap = map;
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

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

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
            Directory.CreateDirectory(".\\cfg\\debug");
            foreach (string path in Directory.GetFiles(Directory.GetCurrentDirectory()))
            {
                if(path.Contains(".lnk"))
                {
                    Debug.WriteLine(path);
                    string newKey = Path.GetFileNameWithoutExtension(path);
                    exeMap[newKey] = path;
                    checkedListBox1.Items.Add(newKey);
                    File.Open(Directory.GetCurrentDirectory() + "\\cfg\\" + Path.GetFileNameWithoutExtension(path) + ".cfg", FileMode.OpenOrCreate).Close();
                }
            }
        }

        async private void installButton_Click(object sender, EventArgs e)
        {
            if (checkedListBox1.CheckedItems.Count != 0)
            {
                OcrEngine ocr;
                ocr = OcrEngine.TryCreateFromUserProfileLanguages();

                for (int x = 0; x < checkedListBox1.CheckedItems.Count; x++)
                {
                    string currItem = exeMap[checkedListBox1.CheckedItems[x].ToString()];
                    ProcessStartInfo info = new ProcessStartInfo(currItem);
                    info.UseShellExecute = true;
                    //info.Arguments = $"/k \"{exeMap[checkedListBox1.CheckedItems[x].ToString()]}\"";
                    Process curr = Process.Start(info);

                    int currPID = curr.Id;
                    String prevScan = "";
                    string dir = Path.GetDirectoryName(currItem) + "\\cfg\\" + checkedListBox1.CheckedItems[x].ToString() + ".cfg";
                    var sr = new StreamReader(File.Open(dir, FileMode.OpenOrCreate));
                    Debug.WriteLine(currPID);

                    Thread.Sleep(5000);

                    while (ProcessExists(currPID))
                    {
                        SetForegroundWindow((IntPtr)curr.MainWindowHandle);
                        SoftwareBitmap currWindow;
                        ImgData imgData = CaptureWindow();
                        if (imgData == null) continue;
                        Bitmap bitmap = imgData.bmap;

                        using (var stream = new Windows.Storage.Streams.InMemoryRandomAccessStream())
                        {
                            var fileStream = File.Create(".\\cfg\\debug\\img1.bmp");
                            bitmap.Save(fileStream, ImageFormat.Png);
                            fileStream.Close();
                            bitmap.Save(stream.AsStream(), ImageFormat.Bmp); //choose the specific image format by your own bitmap source
                            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                            currWindow = await decoder.GetSoftwareBitmapAsync();
                            stream.Dispose();
                        }

                        Thread.Sleep(10000);

                        SetForegroundWindow((IntPtr)curr.MainWindowHandle);
                        SoftwareBitmap currWindow2;
                        ImgData imgData2 = CaptureWindow();
                        if (imgData2 == null) continue;
                        Bitmap bitmap2 = imgData2.bmap;

                        using (var stream = new Windows.Storage.Streams.InMemoryRandomAccessStream())
                        {
                            var fileStream = File.Create(".\\cfg\\debug\\img2.bmp");
                            bitmap2.Save(fileStream, ImageFormat.Png);
                            fileStream.Close();
                            bitmap2.Save(stream.AsStream(), ImageFormat.Bmp); //choose the specific image format by your own bitmap source
                            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                            currWindow2 = await decoder.GetSoftwareBitmapAsync();
                            stream.Dispose();
                        }

                        var ocrResult = await ocr.RecognizeAsync(currWindow);
                        var ocrResult2 = await ocr.RecognizeAsync(currWindow2);
                        Debug.WriteLine(GetDamerauLevenshteinDistance(ocrResult.Text, ocrResult2.Text));
                        if (GetDamerauLevenshteinDistance(ocrResult.Text, ocrResult2.Text) > 0)
                        {
                            continue;
                        }

                        curr.Refresh();

                        string ocrText = ocrResult2.Text.ToLower();

                        if (ocrText != "" && GetDamerauLevenshteinDistance(ocrText, prevScan) > 50)
                        {
                            string currCommand = sr.ReadLine();
                            Debug.WriteLine(ocrText);
                            Debug.WriteLine(GetDamerauLevenshteinDistance(ocrText, prevScan));
                            Debug.WriteLine(currCommand);

                            if (currCommand != null && currCommand != "")
                            {
                                if (currCommand == "-/-/")
                                {
                                    prevScan = ocrText;
                                    continue;
                                }
                                else if (!ocrText.Contains(currCommand.ToLower()))
                                {
                                    curr.Kill();
                                    // TODO: Figure out a good method for bringing this pop-up to focus
                                    var w = new Form() { Size = new Size(0, 0) };
                                    MessageBox.Show(w, "Invalid Command. Please double check the associated steps.cfg file.",
                                        "Cannot find next step.", MessageBoxButtons.OK);
                                    w.BringToFront();
                                    w.Activate();
                                    await Task.Delay(TimeSpan.FromSeconds(5))
                                        .ContinueWith((t) => w.Close(), TaskScheduler.FromCurrentSynchronizationContext());
                                    break;
                                }
                                Debug.WriteLine("First if");
                                List<Tuple<int, int>> wordList = new List<Tuple<int, int>>();
                                foreach (OcrLine line in ocrResult2.Lines.ToArray())
                                {
                                    string currLine = line.Text.ToLower();
                                    if (currLine.Contains(currCommand.ToLower()))
                                    {
                                        Debug.WriteLine("Found line");
                                        foreach (OcrWord word in line.Words)
                                        {
                                            if (word.Text.ToLower().Equals(currCommand.ToLower()))
                                            {
                                                wordList.Add(new Tuple<int, int>((int)word.BoundingRect.X + imgData2.xPos, (int)word.BoundingRect.Y + imgData2.yPos));
                                            }
                                        }
                                    }
                                }

                                foreach (Tuple<int, int> word in wordList)
                                {
                                    Debug.WriteLine("Found word");
                                    GraphicsUnit unit = GraphicsUnit.Point;
                                    RectangleF bounds = bitmap.GetBounds(ref unit);
                                    Thread thread = new Thread(() => LeftMouseClick(word.Item1, word.Item2));
                                    thread.Start();
                                    thread.Join();
                                }
                            }
                        }
                        prevScan = ocrResult2.Text;
                        currWindow2.Dispose();
                        currWindow.Dispose();
                        Thread.Sleep(5000);
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

        // Taken from https://stackoverflow.com/a/57961456, changed null or empty values to force a major inequality
        public static int GetDamerauLevenshteinDistance(string s, string t)
        {
            if (string.IsNullOrEmpty(s))
            {
                return 200;
            }

            if (string.IsNullOrEmpty(t))
            {
                return 200;
            }

            int n = s.Length; // length of s
            int m = t.Length; // length of t

            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            int[] p = new int[n + 1]; //'previous' cost array, horizontally
            int[] d = new int[n + 1]; // cost array, horizontally

            // indexes into strings s and t
            int i; // iterates through s
            int j; // iterates through t

            for (i = 0; i <= n; i++)
            {
                p[i] = i;
            }

            for (j = 1; j <= m; j++)
            {
                char tJ = t[j - 1]; // jth character of t
                d[0] = j;

                for (i = 1; i <= n; i++)
                {
                    int cost = s[i - 1] == tJ ? 0 : 1; // cost
                                                       // minimum of cell to the left+1, to the top+1, diagonally left and up +cost                
                    d[i] = Math.Min(Math.Min(d[i - 1] + 1, p[i] + 1), p[i - 1] + cost);
                }

                // copy current distance counts to 'previous row' distance counts
                int[] dPlaceholder = p; //placeholder to assist in swapping p and d
                p = d;
                d = dPlaceholder;
            }

            // our last action in the above loop was to switch d and p, so p now 
            // actually has the most recent cost counts
            return p[n];
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new()
            {
                Filter = "PowerShell Scripts (*.ps1)|*.ps1|Batch Scripts (*.bat)|*.bat"
            };
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
