using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMCW.Repository.Helper
{
    public static class QueryBuilder
    {
        public static List<BsonDocument> BuildSearchFilter(string query, string indexName, string clientId, List<string> indexedColumns)
        {
            // Match stage for filtering by ClientId and isDeleted flag
            var matchStage = new BsonDocument
             {
                 { "$match", new BsonDocument
                     {
                         { "ClientId", clientId },
                         { "isDeleted", false }
                     }
                 }
             };

            // Create a BsonArray to hold each autocomplete clause with fuzzy search
            var shouldClauses = new BsonArray();
            foreach (var field in indexedColumns)
            {
                var autocompleteClause = new BsonDocument
                 {
                     { "autocomplete", new BsonDocument
                         {
                             { "query", query },
                             { "path", field },
                             { "fuzzy", new BsonDocument
                                 {
                                     { "maxEdits", 2 },  // Allows up to 2 edits (insertions, deletions, or substitutions)
                                     { "prefixLength", query.Length },  // Require the characters to match exactly
                                     { "maxExpansions", 10 }  // Limit the number of variations considered
                                 }
                             }
                         }
                     }
                 };
                shouldClauses.Add(autocompleteClause);
            }

            var searchStage = new BsonDocument
             {
                 { "$search", new BsonDocument
                     {
                         { "index", indexName },
                         { "compound", new BsonDocument
                             {
                                 { "should", shouldClauses }
                             }
                         }
                     }
                 }
             };

            return new List<BsonDocument> { searchStage, matchStage };
        }
    }
}