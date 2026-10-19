using Garden_group.Models;
using MongoDB.Bson;

public interface IUserRepository
{
    List<User> GetAllUser();

    User GetById(ObjectId id);


    void Create(User user);

    bool Update(ObjectId id, User user);

    bool Delete(ObjectId id);
}
