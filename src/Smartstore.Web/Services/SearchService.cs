using Smartstore.Core.Catalog;
using Smartstore.Core.Catalog.Search;
using Smartstore.Web.Models.Catalog;
using Smartstore.Web.Models.Search;

namespace Smartstore.Web.Services;

public class SearchService : PublicController
{

    private readonly SearchSettings _searchSettings;
    private readonly ICatalogSearchService _catalogSearchService;
    private readonly CatalogHelper _catalogHelper;
    private readonly CatalogSettings _catalogSettings;

    public SearchService(SearchSettings searchSettings, ICatalogSearchService catalogSearchService, CatalogHelper catalogHelper, CatalogSettings catalogSettings)
    {
        _searchSettings = searchSettings;
        _catalogSearchService = catalogSearchService;
        _catalogHelper = catalogHelper;
        _catalogSettings = catalogSettings;
    }

    public async Task<SearchResultModel> GetSearchResultModel(CatalogSearchQuery query)
    {
        CatalogSearchResult result = null;
        var model = new SearchResultModel(query);
        var term = query?.DefaultTerm;

        if (term == null || term.Length < _searchSettings.InstantSearchTermMinLength)
        {
            model.SearchResult = new CatalogSearchResult(query);
            model.TopProducts = ProductSummaryModel.Empty;
            model.Error = T("Search.SearchTermMinimumLengthIsNCharacters", _searchSettings.InstantSearchTermMinLength);
            return model;
        }

        try
        {   
            result = await _catalogSearchService.SearchAsync(query);
        }
        catch (Exception ex)
        {
            model.Error = ex.ToString();
            result = new CatalogSearchResult(query);
        }

        //break into it's own method spell check suggestion
        if (result.TotalHitsCount == 0 && result.SpellCheckerSuggestions.Length > 0)
        {
            // No matches, but spell checker made a suggestion.
            // We implicitly search again with the first suggested term.
            var oldSuggestions = result.SpellCheckerSuggestions;

            query.DefaultTerm = oldSuggestions[0];
            result = await _catalogSearchService.SearchAsync(query);

            if (result.TotalHitsCount > 0)
            {
                model.AttemptedTerm = term;
                // Restore the original suggestions.
                result.SpellCheckerSuggestions = oldSuggestions.Where(x => x != query.DefaultTerm).ToArray();
            }
            else
            {
                query.DefaultTerm = term;
            }
        }

        model.SearchResult = result;
        model.Term = query.DefaultTerm;
        model.TotalProductsCount = result.TotalHitsCount;

        var productSummaryViewMode = query.CustomData.Get("ViewMode") is string viewMode && viewMode.EqualsNoCase("list")
            ? ProductSummaryViewMode.List
        : ProductSummaryViewMode.Grid;

        var mappingSettings = _catalogHelper.GetBestFitProductSummaryMappingSettings(productSummaryViewMode);
        var summaryModel = await _catalogHelper.MapProductSummaryModelAsync(result, mappingSettings);

        // Prepare paging/sorting/mode stuff.
        _catalogHelper.MapListActions(summaryModel, null, _catalogSettings.DefaultPageSizeOptions);

        // Add product hits.
        model.TopProducts = summaryModel;
        return model;
    }    
}
