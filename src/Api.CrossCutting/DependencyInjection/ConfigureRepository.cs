using Api.Data.Context;
using Api.Data.Repository;
using Api.Domain.Interfaces;
using Data.Implementations;
using Domain.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.CrossCutting.DependencyInjection {
    public class ConfigureRepository {
        public static void ConfigureDependenciesRepository (IServiceCollection serviceCollection) 
        {
            serviceCollection.AddScoped (typeof (IRepository<>), typeof (BaseRepository<>));
            serviceCollection.AddScoped<IUserRepository, UserImplementation>();

            // serviceCollection.AddDbContext<MyContext> (
            //     options => options.UseMySql ("Server=localhost;Port=3306;Database=dbAPI;Uid=root;Pwd=mudar@123")
            // );

            serviceCollection.AddDbContext<MyContext> (
                options => options.UseSqlServer ("Server=.\\SQLEXPRESS2017;Database=dbAPI;User Id=sa;Password=global2019")
            );
        }
    }
}
