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

        public async Task FileUpload(string fileName, int index, byte[] fragment)
        {
            string[] folderPaths = { Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), fileName };

            var folderPath = System.IO.Path.Combine(folderPaths);

            if (!System.IO.Directory.Exists(folderPath))
            {
                System.IO.Directory.CreateDirectory(folderPath);
            }

            string[] paths = { folderPath, index.ToString() };

            var filePath = System.IO.Path.Combine(paths);

            _logger.LogInformation("recieved_file " + filePath);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using (var bw = new BinaryWriter(fileStream))
                {
                    await fileStream.WriteAsync(fragment, 0, fragment.Length);
                    await Clients.All.SendAsync("ReceiveMessage", "File: ", "Recieved fragment: " + index);
                }
            }
        }

        public async Task FileMerge(string fileName)
        {
            string[] paths = { Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), fileName };

            var filePath = System.IO.Path.Combine(paths);

            _logger.LogInformation("merge_file " + filePath);

            string[] inputFilePaths = Directory.GetFiles(filePath);
            _logger.LogInformation("Files in directory, {0}", inputFilePaths);
            using (var outputStream = File.Create(filePath))
            {
                foreach (var inputFilePath in inputFilePaths)
                {
                    using (var inputStream = File.OpenRead(inputFilePath))
                    {
                        inputStream.CopyTo(outputStream);
                    }
                    _logger.LogInformation("The file {0} has been processed.", inputFilePath);
                }
            }

            await Clients.All.SendAsync("ReceiveMessage", "File merged", fileName);
        }
    }
}