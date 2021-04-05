namespace SampleAuthentication.Commands
{
    public class RegisterExternalUserCommand
    {
        public string Name { get; set; }

        public string UserName { get; set; }

        public string ConnectorFullName { get; set; }

        public string ConnectorMobile { get; set; }

        public string ExternalProvider { get; set; }
    }
}