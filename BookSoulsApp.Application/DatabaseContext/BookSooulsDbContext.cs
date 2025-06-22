using MongoDB.Driver;

namespace BookSoulsApp.Application.DatabaseContext;

public class BookSooulsDbContext(MongoDbSetting mongoDBSettings, IMongoClient mongoClient)
{

    private readonly IMongoDatabase _database = mongoClient.GetDatabase(mongoDBSettings.DatabaseName);

    public IMongoDatabase GetDatabase()
    {
        return _database;
    }
}
