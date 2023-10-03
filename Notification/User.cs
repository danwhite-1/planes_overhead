namespace User;

public class User
{
    public User(int id, string email)
    {
        Id = id;
        Email = email;
    }
    
    public int Id { get; set; }
    public string Email { get; set; }
}
