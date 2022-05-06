using System;
using System.Linq;
using ProxySocks5.LiteSocks;

namespace ProxySocks5 {
  public static class Program {
    public static bool LogEnabled;

    private static void Main (string[] args) {
      var host = "0.0.0.0";
      var port = 8888;

      if (args.Length >= 1) {
        host = args[0];
      }

      if (args.Length >= 2) {
        var isInt = int.TryParse(args[1], out port);
        if (!isInt) {
          Console.WriteLine("An invalid port is provided, use 8888 instead.");
          port = 8888;
        }
      }

      LogEnabled = args.Contains("-l");

      Console.WriteLine($"Server start at {host}:{port}.");
      var proxy = new Proxy(host, port);
      proxy.Start();
    }
  }
}
