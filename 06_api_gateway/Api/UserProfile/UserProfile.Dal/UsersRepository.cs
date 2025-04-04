using Microsoft.EntityFrameworkCore;
using Users.Dal.Entities;

namespace Users.Dal
{
    public class UsersRepository(UsersDbContext context)
    {

        public Task AddUser(User user)
        {
            context.Users.Add(user);
            return context.SaveChangesAsync();
        }

        public Task<List<User>> GetAllUsers()
            => context.Users.ToListAsync();

        public async Task<ProfileInfo?> GetUserProfile(int userId)
        {
            if (await context.Users.AnyAsync(u => u.Id == userId))
            {
                return await context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId) ?? new ProfileInfo{UserId = userId};
            }
            return null;
        }

        public Task<int> UpdateProfile(ProfileInfo profileInfo)
        {
            context.UserProfiles.Update(profileInfo);
            return context.SaveChangesAsync();
        }
    }
}