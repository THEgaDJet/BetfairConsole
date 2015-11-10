using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetfairConsole
{
  class DoubleMovingAverage : iTradeModel
  {

    private Queue<double> priceQueueOne { get; set; }
    private Queue<double> priceQueueTwo { get; set; }
    private Boolean reset { get; set; }
    private int lengthOne;
    private int lengthTwo;
    private int maxSize;

    public DoubleMovingAverage(int lengthOne, int lengthTwo)
    {
      this.priceQueueOne = new Queue<double>();
      this.priceQueueTwo = new Queue<double>();
      this.reset = true;
      this.lengthOne = lengthOne;
      this.lengthTwo = lengthTwo;
      this.maxSize = lengthTwo;
    }

    // this should take in the JSON object that allows us to pull prices
    public void runModel()
    {
      double[] testPrices = { 100, 101, 100, 100, 98, 94, 100, 100, 100, 109, 111, 109, 100, 101, 100, 100, 98, 94, 100, 100, 100, 109, 111, 109, 100, 101, 100, 100, 98, 94, 100, 100, 100, 109, 111, 109};
      int testCounter = 0;

      while (testCounter < 30)
      {
        testCounter = testCounter + 1;

        AddPriceToQueue(priceQueueOne, testPrices[testCounter], this.lengthOne);
        AddPriceToQueue(priceQueueTwo, testPrices[testCounter], this.lengthTwo);

        if (testCounter > this.maxSize)
        {
          Console.WriteLine("run " + testCounter + " " + this.CalculateMovingAverage(priceQueueOne) + " " + this.CalculateMovingAverage(priceQueueTwo));
          //Console.WriteLine(testCounter);
          // if (EnterTrade(this.CalculateMovingAverage(priceQueueOne),this.CalculateMovingAverage(priceQueueTwo)))
          {
            //Console.WriteLine(this.CalculateMovingAverage(priceQueueOne) + " " + this.CalculateMovingAverage(priceQueueTwo));
          }
        }

      }
    }

    public void AddPriceToQueue(Queue<double> priceQueue, double price, int limit)
    {
      if (priceQueue.Count == limit)
      {
        priceQueue.Dequeue();
        priceQueue.Enqueue(price);
      }
      else
        priceQueue.Enqueue(price);
    }

    public double CalculateMovingAverage(Queue<double> prices)
    {
      double total = 0;
      double average = 0;

      foreach(double price in prices)
      {
        total = total + price;
      }

      average = total / prices.Count;

      return average;
    }

    public void CheckResetModel(double smallMovingAveragePrice, double bigMovingAveragePrice)
    {
      if (smallMovingAveragePrice < bigMovingAveragePrice)
        this.reset = true;
    }
  }
}
