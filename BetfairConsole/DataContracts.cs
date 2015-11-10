using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetfairConsole
{
  //Note, we can remove properties that are not listed as required by Betfair in requests and those that we do not need back in reponse parsing
  class DataContracts
  {}

  #region PARAMS
  public class LoginBody
  {
    public KeyValuePair<string, string> username { get; set; }
    public KeyValuePair<string, string> password { get; set; }
  }

  public class MarketCatalogueParams
  {
    public Filter filter { get; set; }
    public string maxResults { get; set; }
    public string[] marketProjection { get; set; }
  }

  public class MarketBookParams
  {
    public List<string> marketIds { get; set; }
    public PriceProjection priceProjection { get; set; }
    public string OrderProjection { get; set; }
    public string MatchProjection { get; set; }
  }

  [DataContract]
  public class PlaceOrderParams
  {
    [DataMember(Name = "marketId")]
    public string MarketId { get; set; }
    [DataMember(Name = "instructions")]
    public List<Instruction> Instructions { get; set; }
    [DataMember(Name = "CustomerRef")]
    public string CustomerRef { get; set; }
  }
  #endregion

  #region REQUESTS
  [DataContract]
  public class GenericRequest
  {
    public string jsonrpc { get; }
    [DataMember(Name = "method")]
    public string Method { get; set; }
    [DataMember(Name = "params")]
    public object Params { get; set; }
    public string id { get; }

    public GenericRequest()
    {
      jsonrpc = Properties.Settings.Default.JsonVersion;
      id = "1";
    }

    public GenericRequest(string method, object _params) : this()
    {
      Method = method;
      Params = _params;
    }
  }
  #endregion

  #region RESPONSES
  public class LoginResponse
  {
    public string sessionToken { get; set; }
    public string loginStatus { get; set; }
  }

  [DataContract]
  public class CatalogueResponse
  {
    [DataMember(Name = "result")]
    public List<CatalogueDetail> catalogueDetails { get; set; }
  }

  [DataContract]
  public class MarketBookResponse
  {
    [DataMember(Name = "result")]
    public List<MarketBook> MarketBooks { get; set; }
  }

  [DataContract]
  public class PlaceOrderResponse
  {
    [DataMember(Name = "result")]
    public PlaceOrderResult OrderResult { get; set; }
  }
  #endregion


  public class Filter
  {
    public string[] eventTypeIds { get; set; }
    public string[] competitionIds { get; set; }
    public string[] marketTypeCodes { get; set; }
    //public bool inPlayOnly { get; set; }
  }

  #region MarketResult

  public class CatalogueDetail
  {
    public string marketId { get; set; }
    public DateTime marketStartTime { get; set; }
    public float totalMatched { get; set; }
    public List<Runner> runners { get; set; }
    public Event Event { get; set; }
  }

  public class MarketCompetitors
  {
    public string marketId { get; set; }
    public string competitor1 { get; set; }  //in football this is home team
    public string competitor2 { get; set; }
    public DateTime marketStartTime { get; set; }
  }

  public class FootballCompetitors : MarketCompetitors
  {
    public string draw { get; set; }
  }
  #endregion

  public class Identifier
  {
    public string id { get; set; }
    public string name { get; set; }
  }

  public class Event
  {
    public string id { get; set; }
    public string name { get; set; }
    public string countryCode { get; set; }
    public string timezone { get; set; }
    public DateTime openDate { get; set; }
  }

  public class Runner
  {
    public long selectionId { get; set; }
    public string runnerName { get; set; }
    public float handicap { get; set; }
    public int sortPriority { get; set; }
    public string runnerId { get; set; } 
  }


  public class MarketBook
  {
    public string marketId { get; set; }
    public bool isMarketDataDelayed { get; set; }
    public string status { get; set; }
    public int betDelay { get; set; }
    public bool bspReconciled { get; set; }
    public bool complete { get; set; }
    public bool inplay { get; set; }
    public int numberOfWinners { get; set; }
    public int numberOfRunners { get; set; }
    public int numberOfActiveRunners { get; set; }
    public DateTime lastMatchTime { get; set; }
    public float totalMatched { get; set; }
    public float totalAvailable { get; set; }
    public bool crossMatching { get; set; }
    public bool runnersVoidable { get; set; }
    public int version { get; set; }
    public List<MarketBookRunner> runners { get; set; }

    public override string ToString()
    {
      return base.ToString();
      //string.Format("marketId:{0} \n\r" +
      //"[runner1_ID:{1}, layPrice: {2}, laySize: {3}, backPrice: {4}, backSize: {5}]\n\r" + 
      //"[runner1_ID:{1}, layPrice: {2}, laySize: {3}, backPrice: {4}, backSize: {5}]", marketId, runners[0].selectionId);
    }
  }

  [DataContract]
  public class MarketBookRunner
  {
    [DataMember(Name = "selectionId")]
    public long SelectionId { get; set; }
    [DataMember(Name = "handicap")]
    public float Handicap { get; set; }
    [DataMember(Name = "status")]
    public string Status { get; set; }
    [DataMember(Name = "lastPriceTraded")]
    public float LastPriceTraded { get; set; }
    [DataMember(Name = "totalMatched")]
    public float TotalMatched { get; set; }
    [DataMember(Name = "ex")]
    public ExchangePrices Prices { get; set; }
  }

  [DataContract]
  public class ExchangePrices
  {
    [DataMember (Name = "availableToBack")]
    public List<PriceSize> availableToBack { get; set; }
    [DataMember(Name = "availableToLay")]
    public List<PriceSize> availableToLay { get; set; }
    [DataMember(Name = "tradedVolume")]
    public List<PriceSize> tradedVolume { get; set; }
  }

  [DataContract]
  public class PriceSize
  {
    [DataMember(Name = "price")]
    public float price { get; set; }
    [DataMember(Name = "size")]
    public float size { get; set; }
  }

  [DataContract]
  public class PriceProjection
  {
    bool virtualise = true;

    [DataMember(Name = "priceData")]
    public ISet<PriceData> PriceData { get; set; }
    [DataMember(Name = "exBestOffersOverrides")]
    public ExBestOffersOverrides ExBestOffersOverrides { get; set; }
    [DataMember(Name = "virtualise")]
    public bool Virtualise
    {
      get { return virtualise; }
      set { virtualise = value; }
    }
  }

  public class ExBestOffersOverrides
  {
    int betPricesDepth = 3;
    string rollupModel = RollupModelEnum.NONE.ToString();
    public int BestPricesDepth
    {
      get { return betPricesDepth; }
      set { betPricesDepth = value; }
    }    
  }

  public class MarketInfo
  {
    public string id { get; set; }
    public DateTime startTime { get; set; }
    public string runnerName { get; set; }
    public long SelectionId { get; set; }
  }

  public class PriceInfo
  {
    public string MarketId { get; set; }
    public string CompetiorName { get; set; }
    public long SelectionId { get; set; }
    public float BackPrice { get; set; }
    public float BackSize { get; set; }
    public float LayPrice { get; set; }
    public float LaySize { get; set; }
    public float TradedPrice { get; set; }
    public float TradedSize { get; set; }
  }

  [DataContract]
  public class PlaceOrderResult
  {
    [DataMember(Name = "marketId")]
    public string MarketId { get; set; }
    [DataMember(Name = "InstructionReports")]
    public List<InstructionReport> InstructionReports { get; set; }
    [DataMember(Name = "status")]
    public string Status { get; set; }
    [DataMember(Name = "errorCode")]
    public string Error { get; set; }
  }

  [DataContract]
  public class InstructionReport
  {
    [DataMember(Name = "instruction")]
    public Instruction Instruction { get; set; }
    [DataMember(Name = "betid")]
    public string BetId { get; set; }
    [DataMember(Name = "placeDate")]
    public DateTime PlacedDate { get; set; }
    [DataMember(Name = "averagePriceMatched")]
    public int AveragePriceMatched { get; set; }
    [DataMember(Name = "sizeMatched")]
    public int SizeMatched { get; set; }
    [DataMember(Name = "status")]
    public string Status { get; set; }
    [DataMember(Name = "errorCode")]
    public string Error { get; set; }
  }

  [DataContract]
  public class Instruction
  {
    [DataMember(Name = "selectionId")]
    public long SelectionId { get; set; }
    [DataMember(Name = "handicap")]
    public double? Handicap { get; set; }
    [DataMember(Name = "side")]
    public string Side { get; set; }
    [DataMember(Name = "orderType")]
    public string OrderType { get; set; }
    [DataMember(Name = "limitOrder")]
    public LimitOrder LimitOrder { get; set; }
  }

  [DataContract]
  public class LimitOrder
  {
    [DataMember(Name = "size")]
    public double Size { get; set; }
    [DataMember(Name = "price")]
    public double Price { get; set; }
    [DataMember(Name = "persistenceType")]
    public string PersistenceType { get; set; }
  }

  public class RunnerId
  {
    public string marketId { get; set; }
    public string runnerName { get; set; }
    public long selectionId { get; set; }
  }

  #region ERROR
  public class ResponseError
  {
    public BetFairError Error { get; set; }
  }

  public class BetFairError
  {
    public string Message { get; set; }
    public string Code { get; set; }
  }
  #endregion

  #region ENUMS
  enum RollupModelEnum
  {
    STAKE,
    PAYOUT,
    MANAGED_LIABILITY,
    NONE
  }

  public enum OrderType
  {
    LIMIT,
    LIMIT_ON_CLOSE,
    MARKET_ON_CLOSE
  }

  public enum Side
  {
    BACK,
    LAY
  }

  public enum PersistenceType
  {
    LAPSE,
    PERSIST,
    MARKET_ON_CLOSE
  }

  public enum PriceData
  {
    SP_AVAILABLE,
    SP_TRADED,
    EX_BEST_OFFERS,
    EX_ALL_OFFERS,
    EX_TRADED,
  }
  #endregion
}