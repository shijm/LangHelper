using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

using FilePath = System.Collections.Generic.KeyValuePair<string, string>;
using Data = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, string>>;

namespace LangHelper {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, System.Windows.Forms.IWin32Window {

        public static event Action<string> Log = delegate { };

        public IntPtr Handle {
            get { return new WindowInteropHelper(this).Handle; }
        }

        public MainWindow() {
            InitializeComponent();
            Log += LogTo;
        }

        protected override void OnContentRendered(EventArgs e) {
            base.OnContentRendered(e);
            //txtInputFolder.Text = @"C:\Users\sjm91\Desktop\新建文件夹";
            //txtOutputFolder.Text = @"C:\Users\sjm91\Desktop\新建文件夹 (2)";
            //txtItems.Text = "Options";
        }

        private void txtInputFolder_TextChanged(object sender, TextChangedEventArgs e) {
            UpdateParseButton();
        }

        private void txtOutputFolder_TextChanged(object sender, TextChangedEventArgs e) {
            UpdateParseButton();
        }

        private void btnChooseOutput_Click(object sender, RoutedEventArgs e) {
            txtOutputFolder.Text = ChooseFolder();
        }

        private void btnChooseInput_Click(object sender, RoutedEventArgs e) {
            txtInputFolder.Text = ChooseFolder();
        }

        private void btnParse_Click(object sender, RoutedEventArgs e) {
            btnParse.IsEnabled = false;
            txtLog.Clear();
            Parse(txtInputFolder.Text.Trim(), txtOutputFolder.Text.Trim(), txtItems.Text.Trim());
            UpdateParseButton();
        }

        private void UpdateParseButton() {
            btnParse.IsEnabled = !string.IsNullOrEmpty(txtInputFolder.Text) && !string.IsNullOrEmpty(txtOutputFolder.Text);
        }

        private string ChooseFolder() {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            if (dlg.ShowDialog(this) == System.Windows.Forms.DialogResult.OK) {
                return dlg.SelectedPath;
            }
            return "";
        }

        private void LogTo(string str) {
            if (!string.IsNullOrEmpty(str)) {
                txtLog.Text += str;
                txtLog.Text += "\n";
            }
        }

        private static void Parse(string srcFolderPath, string dstFolderPath, string itemNames) {
            Log("--------------Begin parsing--------------");

            if (string.IsNullOrEmpty(srcFolderPath) || string.IsNullOrEmpty(dstFolderPath)) {
                Log("请选好源目录和目标目录");
                return;
            }

            if (string.IsNullOrEmpty(itemNames)) {
                Log("请填好要复制的语言项的name");
                return;
            }

            if (srcFolderPath == dstFolderPath || !Directory.Exists(srcFolderPath) || !Directory.Exists(dstFolderPath)) {
                Log("源目录或目标目录不正确");
                return;
            }

            FilePath[] srcFiles = Directory.GetFiles(srcFolderPath, "*.xml").Select(path => new FilePath(Path.GetFileNameWithoutExtension(path), path)).ToArray();
            if (srcFiles.Length == 0) {
                Log("源目录是空的");
                return;
            }

            FilePath[] dstFiles = Directory.GetFiles(dstFolderPath, "*.xml").Select(path => new FilePath(Path.GetFileNameWithoutExtension(path), path)).ToArray();
            if (dstFiles.Length == 0) {
                Log("目标目录是空的");
                return;
            }

            string[] items = itemNames.Split("\r\n".ToCharArray()).Select(str => str.Trim()).Where(str => !string.IsNullOrEmpty(str)).Distinct().ToArray();
            if (items.Length == 0) {
                Log("语言项的name不正确");
                return;
            }

            Data srcData = new Data();
            foreach (FilePath lang in srcFiles) {
                srcData.Add(lang.Key, ReadDataItemValue(lang, items));
            }

            Data dstData = new Data();
            foreach (FilePath lang in dstFiles) {
                dstData.Add(lang.Key, new Dictionary<string, string>());
                foreach (string item in items) {
                    dstData[lang.Key].Add(item, GetDataItemValue(lang.Key, item, srcData));
                }
            }

            SaveDataItems(dstFiles, dstData);

            Log("--------------End parsing--------------");
        }

        private static Dictionary<string, string> ReadDataItemValue(FilePath langFile, string[] items) {
            Dictionary<string, string> ret = new Dictionary<string, string>();

            var xes = XElement.Load(langFile.Value)?.Elements();
            if (xes != null) {
                foreach (var item in items) {
                    var xe = xes.FirstOrDefault(e => e.Attribute("name")?.Value == item);
                    if (xe != null) {
                        ret.Add(item, xe.Value ?? "");
                    }
                    else {
                        Log($"Item {item} not found in {langFile.Key}");
                    }
                }
            }

            return ret;
        }

        private static void SaveDataItems(FilePath[] dstFiles, Data dstData) {
            foreach (var langData in dstData) {
                string path = dstFiles.FirstOrDefault(sf => sf.Key == langData.Key).Value;
                if (string.IsNullOrEmpty(path)) {
                    continue;
                }

                Log($"Saving file {langData.Key}");

                string text = File.ReadAllText(path);
                if (string.IsNullOrEmpty(text)) {
                    Log($"File is empty: {path}");
                    return;
                }

                string indent = "\t";
                string first200 = text.Substring(0, 200);
                if (first200.Contains("  <item")) {
                    indent = "  ";
                }
                else if (first200.Contains("    <item")) {
                    indent = "    ";
                }
                Log($"Indent is \"{indent}\"");

                StringBuilder sb = new StringBuilder();
                foreach (var kvp in langData.Value) {
                    string str = $"{indent}<item name=\"{kvp.Key}\">{kvp.Value}</item>\r\n";
                    sb.Append(str);
                }

                int pos = text.LastIndexOf("</");
                if (pos > 0) {
                    text = text.Substring(0, pos) + sb.ToString() + text.Substring(pos);
                }
                else {
                    Log("End not found...");
                }

                File.WriteAllText(path, text);                
            }
        }

        private static string GetDataItemValue(string lang, string item, Data srcData) {
            string value;
            Dictionary<string, string> itemValue;
            return srcData.TryGetValue(lang, out itemValue) && itemValue.TryGetValue(item, out value) ? value : "";
        }

    }
}
