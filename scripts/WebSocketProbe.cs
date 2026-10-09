// Compiled against the game's own assemblies by test_websocket_transport.py.
using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

public static class WebSocketProbe
{
    public static int Main(string[] args)
    {
        try { Run(new Uri(args[0])).GetAwaiter().GetResult(); Console.WriteLine("Unity Mono WebSocket adapter: PASS"); return 0; }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }

    static async Task Run(Uri uri)
    {
        // More than a single socket/WS buffer, and multiple successive sessions.
        for (int iteration = 0; iteration < 3; iteration++) {
            using (var bridge = new SprkWebSocketTransport.Bridge()) {
                await bridge.Open(uri, TimeSpan.FromSeconds(5));
                using (var client = new TcpClient()) {
                    await client.ConnectAsync("127.0.0.1", bridge.Port);
                    var stream = client.GetStream();
                    byte[] sent = Enumerable.Range(0, 180000).Select(i => (byte)(i % 251)).ToArray();
                    var sending = stream.WriteAsync(sent, 0, sent.Length);
                    var received = new byte[sent.Length];
                    int offset = 0;
                    while (offset < received.Length) {
                        var read = stream.ReadAsync(received, offset, received.Length - offset);
                        if (await Task.WhenAny(read, Task.Delay(5000)) != read) throw new Exception("echo timeout");
                        int count = await read;
                        if (count == 0) throw new Exception("early disconnect");
                        offset += count;
                    }
                    await sending;
                    if (!sent.SequenceEqual(received)) throw new Exception("stream corruption");
                    // Ask the echo fixture to close the remote connection.
                    await stream.WriteAsync(new byte[] { 255 }, 0, 1);
                    var eof = stream.ReadAsync(received, 0, 1);
                    if (await Task.WhenAny(eof, Task.Delay(5000)) != eof || await eof != 0)
                        throw new Exception("remote close did not reach native connector");
                }
                if (await Task.WhenAny(bridge.Completion, Task.Delay(5000)) != bridge.Completion)
                    throw new Exception("relay did not clean up");
            }
        }
        using (var unattached = new SprkWebSocketTransport.Bridge()) {
            await unattached.Open(uri, TimeSpan.FromMilliseconds(150));
            if (await Task.WhenAny(unattached.Completion, Task.Delay(5000)) != unattached.Completion)
                throw new Exception("unattached bridge leaked");
        }
    }
}
