using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ProxySocks5.LiteSocks {
  internal class Connection {
    private readonly int _port;
    private readonly string _host;

    private TcpClient _tcpClient;
    private NetworkStream _upstream;
    private readonly NetworkStream _clientStream;

    public Connection (string host, int port, NetworkStream clientStream) {
      this._port = port;
      this._host = host;
      this._clientStream = clientStream;
    }

    private void Close () {
      this._upstream.Close();
      this._tcpClient.Close();
      this._clientStream.Close();
    }

    private void CopyTo () {
      try {
        this._clientStream.CopyTo(_upstream);
      } catch (ObjectDisposedException) {
        Console.WriteLine("the reader has closed");
      }       catch (IOException) {
        Console.WriteLine("the reader has closed");
      }
    }

    public async Task Response () {
      try {
        Console.WriteLine("connect to upstream: {0}:{1}", _host, _port);
        this._tcpClient = new TcpClient();
        await _tcpClient.ConnectAsync(_host, _port);
        this._upstream = _tcpClient.GetStream();
        this._upstream.ReadTimeout = 3000;

        var thread = new Thread(CopyTo);
        thread.Start();

        await this._upstream.CopyToAsync(_clientStream);
        Close();
      } catch (IOException e) {
        Close();
        Console.WriteLine("connect to upstream {0}:{1} error: ", _host, _port);
        Console.WriteLine(e.Message);
      }
    }
  }
}
