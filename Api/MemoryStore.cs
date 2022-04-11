public class MemoryStore{
    public Dictionary<int, User> Users = new Dictionary<int, User>();
    public Dictionary<int, FireSaleItem> Items = new Dictionary<int, FireSaleItem>();
    public Dictionary<int, List<Purchase>> Purchases = new Dictionary<int, List<Purchase>>();

    private static object locker = new object();

    private Timer timer;

    //Interval in milliseconds
    int interval = 5000;

    private static MemoryStore memoryStore;
    public static MemoryStore Get(ApplicationDbContext context)
    {
        if (memoryStore == null){
            memoryStore = new MemoryStore();
            memoryStore.Users = context.Users.ToDictionary(u => u.Id);
            memoryStore.Items = context.FireSaleItems.ToDictionary(i => i.Id);
            memoryStore.Purchases = context.Purchases.ToList().GroupBy(p => p.UserId).ToDictionary(g => g.Key, g => new List<Purchase>(g.AsEnumerable<Purchase>()));
        }
        return memoryStore;

    }


    public void SetTimer()
    {   
        // this is System.Threading.Timer, of course
        timer = new Timer(UndoExpiredPurchases, null, interval, Timeout.Infinite);
    }

    private void UndoExpiredPurchases(object state)
    {
        try
        {
            lock (locker)
            {
                foreach(var userId in Purchases.Keys)
                {
                    var map = Purchases[userId].ToLookup(p => p.CreatedAtUtc >= DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(30)));
                    var expiredPurchases = map[false];
                    foreach (var purchase in expiredPurchases)
                    {
                        purchase.FireSaleItem.Quantity += 1;
                        purchase.User.Balance += purchase.FireSaleItem.Price;
                    }  
                    Purchases[userId] = map[true].ToList();
                }
            }
        }   
        finally
        {
            timer?.Change(interval, Timeout.Infinite);
        }
    }
    public void AddUser(User user)
    {
        Users[user.Id] = user;
    }
    public void AddFireSaleItem(FireSaleItem item)
    {
        Items[item.Id] = item;
    }

    public Purchase StartPurchase(int userId, int itemId)
    {
        lock (locker)
        {
            if (!Users.TryGetValue(userId, out var user))
            {
                throw new ArgumentException("No such user");
            }
            if (!Items.TryGetValue(itemId, out var item))
            {
                throw new ArgumentException("No such user");
            }
            if (user.Balance < item.Price)
            {
                throw new ArgumentException("Not enough balance");
            }
            if (item.BeginAtUtc > DateTime.UtcNow)
            {
                throw new ArgumentException("Not started yet");
            }            
            if (item.Quantity <= 0)
            {
                throw new ArgumentException("Sold out");
            }
            if (Purchases.TryGetValue(userId, out var userPurchases)){
                if (userPurchases.Any(p => p.FireSaleItemId == itemId))
                {
                    throw new ArgumentException("Already purchased");
                }
            }
            else
            {
                Purchases[userId] =  new List<Purchase>();
            }
            var newPurchase = new Purchase{
                CreatedAtUtc = DateTime.UtcNow,
                FireSaleItemId = itemId,
                FireSaleItem = item,
                UserId = userId,
                User = user,

                State = PurchaseState.Pending

            };
            Purchases[userId].Add(newPurchase);
            user.Balance -= item.Price;
            item.Quantity -= 1;
            return newPurchase;
        }
    }
    public Purchase MarkPurchaseAsCompleted(int userId, int itemId)
    {
        if (!Purchases.TryGetValue(userId, out var userPurchases))
        {
            throw new ArgumentException("No such purchase");
        }
        var purchase = userPurchases.FirstOrDefault(p => p.FireSaleItemId == itemId);
        if (purchase == null || purchase.State != PurchaseState.Pending) 
        {
            throw new ArgumentException("No such purchase/invalid state");
        }
        purchase.State = PurchaseState.Completed;
        return purchase;
    }

}