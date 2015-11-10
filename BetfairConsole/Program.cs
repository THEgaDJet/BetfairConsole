using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;

namespace BetfairConsole
{
  class Program
  {
    static void Main(string[] args)
    {
      string sessionToken = GetSessionToken();

      var eventSportId = "1";
      var competitionId = "31";
      var marketTypeCode = "MATCH_ODDS";
      string maxResults = "10";

      var catalogueParams = new MarketCatalogueParams()
      {
        maxResults = maxResults,
        filter = new Filter()
        {
          eventTypeIds = new string[] { eventSportId },
          competitionIds = new string[] { competitionId },
          marketTypeCodes = new string[] { marketTypeCode }
        },        
        marketProjection = new string[]
        {
          "COMPETITION",
          "EVENT",
          "EVENT_TYPE",
          "RUNNER_DESCRIPTION",
          "RUNNER_METADATA",
          "MARKET_START_TIME"
        }
      };

      var catalogueDetails = GetCatalogueDetails(sessionToken, catalogueParams);
      var runnerNamesAndIds = GetRunnerNamesAndSelectionIds(catalogueDetails);
      var marketIds = catalogueDetails.Select(m => m.marketId).ToList();

      ISet<PriceData> priceData = new HashSet<PriceData>();
      priceData.Add(PriceData.EX_BEST_OFFERS);  

      var priceProjection = new PriceProjection();
      priceProjection.PriceData = priceData;

      var marketBookParams = new MarketBookParams()
      {
        marketIds = marketIds,
        priceProjection = priceProjection,
      };

      var marketBooks = GetMarketBooks(sessionToken, marketBookParams);

      var runnerids = marketIds.Join(runnerNamesAndIds,
                                  marketId => marketId,
                                  marketInfo => marketInfo.id,
                                  (marketId, marketInfo) =>
                                  new { marketId = marketId, runnerName = marketInfo.runnerName, selectionId = marketInfo.SelectionId }).ToList();

      var competitorPricesList = new List<PriceInfo>();
      foreach (var book in marketBooks)
      {
        foreach (var runner in book.runners)
        {
          competitorPricesList.Add(new PriceInfo()
          {
            MarketId = book.marketId,
            CompetiorName = runnerids.Find(x => x.selectionId == runner.SelectionId).runnerName,
            SelectionId = runner.SelectionId,
            BackPrice = PricesAvailable(runner.Prices.availableToBack) ? runner.Prices.availableToBack[0].price : 0,
            BackSize = PricesAvailable(runner.Prices.availableToBack) ? runner.Prices.availableToBack[0].size : 0,
            LayPrice = PricesAvailable(runner.Prices.availableToLay) ? runner.Prices.availableToLay[0].price : 0,
            LaySize = PricesAvailable(runner.Prices.availableToLay) ? runner.Prices.availableToLay[0].size : 0,
            TradedPrice = PricesAvailable(runner.Prices.tradedVolume) ? runner.Prices.tradedVolume[0].price : 0,
            TradedSize = PricesAvailable(runner.Prices.tradedVolume) ? runner.Prices.tradedVolume[0].size : 0
          });
        }
      }

      foreach (var priceInfo in competitorPricesList)
      {
        Console.WriteLine(string.Format("marketid:{0}, name:{1}, selectionId:{2}, back:{3}, size:{4}, lay:{5}, size:{6}, traded:{7}, size:{8}",
                          priceInfo.MarketId, priceInfo.CompetiorName, priceInfo.SelectionId, priceInfo.BackPrice, priceInfo.BackSize, 
                          priceInfo.LayPrice, priceInfo.LaySize, priceInfo.TradedPrice, priceInfo.TradedSize));
      }

      var placeOrderParams = new PlaceOrderParams()
      {
        MarketId = "1.121723113",
        CustomerRef = "123456",
        Instructions = new List<Instruction>()
        {
          new Instruction()
          {
            Handicap = 0,
            SelectionId = 1703,
            Side = Side.BACK.ToString(),
            OrderType = OrderType.LIMIT.ToString(),
            LimitOrder = new LimitOrder() { PersistenceType = PersistenceType.LAPSE.ToString(), Price = 1000.0, Size = 0.01 },
          }
        }
      };

      var orderResult = PlaceOrder(sessionToken, placeOrderParams);

      Console.WriteLine(string.Format("{0} {1}", orderResult.Status, orderResult.InstructionReports[0].BetId));

      Console.ReadKey();
    }

