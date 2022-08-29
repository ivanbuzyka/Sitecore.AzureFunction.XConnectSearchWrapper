using Sitecore.XConnect;
using Sitecore.XConnect.Client;
using Sitecore.XConnect.Collection.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XConnectSearchWrapperFunction
{
  internal static class PaginationExample
  {
    public static async Task<List<Interaction>> Run(XConnectClient client)
    {
      var takeCount = 2;
      var skipCount = 2;

      IAsyncQueryable<Sitecore.XConnect.Interaction> queryable = client.Interactions
          .Where(x => x.GetFacet<WebVisit>(CollectionModel.FacetKeys.WebVisit).Browser.BrowserMajorName == "Chrome")
              .Skip(skipCount)
              .Take(takeCount);

      var results = await queryable.ToSearchResults();

      var count = results.Count; // Actual count of all results

      var interactions = await results.Results.Select(x => x.Item).ToList();

      return interactions;
    }
  }
}
