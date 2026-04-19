using Confluent.Kafka;

namespace APZ_PZ2;

using System.Text.Json;
using StackExchange.Redis;
using Dapper;
using MySql.Data.MySqlClient;

//Приклад №1: Cache-Aside Pattern (C#)
//Запит до ШІ:
//"Напиши сервіс на C#, який реалізує патерн Cache-Aside для отримання даних профілю користувача.
//Код повинен містити перевірку наявності даних у кеші, запит до бази даних у разі cache miss
//та подальше оновлення кешу з встановленням TTL."

public class UserProfile
{
    public long Id { get; set; }
    public string Username { get; set; }
    public string Bio { get; set; }
}

public class UserProfileService
{
    private readonly IDatabase _cache;

    private readonly string _dbConnectionString;

    private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(1);

    public UserProfileService(IConnectionMultiplexer redis, string dbConnectionString)
    {
        _cache = redis.GetDatabase();
        _dbConnectionString = dbConnectionString;
    }

    public async Task<UserProfile> GetProfileAsync(long userId)
    {
        string cacheKey = $"user:profile:{userId}";

        var cachedData = await _cache.StringGetAsync(cacheKey);
        if (cachedData.HasValue)
        {
            return JsonSerializer.Deserialize<UserProfile>(cachedData!);
        }

        using var connection = new MySqlConnection(_dbConnectionString);
        string sql = "SELECT Id, Username, Bio FROM Users WHERE Id = @Id";
        var profile = await connection.QuerySingleOrDefaultAsync<UserProfile>(sql, new { Id = userId });

        if (profile != null)
        {
            var serialized = JsonSerializer.Serialize(profile);
            await _cache.StringSetAsync(cacheKey, serialized, _cacheExpiration);
        }

        return profile;
    }
}

//Приклад №2: Асинхронний Publisher у Kafka (C#)
//Запит до ШІ:
//"Створи клас на мові C# для публікації повідомлень у Kafka за допомогою бібліотеки.
//Об'єкт повідомлення повинен представляти подію створення піна і містити id об'єкта,
//користувача та шлях до сховища. Налаштуй параметри продюсера для роботи у високонавантаженій системі."

public class MediaPipelinePublisher
{
    private readonly IProducer<Null, string> _producer;
    private const string TopicName = "events_pin_uploaded";

    public MediaPipelinePublisher(string kafkaBrokerList)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = kafkaBrokerList,
            Acks = Acks.Leader,
            LingerMs = 5
        };
        _producer = new ProducerBuilder<Null, string>(config).Build();
    }

    public async Task PublishPinUploadEventAsync(long pinId, long userId, string s3Url)
    {
        var eventPayload = new
        {
            EventId = Guid.NewGuid(),
            EventType = "PIN_CREATED",
            PinId = pinId,
            UserId = userId,
            S3ObjectKey = s3Url,
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        string jsonPayload = JsonSerializer.Serialize(eventPayload);
        var message = new Message<Null, string> { Value = jsonPayload };

        await _producer.ProduceAsync(TopicName, message);
    }
}