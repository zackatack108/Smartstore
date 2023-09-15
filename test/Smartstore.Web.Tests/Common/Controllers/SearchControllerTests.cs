using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Smartstore.Core.Catalog.Search;
using Smartstore.Test.Common;
using Smartstore.Web.Controllers;
using Smartstore.Web.Models.Search;
using Microsoft.AspNetCore.Mvc;
using Smartstore.Web.Models.Catalog;
using Smartstore.Collections;
using Moq;
using Smartstore.Core.Catalog.Products;
using Smartstore.Core.Search.Facets;
using Smartstore.Core.Search;
using Smartstore.Web.Services;
using Autofac.Core;

namespace Smartstore.Web.Tests.Common.Controllers;

[TestFixture]
public class SearchControllerTests
{
    [Test]
    public async Task Term_Not_Found()
    {
        SearchSettings settings = new SearchSettings();
        settings.InstantSearchTermMinLength = 2;

        SearchService service = new(settings, null, null, null);
        CatalogSearchQuery query = new CatalogSearchQuery();
        query.DefaultTerm = "a";
        var actual = await service.GetSearchResultModel(query);

        SearchResultModel expected_result = new SearchResultModel(query);
        expected_result.SearchResult = new CatalogSearchResult(query);
        expected_result.TopProducts = ProductSummaryModel.Empty;
        expected_result.Error = "Search.SearchTermMinimumLengthIsNCharacters";

        Assert.AreEqual(expected_result.SearchResult.TotalHitsCount, actual.SearchResult.TotalHitsCount);
        Assert.AreEqual(expected_result.TopProducts, actual.TopProducts);
        Assert.AreEqual(expected_result.Error, actual.Error);
    }

    [Test]
    public async Task Search_Item()
    {
        SearchSettings settings = new SearchSettings();
        settings.InstantSearchTermMinLength = 2;
        CatalogSearchQuery query = new CatalogSearchQuery();
        query.DefaultTerm = "baseball";

        var catalogSerchResult = new CatalogSearchResult(null, query, null, 1, null, null, null);

        var searchServiceMock = new Mock<ICatalogSearchService>();
        searchServiceMock.Setup(x => x.SearchAsync(query, false)).ReturnsAsync(catalogSerchResult);
        var searchServiceMockObject = searchServiceMock.Object;

        SearchService service = new(settings, searchServiceMockObject, null, null);
        var actual = await service.GetSearchResultModel(query);
        
        Assert.AreEqual(1, actual.TopProducts.Items.Count);
    }
}
