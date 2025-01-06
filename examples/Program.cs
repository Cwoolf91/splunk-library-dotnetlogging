using Splunk.Logging;
using System;
using System.Diagnostics;

namespace examples
{
    class Program
    {
        static void Main(string[] args)
        {
            EnableSelfSignedCertificates();

            TraceListenerExample();
        }

        private static void TraceListenerExample()
        {
            var myListener = new HttpEventCollectorTraceListener(
                uri: new Uri("https://splunk-server:8088"),
                token: "205A7CE0-24B6-44CD-9299-333E29BBBCF1");
            myListener.AddLoggingFailureHandler((HttpEventCollectorException e) => {
                Console.WriteLine("{0}", e);
            });
            // Replace with your HEC token
            string token = "8f574673-11c7-40cb-b400-2e364efc1c33";

            // TraceListener
            var trace = new TraceSource("Order");
            trace.Switch.Level = SourceLevels.All;
            var url = "https://splunk-indexer-hec.internal.repay.ninja:8088/services/collec";
            var listener = new HttpEventCollectorTraceListener(
                uri: new Uri(url),
                token: token,
                batchSizeCount: 1);
            bool isFalure = false;
            listener.AddLoggingFailureHandler((HttpEventCollectorException e) =>
            {
                isFalure = true;
                Console.WriteLine(e.Message);
            });
            trace.Listeners.Add(listener);
            if (isFalure)
            {
                
            }
            // Send some events
            trace.TraceEvent(TraceEventType.Error, 0, "hello world 0");
            trace.TraceEvent(TraceEventType.Information, 1, "hello world 1");
            trace.TraceData(TraceEventType.Information, 2, "hello world 2");
            trace.TraceData(TraceEventType.Error, 3, "hello world 3");
            trace.TraceData(TraceEventType.Information, 4, "hello world 4");
            trace.Close();

            // Now search splunk index that used by your HEC token you should see above 5 events are indexed
        }

        private static void EnableSelfSignedCertificates()
        {
            // Enable self signed certificates
            System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                delegate (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                    System.Security.Cryptography.X509Certificates.X509Chain chain,
                    System.Net.Security.SslPolicyErrors sslPolicyErrors)
                {
                    return true;
                };
        }
    }
}
