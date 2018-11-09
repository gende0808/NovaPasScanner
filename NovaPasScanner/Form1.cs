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

namespace NovaPasScanner
{
    public partial class Form1 : Form
    {
        BackgroundWorker workingClass;
        public string readerName;
        public Form1()
        {
            InitializeComponent();
            workingClass.DoWork += new DoWorkEventHandler(ScanPasses);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        void ScanPasses(object sender, DoWorkEventArgs e)
        {

        }

        public String passId()
        {
            var contextFactor = ContextFactory.Instance;
            using (var context = contextFactor.Establish(SCardScope.System))
            {
                Console.WriteLine("Currently connected readers: ");
                var readerNames = context.GetReaders();
                //expects to find a single NFC reader but requires this loop to go through 'all' of them.
                foreach (var readerName in readerNames)
                {
                    Console.WriteLine("\t" + readerName);
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


                    var response = isoReader.Transmit(apdu);
                    Console.WriteLine("SW1 SW2 = {0:X2} {1:X2}", response.SW1, response.SW2);
                    Console.WriteLine(response.HasData ? BitConverter.ToString(response.GetData()) : "No uid received");


                    Console.ReadLine();
                }

            }
        
            return "";
        }
    }
}
