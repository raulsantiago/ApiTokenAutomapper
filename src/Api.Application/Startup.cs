using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.CrossCutting.DependencyInjection;
using AutoMapper;
using CrossCutting.Mappings;
using Domain.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;

namespace Application {
    public class Startup {
        public Startup (IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {

            ConfigureService.ConfigureDependenciesService(services);
            ConfigureRepository.ConfigureDependenciesRepository(services);

            var config = new AutoMapper.MapperConfiguration(cfg =>
                {
                    cfg.AddProfile(new DtoToModelProfile());
                    cfg.AddProfile(new EntityToDtoProfile());
                    cfg.AddProfile(new ModelToEntityProfile());
                });

            IMapper mapper = config.CreateMapper();
            services.AddSingleton(mapper);

            // Para setar as configurações definidas do token através do arquivo appsttings.json
            var signingConfigurations = new SigningConfigurations();
            services.AddSingleton(signingConfigurations);
            var tokenConfigurations = new TokenConfigurations();
            new ConfigureFromConfigurationOptions<TokenConfigurations>(
                Configuration.GetSection("TokenConfigurations"))
                .Configure(tokenConfigurations);
            services.AddSingleton(tokenConfigurations);

            services.AddAuthentication(authOptions =>
                {
                    authOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    authOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                }).AddJwtBearer(bearerOptions =>
                {
                    var paramsValidations = bearerOptions.TokenValidationParameters;
                    paramsValidations.IssuerSigningKey = signingConfigurations.Key;
                    paramsValidations.ValidAudience = tokenConfigurations.Audience;
                    paramsValidations.ValidIssuer = tokenConfigurations.Issuer;
                    paramsValidations.ValidateIssuerSigningKey = true;
                    paramsValidations.ValidateLifetime = true;
                    paramsValidations.ClockSkew = TimeSpan.Zero;
                });

            services.AddAuthorization(auth =>
            {
                auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                        .RequireAuthenticatedUser().Build());
                        });

            services.AddSwaggerGen (c => {
                c.SwaggerDoc ("v1",
                    new Info {
                        Title = "Curso de AspNetCore 2.2 de API com AutoMapper e Token JWT",
                            Version = "v1",
                            Description = "Exemplo de API REST criada com o ASP.NET Core, AutoMapper e Token JWT",
                            Contact = new Contact {
                                Name = "Raul Santiago",
                                    Url = "https://github.com/raulsantiago"
                            }
                    });
                // Implemetando o botão Autorize inicio
                c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    In = "header",
                    Description = "Entre com o Token JWT",
                    Name = "Authorization",
                    Type = "apiKey"
                });

                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                    {{"Bearer", Enumerable.Empty<string>()}                        
                    });
                // Implemetando o botão Autorize final

            });

            services.AddMvc ().SetCompatibilityVersion (CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure (IApplicationBuilder app, IHostingEnvironment env) {
            if (env.IsDevelopment ()) {
                app.UseDeveloperExceptionPage ();
            }

            // Ativando middlewares para uso do Swagger
            app.UseSwagger ();
            app.UseSwaggerUI (c => {
                c.RoutePrefix = string.Empty;
                c.SwaggerEndpoint ("/swagger/v1/swagger.json", "Curso de API com AspNetCore 2.2");
            });

            // Redireciona o Link para o Swagger, quando acessar a rota principal
            var option = new RewriteOptions ();
            option.AddRedirect ("^$", "swagger");
            app.UseRewriter (option);

            app.UseMvc ();
        }
    }
}
