using FTPForSTEAM.Models;
using Renci.SshNet;

namespace FTPForSTEAM.Functions
{
    internal class FtpService
    {
        public static void upload(DateTime currentDate, string pathFile, AppSettings settings)
        {
            string test = settings.FTPPath.FTP_FOLDER;
            string uname = settings.FTPServer.USER;
            string pwd = settings.FTPServer.PASSWORD;
            string host = settings.FTPServer.HOST;
            string path = settings.FTPServer.FOLDER_DESTINATION;
            FileStream fileStream = new FileStream(pathFile, FileMode.Open, FileAccess.Read);

            SftpClient sftp = new SftpClient(host, 22, uname, pwd);
            //sftp.BufferSize = 2*1024;
            try
            {
                sftp.Connect();
                var connecting = sftp.IsConnected;
                Console.WriteLine("is connected: " + connecting);
                var getSSHWorkingDirectory = sftp.ListDirectory("/");
                var workingDirectory = getSSHWorkingDirectory.SelectMany(x => x.FullName).ToString();
                sftp.ChangeDirectory(path);
                using (var uplfileStream = File.OpenRead(pathFile))
                {
                    sftp.UploadFile(fileStream, Path.GetFileName(pathFile), true);
                    uplfileStream.Close();

                };
                fileStream.Close();
                Console.WriteLine($"file {test} is inputted");
                File.Delete(pathFile);

            }
            catch (Exception e)
            {
                Console.WriteLine($"file {test} is failed inputted because of {e}");
            };
        }
    }
}
