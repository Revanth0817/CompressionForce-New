using Microsoft.AspNetCore.SignalR;
using CompressionForce.Domain.Events;
using System.Threading.Tasks;

namespace CompressionForce.Web.Hubs
{
    // 🔴 DO NOT put business logic here
    // This hub only broadcasts events
    public sealed class SignalHub : Hub
    {
        // Optional: client can call this to test connectivity
        public Task Ping()
            => Clients.Caller.SendAsync("pong");
    }
}