    #region API methods
    private static List<CatalogueDetail> GetCatalogueDetails(string sessionToken, MarketCatalogueParams marketCatalogueParams)
    {
      var genericRequest = new GenericRequest(Properties.Settings.Default.MarketCatalogueMethod, marketCatalogueParams);
      var catalogueResponse = GetResultFromResponse<CatalogueResponse>(sessionToken, genericRequest);

      return catalogueResponse.catalogueDetails;
    }

    static List<MarketBook> GetMarketBooks(string sessionToken, MarketBookParams marketBookParams)
    {
      var genericRequest = new GenericRequest(Properties.Settings.Default.MarketBookMethod, marketBookParams);
      var marketBookResponse = GetResultFromResponse<MarketBookResponse>(sessionToken, genericRequest);

      return marketBookResponse.MarketBooks;
    }

    static PlaceOrderResult PlaceOrder(string sessionToken, PlaceOrderParams placeOrderParams)
    {
      var genericRequest = new GenericRequest(Properties.Settings.Default.PlaceOrderMethod, placeOrderParams);
      var placeOrderResponse = GetResultFromResponse<PlaceOrderResponse>(sessionToken, genericRequest);

      return placeOrderResponse.OrderResult;
    }

    static string GetSessionToken()
    {
      var betFairRestAuthClient = new BetFairRestAuthClient();

      var loginBodyUnenc = new List<KeyValuePair<string, string>>();
      loginBodyUnenc.Add(new KeyValuePair<string, string>("username", Properties.Settings.Default.username));
      loginBodyUnenc.Add(new KeyValuePair<string, string>("password", Properties.Settings.Default.password));
      var loginBodyEnc = new FormUrlEncodedContent(loginBodyUnenc);

      var loginResponseString = betFairRestAuthClient.GetPostResponseFormEncoded(Properties.Settings.Default.LoginParams, loginBodyEnc);
      var loginResponse = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<LoginResponse>(loginResponseString)).Result;

      if (loginResponse.loginStatus.Equals("SUCCESS", StringComparison.CurrentCultureIgnoreCase))
      { return loginResponse.sessionToken; }
      else
      { return null; }
    }

    static T GetResultFromResponse<T>(string sessionToken, GenericRequest genericRequest)
    {
      var betFairRestClient = new BetFairRestClient();
      betFairRestClient.AddSessionToken(sessionToken);

      var jsonRespStr = betFairRestClient.GetPostResponseTyped<GenericRequest>(Properties.Settings.Default.RpcParam, genericRequest);
      var responseTyped = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<T>(jsonRespStr)).Result;
      if (!jsonRespStr.Contains("result"))
      {
        var responseError = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<ResponseError>(jsonRespStr)).Result;
        throw new BetFairException(responseError.Error.Message + responseError.Error.Code);
      }

      return responseTyped;
    }
    #endregion

    #region local helpers
    private static List<MarketInfo> GetRunnerNamesAndSelectionIds(List<CatalogueDetail> catalogueDetails)
    {
      var runnerNamesAndIds = new List<MarketInfo>();
      foreach (var catalogueDetail in catalogueDetails)
      {
        foreach (var runner in catalogueDetail.runners)
        {
          runnerNamesAndIds.Add(new MarketInfo
          {
            id = catalogueDetail.marketId,
            startTime = catalogueDetail.marketStartTime,
            runnerName = runner.runnerName,
            SelectionId = runner.selectionId
          });
        }
      }
      return runnerNamesAndIds;
    }

    static bool PricesAvailable(IList<PriceSize> priceType)
    {
      return (priceType != null && priceType.Count > 0);
    }
    #endregion

    [System.Serializable]
    public class BetFairException : Exception
    {
      public BetFairException() { }
      public BetFairException(string message) : base(message) { }
      public BetFairException(string message, Exception inner) : base(message, inner) { }
      protected BetFairException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context)
      { }
    }
  }
}
