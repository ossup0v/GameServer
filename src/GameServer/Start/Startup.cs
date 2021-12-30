//using GameServer.Common;
//using GameServer.Configs;
//using GameServer.DAL;
//using GameServer.DAL.Mongo;
//using GameServer.Metagame;
//using GameServer.Metagame.GameRoom;
//using GameServer.Network;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;

//namespace GameServer.Start
//{

//    public class Startup
//    {
//        public Startup(IConfiguration configuration)
//        {
//            Configuration = configuration;
//        }

//        public IConfiguration Configuration { get; }

//        This method gets called by the runtime.Use this method to add services to the container.
//        public void ConfigureServices(IServiceCollection services)
//        {

//            services.AddSingleton<IMongoDatabase>(provider =>
//            {
//                var config = Configuration.GetSection("Mongo").Get<MongoDbConfig>();
//                var client = provider.GetRequiredService<IMongoClient>();
//                return client.GetDatabase(config.DbName);
//            });

//            services.Configure<RoomManagerConfig>(Configuration.GetSection(RoomManagerConfig.SectionName));
//            services.Configure<GameServerConfig>(Configuration.GetSection(GameServerConfig.SectionName));

//            services.AddSingleton<IRoomManager, RoomManager>();
//            services.AddSingleton<IGameServer, GameServer.Network.GameServer>();
//            services.AddSingleton<IGameManager, GameManager>();
//            services.AddSingleton<IServerSend, ServerSend>();
//            services.AddSingleton<IServerHandler, ServerHandler>();
//            services.AddSingleton<IUserRepository, TempUserRepository>();

//            add here new profiles from AutoMapper
//            services.AddAutoMapper(config =>
//            {
//                config.AddProfile<AutoMapperProfile>();
//            });
//        }
//    }
//}
