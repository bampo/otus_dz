namespace Auth.Dal
{
    public class UserRepository(ApplicationDbContext context)
    {
        public List<User> GetAllUsers()
        {
            return context.Users.ToList();
        }

    }
}