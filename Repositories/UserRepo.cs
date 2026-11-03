using Garden_group.Models;
using MongoDB.Bson;
using MongoDB.Driver;


namespace Garden_group.Repositories
{
    public class UserRepo : IUserRepository
    {
        private readonly IMongoCollection<User> _usersCollection;

        public UserRepo(IMongoDatabase database)
        {
            _usersCollection = database.GetCollection<User>("users");
        }

        public List<User> GetAllUser()
        {
            var filter = Builders<User>.Filter.Empty;

            var users = _usersCollection.Find(filter).ToList();

            return users;
        }

        public User? GetById(ObjectId id)
        {
            var filter = Builders<User>.Filter.Eq("_id", id);

            var user = _usersCollection.Find(filter).FirstOrDefault();

            return user;
        }

        public void Create(User user)
        {
            _usersCollection.InsertOne(user);
        }

        public bool Update(ObjectId id, User user)
        {
            var filter = Builders<User>.Filter.Eq("_id", id);

            user.Id = id;

            var result = _usersCollection.ReplaceOne(filter, user);

            return result.MatchedCount > 0;
        }

        public bool Delete(ObjectId id)
        {
            var filter = Builders<User>.Filter.Eq("_id", id);

            var result = _usersCollection.DeleteOne(filter);

            return result.DeletedCount > 0;
        }
    }
}
