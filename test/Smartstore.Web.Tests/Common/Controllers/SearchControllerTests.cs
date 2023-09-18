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
using Smartstore.Core.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

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

        List<Product> products = new(
            //new Product()
            //{
            //    Id = 1,

            //}
            );

        var dbProducts = GetQueryableMockDbSet<Product>(products);
        var catalogSerchResult = new CatalogSearchResult(null, query, dbProducts, 1, null, null, null);
        

        var searchServiceMock = new Mock<ICatalogSearchService>();
        searchServiceMock.Setup(x => x.SearchAsync(query, false)).ReturnsAsync(catalogSerchResult);
        var searchServiceMockObject = searchServiceMock.Object;

        SearchService service = new(settings, searchServiceMockObject, null, null);
        var actual = await service.GetSearchResultModel(query);
        
        Assert.AreEqual(1, actual.TopProducts.Items.Count);
    }

    public static DbSet<T> GetQueryableMockDbSet<T>(List<T> sourceList) where T : class
    {
        var queryable = sourceList.AsQueryable();
        var dbSet = new Mock<DbSet<T>>();
        dbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
        dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
        dbSet.Setup(d => d.Add(It.IsAny<T>())).Callback<T>((s) => sourceList.Add(s));
        return dbSet.Object;
    }
}
