// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using BotGame.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BotGame
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddBot<BotGameBot>(options =>
           {
               var secretKey = Configuration.GetSection("botFileSecret")?.Value;

                // Loads .bot configuration file and adds a singleton that your Bot can access through dependency injection.
                var botConfig = BotConfiguration.Load(@".\BotGame.bot", secretKey);
               services.AddSingleton(sp => botConfig);

                // Retrieve current endpoint.
                var service = botConfig.Services.Where(s => s.Type == "endpoint" && s.Name == "development").FirstOrDefault();
               if (!(service is EndpointService endpointService))
               {
                   throw new InvalidOperationException($"The .bot file does not contain a development endpoint.");
               }

               options.CredentialProvider = new SimpleCredentialProvider(endpointService.AppId, endpointService.AppPassword);

                // Catches any errors that occur during a conversation turn and logs them.
                options.OnTurnError = async (context, exception) =>
               {
                   await context.SendActivityAsync("Sorry, it looks like something went wrong.");
               };

               IStorage database = new MemoryStorage();

               var _conversationState = new ConversationState(database);
               var _userState = new UserState(database);

#pragma warning disable CS0618 // Type or member is obsolete
               options.State.Add(_conversationState);
               options.State.Add(_userState);
#pragma warning restore CS0618 // Type or member is obsolete
           });

            services.AddSingleton<BotGameAccessors>(sp =>
            {
                // We need to grab the conversationState we added on the options in the previous step
                var options = sp.GetRequiredService<IOptions<BotFrameworkOptions>>().Value;
#pragma warning disable CS0618 // Type or member is obsolete
                var conversationState = options.State.OfType<ConversationState>().FirstOrDefault();
                var userState = options.State.OfType<UserState>().FirstOrDefault();
#pragma warning restore CS0618 // Type or member is obsolete

                // Create the custom state accessor.
                // State accessors enable other components to read and write individual properties of state.
                var accessors = new BotGameAccessors(conversationState, userState)
                {
                    ConversationDialogState = conversationState.CreateProperty<DialogState>("DialogState"),
                    Usuario = userState.CreateProperty<Usuario>("Usuario"),
                    Game = userState.CreateProperty<Game>("Game"),
                };

                return accessors;
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseBotFramework();
        }
    }
}
