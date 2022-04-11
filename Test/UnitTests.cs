using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;

namespace Test;

public class UnitTests : IDisposable
{
    private HttpClient httpClient;

    public UnitTests()
    {
        var application = new MyWebApplication();
        httpClient = application.CreateClient();   
    }

    public void Dispose()
    {
        httpClient.Dispose();
    }
    private async Task<int> AddUser(Decimal balance = 100)
    {
        var mutation = @"
mutation{{
  addUser(input: {{
    name: 'user10',
    balance: {0}
  }}) {{
    user {{
      id
      name
      balance
    }}
  }}
}}
        ";
        mutation = String.Format(mutation.Replace("\'", "\\\"").Replace("\r\n", " "), balance);
        var responseMessage = await httpClient.PostAsync("/graphql", new StringContent($"{{\"query\": \"{mutation}\"}}", Encoding.UTF8, "application/json"));
        var responseString = await responseMessage.Content.ReadAsStringAsync();

        Assert.True(responseMessage.IsSuccessStatusCode);
        dynamic result = JsonConvert.DeserializeObject(responseString);
        var id = result.data.addUser.user.id;
        return int.Parse(id.ToString());
    }
    private async Task<int> AddItem(Decimal price = 5, int quantity = 100, DateTime? beginAtUtc = null)
    {
        var mutation = @"
mutation{{
  addFireSaleItem(input: {{
    name: 'item1',
    price: {0},
    quantity: {1},
    beginAtUtc: '{2}'
  }}) {{
    fireSaleItem {{
      id
      name
    }}
  }}
}}
        ";
        mutation = String.Format(mutation.Replace("\'", "\\\"").Replace("\r\n", " "), price, quantity, (beginAtUtc ?? DateTime.UtcNow).ToString("o"));
        var responseMessage = await httpClient.PostAsync("/graphql", new StringContent($"{{\"query\": \"{mutation}\"}}", Encoding.UTF8, "application/json"));
        var responseString = await responseMessage.Content.ReadAsStringAsync();

        Assert.True(responseMessage.IsSuccessStatusCode);
        dynamic result = JsonConvert.DeserializeObject(responseString);
        var id = result.data.addFireSaleItem.fireSaleItem.id;
        return int.Parse(id.ToString());
    }

    private async Task<dynamic> StartPurchase(int userId, int itemId)
    {
        var mutation = @"
mutation{{
  purchaseFireSaleItem (input: {{
    userId: {0},
    fireSaleItemId: {1}
  }}) {{
    purchase {{
      userId
      fireSaleItemId
      createdAtUtc
      state
    }}
  }}
}}
        ";
        mutation = String.Format(mutation.Replace("\'", "\\\"").Replace("\r\n", " "), userId, itemId);
        var responseMessage = await httpClient.PostAsync("/graphql", new StringContent($"{{\"query\": \"{mutation}\"}}", Encoding.UTF8, "application/json"));
        var responseString = await responseMessage.Content.ReadAsStringAsync();


        return JsonConvert.DeserializeObject(responseString); 
    }
    private async Task<dynamic> CompletePurchase(int userId, int itemId)
    {
        var mutation = @"
mutation{{
  completePurchase (input: {{
    userId: {0},
    fireSaleItemId: {1}
  }}) {{
    purchase {{
      userId
      user {{id}}
      fireSaleItemId
      fireSaleItem {{ id }}
      createdAtUtc
      state
    }}
  }}
}}
        ";
        mutation = String.Format(mutation.Replace("\'", "\\\"").Replace("\r\n", " "), userId, itemId);
        var responseMessage = await httpClient.PostAsync("/graphql", new StringContent($"{{\"query\": \"{mutation}\"}}", Encoding.UTF8, "application/json"));
        var responseString = await responseMessage.Content.ReadAsStringAsync();


        return JsonConvert.DeserializeObject(responseString); 
    }



    [Fact]
    public async Task PurchaseSecondTimeFails(){
        var userId = await AddUser();
        var itemId = await AddItem();
        var purchaseObject = await StartPurchase(userId, itemId);
        Assert.Equal(userId.ToString(), (string)purchaseObject.data.purchaseFireSaleItem.purchase.userId.ToString());
        Assert.Equal(itemId.ToString(), (string)purchaseObject.data.purchaseFireSaleItem.purchase.fireSaleItemId.ToString());
        Assert.Equal(nameof(PurchaseState.Pending).ToUpper(), (string)purchaseObject.data.purchaseFireSaleItem.purchase.state.ToString());
        Assert.Null(purchaseObject.errors);
        purchaseObject = await StartPurchase(userId, itemId);
        Assert.NotNull(purchaseObject.errors);
        purchaseObject = await CompletePurchase(userId, itemId);
        Assert.Equal(userId.ToString(), (string)purchaseObject.data.completePurchase.purchase.userId.ToString());
        Assert.Equal(itemId.ToString(), (string)purchaseObject.data.completePurchase.purchase.fireSaleItemId.ToString());
        Assert.Equal(nameof(PurchaseState.Completed).ToUpper(), (string)purchaseObject.data.completePurchase.purchase.state.ToString());
        Assert.Null(purchaseObject.errors);
        purchaseObject = await CompletePurchase(userId, itemId);
        Assert.NotNull(purchaseObject.errors);
    }

    [Fact]
    public async Task PurchaseNotEnoughBalanceFails(){
        var userId = await AddUser(balance:99);
        var itemId = await AddItem(price:100);
        var purchaseObject = await StartPurchase(userId, itemId);
        Assert.NotNull(purchaseObject.errors);
    }

    
    [Fact]
    public async Task PurchaseNotEnoughQuantityFails(){
        var userId = await AddUser(balance:99);
        var itemId = await AddItem(quantity:0);
        var purchaseObject = await StartPurchase(userId, itemId);
        Assert.NotNull(purchaseObject.errors);
    }

    [Fact]
    public async Task PurchaseTooEarlyFails(){
        var userId = await AddUser();
        var itemId = await AddItem(beginAtUtc: DateTime.UtcNow.Add(TimeSpan.FromSeconds(1)));
        var purchaseObject = await StartPurchase(userId, itemId);
        Assert.NotNull(purchaseObject.errors);
    }
}