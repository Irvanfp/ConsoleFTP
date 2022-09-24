namespace FTPForSTEAM.Models
{
    public class AppSettings
    {
        public FTPPath FTPPath { get; set; }
        public FTPServer FTPServer { get; set; }
    }
    public class FTPPath
    {
        public string FTP_FOLDER { get; set; }
        public string INPUT_FOLDER { get; set; }
        public string INPUT_ARCHIVE_FOLDER { get; set; }
        public string INPUT_ERROR_FOLDER { get; set; }
        public string INPUT_LOG_FOLDER { get; set; }
        public bool LOG_ENABLED { get; set; }
    }
    public class FTPServer
    {
        public string HOST { get; set; }
        public string USER { get; set; }
        public string PASSWORD { get; set; }
        public bool IS_FTP_ASCII { get; set; }
        public string FOLDER_DESTINATION { get; set; }
    }
}
