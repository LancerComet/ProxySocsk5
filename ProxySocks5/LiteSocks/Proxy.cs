// https://samsclass.info/122/proj/how-socks5-works.html

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ProxySocks5.LiteSocks {
    public class Proxy {
      private static int ParseSocksVersion (NetworkStream stream) {
        var numBytesToRead = 3;
        var numberOfByteshasRead = 0;
        var bytes = new byte[3];
        do {
          var n = stream.Read(bytes, numberOfByteshasRead, numBytesToRead);
          numberOfByteshasRead += n;
          numBytesToRead -= n;
        } while (numBytesToRead > 0);

        if (bytes[0] != 5) {
          throw new SocksVersionException("socks version is wrong");
        }

        stream.Write(new byte[] { 0x5, 0x0 }, 0, 2);
        return bytes[0];
      }

      private static string GetHost (NetworkStream stream, byte flag) {
        var bytes = new byte[1024];

        switch (flag) {
          // 如果采用的是域名
          case 0x03:
            // 获取域名长度
            var numBytesToRead = 1;
            var numberOfBytesHasRead = 0;
            Array.Clear(bytes, 0, bytes.Length);
            do {
              var n = stream.Read(bytes, numberOfBytesHasRead, numBytesToRead);
              numBytesToRead -= n;
              numberOfBytesHasRead += n;
            } while (numBytesToRead > 0);

            // 获取域名
            numBytesToRead = bytes[0];
            numberOfBytesHasRead = 0;
            Array.Clear(bytes, 0, bytes.Length);
            do {
              var n = stream.Read(bytes, numberOfBytesHasRead, numBytesToRead);
              numBytesToRead -= n;
              numberOfBytesHasRead += n;
            } while (numBytesToRead > 0);

            var domain = Encoding.ASCII.GetString(bytes, 0, numberOfBytesHasRead);
            Logger.Log($"Socks5: upstream domain is {domain}");
            return domain;

          case 0x01:
            break;

          default:
            throw new SocksDomainException("can not get upstream domain or ip");
        }

        return null;
      }

      private static int GetPort (NetworkStream stream) {
        var numBytesToRead = 2;
        var numberOfByteshasRead = 0;
        var bytes = new byte[2];
        Array.Clear(bytes, 0, bytes.Length);
        do
        {
          var n = stream.Read(bytes, numberOfByteshasRead, numBytesToRead);
          numberOfByteshasRead += n;
          numBytesToRead -= n;
        } while (numBytesToRead > 0);
        if (numberOfByteshasRead == 0)
          throw new SocksUpstreamPortException("can not parse upstream port");
        Logger.Log($"Socks5: upstream port {bytes[0] * 256 + bytes[1]}");
        return bytes[0] * 256 + bytes[1];
      }

      private static void ResponseToSocks(NetworkStream stream) {
        var sockResponse = new byte[] {
          0x05, 0x00, 0x00, 0x01,
          0x00, 0x00, 0x00, 0x00,
          0x1f, 0x40
        };
        stream.Write(sockResponse, 0, sockResponse.Length);
      }

      private static async void Run(NetworkStream stream) {
        try {
          // 解析 socks 头
          var numBytesToRead = 4;
          var numberOfByteshasRead = 0;
          var bytes = new byte[4];
          var version = ParseSocksVersion(stream);
          do {
            var n = stream.Read(bytes, numberOfByteshasRead, numBytesToRead);
            numberOfByteshasRead += n;
            numBytesToRead -= n;
          } while (numBytesToRead > 0);

          var conn = new Connection(GetHost(stream, bytes[3]), GetPort(stream), stream);
          ResponseToSocks(stream);

          // 转发请求并获取响应
          await conn.Response();
        } catch (IOException e) {
          Logger.Log(e.Message);
        } catch (SocksVersionException e) {
          Logger.Log(e.Message);
        } catch (SocksDomainException e) {
          Logger.Log(e.Message);
        } catch (SocksUpstreamPortException e) {
          Logger.Log(e.Message);
        } catch (Exception e) {
          Logger.Log(e.Message);
        }
      }

      private readonly TcpListener _tcpListener;

      public void Start () {
        this._tcpListener.Start();
        while (true) {
          var client = this._tcpListener.AcceptTcpClient();
          var stream = client.GetStream();
          Run(stream);
        }
      }

      public Proxy (string bindIp, int port) {
        var address = IPAddress.Parse(bindIp);
        this._tcpListener = new TcpListener(address, port);
      }
    }
}
