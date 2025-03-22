namespace UserProfile.Dal
{
    public class UserRepository(ApplicationDbContext context)
    {

        public void AddUser(User user)
        {
            context.Users.Add(user);
            context.SaveChanges();
        }

        public List<User> GetAllUsers()
        {
            return context.Users.ToList();
        }

        public void UpdateUser(User user)
        {
            context.Users.Update(user);
            context.SaveChanges();
        }

        public void DeleteUser(int id)
        {
            var user = context.Users.Find(id);
            if (user == null) return;
            context.Users.Remove(user);
            context.SaveChanges();
        }
    }
}