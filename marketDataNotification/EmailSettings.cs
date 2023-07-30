namespace marketDataNotification {
    public class EmailConfig {
        public string Domain { get; set; }
        public int Port { get; set; }
        public string UsernameEmail { get; set; }
        public string UsernamePassword { get; set; }
        public List<string> ToEmail { get; set; } = new List<string>();
        public List<string> CcEmail { get; set; } = new List<string>();
    }

}