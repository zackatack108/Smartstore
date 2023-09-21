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
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;
using static System.Formats.Asn1.AsnWriter;
using Smartstore.Core;
using System.Threading;
using System.Collections;

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
        CatalogSearchQuery query = new CatalogSearchQuery("searchterm", "shoes");

        var product = new Product
        {
            ProductType = ProductType.SimpleProduct,
            Name = "SUPERSTAR SHOE",
            MetaTitle = "SUPERSTAR SHOE",
            ShortDescription = "THE STREETWEAR CLASSIC WITH THE SHELL TOE.",
            FullDescription = "<p>The Adidas Superstar was first released in 1969 and soon lived up to its name. Today he is considered a street style legend. In this version, the shoe comes with a comfortable full grain leather upper. The look is perfected by the classic rubber shell toe for added durability.</p>",
            Sku = "Adidas-C77124",
            ProductTemplateId = 1,
            AllowCustomerReviews = true,
            Published = true,
            Price = 99.95M,
            ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            OrderMinimumQuantity = 1,
            OrderMaximumQuantity = 10000,
            StockQuantity = 10000,
            NotifyAdminForQuantityBelow = 1,
            IsShippingEnabled = true,
            DeliveryTime = null,
            DisplayOrder = 5,
            TaxCategoryId = 1
        };

        List<Product> products = new List<Product>{ product };

        var dbProducts = GetQueryableMockDbSet<Product>(products);
        var catalogSearchResult = new CatalogSearchResult(null, query, dbProducts, 1, null, null, null);
        

        var searchServiceMock = new Mock<ICatalogSearchService>();

        searchServiceMock.Setup(x => x.SearchAsync(query,false)).ReturnsAsync(catalogSearchResult);
        var searchServiceMockObject = searchServiceMock.Object;

        var mapSetting = new ProductSummaryMappingSettings();
        var productSummaryModel = new ProductSummaryModel();
        productSummaryModel.Items = new List<ProductSummaryItemModel> {
            new ProductSummaryItemModel(productSummaryModel) {
                SeName = "SUPERSTAR SHOE"
            }
        };

        var catalogHelperMock = new Mock<ICatalogHelper>();
        catalogHelperMock.Setup(x => x.MapProductSummaryModelAsync(catalogSearchResult, null)).ReturnsAsync(productSummaryModel);
        var catalogHelperMockObject = catalogHelperMock.Object;


        CatalogSearchResult actualCatalogSearchResult = null;
        SearchResultModel actualModel = new(query);

        SearchControllerService service = new(searchServiceMockObject, settings, null, catalogHelperMockObject);
        var actual = await service.GetSearchResultService(actualModel, actualCatalogSearchResult, query);
        
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
