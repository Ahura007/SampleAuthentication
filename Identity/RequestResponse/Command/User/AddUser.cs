namespace Identity.RequestResponse.Command.User;

public class AddUser
{
    public string Mobile { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}