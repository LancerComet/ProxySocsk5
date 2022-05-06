using System;

namespace ProxySocks5.LiteSocks {
  class SocksVersionException : Exception {
    public SocksVersionException (string message) : base(message) {
    }
  }

  class SocksDomainException : Exception {
    public SocksDomainException(string message) : base(message) {
    }
  }

  class SocksUpstreamPortException : Exception {
    public SocksUpstreamPortException (string message) : base(message) {
    }
  }
}