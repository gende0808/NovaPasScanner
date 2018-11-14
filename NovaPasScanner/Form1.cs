using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PCSC;
using PCSC.Iso7816;
using System.Threading;
namespace NovaPasScanner
{
    public partial class Form1 : Form
    {
        
        public string readerName;
        public Form1()
        {
            InitializeComponent();
            backgroundWorker1.RunWorkerAsync();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string passId = "";
            string scan = "";
            int i = 1;
            while (true)
            {
                try
                {
                   scan = scanForPass();
                }
                catch
                {
                    continue;
                }

                Thread.Sleep(2000);
                if (passId != scan)
                {
                    passId = scan;
                    this.Invoke(new MethodInvoker(delegate () { textBox1.Text = passId; }));
                }
            }
        }
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            
        }

        public String scanForPass()
        {
            string result = "";
            var contextFactor = ContextFactory.Instance;
            using (var context = contextFactor.Establish(SCardScope.System))
            {
                var readerNames = context.GetReaders();
                //expects to find a single NFC reader but requires this loop to go through 'all' of them.
                foreach (var readerName in readerNames)
                {
                    this.readerName = readerName;
                }
            }
            var contextFactory = ContextFactory.Instance;
            using (var ctx = contextFactory.Establish(SCardScope.System))
            {
                using (var isoReader = new IsoReader(ctx, readerName, SCardShareMode.Shared, SCardProtocol.Any, false))
                {

                    var apdu = new CommandApdu(IsoCase.Case2Short, isoReader.ActiveProtocol)
                    {
                        CLA = 0xFF, // Class
                        Instruction = InstructionCode.GetData,
                        P1 = 0x00, // Parameter 1
                        P2 = 0x00, // Parameter 2
                        Le = 0 // Expected length of the returned data
                    };

                    string str;
                    var response = isoReader.Transmit(apdu);
                    //Console.WriteLine("SW1 SW2 = {0:X2} {1:X2}", response.SW1, response.SW2);
                     string concatHex = response.HasData ? BitConverter.ToString(response.GetData()) : "No uid received";
                    str = new string((from c in concatHex
                                      where char.IsWhiteSpace(c) || char.IsLetterOrDigit(c)
                                      select c).ToArray());
          
                    byte[] data = response.GetData();

                    //For OV passes
                    if (data.Length == 4)
                    {
                        string str1 = str.Substring(0, 2);
                        string str2 = str.Substring(2, 4);
                        string str3 = str.Substring(4, 6);
                        string str4 = str.Substring(6, 8);
                        str = str4 + str3 + str2 + str1;
                        long intValue = Int64.Parse(str, System.Globalization.NumberStyles.HexNumber);
                        str = intValue.ToString();
                         return str;
                            
                    }

                    //For student passes
                    if(data.Length == 7)
                    {
                        string str1 = str.Substring(0,2);
                        string str2 = str.Substring(2,2);
                        string str3 = str.Substring(4,2);
                        string str4 = "88";

                        str = str3 + str2 + str1 + str4;
                        long intValue = Int64.Parse(str, System.Globalization.NumberStyles.HexNumber);
                        str = intValue.ToString();
                        return str;
                    }
                        
                        
                    }
                }
            return "Pass was found/read but no number was.";
        
        }

      
    }
}
