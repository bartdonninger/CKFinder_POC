using CKSource.FileSystem.Local;
using CKSource.CKFinder.Connector.Config;
using CKSource.CKFinder.Connector.Core.Builders;
using CKSource.CKFinder.Connector.Core.Logs;
using CKSource.CKFinder.Connector.Host.Owin;
using Microsoft.Owin;
using Owin;
using CKSource.CKFinder.Connector.Logs.NLog;
using Microsoft.Owin.Cors;
using System.Web.Http;

[assembly: OwinStartup(typeof(TestAPI.Startup))]

namespace TestAPI
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Setup logger for CKFinder
            LoggerManager.LoggerAdapterFactory = new NLogLoggerAdapterFactory();

            // TODO: Needs to be more secure? See: http://benfoster.io/blog/aspnet-webapi-cors
            // TODO: Editing images doesn't werk yet: 'Failed to execute 'getImageData' on 'CanvasRenderingContext2D': The canvas has been tainted by cross-origin data.'
            // TODO: Do we want to use OWIN Cors for the complete API or do we want to use Web API Cors for the rest of the api
            // and only use OWIN for the CKFinder Mapping? We need to move it into the setup conenctor if we do.
            app.UseCors(CorsOptions.AllowAll);

            /*
             * Register the "local" type backend file system.
             */
            FileSystemFactory.RegisterFileSystem<LocalStorage>();

            /*
             * Map the CKFinder connector service under a given path. By default the CKFinder JavaScript
             * client expect the ASP.NET connector to be accessible under the "/ckfinder/connector" route.
             */
            app.Map("/ckfinder/connector", SetupConnector);

            // Setup WebApi
            HttpConfiguration httpConfiguration = new HttpConfiguration();
            WebApiConfig.Register(httpConfiguration);
            app.UseWebApi(httpConfiguration);
        }

        private static void SetupConnector(IAppBuilder app)
        {
            

            /*
             * Create a connector instance using ConnectorBuilder. The call to the LoadConfig() method
             * will configure the connector using CKFinder configuration options defined in Web.config.
             */
            var connectorFactory = new OwinConnectorFactory();
            var connectorBuilder = new ConnectorBuilder();
            /*
             * Create an instance of authenticator implemented in the previous step.
             */
            var customAuthenticator = new CustomCKFinderAuthenticator();
            connectorBuilder
                /*
                 * Provide the global configuration.
                 *
                 * If you installed CKSource.CKFinder.Connector.Config you should load the static configuration
                 * from XML:
                 * connectorBuilder.LoadConfig();
                 */
                .LoadConfig()
                .SetAuthenticator(customAuthenticator)
                .SetRequestConfiguration(
                    (request, config) =>
                    {
                        // TODO: Do we need EntityFrameworkKeyValueStoreProvider?


                        /*
                         *
                         * If you installed CKSource.CKFinder.Connector.KeyValue.EntityFramework, you may enable caching:
                         * config.SetKeyValueStoreProvider(
                         *     new EntityFrameworkKeyValueStoreProvider("CKFinderCacheConnection"));
                         */

                        config.LoadConfig();
                    }
                );
            /*
             * Build the connector middleware.
             */
            var connector = connectorBuilder
                .Build(connectorFactory);
            /*
             * Add the CKFinder connector middleware to the web application pipeline.
             */
            //app.Use(typeof(MyMiddleWare));
            app.UseConnector(connector);
        }
    }
}
