namespace OrderDal.Models;

  public class Order
  {
      public int Id { get; set; }
      public string UserId { get; set; }
      public decimal Amount { get; set; }
      public bool IsSuccessful { get; set; }
  }
