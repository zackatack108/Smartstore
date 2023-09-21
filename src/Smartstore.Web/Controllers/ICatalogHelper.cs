using Smartstore.Collections;
using Smartstore.Core.Catalog.Attributes;
using Smartstore.Core.Catalog.Brands;
using Smartstore.Core.Catalog.Categories;
using Smartstore.Core.Catalog.Products;
using Smartstore.Core.Catalog.Search;
using Smartstore.Core.Content.Media;
using Smartstore.Core.Content.Menus;
using Smartstore.Core.Localization;
using Smartstore.Web.Models.Catalog;
using Smartstore.Web.Models.Media;

namespace Smartstore.Web.Controllers
{
    public interface ICatalogHelper
    {
        DbQuerySettings QuerySettings { get; set; }
        Localizer T { get; set; }

        ProductSummaryMappingSettings GetBestFitProductSummaryMappingSettings(ProductSummaryViewMode viewMode);
        ProductSummaryMappingSettings GetBestFitProductSummaryMappingSettings(ProductSummaryViewMode viewMode, Action<ProductSummaryMappingSettings> fn);
        Task GetBreadcrumbAsync(IBreadcrumb breadcrumb, ActionContext context, Product product = null);
        Task<IEnumerable<int>> GetChildCategoryIdsAsync(int parentCategoryId, bool deep = true);
        Task<List<CategorySummaryModel>> MapCategorySummaryModelAsync(IEnumerable<Category> categories, int thumbSize);
        void MapListActions(ProductSummaryModel model, IPagingOptions entity, string defaultPageSizeOptions);
        Task<ProductDetailsModel> MapProductDetailsPageModelAsync(Product product, ProductVariantQuery query);
        Task<ProductSummaryModel> MapProductSummaryModelAsync(CatalogSearchResult sourceResult, ProductSummaryMappingSettings settings);
        Task<ProductSummaryModel> MapProductSummaryModelAsync(IList<Product> products, ProductSummaryMappingSettings settings);
        Task<ProductSummaryModel> MapProductSummaryModelAsync(IPagedList<Product> products, CatalogSearchResult sourceResult, ProductSummaryMappingSettings settings);
        Task<ProductSummaryModel> MapProductSummaryModelAsync(IPagedList<Product> products, ProductSummaryMappingSettings settings);
        Task<ImageModel> PrepareBrandImageModelAsync(Manufacturer brand, string localizedName, IDictionary<int, MediaFileInfo> fileLookup = null);
        Task<BrandModel> PrepareBrandModelAsync(Manufacturer manufacturer);
        Task<BrandNavigationModel> PrepareBrandNavigationModelAsync(int brandItemsToDisplay);
        Task<List<BrandOverviewModel>> PrepareBrandOverviewModelAsync(ICollection<ProductManufacturer> brands, IDictionary<int, BrandOverviewModel> cachedModels = null, bool withPicture = false);
        Task<ImageModel> PrepareCategoryImageModelAsync(Category category, string localizedName, IDictionary<int, MediaFileInfo> fileLookup = null);
        Task<CategoryModel> PrepareCategoryModelAsync(Category category);
        Task PrepareProductDetailModelAsync(ProductDetailsModel model, ProductDetailsModelContext modelContext, int selectedQuantity = 1, bool callCustomMapper = false);
        MediaGalleryModel PrepareProductDetailsMediaGalleryModel(IList<MediaFileInfo> files, string productName, ICollection<int> allCombinationImageIds, bool isAssociatedProduct, ProductBundleItem bundleItem = null, ProductVariantAttributeCombination combination = null);
        Task PrepareProductReviewsModelAsync(ProductReviewsModel model, Product product, int? take = null);
    }
}