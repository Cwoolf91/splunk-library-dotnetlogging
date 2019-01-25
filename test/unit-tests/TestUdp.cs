﻿using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Splunk.Logging;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace integration_tests
{
    public class TestUdp
    {
        [Trait("integration-tests", "Splunk.Logging.UdpTraceListener")]
        [Fact]
        public async Task TestUdpTraceListener()
        {
            int port = 11000;
            var udpclient = new UdpClient(port);

            var traceSource = new TraceSource("UnitTestLogger");
            traceSource.Listeners.Remove("Default");
            traceSource.Switch.Level = SourceLevels.All;
            traceSource.Listeners.Add(new UdpTraceListener(IPAddress.Loopback, port));
            
            traceSource.TraceEvent(TraceEventType.Information, 100, "Boris");

            var dgram = await udpclient.ReceiveAsync();
            var receivedText = Encoding.UTF8.GetString(dgram.Buffer);
            Assert.Equal("UnitTestLogger Information: 100 : ", receivedText);

            dgram = await udpclient.ReceiveAsync();
            receivedText = Encoding.UTF8.GetString(dgram.Buffer);
            Assert.Equal("Boris\r\n", receivedText);

            udpclient.Close();
            traceSource.Close();
        }

        [Trait("integration-tests", "Splunk.Logging.UdpEventSink")]
        [Fact]
        public async Task TestUdpEventSink()
        {
            int port = 11001;
            var udpclient = new UdpClient(port);

            var slabListener = new ObservableEventListener();
            slabListener.Subscribe(new UdpEventSink(IPAddress.Loopback, port, new TestEventFormatter()));
            var source = TestEventSource.GetInstance();
            slabListener.EnableEvents(source, EventLevel.LogAlways, Keywords.All);

            var t = udpclient.ReceiveAsync();

            source.Message("Boris", "Meep");

            var receivedText = Encoding.UTF8.GetString((await t).Buffer);

            Assert.Equal(
                "EventId=1 EventName=MessageInfo Level=Error \"FormattedMessage=Meep - Boris\" \"message=Boris\" \"caller=Meep\"\r\n",
                receivedText);

            udpclient.Close();
            slabListener.Dispose();
        }
    }
}
