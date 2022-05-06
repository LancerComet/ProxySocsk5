using System;

namespace ProxySocks5.LiteSocks {
  public static class Logger {
    public static void Log (string message) {
      if (Program.LogEnabled) {
        Console.WriteLine(message);
      }
    }
  }
}
