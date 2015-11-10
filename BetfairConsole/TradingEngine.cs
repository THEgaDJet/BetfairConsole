using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetfairConsole
{
  class TradingEngine
  {
    public bool ShouldTrade(double smallMovingAveragePrice, double bigMovingAveragePrice)
    {
      //CheckResetModel(smallMovingAveragePrice, bigMovingAveragePrice);

      //if (smallMovingAveragePrice > bigMovingAveragePrice && reset)
      {
        //this.reset = false;
        return true;
      }

      return false;
    }

    void SubmitTrade()
    {
      //call client class to book trade
    }
  }
}
