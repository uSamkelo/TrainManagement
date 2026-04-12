using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

public class TrainUpdateHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("Connected", $"You connected with ID: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendTrainUpdate(object update)
    {
        await Clients.All.SendAsync("ReceiveTrainUpdate", update);
    }
}
