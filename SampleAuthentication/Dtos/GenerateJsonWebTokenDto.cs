using System.Collections.Generic;

namespace SampleAuthentication.Dtos
{
    public class GenerateJsonWebTokenDto
    {
        public string name { get; set; }
        public string userName { get; set; }
        public string token_type { get; set; }
        public string id { get; set; }
        public string auth_token { get; set; }
        public int expires_in { get; set; }
        public string userType { get; set; }
        public bool isSuperAdmin { get; set; }
        public bool first_login { get; set; }
        public IReadOnlyCollection<string> roleIds { get; set;}
        public string personType { get; set; }

    }
}
