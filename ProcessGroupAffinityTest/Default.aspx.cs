using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ProcessGroupAffinityTest
{
    public partial class _Default : Page
    {

        private static CancellationTokenSource cts = new CancellationTokenSource();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                RefreshAffinityInfo();
            }
        }

        protected void BtnGenerateLoad_Click(object sender, EventArgs e)
        {
            if (((Button)sender).Text == "Generate CPU Load")
            {
                StartCpuLoad();
                ((Button)sender).Text = "Stop CPU Load";
            }
            else
            {
                StopCpuLoad();
                ((Button)sender).Text = "Generate CPU Load";
            }
            RefreshAffinityInfo();
        }

        private void StartCpuLoad()
        {
            cts = new CancellationTokenSource();
            int processorCount = Environment.ProcessorCount;

            for (int i = 0; i < processorCount; i++)
            {
                int processorIndex = i;
                Task.Run(() =>
                {
                    GenerateLoad(cts.Token, processorIndex);
                }, cts.Token);
            }
        }

        private void GenerateLoad(CancellationToken token, int processorIndex)
        {
            int processorCount = Environment.ProcessorCount;
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = processorCount,
                CancellationToken = token
            };

            Parallel.ForEach(Enumerable.Range(0, processorCount), options, (i) =>
            {
                Thread.CurrentThread.Priority = ThreadPriority.Highest;
                while (!token.IsCancellationRequested)
                {
                    // Perform CPU-intensive calculations
                    double value = 0;
                    for (int j = 0; j < 100000; j++)
                    {
                        value += Math.Sqrt(j);
                    }
                }
            });
        }

        private void StopCpuLoad()
        {
            cts.Cancel();
        }

        private void RefreshAffinityInfo()
        {
            var process = Process.GetCurrentProcess();
            long processorAffinity = process.ProcessorAffinity.ToInt64();
            var msg = new StringBuilder();
            msg.AppendLine($"Process {process.ProcessName}:{process.Id}");
            msg.AppendLine($"Affinity Mask: {FormatAffinity(processorAffinity)}");

            try
            {
                ushort[] groupAffinity = GetProcessGroupAffinity(process.Handle);
                msg.AppendLine($"Associated with {groupAffinity.Length} group(s):");
                for (int i = 0; i < groupAffinity.Length; i++)
                {
                    msg.AppendLine($"Group: {groupAffinity[i]}");
                }
            }
            catch (Exception ex)
            {
                msg.AppendLine($"Failed to get process group affinity: {ex.Message}");
            }

            lblProcessorGroupInfo.Text = msg.ToString().Replace("\r\n", "<br/>");
        }

        private static string FormatAffinity(long affinity)
        {
            return Convert.ToString(affinity, 2).PadLeft(Environment.ProcessorCount, '0');
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetProcessGroupAffinity(
            IntPtr hProcess,
            ref ushort GroupCount,
            [MarshalAs(UnmanagedType.LPArray), Out] ushort[] GroupArray);

        public static ushort[] GetProcessGroupAffinity(IntPtr processHandle)
        {
            ushort groupCount = 0;
            ushort[] groupArray = new ushort[1]; // Start with a small buffer

            while (!GetProcessGroupAffinity(processHandle, ref groupCount, groupArray))
            {
                int error = Marshal.GetLastWin32Error();
                if (error == 122) // ERROR_INSUFFICIENT_BUFFER
                {
                    // Increase the buffer size and try again
                    Array.Resize(ref groupArray, groupCount);
                }
                else
                {
                    throw new Exception($"Failed to get process group affinity. Error code: {error}");
                }
            }

            // Trim the array to the actual number of groups
            Array.Resize(ref groupArray, groupCount);
            return groupArray;
        }

    }
}