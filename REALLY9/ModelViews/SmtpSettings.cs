﻿namespace REALLY9.ModelViews
{
    public class SmtpSettings
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public bool EnableSsl { get; set; } // Thêm dòng này
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
