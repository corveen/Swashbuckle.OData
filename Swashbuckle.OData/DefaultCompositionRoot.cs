﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using Swashbuckle.Application;
using Swashbuckle.OData.Descriptions;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData
{
    /// <summary>
    /// Where default Swashbuckle.OData implementations are composed into the <see cref="ODataSwaggerProvider"/> object graph.
    /// </summary>
    internal static class DefaultCompositionRoot
    {
        public static IApiExplorer GetApiExplorer(HttpConfiguration httpConfiguration)
        {
            return new ODataApiExplorer(httpConfiguration, GetODataActionDescriptorExplorers(), GetApiDescriptionMappers());
        }

        private static IEnumerable<IODataActionDescriptorExplorer> GetODataActionDescriptorExplorers()
        {
            return new List<IODataActionDescriptorExplorer>
            {
                new SwaggerRouteStrategy(GetSwaggerRouteGenerators()),
                new AttributeRouteStrategy()
            };
        }

        private static IEnumerable<ISwaggerRouteGenerator> GetSwaggerRouteGenerators()
        {
            return new List<ISwaggerRouteGenerator>
            {
                new StandardSwaggerRouteGenerator(),
                new CustomSwaggerRouteGenerator()
            };
        }

        private static IEnumerable<IODataActionDescriptorMapper> GetApiDescriptionMappers()
        {
            return new List<IODataActionDescriptorMapper>
            {
                new SwaggerOperationMapper(GetParameterMappers()),
                new ODataActionDescriptorMapper()
            };
        }

        private static IEnumerable<IParameterMapper> GetParameterMappers()
        {
            return new List<IParameterMapper>
            {
                new MapRestierParameter(),
                new MapByParameterName(),
                new MapByDescription(),
                new MapByIndex(),
                new MapToDefault()
            };
        }

        public static SwaggerProviderOptions GetSwaggerProviderOptions(SwaggerDocsConfig swaggerDocsConfig)
        {
            Contract.Requires(swaggerDocsConfig != null);

            return new SwaggerProviderOptions(
                swaggerDocsConfig.GetFieldValue<Func<ApiDescription, string, bool>>("_versionSupportResolver"),
                swaggerDocsConfig.GetFieldValue<IEnumerable<string>>("_schemes"),
                swaggerDocsConfig.GetSecurityDefinitions(),
                swaggerDocsConfig.GetFieldValue<bool>("_ignoreObsoleteActions"),
                swaggerDocsConfig.GetFieldValue<Func<ApiDescription, string>>("_groupingKeySelector"),
                swaggerDocsConfig.GetFieldValue<IComparer<string>>("_groupingKeyComparer"),
                swaggerDocsConfig.GetFieldValue<IDictionary<Type, Func<Schema>>>("_customSchemaMappings"),
                swaggerDocsConfig.GetFieldValue<IList<Func<ISchemaFilter>>>("_schemaFilters").Select(factory => factory()),
                swaggerDocsConfig.GetFieldValue<IList<Func<IModelFilter>>>("_modelFilters").Select(factory => factory()),
                swaggerDocsConfig.GetFieldValue<bool>("_ignoreObsoleteProperties"),
                swaggerDocsConfig.GetFieldValue<Func<Type, string>>("_schemaIdSelector"),
                swaggerDocsConfig.GetFieldValue<bool>("_describeAllEnumsAsStrings"),
                swaggerDocsConfig.GetFieldValue<bool>("_describeStringEnumsInCamelCase"),
                swaggerDocsConfig.GetFieldValue<IList<Func<IOperationFilter>>>("_operationFilters").Select(factory => factory()),
                swaggerDocsConfig.GetFieldValue<IList<Func<IDocumentFilter>>>("_documentFilters").Select(factory => factory()),
                swaggerDocsConfig.GetFieldValue<Func<IEnumerable<ApiDescription>, ApiDescription>>("_conflictingActionsResolver")
            );
        }

        /// <summary>
        /// Gets the API versions. I'd rather not use reflection because the implementation may change, but can't find a better way.
        /// </summary>
        /// <param name="swaggerDocsConfig">The swagger docs configuration.</param>
        /// <returns></returns>
        public static IDictionary<string, Info> GetApiVersions(SwaggerDocsConfig swaggerDocsConfig)
        {
            Contract.Requires(swaggerDocsConfig != null);

            return swaggerDocsConfig.GetFieldValue<VersionInfoBuilder>("_versionInfoBuilder").Build();
        }
    }
}