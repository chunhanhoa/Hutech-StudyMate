using MongoDB.Driver;
using Check.Models;

namespace Check.Services;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoDB");
        var databaseName = configuration.GetValue<string>("MongoDB:DatabaseName");
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
    public IMongoCollection<Activity> Activities => _database.GetCollection<Activity>("Activities");
    public IMongoCollection<UserActivity> UserActivities => _database.GetCollection<UserActivity>("UserActivities");
    public IMongoCollection<SV5TRegistration> SV5TRegistrations => _database.GetCollection<SV5TRegistration>("SV5TRegistrations");
    public IMongoCollection<StudentActivity> StudentActivities => _database.GetCollection<StudentActivity>("StudentActivities");
    public IMongoCollection<ActivityArticle> ActivityArticles => _database.GetCollection<ActivityArticle>("ActivityArticles");
}
