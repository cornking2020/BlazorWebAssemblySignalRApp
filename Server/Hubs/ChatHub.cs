using Microsoft.AspNetCore.SignalR;

namespace BlazorWebAssemblySignalRApp.Server.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ILogger _logger;

        public ChatHub(ILogger<ChatHub> logger)
        {
            _logger = logger;
        }
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task FileUpload(string fileName, int percent, byte[] file)
        {
            _logger.LogInformation("recieved_file " + fileName + ", percent " + percent);

            var filePath = @"C:\\Users\\kuzon\\Documents\\" + fileName;

            if (percent == 0 && System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            using (var fileStream = new FileStream(filePath, FileMode.Append, FileAccess.Write))
            {
                using (var bw = new BinaryWriter(fileStream))
                {
                    await fileStream.WriteAsync(file, 0, file.Length);
                    await Clients.All.SendAsync("ReceiveMessage", "System", "Recieved percent: " + percent);
                }
            }
        }
    }
}