using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace Core.Azure
{
    /// <summary>
    /// A Class for treating Azure DocumentDB
    /// </summary>
    public static class DocumentDBRepository
    {
        #region class member
        private static readonly string DatabaseId = ConfigurationManager.AppSettings["database"];
        private static readonly string CollectionId = ConfigurationManager.AppSettings["collection"];
        private static DocumentClient client;
        #endregion

        #region initialize
        public static void Initialize()
        {
            client = new DocumentClient(new Uri(ConfigurationManager.AppSettings["endpoint"]), ConfigurationManager.AppSettings["authKey"]);
            CreateDatabaseIfNotExistsAsync().Wait();
            CreateCollectionIfNotExistsAsync().Wait();
        }
        #endregion

        #region CreateDatabaseIfNotExistsAsync
        /// <summary>
        /// CreateDatabaseIfNotExistsAsync
        /// </summary>
        /// <returns></returns>
        private static async Task CreateDatabaseIfNotExistsAsync()
        {
            try
            {
                await client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(DatabaseId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await client.CreateDatabaseAsync(new Database { Id = DatabaseId });
                }
                else
                {
                    throw;
                }
            }
        }
        #endregion

        #region CreateCollectionIfNotExistsAsync
        /// <summary>
        /// CreateCollectionIfNotExistsAsync 
        /// </summary>
        /// <returns></returns>
        private static async Task CreateCollectionIfNotExistsAsync()
        {
            try
            {
                await client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(DatabaseId),
                        new DocumentCollection { Id = CollectionId },
                        new RequestOptions { OfferThroughput = 1000 });
                }
                else
                {
                    throw;
                }
            }
        }
        #endregion

        //#region GetItemsAsync
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="predicate"></param>
        ///// <returns></returns>
        //public static async Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, bool>> predicate)
        //{
        //    IDocumentQuery<T> query = client.CreateDocumentQuery<T>(
        //        UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId))
        //        .Where(predicate)
        //        .AsDocumentQuery();

        //    List<T> results = new List<T>();
        //    while (query.HasMoreResults)
        //    {
        //        results.AddRange(await query.ExecuteNextAsync<T>());
        //    }

        //    return results;
        //}
        //#endregion

        #region CreateItemAsync
        public static async Task<Document> CreateItemAsync(string item)
        {
            return await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId), item);
        }
        #endregion

    }
}
